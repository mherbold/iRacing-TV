
#region Using

using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Serialization;
using System.Runtime.InteropServices;
using System.Numerics;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

using irsdkSharp;
using irsdkSharp.Enums;
using irsdkSharp.Serialization;
using irsdkSharp.Serialization.Enums.Fastest;
using irsdkSharp.Serialization.Models.Session;
using irsdkSharp.Serialization.Models.Data;
using irsdkSharp.Serialization.Models.Session.DriverInfo;

using Veldrid.ImageSharp;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using Veldrid;

using ImGuiNET;

#endregion

namespace iRacingTV
{
	public partial class MainWindow : Window
	{
		#region DLL imports

		[StructLayout( LayoutKind.Sequential )]
		public struct MARGINS
		{
			public int Left;
			public int Right;
			public int Top;
			public int Bottom;
		}

		[DllImport( "user32.dll", SetLastError = true )]
		private static extern UInt32 GetWindowLong( IntPtr hWnd, int nIndex );
		[DllImport( "user32.dll" )]
		static extern int SetWindowLong( IntPtr hWnd, int nIndex, IntPtr dwNewLong );
		[DllImport( "user32.dll" )]
		static extern bool SetLayeredWindowAttributes( IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags );
		[DllImport( "dwmapi.dll" )]
		static extern int DwmExtendFrameIntoClientArea( IntPtr hwnd, ref MARGINS margins );

		public const int GWL_EXSTYLE = -20;

		public const int WS_EX_TOPMOST = 0x8;
		public const int WS_EX_TRANSPARENT = 0x20;
		public const int WS_EX_LAYERED = 0x80000;

		#endregion

		#region Enums

		private enum TextureEnum
		{
			RaceStatus,
			Leaderboard,
			Sponsor,
			LightBlack,
			LightGreen,
			LightWhite,
			LightYellow,
			FlagCaution,
			FlagCheckered,
			FlagGreen,
			CurrentTarget,
			VoiceOf,
			NumTextures
		};

		private enum CameraGroupEnum
		{
			Inside,
			Close,
			Medium,
			Far,
			Blimp,
			Scenic,
			NumCameraGroups
		};

		#endregion

		#region Consts

		private const int MaxNumDrivers = 63;

		private const string OverlayWindowName = "iRacing TV Overlay";
		private const string GeneralLogFileName = "iRacing-TV.log";
		private const string SettingsFileName = "Settings.xml";

		private const string StatusDisconnectedImageFileName = "status-disconnected.png";
		private const string StatusConnectedImageFileName = "status-connected.png";

		private const int MinimumSendMessageWaitTicks = 50;
		private const int MinimumCameraSwitchWaitTicks = 150;

		#endregion

		#region Private classes

		private class SessionFlagsRecord
		{
			public int sessionTick;
			public float sessionTime;
			public int sessionFlags;

			public SessionFlagsRecord( int _sessionTick, float _sessionTime, int _sessionFlags )
			{
				sessionTick = _sessionTick;
				sessionTime = _sessionTime;
				sessionFlags = _sessionFlags;
			}
		}

		private class TrackedCar : IComparable<TrackedCar>
		{
			public int _driverIdx;

			public DriverModel? _driver;
			public CarModel? _car;

			public float _speed;
			public float _heat;
			public float _distanceToCarInFrontInMeters;
			public float _distanceToCarBehindInMeters;

			public TrackedCar()
			{
				_driverIdx = 0;

				_driver = null;
				_car = null;

				_speed = 0;
				_heat = 0;
				_distanceToCarInFrontInMeters = 0;
				_distanceToCarBehindInMeters = 0;
			}

			public int CompareTo( TrackedCar? other )
			{
				if ( other == null )
				{
					return -1;
				}

				if ( ( _driver == null ) || ( _car == null ) )
				{
					if ( ( other._driver == null ) || ( other._car == null ) )
					{
						return 0;
					}

					return 1;
				}

				if ( ( other._driver == null ) || ( other._car == null ) )
				{
					return -1;
				}

				if ( _car.CarIdxPosition <= 0 )
				{
					if ( other._car.CarIdxPosition <= 0 )
					{
						return 0;
					}

					return 1;
				}

				if ( other._car.CarIdxPosition <= 0 )
				{
					return -1;
				}

				return _car.CarIdxPosition.CompareTo( other._car.CarIdxPosition );
			}
		}

		private class Message
		{
			public BroadcastMessageTypes msg;

			public int var1;
			public int var2;
			public int var3;

			public Message( BroadcastMessageTypes _msg, int _var1, int _var2, int _var3 )
			{
				msg = _msg;

				var1 = _var1;
				var2 = _var2;
				var3 = _var3;
			}
		}

		#endregion

		#region Properties

		private readonly BitmapImage _statusDisconnectedImage = new( new Uri( $"pack://application:,,,/{StatusDisconnectedImageFileName}" ) );
		private readonly BitmapImage _statusConnectedImage = new( new Uri( $"pack://application:,,,/{StatusConnectedImageFileName}" ) );

		private readonly DispatcherTimer _dispatchTimer = new( DispatcherPriority.Render );

		private readonly Vector4 _black = new( 0, 0, 0, 1 );
		private readonly Vector4 _white = new( 1, 1, 1, 1 );

		private readonly IRacingSDK _iRacingSdk = new();

		private bool _overlayIsVisible = false;
		private bool _directorIsEnabled = false;

		private bool _isConnected = false;
		private bool _wasConnected = false;

		private IRacingSessionModel? _session = null;
		private IRacingDataModel? _data = null;

		private int _sessionInfoUpdate = -1;
		private int _sessionID = -1;
		private int _subSessionID = -1;

		private int _sessionFlags = 0;
		private bool _isUnderCaution = false;
		private float _trackLengthInMeters = 0;
		private double _lastSessionTime = 0;
		private double _sessionTimeDelta = 0;
		private int _currentLap = 0;
		private int _radioTransmitCarIdx = -1;
		private bool _positionsValid = false;

		private readonly float[] _lastLapDistPct = new float[ MaxNumDrivers ];
		private readonly float[] _lastSpeed = new float[ MaxNumDrivers ];

		private List<SessionFlagsRecord>? _sessionFlagsRecordList = null;

		public string _appDataFolder = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ) + "\\iRacing-TV\\";
		public string _documentsFolder = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments );

		private Settings _settings = new();

		private Texture[] _textureList = new Texture[ (int) TextureEnum.NumTextures ];
		private readonly nint[] _textureIdList = new nint[ (int) TextureEnum.NumTextures ];

		private Sdl2Window? _sdl2Window = null;
		private GraphicsDevice? _graphicsDevice = null;
		private CommandList? _commandList = null;

		private ImGuiRenderer? _renderer = null;
		private ImFontPtr _fontA = 0;
		private ImFontPtr _fontB = 0;
		private ImFontPtr _fontC = 0;
		private ImFontPtr _fontD = 0;

		private StreamWriter? _sessionFlagsStreamWriter = null;

		private readonly List<TrackedCar> _trackedCarList = new( MaxNumDrivers );

		private float _voiceOfSliderTime = 0.0f;

		private readonly List<Message> _messageBuffer = new();
		private int _sendMessageWaitTicksRemaining = 0;

		private int _currentCameraGroupNumber = 0;
		private int _currentCameraNumber = 0;
		private int _currentCameraCarIdx = 0;
		private int _cameraSwitchWaitTicksRemaining = 0;

		private int _targetCameraCarIdx = 0;
		private int _targetCameraDriverIdx = 0;
		private int _targetCameraGroupNumber = 0;
		private int _targetCameraNumber = 0;
		
		private int[] _cameraGroupNumbers = new int[ (int) CameraGroupEnum.NumCameraGroups ];

		#endregion

		public MainWindow()
		{
			InitializeComponent();

			InitializeLog();

			LoadSettings();

			ReinitializeOverlay();

			for ( var i = 0; i < MaxNumDrivers; i++ )
			{
				_trackedCarList.Add( new TrackedCar() );
			}

			_dispatchTimer.Tick += new EventHandler( DispatchTimer_Tick );
			_dispatchTimer.Interval = TimeSpan.FromMilliseconds( 20 );

			_dispatchTimer.Start();
		}

		#region Log functions

		private void InitializeLog()
		{
			if ( File.Exists( _appDataFolder + GeneralLogFileName ) )
			{
				File.Delete( _appDataFolder + GeneralLogFileName );
			}
		}

		private void Log( string message )
		{
			File.AppendAllText( _appDataFolder + GeneralLogFileName, message );

			StatusTextBox.Text += message;

			StatusTextBox.ScrollToEnd();
		}

		#endregion

		#region Settings functions

		private void LoadSettings()
		{
			Log( "Loading settings..." );

			if ( File.Exists( _appDataFolder + SettingsFileName ) )
			{
				var xmlSerializer = new XmlSerializer( typeof( Settings ) );

				var fileStream = new FileStream( _appDataFolder + SettingsFileName, FileMode.Open );

				var deserializedObject = xmlSerializer.Deserialize( fileStream );

				if ( deserializedObject != null )
				{
					_settings = (Settings) deserializedObject;
				}

				fileStream.Close();
			}

			OverlayX.Text = _settings._overlayX.ToString();
			OverlayY.Text = _settings._overlayY.ToString();
			OverlayWidth.Text = _settings._overlayWidth.ToString();
			OverlayHeight.Text = _settings._overlayHeight.ToString();

			InsideCameraTextBox.Text = _settings._insideCameraGroupName.ToString();
			CloseCameraTextBox.Text = _settings._closeCameraGroupName.ToString();
			MediumCameraTextBox.Text = _settings._mediumCameraGroupName.ToString();
			FarCameraTextBox.Text = _settings._farCameraGroupName.ToString();
			BlimpCameraTextBox.Text = _settings._blimpCameraGroupName.ToString();
			ScenicCameraTextBox.Text = _settings._scenicCameraGroupName.ToString();

			PreferredCarNumber.Text = _settings._preferredCarNumber;

			Log( " OK\r\n" );
		}

		private void SaveSettings()
		{
			Log( "Saving settings..." );

			var xmlSerializer = new XmlSerializer( typeof( Settings ) );

			var streamWriter = new StreamWriter( _appDataFolder + SettingsFileName );

			xmlSerializer.Serialize( streamWriter, _settings );

			streamWriter.Close();

			Log( " OK\r\n" );
		}

		#endregion

		private void DisposeOfOverlay()
		{
			for ( var i = 0; i < _textureList.Length; i++ )
			{
				if ( _textureList[ i ] != null )
				{
					_textureList[ i ].Dispose();
				}
			}

			_textureList = new Texture[ (int) TextureEnum.NumTextures ];

			if ( _renderer != null )
			{
				_renderer.Dispose();

				_renderer = null;
			}

			if ( _graphicsDevice != null )
			{
				_graphicsDevice.Dispose();

				_graphicsDevice = null;
			}

			if ( _commandList != null )
			{
				_commandList.Dispose();

				_commandList = null;
			}

			if ( _sdl2Window != null )
			{
				_sdl2Window.Close();

				_sdl2Window.PumpEvents();

				_sdl2Window = null;
			}
		}

		private void ReinitializeOverlay()
		{
			DisposeOfOverlay();

			#region Window

			Log( "Creating window..." );

			var flags = SDL_WindowFlags.OpenGL | SDL_WindowFlags.Borderless | SDL_WindowFlags.AlwaysOnTop | SDL_WindowFlags.Shown;

			_sdl2Window = new Sdl2Window( OverlayWindowName, 0, 0, _settings._overlayWidth, (int) _settings._overlayHeight, flags, true );

			var windowHandle = _sdl2Window.Handle;

			var windowLong = GetWindowLong( windowHandle, GWL_EXSTYLE );

			SetWindowLong( windowHandle, GWL_EXSTYLE, (IntPtr) windowLong | WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOPMOST );

			MARGINS marg = new() { Left = -1, Right = -1, Top = -1, Bottom = -1 };

			DwmExtendFrameIntoClientArea( windowHandle, ref marg );

			Log( " OK!\r\n" );

			#endregion

			#region Graphics device

			Log( "Creating graphics device..." );

			var graphicsDeviceOptions = new GraphicsDeviceOptions
			{
				Debug = false,
				HasMainSwapchain = true,
				SwapchainSrgbFormat = true
			};

			_graphicsDevice = VeldridStartup.CreateGraphicsDevice( _sdl2Window, graphicsDeviceOptions, GraphicsBackend.Direct3D11 );

			_commandList = _graphicsDevice.ResourceFactory.CreateCommandList();

			Log( " OK!\r\n" );

			#endregion

			#region Dear ImGui renderer

			Log( "Creating Dear ImGui renderer..." );

			_renderer = new ImGuiRenderer( _graphicsDevice, _graphicsDevice.MainSwapchain.Framebuffer.OutputDescription, (int) _graphicsDevice.MainSwapchain.Framebuffer.Width, (int) _graphicsDevice.MainSwapchain.Framebuffer.Height, ColorSpaceHandling.Linear );

			Log( " OK!\r\n" );

			#endregion

			#region Fonts

			Log( "Adding fonts..." );

			_fontA = ImGui.GetIO().Fonts.AddFontFromFileTTF( _settings._fontAFileName, _settings._fontASize );
			_fontB = ImGui.GetIO().Fonts.AddFontFromFileTTF( _settings._fontBFileName, _settings._fontBSize );
			_fontC = ImGui.GetIO().Fonts.AddFontFromFileTTF( _settings._fontCFileName, _settings._fontCSize );
			_fontD = ImGui.GetIO().Fonts.AddFontFromFileTTF( _settings._fontDFileName, _settings._fontDSize );

			_renderer.RecreateFontDeviceTexture();

			Log( " OK!\r\n" );

			#endregion

			#region Textures

			Log( "Loading textures..." );

			var textureFileNameList = new string[ (int) TextureEnum.NumTextures ];

			textureFileNameList[ (int) TextureEnum.RaceStatus ] = _settings._raceStatusFileName;
			textureFileNameList[ (int) TextureEnum.Leaderboard ] = _settings._leaderboardFileName;
			textureFileNameList[ (int) TextureEnum.Sponsor ] = _settings._sponsorFileName;
			textureFileNameList[ (int) TextureEnum.LightBlack ] = _settings._lightBlackFileName;
			textureFileNameList[ (int) TextureEnum.LightGreen ] = _settings._lightGreenFileName;
			textureFileNameList[ (int) TextureEnum.LightWhite ] = _settings._lightWhiteFileName;
			textureFileNameList[ (int) TextureEnum.LightYellow ] = _settings._lightYellowFileName;
			textureFileNameList[ (int) TextureEnum.FlagCaution ] = _settings._flagCautionFileName;
			textureFileNameList[ (int) TextureEnum.FlagCheckered ] = _settings._flagCheckeredFileName;
			textureFileNameList[ (int) TextureEnum.FlagGreen ] = _settings._flagGreenFileName;
			textureFileNameList[ (int) TextureEnum.CurrentTarget ] = _settings._currentTargetFileName;
			textureFileNameList[ (int) TextureEnum.VoiceOf ] = _settings._voiceOfFileName;

			for ( var i = 0; i < _textureList.Length; i++ )
			{
				var imageSharpTexture = new ImageSharpTexture( textureFileNameList[ i ] );

				_textureList[ i ] = imageSharpTexture.CreateDeviceTexture( _graphicsDevice, _graphicsDevice.ResourceFactory );

				var textureViewDescription = new TextureViewDescription( _textureList[ i ], PixelFormat.R8_G8_B8_A8_UNorm_SRgb );

				var textureView = _graphicsDevice.ResourceFactory.CreateTextureView( textureViewDescription );

				_textureIdList[ i ] = _renderer.GetOrCreateImGuiBinding( _graphicsDevice.ResourceFactory, textureView );
			}

			Log( " OK!\r\n" );

			#endregion

			if ( !_overlayIsVisible )
			{
				_sdl2Window.Visible = false;
			}
		}

		private void DispatchTimer_Tick( object? sender, EventArgs e )
		{
			UpdateIRacingSDK();

			if ( _sessionTimeDelta > 0 )
			{
				UpdateTrackedCarList();
				UpdateDirector();
			}

			UpdateOverlay();
		}

		private Vector4 ColorHexToVector( string hex )
		{
			var color = ColorTranslator.FromHtml( hex );

			return new Vector4()
			{
				X = color.R / 255.0f,
				Y = color.G / 255.0f,
				Z = color.B / 255.0f,
				W = 1.0f
			};
		}

		private uint ColorVectorToUint( Vector4 vector )
		{
			return 0xFF000000 | ( ( (uint) Math.Round( vector.Z * 255.0 ) ) << 16 ) | ( ( (uint) Math.Round( vector.Y * 255.0 ) ) << 8 ) | ( (uint) Math.Round( vector.X * 255.0 ) );
		}

		private void UpdateCameraGroupNumbers()
		{
			for ( var i = 0; i < _cameraGroupNumbers.Length; i++ )
			{
				_cameraGroupNumbers[ i ] = 0;
			}

			if ( _session != null )
			{
				foreach ( var cameraGroup in _session.CameraInfo.Groups )
				{
					if ( cameraGroup.GroupName == _settings._insideCameraGroupName )
					{
						_cameraGroupNumbers[ (int) CameraGroupEnum.Inside ] = cameraGroup.GroupNum;
					}
					else if ( cameraGroup.GroupName == _settings._closeCameraGroupName )
					{
						_cameraGroupNumbers[ (int) CameraGroupEnum.Close ] = cameraGroup.GroupNum;
					}
					else if ( cameraGroup.GroupName == _settings._mediumCameraGroupName )
					{
						_cameraGroupNumbers[ (int) CameraGroupEnum.Medium ] = cameraGroup.GroupNum;
					}
					else if ( cameraGroup.GroupName == _settings._farCameraGroupName )
					{
						_cameraGroupNumbers[ (int) CameraGroupEnum.Far ] = cameraGroup.GroupNum;
					}
					else if ( cameraGroup.GroupName == _settings._blimpCameraGroupName )
					{
						_cameraGroupNumbers[ (int) CameraGroupEnum.Blimp ] = cameraGroup.GroupNum;
					}
					else if ( cameraGroup.GroupName == _settings._scenicCameraGroupName)
					{
						_cameraGroupNumbers[ (int) CameraGroupEnum.Scenic ] = cameraGroup.GroupNum;
					}
				}
			}
		}

		private void UpdateIRacingSDK()
		{
			_isConnected = _iRacingSdk.IsConnected();

			#region Connection status update

			if ( _isConnected != _wasConnected )
			{
				_wasConnected = _isConnected;

				StatusImage.Source = _isConnected ? _statusConnectedImage : _statusDisconnectedImage;

				ScanForIncidentsButton.IsEnabled = _isConnected;
				ShowOverlayButton.IsEnabled = _isConnected;
				EnableAIDirectorButton.IsEnabled = _isConnected;
			}

			#endregion

			if ( _isConnected )
			{
				#region Update session info

				if ( _sessionInfoUpdate != _iRacingSdk.Header.SessionInfoUpdate )
				{
					#region Session info

					_sessionInfoUpdate = _iRacingSdk.Header.SessionInfoUpdate;

					_session = _iRacingSdk.GetSerializedSessionInfo();

					#endregion

					#region Update track length

					var match = Regex.Match( _session.WeekendInfo.TrackLength, @"([-+]?[0-9]*\.?[0-9]+)" );

					if ( match.Success )
					{
						var trackLengthInKilometers = Convert.ToSingle( match.Groups[ 1 ].Value );

						_trackLengthInMeters = trackLengthInKilometers * 1000;
					}
					else
					{
						Log( "WARNING - Track length could not be determined!" );
					}

					#endregion

					#region Update camera group numbers

					UpdateCameraGroupNumbers();

					#endregion

					#region Session flags file

					if ( ( _sessionID != _session.WeekendInfo.SessionID ) || ( _subSessionID != _session.WeekendInfo.SubSessionID ) )
					{
						_sessionID = _session.WeekendInfo.SessionID;
						_subSessionID = _session.WeekendInfo.SubSessionID;

						var sessionFlagsFileName = $"{_documentsFolder}\\iRacing-TV\\SessionFlags\\{_sessionID}-{_subSessionID}.csv";

						if ( _session.WeekendInfo.SimMode == "replay" )
						{
							_sessionFlagsRecordList = new List<SessionFlagsRecord>();

							if ( File.Exists( sessionFlagsFileName ) )
							{
								Log( $"Loading session flags file..." );

								var streamReader = File.OpenText( sessionFlagsFileName );

								while ( true )
								{
									var line = streamReader.ReadLine();

									if ( line == null )
									{
										break;
									}

									match = Regex.Match( line, @"(.*),(.*),0x(.*)" );

									if ( match.Success )
									{
										_sessionFlagsRecordList.Add( new SessionFlagsRecord( int.Parse( match.Groups[ 1 ].Value ), float.Parse( match.Groups[ 2 ].Value ), int.Parse( match.Groups[ 3 ].Value, System.Globalization.NumberStyles.HexNumber ) ) );
									}
								}

								Log( $" OK!\r\n" );
							}

							_sessionFlagsRecordList.Reverse();
						}
						else
						{
							if ( _sessionFlagsStreamWriter != null )
							{
								_sessionFlagsStreamWriter.Close();

								_sessionFlagsStreamWriter = null;
							}

							Log( "Opening session flags file..." );

							Directory.CreateDirectory( $"{_documentsFolder}\\iRacing-TV\\SessionFlags" );

							_sessionFlagsStreamWriter = File.AppendText( sessionFlagsFileName );

							Log( $" OK!\r\n" );
						}
					}

					#endregion
				}

				#endregion

				#region Update telemetry data

				if ( _data != null )
				{
					_lastSessionTime = _data.Data.SessionTime;
				}

				_data = _iRacingSdk.GetSerializedData();

				_sessionTimeDelta = 0;

				if ( ( _data.Data.SessionTime > 0 ) && ( _lastSessionTime > 0 ) )
				{
					_sessionTimeDelta = Math.Max( 0, _data.Data.SessionTime - _lastSessionTime );

					_sessionTimeDelta = Math.Round( _sessionTimeDelta / ( 1.0f / 60.0f ) ) * ( 1.0f / 60.0f );
				}

				#endregion

				#region Session flags recording and playback

				var lastSessionFlags = _sessionFlags;

				if ( _session != null )
				{
					if ( _session.WeekendInfo.SimMode == "replay" )
					{
						_sessionFlags = 0;

						if ( _sessionFlagsRecordList != null )
						{
							foreach ( var sessionFlagsRecord in _sessionFlagsRecordList )
							{
								if ( _data.Data.SessionTime >= sessionFlagsRecord.sessionTime )
								{
									_sessionFlags = sessionFlagsRecord.sessionFlags;

									break;
								}
							}
						}
					}
					else
					{
						if ( _sessionFlags != _data.Data.SessionFlags )
						{
							_sessionFlags = _data.Data.SessionFlags;

							if ( _sessionFlagsStreamWriter != null )
							{
								var sessionFlagsAsHex = _sessionFlags.ToString( "X8" );

								_sessionFlagsStreamWriter.WriteLine( $"{_data.Data.SessionTick},{_data.Data.SessionTime:0.000},0x{sessionFlagsAsHex}" );
								_sessionFlagsStreamWriter.Flush();
							}
						}
					}
				}

				#region Report changes to session flags

				if ( _sessionFlags != lastSessionFlags )
				{
					var sessionFlagsString = string.Empty;

					if ( ( _sessionFlags & (uint) SessionFlags.Checkered ) != 0 )
					{
						sessionFlagsString += SessionFlags.Checkered;
					}

					if ( ( _sessionFlags & (uint) SessionFlags.White ) != 0 )
					{
						sessionFlagsString += SessionFlags.White;
					}

					if ( ( _sessionFlags & (uint) SessionFlags.Green ) != 0 )
					{
						sessionFlagsString += SessionFlags.Green;
					}

					if ( ( _sessionFlags & (uint) SessionFlags.Yellow ) != 0 )
					{
						sessionFlagsString += SessionFlags.Yellow;
					}

					if ( ( _sessionFlags & (uint) SessionFlags.Red ) != 0 )
					{
						sessionFlagsString += SessionFlags.Red;
					}

					if ( ( _sessionFlags & (uint) SessionFlags.Blue ) != 0 )
					{
						sessionFlagsString += SessionFlags.Blue;
					}

					if ( ( _sessionFlags & (uint) SessionFlags.Debris ) != 0 )
					{
						sessionFlagsString += SessionFlags.Debris;
					}

					if ( ( _sessionFlags & (uint) SessionFlags.Crossed ) != 0 )
					{
						sessionFlagsString += SessionFlags.Crossed;
					}

					if ( ( _sessionFlags & (uint) SessionFlags.YellowWaving ) != 0 )
					{
						sessionFlagsString += SessionFlags.YellowWaving;
					}

					if ( ( _sessionFlags & (uint) SessionFlags.OneLapToGreen ) != 0 )
					{
						sessionFlagsString += SessionFlags.OneLapToGreen;
					}

					if ( ( _sessionFlags & (uint) SessionFlags.GreenHeld ) != 0 )
					{
						sessionFlagsString += SessionFlags.GreenHeld;
					}

					if ( ( _sessionFlags & (uint) SessionFlags.TenToGo ) != 0 )
					{
						sessionFlagsString += SessionFlags.TenToGo;
					}

					if ( ( _sessionFlags & (uint) SessionFlags.FiveToGo ) != 0 )
					{
						sessionFlagsString += SessionFlags.FiveToGo;
					}

					if ( ( _sessionFlags & (uint) SessionFlags.RandomWaving ) != 0 )
					{
						sessionFlagsString += SessionFlags.RandomWaving;
					}

					if ( ( _sessionFlags & (uint) SessionFlags.Caution ) != 0 )
					{
						sessionFlagsString += SessionFlags.Caution;
					}

					if ( ( _sessionFlags & (uint) SessionFlags.CautionWaving ) != 0 )
					{
						sessionFlagsString += SessionFlags.CautionWaving;
					}

					if ( ( _sessionFlags & (uint) SessionFlags.Black ) != 0 )
					{
						sessionFlagsString += SessionFlags.Black;
					}

					if ( ( _sessionFlags & (uint) SessionFlags.Disqualify ) != 0 )
					{
						sessionFlagsString += SessionFlags.Disqualify;
					}

					if ( ( _sessionFlags & (uint) SessionFlags.Servicible ) != 0 )
					{
						sessionFlagsString += SessionFlags.Servicible;
					}

					if ( ( _sessionFlags & (uint) SessionFlags.Furled ) != 0 )
					{
						sessionFlagsString += SessionFlags.Furled;
					}

					if ( ( _sessionFlags & (uint) SessionFlags.Repair ) != 0 )
					{
						sessionFlagsString += SessionFlags.Repair;
					}

					if ( ( _sessionFlags & (uint) SessionFlags.StartHidden ) != 0 )
					{
						sessionFlagsString += SessionFlags.StartHidden;
					}

					if ( ( _sessionFlags & (uint) SessionFlags.StartReady ) != 0 )
					{
						sessionFlagsString += SessionFlags.StartReady;
					}

					if ( ( _sessionFlags & (uint) SessionFlags.StartSet ) != 0 )
					{
						sessionFlagsString += SessionFlags.StartSet;
					}

					if ( ( _sessionFlags & (uint) SessionFlags.StartGo ) != 0 )
					{
						sessionFlagsString += SessionFlags.StartGo;
					}

					Log( $"Session flags = {sessionFlagsString}\r\n" );
				}

				#endregion

				#endregion

				#region Update misc properties

				_isUnderCaution = ( ( (uint) _sessionFlags & ( (uint) SessionFlags.CautionWaving | (uint) SessionFlags.Caution | (uint) SessionFlags.YellowWaving | (uint) SessionFlags.Yellow ) ) != 0 );
				_currentLap = _data.Data.SessionLapsTotal - _data.Data.SessionLapsRemain;

				#endregion
			}
			else
			{
				_session = null;
				_data = null;

				_sessionInfoUpdate = -1;
				_sessionID = -1;
				_subSessionID = -1;

				_sessionFlags = 0;
				_isUnderCaution = false;
				_trackLengthInMeters = 0;
				_lastSessionTime = 0;
				_sessionTimeDelta = 0;
				_currentLap = 0;
				_radioTransmitCarIdx = -1;
				_positionsValid = false;

				for ( var i = 0; i < MaxNumDrivers; i++ )
				{
					_lastLapDistPct[ i ] = 0;
					_lastSpeed[ i ] = 0;
				}

				_messageBuffer.Clear();

				_sendMessageWaitTicksRemaining = 0;

				_currentCameraGroupNumber = 0;
				_currentCameraNumber = 0;
				_currentCameraCarIdx = 0;
				_cameraSwitchWaitTicksRemaining = 0;

				_targetCameraGroupNumber = 0;
				_targetCameraNumber = 0;
				_targetCameraCarIdx = 0;
				_targetCameraDriverIdx = 0;
			}
		}

		private void UpdateTrackedCarList()
		{
			if ( ( _session == null ) || ( _data == null ) )
			{
				return;
			}

			#region Reset tracked cars

			for ( var i = 0; i < MaxNumDrivers; i++ )
			{
				var trackedCar = _trackedCarList[ i ];

				trackedCar._driverIdx = 0;

				trackedCar._driver = null;
				trackedCar._car = null;

				trackedCar._speed = 0;
				trackedCar._heat = 0;
				trackedCar._distanceToCarInFrontInMeters = float.MaxValue;
				trackedCar._distanceToCarBehindInMeters = float.MaxValue;
			}

			#endregion

			#region Update tracked cars (except for heat and distances)

			_positionsValid = false;

			for ( var i = 0; i < _session.DriverInfo.Drivers.Count; i++ )
			{
				var driver = _session.DriverInfo.Drivers[ i ];

				if ( ( driver.CarIsPaceCar != "1" ) && ( driver.IsSpectator == 0 ) )
				{
					var trackedCar = _trackedCarList[ i ];

					var car = _data.Data.Cars[ driver.CarIdx ];

					trackedCar._driverIdx = i;
					trackedCar._driver = driver;
					trackedCar._car = car;
					trackedCar._speed = 0;

					if ( _lastLapDistPct[ i ] >= 0 )
					{
						var deltaLapDistPct = car.CarIdxLapDistPct - _lastLapDistPct[ i ];

						if ( deltaLapDistPct > 0.5f )
						{
							deltaLapDistPct -= 1.0f;
						}
						else if ( deltaLapDistPct < -0.5f )
						{
							deltaLapDistPct += 1.0f;
						}

						deltaLapDistPct = Math.Abs( deltaLapDistPct );

						if ( deltaLapDistPct < 0.05f )
						{
							trackedCar._speed = _lastSpeed[ i ] * 0.92f + (float) ( deltaLapDistPct / _sessionTimeDelta * _trackLengthInMeters ) * 0.08f;
						}
					}

					_lastLapDistPct[ i ] = car.CarIdxLapDistPct;
					_lastSpeed[ i ] = trackedCar._speed;

					if ( car.CarIdxPosition > 1 )
					{
						_positionsValid = true;
					}
				}
			}

			#endregion

			#region Update heat and distances

			for ( var i = 0; i < _session.DriverInfo.Drivers.Count; i++ )
			{
				var trackedCar = _trackedCarList[ i ];

				if ( ( trackedCar._driver != null ) && ( trackedCar._car != null ) )
				{
					if ( ( trackedCar._driver.CarIsPaceCar != "1" ) && ( trackedCar._driver.IsSpectator == 0 ) && !trackedCar._car.CarIdxOnPitRoad && ( trackedCar._car.CarIdxLapDistPct >= 0 ) )
					{
						for ( var j = 0; j < _session.DriverInfo.Drivers.Count; j++ )
						{
							var otherTrackedCar = _trackedCarList[ j ];

							if ( ( otherTrackedCar._driver != null ) && ( otherTrackedCar._car != null ) )
							{
								if ( ( otherTrackedCar._driver.CarIsPaceCar != "1" ) && ( otherTrackedCar._driver.IsSpectator == 0 ) && !otherTrackedCar._car.CarIdxOnPitRoad && ( otherTrackedCar._car.CarIdxLapDistPct >= 0 ) )
								{
									var distanceToOtherCar = otherTrackedCar._car.CarIdxLapDistPct - trackedCar._car.CarIdxLapDistPct;

									if ( distanceToOtherCar > 0.5f )
									{
										distanceToOtherCar -= 1.0f;
									}
									else if ( distanceToOtherCar < -0.5f )
									{
										distanceToOtherCar += 1.0f;
									}

									var heat = Math.Max( 0.0f, 1.0f - Math.Abs( distanceToOtherCar ) * 10.0f );

									trackedCar._heat += heat * heat;

									if ( i != j )
									{
										var distanceToCarInMeters = distanceToOtherCar * _trackLengthInMeters;

										if ( distanceToCarInMeters >= 0.0f )
										{
											trackedCar._distanceToCarInFrontInMeters = Math.Min( trackedCar._distanceToCarInFrontInMeters, distanceToCarInMeters );
										}
										else
										{
											trackedCar._distanceToCarBehindInMeters = Math.Min( trackedCar._distanceToCarBehindInMeters, -distanceToCarInMeters );
										}
									}
								}
							}
						}
					}
				}
			}

			#endregion

			_trackedCarList.Sort();
		}

		private void UpdateDirector()
		{
			if ( !_directorIsEnabled || ( _session == null ) || ( _data == null ) )
			{
				return;
			}

			#region Camera selection

			var cameraSwitchReason = string.Empty;
			var cameraGroup = CameraGroupEnum.Far;

			_targetCameraNumber = 0;

			if ( ( _sessionFlags & ( (uint) SessionFlags.Checkered ) ) != 0 )
			{
				cameraGroup = CameraGroupEnum.Scenic;

				_targetCameraNumber = 1;

				cameraSwitchReason = $"Session flags = Checkered.";
			}
			else if ( !_positionsValid || ( ( _sessionFlags & ( (uint) SessionFlags.GreenHeld | (uint) SessionFlags.StartReady | (uint) SessionFlags.StartSet | (uint) SessionFlags.StartGo ) ) != 0 ) )
			{
				var highestLapDistPct = 0.0f;

				for ( var i = 0; i < _session.DriverInfo.Drivers.Count; i++ )
				{
					var trackedCar = _trackedCarList[ i ];

					if ( ( trackedCar._driver != null ) && ( trackedCar._car != null ) )
					{
						if ( trackedCar._car.CarIdxLapDistPct > highestLapDistPct )
						{
							highestLapDistPct = trackedCar._car.CarIdxLapDistPct;

							_targetCameraCarIdx = trackedCar._driver.CarIdx;
							_targetCameraDriverIdx = trackedCar._driverIdx;

							cameraSwitchReason = $"Positions not valid or session flags = GreenHeld|StartReady|StartSet|StartGo, Car #{trackedCar._driver.CarNumber}.";
						}
					}
				}
			}
			else
			{
				var highestHeat = 0.0f;

				for ( var i = 0; i < _session.DriverInfo.Drivers.Count; i++ )
				{
					var trackedCar = _trackedCarList[ i ];

					if ( trackedCar._driver != null )
					{
						if ( trackedCar._heat > highestHeat )
						{
							highestHeat = trackedCar._heat;

							_targetCameraCarIdx = trackedCar._driver.CarIdx;
							_targetCameraDriverIdx = trackedCar._driverIdx;

							cameraSwitchReason = $"Hottest car #{trackedCar._driver.CarNumber}.";
						}
					}
				}

				cameraGroup = CameraGroupEnum.Medium;

				if ( ( _sessionFlags & ( (uint) SessionFlags.CautionWaving | (uint) SessionFlags.YellowWaving ) ) != 0 )
				{
					cameraGroup = CameraGroupEnum.Far;

					cameraSwitchReason += " (Caution waving)";
				}
				else
				{
					for ( var i = 0; i < _session.DriverInfo.Drivers.Count; i++ )
					{
						var trackedCar = _trackedCarList[ i ];

						if ( trackedCar._driver != null )
						{
							if ( trackedCar._driver.CarNumber == _settings._preferredCarNumber )
							{
								var nearestCarDistance = Math.Min( trackedCar._distanceToCarInFrontInMeters, trackedCar._distanceToCarBehindInMeters );

								if ( nearestCarDistance < 50 )
								{
									_targetCameraCarIdx = trackedCar._driver.CarIdx;
									_targetCameraDriverIdx = trackedCar._driverIdx;

									cameraSwitchReason = $"Preferred car #{trackedCar._driver.CarNumber}.";

									if ( _isUnderCaution )
									{
										cameraGroup = CameraGroupEnum.Far;

										cameraSwitchReason += " (Under caution)";
									}
									else if ( trackedCar._distanceToCarInFrontInMeters < 10 )
									{
										cameraGroup = CameraGroupEnum.Inside;

										cameraSwitchReason += " (Car in front < 10m)";
									}
									else if ( nearestCarDistance < 10 )
									{
										cameraGroup = CameraGroupEnum.Close;

										cameraSwitchReason += " (Car within 10m)";
									}
									else if ( nearestCarDistance < 20 )
									{
										cameraGroup = CameraGroupEnum.Medium;

										cameraSwitchReason += " (Car within 20m)";
									}
									else if ( nearestCarDistance < 30 )
									{
										cameraGroup = CameraGroupEnum.Far;

										cameraSwitchReason += " (Car within 30m)";
									}
									else
									{
										cameraGroup = CameraGroupEnum.Blimp;

										cameraSwitchReason += " (Car within 50m)";
									}
								}

								break;
							}
						}
					}
				}
			}

			_targetCameraGroupNumber = _cameraGroupNumbers[ (int) cameraGroup ];

			cameraSwitchReason += $" ({cameraGroup})";

			#endregion

			#region Message dispatching

			if ( _session != null )
			{
				_cameraSwitchWaitTicksRemaining--;

				if ( ( _data.Data.CamGroupNumber != _currentCameraGroupNumber ) || ( _data.Data.CamCameraNumber != _currentCameraNumber ) || ( _data.Data.CamCarIdx != _currentCameraCarIdx ) )
				{
					_currentCameraGroupNumber = _data.Data.CamGroupNumber;
					_currentCameraNumber = _data.Data.CamCameraNumber;
					_currentCameraCarIdx = _data.Data.CamCarIdx;

					_cameraSwitchWaitTicksRemaining = MinimumCameraSwitchWaitTicks;
				}

				_sendMessageWaitTicksRemaining--;

				if ( _sendMessageWaitTicksRemaining <= 0 )
				{
					if ( _messageBuffer.Count > 0 )
					{
						var message = _messageBuffer.First();

						_messageBuffer.RemoveAt( 0 );

						_iRacingSdk.BroadcastMessage( message.msg, message.var1, message.var2, message.var3 );

						_sendMessageWaitTicksRemaining = MinimumSendMessageWaitTicks;
					}
					else if ( ( _cameraSwitchWaitTicksRemaining <= 0 ) && ( ( _currentCameraCarIdx != _targetCameraCarIdx ) || ( _currentCameraGroupNumber != _targetCameraGroupNumber ) || ( _currentCameraNumber != _targetCameraNumber ) ) )
					{
						var carNumberRaw = _session.DriverInfo.Drivers[ _targetCameraDriverIdx ].CarNumberRaw;

						_iRacingSdk.BroadcastMessage( BroadcastMessageTypes.CamSwitchNum, carNumberRaw, _targetCameraGroupNumber, _targetCameraNumber );

						_sendMessageWaitTicksRemaining = MinimumSendMessageWaitTicks;
						_cameraSwitchWaitTicksRemaining = MinimumCameraSwitchWaitTicks;

						Log( $"{cameraSwitchReason} ({_targetCameraNumber})\r\n" );
					}
				}
			}

			#endregion
		}

		private void UpdateOverlay()
		{
			if ( _sdl2Window != null )
			{
				var inputSnapshot = _sdl2Window.PumpEvents();

				if ( ( _graphicsDevice != null ) && ( _commandList != null ) && ( _renderer != null ) && ( _session != null ) && ( _data != null ) )
				{
					#region Begin

					_commandList.Begin();
					_commandList.SetFramebuffer( _graphicsDevice.MainSwapchain.Framebuffer );
					_commandList.SetFullViewports();
					_commandList.ClearColorTarget( 0, new RgbaFloat( 0.0f, 0.0f, 0.0f, 0.0f ) );

					_renderer.Update( 0.1f, inputSnapshot );

					string textString;
					Vector2 textSize;
					Vector4 textColor;

					ImGui.PushStyleVar( ImGuiStyleVar.WindowPadding, Vector2.Zero );
					ImGui.Begin( OverlayWindowName, ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoBackground );
					ImGui.SetWindowPos( Vector2.Zero );
					ImGui.SetWindowSize( new Vector2( _graphicsDevice.MainSwapchain.Framebuffer.Width, _graphicsDevice.MainSwapchain.Framebuffer.Height ) );

					ImDrawListPtr drawList = ImGui.GetWindowDrawList();

					#endregion

					#region Race status image

					ImGui.SetCursorPos( _settings._raceStatusPosition );
					ImGui.Image( _textureIdList[ (int) TextureEnum.RaceStatus ], new Vector2( _textureList[ (int) TextureEnum.RaceStatus ].Width, _textureList[ (int) TextureEnum.RaceStatus ].Height ), Vector2.Zero, Vector2.One, _settings._raceStatusTint );

					#endregion

					#region Leaderboard image

					if ( _positionsValid )
					{
						ImGui.SetCursorPos( _settings._leaderboardPosition );
						ImGui.Image( _textureIdList[ (int) TextureEnum.Leaderboard ], new Vector2( _textureList[ (int) TextureEnum.Leaderboard ].Width, _textureList[ (int) TextureEnum.Leaderboard ].Height ), Vector2.Zero, Vector2.One, _settings._leaderboardTint );
					}

					#endregion

					#region Sponsor image

					ImGui.SetCursorPos( _settings._sponsorPosition );
					ImGui.Image( _textureIdList[ (int) TextureEnum.Sponsor ], new Vector2( _textureList[ (int) TextureEnum.Sponsor ].Width, _textureList[ (int) TextureEnum.Sponsor ].Height ) );

					#endregion

					#region Lap string

					ImGui.PushFont( _fontB );
					ImGui.SetCursorPos( _settings._lapPosition );
					ImGui.TextColored( _settings._lapColor, _settings._lapString );
					ImGui.PopFont();

					#endregion

					#region Laps remaining

					if ( _data.Data.SessionLapsRemain == 0 )
					{
						ImGui.PushFont( _fontB );
						textString = _settings._finalLapString;
						textSize = ImGui.CalcTextSize( textString );
						ImGui.SetCursorPos( _settings._lapsRemainingPosition - new Vector2( textSize.X, 0.0f ) );
						ImGui.TextColored( _settings._lapsRemainingColor, textString );
						ImGui.PopFont();
					}
					else if ( ( _data.Data.SessionLapsRemain > 0 ) && ( _data.Data.SessionLapsRemain != 32767 ) )
					{
						ImGui.PushFont( _fontB );
						textString = Math.Min( _data.Data.SessionLapsTotal, _data.Data.SessionLapsRemain + 1 ).ToString() + _settings._lapsRemainingString;
						textSize = ImGui.CalcTextSize( textString );
						ImGui.SetCursorPos( _settings._lapsRemainingPosition - new Vector2( textSize.X, 0.0f ) );
						ImGui.TextColored( _settings._lapsRemainingColor, textString );
						ImGui.PopFont();
					}

					#endregion

					#region Light

					TextureEnum lightTextureEnum;

					if ( _isUnderCaution )
					{
						lightTextureEnum = TextureEnum.LightYellow;
					}
					else if ( ( _currentLap == 0 ) || ( _data.Data.SessionLapsRemain == 0 ) || ( ( (uint) _sessionFlags & ( (uint) SessionFlags.White ) ) != 0 ) )
					{
						lightTextureEnum = TextureEnum.LightWhite;
					}
					else
					{
						lightTextureEnum = TextureEnum.LightGreen;
					}

					ImGui.SetCursorPos( _settings._lightPosition );
					ImGui.Image( _textureIdList[ (int) lightTextureEnum ], new Vector2( _textureList[ (int) lightTextureEnum ].Width, _textureList[ (int) lightTextureEnum ].Height ) );

					#endregion

					#region Current lap

					ImGui.PushFont( _fontA );
					textString = _currentLap.ToString();
					textSize = ImGui.CalcTextSize( textString );
					ImGui.SetCursorPos( _settings._currentLapPosition - new Vector2( textSize.X, 0.0f ) );
					ImGui.TextColored( _settings._currentLapColor, textString );
					ImGui.PopFont();

					#endregion

					#region Total laps

					if ( _data.Data.SessionLapsTotal != 32767 )
					{
						textString = _data.Data.SessionLapsTotal.ToString();
					}
					else
					{
						textString = "---";
					}

					ImGui.PushFont( _fontA );
					ImGui.SetCursorPos( _settings._totalLapsPosition );
					ImGui.TextColored( _settings._totalLapsColor, textString );
					ImGui.PopFont();

					#endregion

					#region Leaderboard

					if ( _positionsValid )
					{
						for ( var i = 0; i < ( _session.DriverInfo.Drivers.Count ) && ( i < _settings._placeCount ); i++ )
						{
							var trackedCar = _trackedCarList[ i ];

							if ( ( trackedCar._driver == null ) || ( trackedCar._car == null ) )
							{
								break;
							}

							#region Place

							ImGui.PushFont( _fontC );

							textString = ( i + 1 ).ToString();
							textSize = ImGui.CalcTextSize( textString );

							var placePosition = _settings._placePosition + new Vector2( 0.0f, _settings._placeSpacing * i ) - new Vector2( textSize.X, 0.0f );

							ImGui.SetCursorPos( placePosition );
							ImGui.TextColored( _settings._placeColor, textString );
							ImGui.PopFont();

							#endregion

							#region Car number

							var carNumberPosition = _settings._carNumberPosition + new Vector2( 0, _settings._placeSpacing * i );

							DrawCarNumber( drawList, trackedCar, carNumberPosition, _settings._carNumberSize );

							#endregion

							#region Driver name

							if ( trackedCar._driver.AbbrevName != null )
							{
								textString = Regex.Replace( trackedCar._driver.AbbrevName, @"[\d-]", string.Empty );
							}
							else
							{
								textString = "---";
							}

							var driverNamePosition = _settings._driverNamePosition + new Vector2( 0.0f, _settings._placeSpacing * i );

							ImGui.PushFont( _fontD );
							ImGui.SetCursorPos( driverNamePosition );
							ImGui.TextColored( _settings._driverNameColor, textString );
							ImGui.PopFont();

							#endregion

							#region Laps down / Pit / Out

							textString = string.Empty;
							textColor = Vector4.Zero;

							if ( trackedCar._car.CarIdxOnPitRoad )
							{
								textString = _settings._pitString;
								textColor = _settings._pitColor;
							}
							else if ( trackedCar._car.CarIdxLapDistPct == -1 )
							{
								textString = _settings._outString;
								textColor = _settings._outColor;
							}
							else
							{
								textString = $"{trackedCar._car.CarIdxF2Time:0.0}";
								textColor = _settings._lapsDownColor;

								if ( textString == "0.0" )
								{
									textString = string.Empty;
								}
							}

							if ( textString != string.Empty )
							{
								ImGui.PushFont( _fontC );

								textSize = ImGui.CalcTextSize( textString );

								var lapsDownPosition = _settings._lapsDownPosition + new Vector2( 0.0f, _settings._placeSpacing * i ) - new Vector2( textSize.X, 0.0f );

								ImGui.SetCursorPos( lapsDownPosition );
								ImGui.TextColored( textColor, textString );
								ImGui.PopFont();
							}

							#endregion

							#region Current target

							if ( _data.Data.CamCarIdx == trackedCar._car.CarIdx )
							{
								var currentTargetPosition = _settings._currentTargetPosition + new Vector2( 0.0f, _settings._placeSpacing * i );

								ImGui.SetCursorPos( currentTargetPosition );
								ImGui.Image( _textureIdList[ (int) TextureEnum.CurrentTarget ], new Vector2( _textureList[ (int) TextureEnum.CurrentTarget ].Width, _textureList[ (int) TextureEnum.CurrentTarget ].Height ) );

								ImGui.PushFont( _fontC );

								textString = $"{trackedCar._speed * ( ( _data.Data.DisplayUnits == 0 ) ? 2.23694f : 3.6f ):0} {( ( _data.Data.DisplayUnits == 0 ) ? _settings._mphString : _settings._kphString )}";

								textSize = ImGui.CalcTextSize( textString );

								var speedPosition = currentTargetPosition + _settings._currentTargetSpeedOffset - new Vector2( textSize.X, 0 );

								ImGui.SetCursorPos( speedPosition );
								ImGui.TextColored( _settings._currentTargetSpeedColor, textString );
								ImGui.PopFont();
							}

							#endregion
						}
					}

					#endregion

					#region Flags

					if ( ( (uint) _sessionFlags & ( (uint) SessionFlags.Checkered ) ) != 0 )
					{
						ImGui.SetCursorPos( _settings._flagPosition );
						ImGui.Image( _textureIdList[ (int) TextureEnum.FlagCheckered ], new Vector2( _textureList[ (int) TextureEnum.FlagCheckered ].Width, _textureList[ (int) TextureEnum.FlagCheckered ].Height ) );
					}
					else if ( ( (uint) _sessionFlags & ( (uint) SessionFlags.CautionWaving | (uint) SessionFlags.YellowWaving ) ) != 0 )
					{
						ImGui.SetCursorPos( _settings._flagPosition );
						ImGui.Image( _textureIdList[ (int) TextureEnum.FlagCaution ], new Vector2( _textureList[ (int) TextureEnum.FlagCaution ].Width, _textureList[ (int) TextureEnum.FlagCaution ].Height ) );
					}
					else if ( ( (uint) _sessionFlags & ( (uint) SessionFlags.StartGo ) ) != 0 )
					{
						ImGui.SetCursorPos( _settings._flagPosition );
						ImGui.Image( _textureIdList[ (int) TextureEnum.FlagGreen ], new Vector2( _textureList[ (int) TextureEnum.FlagGreen ].Width, _textureList[ (int) TextureEnum.FlagGreen ].Height ) );
					}

					#endregion

					#region Voice of

					if ( _data.Data.RadioTransmitCarIdx == -1 )
					{
						_voiceOfSliderTime -= 0.08f;

						if ( _voiceOfSliderTime < 0.0f )
						{
							_voiceOfSliderTime = 0.0f;
						}
					}
					else
					{
						_voiceOfSliderTime += 0.08f;

						if ( _voiceOfSliderTime > 1.0f )
						{
							_voiceOfSliderTime = 1.0f;
						}

						_radioTransmitCarIdx = _data.Data.RadioTransmitCarIdx;
					}

					if ( _voiceOfSliderTime > 0.0f )
					{
						var t = (float) Math.Sin( _voiceOfSliderTime * Math.PI - Math.PI / 2 ) / 2 + 0.5f;

						var voiceOfPosition = t * ( _settings._voiceOfFinalPosition - _settings._voiceOfStartPosition ) + _settings._voiceOfStartPosition;

						ImGui.SetCursorPos( voiceOfPosition );
						ImGui.Image( _textureIdList[ (int) TextureEnum.VoiceOf ], new Vector2( _textureList[ (int) TextureEnum.VoiceOf ].Width, _textureList[ (int) TextureEnum.VoiceOf ].Height ), Vector2.Zero, Vector2.One, _settings._voiceOfTint );

						ImGui.PushFont( _fontB );
						ImGui.SetCursorPos( voiceOfPosition + _settings._voiceOfStringOffset );
						ImGui.TextColored( _settings._voiceOfStringColor, _settings._voiceOfString );
						ImGui.PopFont();

						foreach ( var trackedCar in _trackedCarList )
						{
							if ( trackedCar._driver?.CarIdx == _radioTransmitCarIdx )
							{
								textString = Regex.Replace( trackedCar._driver.UserName, @"[\d-]", string.Empty );

								ImGui.PushFont( _fontA );
								ImGui.SetCursorPos( voiceOfPosition + _settings._voiceOfNameOffset );
								ImGui.TextColored( _settings._voiceOfNameColor, textString );
								ImGui.PopFont();

								DrawCarNumber( drawList, trackedCar, voiceOfPosition + _settings._voiceOfNumberOffset, _settings._voiceOfNumberSize );

								break;
							}
						}
					}

					#endregion

					#region End

					ImGui.End();
					ImGui.PopStyleVar();

					_renderer.Render( _graphicsDevice, _commandList );

					_commandList.End();

					_graphicsDevice.SubmitCommands( _commandList );

					_graphicsDevice.SwapBuffers();

					#endregion
				}
			}
		}

		private void DrawCarNumber( ImDrawListPtr drawList, TrackedCar trackedCar, Vector2 position, Vector2 size )
		{
			if ( trackedCar._driver == null )
			{
				return;
			}

			uint backColorA = 0xFF000000;
			uint backColorB = 0xFF000000;
			uint backColorC = 0xFF000000;
			uint backColorD = 0xFF000000;

			var match = Regex.Match( trackedCar._driver.CarDesignStr, @"(\d+),(.{6}),(.{6}),(.{6})" );

			if ( match.Success )
			{
				backColorA = ColorVectorToUint( ColorHexToVector( $"#{match.Groups[ 2 ].Value}" ) );
				backColorB = ColorVectorToUint( ColorHexToVector( $"#{match.Groups[ 3 ].Value}" ) );
				backColorC = ColorVectorToUint( ColorHexToVector( $"#{match.Groups[ 4 ].Value}" ) );
			}

			match = Regex.Match( trackedCar._driver.CarNumberDesignStr, @"(\d+),(.{6}),(.{6}),(.{6})" );

			if ( match.Success )
			{
				backColorD = ColorVectorToUint( ColorHexToVector( $"#{match.Groups[ 2 ].Value}" ) );
			}

			var carNumberTL = position;
			var carNumberBR = size + carNumberTL;

			var carNumberBackgroundTL = carNumberTL;
			var carNumberBackgroundBR = carNumberBackgroundTL + new Vector2( size.X, size.Y / 5 );

			drawList.AddRectFilled( carNumberBackgroundTL, carNumberBackgroundBR, backColorA );

			carNumberBackgroundTL += new Vector2( 0, size.Y / 5 );
			carNumberBackgroundBR += new Vector2( 0, size.Y / 5 );

			drawList.AddRectFilled( carNumberBackgroundTL, carNumberBackgroundBR, backColorB );

			carNumberBackgroundTL += new Vector2( 0, size.Y / 5 );
			carNumberBackgroundBR += new Vector2( 0, size.Y / 5 );

			drawList.AddRectFilled( carNumberBackgroundTL, carNumberBackgroundBR, backColorC );

			carNumberBackgroundTL += new Vector2( 0, size.Y / 5 );
			carNumberBackgroundBR += new Vector2( 0, size.Y / 5 );

			drawList.AddRectFilled( carNumberBackgroundTL, carNumberBackgroundBR, backColorB );

			carNumberBackgroundTL += new Vector2( 0, size.Y / 5 );
			carNumberBackgroundBR += new Vector2( 0, size.Y / 5 );

			drawList.AddRectFilled( carNumberBackgroundTL, carNumberBackgroundBR, backColorA );

			var carNumberCenter = ( carNumberTL + carNumberBR ) / 2;
			var carNumberRadius = size.Y * 0.45f;

			drawList.AddCircleFilled( carNumberCenter, carNumberRadius, backColorD );

			ImGui.PushFont( _fontB );

			var textString = trackedCar._driver.CarNumber;
			var textSize = ImGui.CalcTextSize( textString );

			var carNumberStringPosition = carNumberCenter - textSize / 2;

			ImGui.SetCursorPos( carNumberStringPosition + new Vector2( -1, -1 ) );
			ImGui.TextColored( _white, textString );

			ImGui.SetCursorPos( carNumberStringPosition + new Vector2( 0, -1 ) );
			ImGui.TextColored( _white, textString );

			ImGui.SetCursorPos( carNumberStringPosition + new Vector2( 1, -1 ) );
			ImGui.TextColored( _white, textString );

			ImGui.SetCursorPos( carNumberStringPosition + new Vector2( -1, 0 ) );
			ImGui.TextColored( _white, textString );

			ImGui.SetCursorPos( carNumberStringPosition + new Vector2( 1, 0 ) );
			ImGui.TextColored( _white, textString );

			ImGui.SetCursorPos( carNumberStringPosition + new Vector2( -1, 1 ) );
			ImGui.TextColored( _white, textString );

			ImGui.SetCursorPos( carNumberStringPosition + new Vector2( 0, 1 ) );
			ImGui.TextColored( _white, textString );

			ImGui.SetCursorPos( carNumberStringPosition + new Vector2( 1, 1 ) );
			ImGui.TextColored( _white, textString );

			ImGui.SetCursorPos( carNumberStringPosition );
			ImGui.TextColored( _black, textString );

			ImGui.PopFont();
		}

		private void OverlayX_TextChanged( object sender, System.Windows.Controls.TextChangedEventArgs e )
		{
			_settings._overlayX = int.Parse( OverlayX.Text );
		}

		private void OverlayY_TextChanged( object sender, System.Windows.Controls.TextChangedEventArgs e )
		{
			_settings._overlayY = int.Parse( OverlayY.Text );
		}

		private void OverlayWidth_TextChanged( object sender, System.Windows.Controls.TextChangedEventArgs e )
		{
			_settings._overlayWidth = int.Parse( OverlayWidth.Text );
		}

		private void OverlayHeight_TextChanged( object sender, System.Windows.Controls.TextChangedEventArgs e )
		{
			_settings._overlayHeight = int.Parse( OverlayHeight.Text );
		}

		private void PreferredCarNumber_TextChanged( object sender, System.Windows.Controls.TextChangedEventArgs e )
		{
			_settings._preferredCarNumber = PreferredCarNumber.Text;
		}

		private void ShowOverlayButton_Click( object sender, RoutedEventArgs e )
		{
			_overlayIsVisible = !_overlayIsVisible;

			if ( _overlayIsVisible )
			{
				ShowOverlayButton.Content = "Hide Overlay";
			}
			else
			{
				ShowOverlayButton.Content = "Show Overlay";
			}

			if ( _sdl2Window != null )
			{
				_sdl2Window.Visible = _overlayIsVisible;
			}
		}

		private void EnableAIDirectorButton_Click( object sender, RoutedEventArgs e )
		{
			_directorIsEnabled = !_directorIsEnabled;

			if ( _directorIsEnabled )
			{
				EnableAIDirectorButton.Content = "Disable AI Director";
			}
			else
			{
				EnableAIDirectorButton.Content = "Enable AI Director";
			}
		}

		private void ApplyChangesButton_Click( object sender, RoutedEventArgs e )
		{
			MainWindowTabControl.SelectedIndex = 0;

			SaveSettings();

			ReinitializeOverlay();

			UpdateCameraGroupNumbers();
		}

		private void InsideCameraTextBox_TextChanged( object sender, System.Windows.Controls.TextChangedEventArgs e )
		{
			_settings._insideCameraGroupName = InsideCameraTextBox.Text;
		}

		private void CloseCameraTextBox_TextChanged( object sender, System.Windows.Controls.TextChangedEventArgs e )
		{
			_settings._closeCameraGroupName = CloseCameraTextBox.Text;
		}

		private void MediumCameraTextBox_TextChanged( object sender, System.Windows.Controls.TextChangedEventArgs e )
		{
			_settings._mediumCameraGroupName = MediumCameraTextBox.Text;
		}

		private void FarCameraTextBox_TextChanged( object sender, System.Windows.Controls.TextChangedEventArgs e )
		{
			_settings._farCameraGroupName = FarCameraTextBox.Text;
		}

		private void BlimpCameraTextBox_TextChanged( object sender, System.Windows.Controls.TextChangedEventArgs e )
		{
			_settings._blimpCameraGroupName = BlimpCameraTextBox.Text;
		}

		private void ScenicCameraTextBox_TextChanged( object sender, System.Windows.Controls.TextChangedEventArgs e )
		{
			_settings._scenicCameraGroupName = ScenicCameraTextBox.Text;
		}
	}
}
