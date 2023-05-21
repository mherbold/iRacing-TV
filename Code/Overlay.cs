
using System;
using System.Numerics;
using System.Text.RegularExpressions;

using irsdkSharp.Serialization.Enums.Fastest;

using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using Veldrid;

using ImGuiNET;

namespace iRacingTV
{
	internal static partial class Overlay
	{
		public const string OverlayWindowName = $"{Program.AppName} Overlay";

		public static bool isVisible = false;

		public static Sdl2Window? sdl2Window = null;
		public static GraphicsDevice? graphicsDevice = null;
		public static CommandList? commandList = null;
		public static ImGuiRenderer? imGuiRenderer = null;

		public static ImFontPtr fontA = 0;
		public static ImFontPtr fontB = 0;
		public static ImFontPtr fontC = 0;
		public static ImFontPtr fontD = 0;

		public static ImFontPtr subtitleFont = 0;

		public static float voiceOfSliderTime = 0.0f;
		public static int radioTransmitCarIdx = -1;

		public enum TextureEnum
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

		public static readonly OverlayTexture?[] overlayTextures = new OverlayTexture[ (int) TextureEnum.NumTextures ];

		public static void Initialize()
		{
			Dispose();

			InitializeWindow();
			InitializeGraphicsDevice();
			InitializeImGuiRenderer();
			InitializeFonts();
			InitializeTextures();

			if ( sdl2Window == null ) { throw new Exception( "Overlay window was not initialized." ); }

			if ( !isVisible )
			{
				sdl2Window.Visible = false;
			}
		}

		public static void Dispose()
		{
			for ( var i = 0; i < overlayTextures.Length; i++ )
			{
				overlayTextures[ i ] = null;
			}

			if ( imGuiRenderer != null )
			{
				imGuiRenderer.Dispose();

				imGuiRenderer = null;
			}

			if ( commandList != null )
			{
				commandList.Dispose();

				commandList = null;
			}

			if ( graphicsDevice != null )
			{
				graphicsDevice.Dispose();

				graphicsDevice = null;
			}

			if ( sdl2Window != null )
			{
				sdl2Window.Close();

				sdl2Window.PumpEvents();

				sdl2Window = null;
			}
		}

		public static void InitializeWindow()
		{
			LogFile.Write( "Initializing window..." );

			var flags = SDL_WindowFlags.OpenGL | SDL_WindowFlags.Borderless | SDL_WindowFlags.AlwaysOnTop | SDL_WindowFlags.Shown;

			sdl2Window = new Sdl2Window( OverlayWindowName, 0, 0, Settings.data.OverlayWidth, Settings.data.OverlayHeight, flags, true );

			var windowLong = Windows.GetWindowLong( sdl2Window.Handle, Windows.GWL_EXSTYLE );

			_ = Windows.SetWindowLong( sdl2Window.Handle, Windows.GWL_EXSTYLE, (IntPtr) windowLong | Windows.WS_EX_LAYERED | Windows.WS_EX_TRANSPARENT | Windows.WS_EX_TOPMOST );

			Windows.MARGINS marg = new() { Left = -1, Right = -1, Top = -1, Bottom = -1 };

			_ = Windows.DwmExtendFrameIntoClientArea( sdl2Window.Handle, ref marg );

			LogFile.Write( " OK!\r\n" );
		}

		public static void InitializeGraphicsDevice()
		{
			LogFile.Write( "Initializing graphics device..." );

			var graphicsDeviceOptions = new GraphicsDeviceOptions
			{
				Debug = false,
				HasMainSwapchain = true,
				SwapchainSrgbFormat = true
			};

			graphicsDevice = VeldridStartup.CreateGraphicsDevice( sdl2Window, graphicsDeviceOptions, GraphicsBackend.Direct3D11 );

			commandList = graphicsDevice.ResourceFactory.CreateCommandList();

			LogFile.Write( " OK!\r\n" );
		}

		public static void InitializeImGuiRenderer()
		{
			LogFile.Write( "Initializing ImGui renderer..." );

			if ( graphicsDevice == null ) { throw new Exception( "Graphics device was not initialized." ); }

			imGuiRenderer = new ImGuiRenderer( graphicsDevice, graphicsDevice.MainSwapchain.Framebuffer.OutputDescription, (int) graphicsDevice.MainSwapchain.Framebuffer.Width, (int) graphicsDevice.MainSwapchain.Framebuffer.Height, ColorSpaceHandling.Linear );

			LogFile.Write( " OK!\r\n" );
		}

		public static void InitializeFonts()
		{
			LogFile.Write( "Adding fonts..." );

			if ( imGuiRenderer == null ) { throw new Exception( "ImGui renderer was not initialized." ); }

			fontA = ImGui.GetIO().Fonts.AddFontFromFileTTF( Settings.data.FontAFileName, Settings.data.FontASize );
			fontB = ImGui.GetIO().Fonts.AddFontFromFileTTF( Settings.data.FontBFileName, Settings.data.FontBSize );
			fontC = ImGui.GetIO().Fonts.AddFontFromFileTTF( Settings.data.FontCFileName, Settings.data.FontCSize );
			fontD = ImGui.GetIO().Fonts.AddFontFromFileTTF( Settings.data.FontDFileName, Settings.data.FontDSize );

			subtitleFont = ImGui.GetIO().Fonts.AddFontFromFileTTF( Settings.data.SubtitlesFontFileName, Settings.data.SubtitlesFontSize );

			imGuiRenderer.RecreateFontDeviceTexture();

			LogFile.Write( " OK!\r\n" );
		}

		public static void InitializeTextures()
		{
			LogFile.Write( "Loading textures..." );

			overlayTextures[ (int) TextureEnum.RaceStatus ] = new OverlayTexture( Settings.data.RaceStatusImageFileName );
			overlayTextures[ (int) TextureEnum.Leaderboard ] = new OverlayTexture( Settings.data.LeaderboardImageFileName );
			overlayTextures[ (int) TextureEnum.Sponsor ] = new OverlayTexture( Settings.data.SponsorImageFileName );
			overlayTextures[ (int) TextureEnum.LightBlack ] = new OverlayTexture( Settings.data.LightImageBlackFileName );
			overlayTextures[ (int) TextureEnum.LightGreen ] = new OverlayTexture( Settings.data.LightImageGreenFileName );
			overlayTextures[ (int) TextureEnum.LightWhite ] = new OverlayTexture( Settings.data.LightImageWhiteFileName );
			overlayTextures[ (int) TextureEnum.LightYellow ] = new OverlayTexture( Settings.data.LightImageYellowFileName );
			overlayTextures[ (int) TextureEnum.FlagCaution ] = new OverlayTexture( Settings.data.FlagImageCautionFileName );
			overlayTextures[ (int) TextureEnum.FlagCheckered ] = new OverlayTexture( Settings.data.FlagImageCheckeredFileName );
			overlayTextures[ (int) TextureEnum.FlagGreen ] = new OverlayTexture( Settings.data.FlagImageGreenFileName );
			overlayTextures[ (int) TextureEnum.CurrentTarget ] = new OverlayTexture( Settings.data.CurrentTargetImageFileName );
			overlayTextures[ (int) TextureEnum.VoiceOf ] = new OverlayTexture( Settings.data.VoiceOfImageFileName );

			LogFile.Write( " OK!\r\n" );
		}

		public static bool ToggleVisibility()
		{
			isVisible = !isVisible;

			if ( sdl2Window != null )
			{
				sdl2Window.Visible = isVisible;
			}

			return isVisible;
		}

		public static void Update()
		{
			if ( sdl2Window == null ) { throw new Exception( "Overlay window was not initialized." ); }
			if ( graphicsDevice == null ) { throw new Exception( "Graphics device was not initialized." ); }
			if ( commandList == null ) { throw new Exception( "Command list was not initialized." ); }
			if ( imGuiRenderer == null ) { throw new Exception( "ImGui renderer was not initialized." ); }

			var inputSnapshot = sdl2Window.PumpEvents();

			if ( !isVisible )
			{
				return;
			}

			commandList.Begin();
			commandList.SetFramebuffer( graphicsDevice.MainSwapchain.Framebuffer );
			commandList.SetFullViewports();
			commandList.ClearColorTarget( 0, new RgbaFloat( 0.0f, 0.0f, 0.0f, 0.0f ) );

			imGuiRenderer.Update( 0.1f, inputSnapshot );

			string textString;
			Vector4 textColor;

			ImGui.PushStyleVar( ImGuiStyleVar.WindowPadding, Vector2.Zero );
			ImGui.Begin( OverlayWindowName, ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoBackground );
			ImGui.SetWindowPos( Vector2.Zero );
			ImGui.SetWindowSize( new Vector2( graphicsDevice.MainSwapchain.Framebuffer.Width, graphicsDevice.MainSwapchain.Framebuffer.Height ) );

			// draw main backgrounds

			overlayTextures[ (int) TextureEnum.RaceStatus ]?.Draw( Settings.data.RaceStatusImagePosition, null, Settings.data.RaceStatusImageTint );
			overlayTextures[ (int) TextureEnum.Leaderboard ]?.Draw( Settings.data.LeaderboardImagePosition, null, Settings.data.LeaderboardImageTint );
			overlayTextures[ (int) TextureEnum.Sponsor ]?.Draw( Settings.data.SponsorImagePosition, null, Settings.data.SponsorImageTint );

			// remaining laps

			if ( IRSDK.normalizedSession.sessionLapsRemain == 0 )
			{
				DrawText( fontB, Settings.data.LapsRemainingTextPosition, Settings.data.LapsRemainingTextColor, Settings.data.FinalLapString, 2 );
			}
			else if ( IRSDK.normalizedSession.sessionLapsRemain > 0 && IRSDK.normalizedSession.sessionLapsRemain != 32767 )
			{
				textString = Math.Min( IRSDK.normalizedSession.sessionLapsTotal, IRSDK.normalizedSession.sessionLapsRemain + 1 ).ToString() + " " + Settings.data.LapsRemainingString;

				DrawText( fontB, Settings.data.LapsRemainingTextPosition, Settings.data.LapsRemainingTextColor, textString, 2 );
			}

			// race status light

			TextureEnum lightTextureEnum;

			if ( IRSDK.normalizedSession.isUnderCaution )
			{
				lightTextureEnum = TextureEnum.LightYellow;
			}
			else if ( IRSDK.normalizedSession.sessionLap == 0 )
			{
				lightTextureEnum = TextureEnum.LightBlack;
			}
			else if ( IRSDK.normalizedSession.sessionLapsRemain == 0 || ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.White ) != 0 )
			{
				lightTextureEnum = TextureEnum.LightWhite;
			}
			else
			{
				lightTextureEnum = TextureEnum.LightGreen;
			}

			overlayTextures[ (int) lightTextureEnum ]?.Draw( Settings.data.LightImagePosition );

			// lap string

			DrawText( fontB, Settings.data.LapTextPosition, Settings.data.LapTextColor, Settings.data.LapString );

			// current lap

			DrawText( fontA, Settings.data.CurrentLapTextPosition, Settings.data.CurrentLapTextColor, IRSDK.normalizedSession.sessionLap.ToString(), 2 );

			// total laps

			if ( IRSDK.normalizedSession.sessionLapsTotal != 32767 )
			{
				textString = IRSDK.normalizedSession.sessionLapsTotal.ToString();
			}
			else
			{
				textString = "---";
			}

			DrawText( fontA, Settings.data.TotalLapsTextPosition, Settings.data.TotalLapsTextColor, textString );

			// leaderboard

			var carInFrontLapPosition = 0.0f;

			NormalizedCar? normalizedCarInFront = null;

			foreach ( var normalizedCar in IRSDK.normalizedSession.normalizedCars )
			{
				if ( !normalizedCar.includeInLeaderboard )
				{
					break;
				}

				var leaderboardIndex = normalizedCar.leaderboardPosition - 1;

				if ( leaderboardIndex >= Settings.data.LeaderboardPlaceCount )
				{
					break;
				}

				// place

				var placePosition = Settings.data.PlaceTextPosition + new Vector2( 0.0f, Settings.data.LeaderboardPlaceSpacing * leaderboardIndex );

				DrawText( fontC, placePosition, Settings.data.PlaceTextColor, normalizedCar.leaderboardPosition.ToString(), 2 );

				// car number

				var carNumberPosition = Settings.data.CarNumberImagePosition + new Vector2( 0, Settings.data.LeaderboardPlaceSpacing * leaderboardIndex );

				if ( normalizedCar.carNumberOverlayTexture != null )
				{
					if ( normalizedCar.carNumberOverlayTexture.texture.Height > 0 )
					{
						var heightRatio = Settings.data.CarNumberImageHeight / normalizedCar.carNumberOverlayTexture.texture.Height;
						var adjustedWidth = normalizedCar.carNumberOverlayTexture.texture.Width * heightRatio;
						var xOffset = adjustedWidth * -0.5f;

						normalizedCar.carNumberOverlayTexture.Draw( new Vector2( carNumberPosition.X + xOffset, carNumberPosition.Y ), new Vector2( adjustedWidth, Settings.data.CarNumberImageHeight ) );
					}
				}

				// driver name

				if ( normalizedCar.abbrevName != null )
				{
					textString = Regex.Replace( normalizedCar.abbrevName, @"[\d-]", string.Empty );
				}
				else
				{
					textString = "---";
				}

				var driverNamePosition = Settings.data.DriverNameTextPosition + new Vector2( 0.0f, Settings.data.LeaderboardPlaceSpacing * leaderboardIndex );

				DrawText( fontD, driverNamePosition, Settings.data.DriverNameTextColor, textString );

				// telemetry

				textString = string.Empty;
				textColor = Vector4.Zero;

				if ( normalizedCar.isOnPitRoad )
				{
					textString = Settings.data.PitString;
					textColor = Settings.data.PitTextColor;
				}
				else if ( normalizedCar.isOutOfCar )
				{
					textString = Settings.data.OutString;
					textColor = Settings.data.OutTextColor;
				}
				else
				{
					if ( normalizedCar.hasCrossedStartLine )
					{
						if ( !Settings.data.BetweenCars && normalizedCar.lapPositionRelativeToLeader >= 1.0f )
						{
							var wholeLapsDown = Math.Floor( normalizedCar.lapPositionRelativeToLeader );

							textString = $"-{wholeLapsDown:0} {Settings.data.DistanceLapPositionString}";
						}
						else if ( !IRSDK.normalizedSession.isUnderCaution )
						{
							var lapPosition = Settings.data.BetweenCars ? carInFrontLapPosition - normalizedCar.lapPosition : normalizedCar.lapPositionRelativeToLeader;

							if ( lapPosition > 0 )
							{
								if ( Settings.data.ShowLaps )
								{
									textString = $"-{lapPosition:0.000} {Settings.data.DistanceLapPositionString}";
								}
								else if ( Settings.data.ShowDistance )
								{
									var distance = lapPosition * IRSDK.normalizedSession.trackLengthInMeters;

									if ( IRSDK.normalizedSession.displayIsMetric )
									{
										var distanceString = $"{distance:0}";

										if ( distanceString != "0" )
										{
											textString = $"-{distanceString} {Settings.data.DistanceMetersString}";
										}
									}
									else
									{
										distance *= 3.28084f;

										var distanceString = $"{distance:0}";

										if ( distanceString != "0" )
										{
											textString = $"-{distanceString} {Settings.data.DistanceFeetString}";
										}
									}
								}
								else
								{
									if ( normalizedCarInFront != null )
									{
										var deltaTime = normalizedCarInFront.checkpoints[ normalizedCar.checkpointIdx ] - normalizedCar.checkpoints[ normalizedCar.checkpointIdx ];

										var timeString = $"{deltaTime:0.00}";

										textString = timeString;
									}
								}
							}
						}
					}

					textColor = Settings.data.TelemetryTextColor;
				}

				carInFrontLapPosition = normalizedCar.lapPosition;

				if ( textString != string.Empty )
				{
					var lapsDownPosition = Settings.data.TelemetryTextPosition + new Vector2( 0.0f, Settings.data.LeaderboardPlaceSpacing * leaderboardIndex );

					DrawText( fontC, lapsDownPosition, textColor, textString, 2 );
				}

				// speed

				if ( IRSDK.normalizedSession.camCarIdx == normalizedCar.carIdx )
				{
					var currentTargetPosition = Settings.data.CurrentTargetImagePosition + new Vector2( 0.0f, Settings.data.LeaderboardPlaceSpacing * leaderboardIndex );

					overlayTextures[ (int) TextureEnum.CurrentTarget ]?.Draw( currentTargetPosition );

					var speedPosition = currentTargetPosition + Settings.data.CurrentTargetSpeedTextOffset;

					textString = $"{normalizedCar.speedInMetersPerSecond * ( IRSDK.normalizedSession.displayIsMetric ? 3.6f : 2.23694f ):0} {( IRSDK.normalizedSession.displayIsMetric ? Settings.data.KphString : Settings.data.MphString )}";

					DrawText( fontC, speedPosition, Settings.data.CurrentTargetSpeedTextColor, textString, 2 );
				}

				//

				if ( normalizedCarInFront == null || Settings.data.BetweenCars )
				{
					normalizedCarInFront = normalizedCar;
				}
			}

			// flags

			if ( IRSDK.normalizedSession.isCheckeredFlag )
			{
				overlayTextures[ (int) TextureEnum.FlagCheckered ]?.Draw( Settings.data.FlagImagePosition );
			}
			else if ( ( IRSDK.normalizedSession.sessionFlags & ( (uint) SessionFlags.CautionWaving | (uint) SessionFlags.YellowWaving ) ) != 0 )
			{
				overlayTextures[ (int) TextureEnum.FlagCaution ]?.Draw( Settings.data.FlagImagePosition );
			}
			else if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.StartGo ) != 0 )
			{
				overlayTextures[ (int) TextureEnum.FlagGreen ]?.Draw( Settings.data.FlagImagePosition );
			}

			// voice of

			if ( IRSDK.normalizedSession.radioTransmitCarIdx == -1 )
			{
				voiceOfSliderTime -= 0.08f;

				if ( voiceOfSliderTime < 0.0f )
				{
					voiceOfSliderTime = 0.0f;
				}
			}
			else
			{
				radioTransmitCarIdx = IRSDK.normalizedSession.radioTransmitCarIdx;

				voiceOfSliderTime += 0.08f;

				if ( voiceOfSliderTime > 1.0f )
				{
					voiceOfSliderTime = 1.0f;
				}
			}

			if ( voiceOfSliderTime > 0.0f )
			{
				var t = (float) Math.Sin( voiceOfSliderTime * Math.PI - Math.PI / 2 ) / 2 + 0.5f;

				var voiceOfPosition = t * ( Settings.data.VoiceOfImageFinalPosition - Settings.data.VoiceOfImageStartPosition ) + Settings.data.VoiceOfImageStartPosition;

				overlayTextures[ (int) TextureEnum.VoiceOf ]?.Draw( voiceOfPosition );

				DrawText( fontB, voiceOfPosition + Settings.data.VoiceOfTextOffset, Settings.data.VoiceOfTextColor, Settings.data.VoiceOfString );

				var normalizedCar = IRSDK.normalizedSession.FindNormalizedCarByCarIdx( radioTransmitCarIdx );

				if ( normalizedCar != null )
				{
					textString = DigitsMatchRegex().Replace( normalizedCar.userName, string.Empty );

					DrawText( fontA, voiceOfPosition + Settings.data.VoiceOfDriverNameOffset, Settings.data.VoiceOfDriverNameColor, textString );

					if ( normalizedCar.carOverlayTexture != null )
					{
						if ( normalizedCar.carOverlayTexture.texture.Height > 0 )
						{
							var heightRatio = (float) Settings.data.VoiceOfCarImageHeight / normalizedCar.carOverlayTexture.texture.Height;
							var adjustedWidth = normalizedCar.carOverlayTexture.texture.Width * heightRatio;

							normalizedCar.carOverlayTexture.Draw( voiceOfPosition + Settings.data.VoiceOfCarImageOffset, new Vector2( adjustedWidth, Settings.data.VoiceOfCarImageHeight ) );
						}
					}
				}
			}

			// subtitles

			if ( IRSDK.normalizedSession.chatLogData != null )
			{
				var drawList = ImGui.GetWindowDrawList();

				ImGui.PushFont( subtitleFont );
				ImGui.SetCursorPos( Vector2.Zero );
				ImGui.PushTextWrapPos( Settings.data.SubtitleMaxWidth );

				var textSize = ImGui.CalcTextSize( IRSDK.normalizedSession.chatLogData.text, (float) Settings.data.SubtitleMaxWidth );
				var textPosition = Settings.data.SubtitleCenterPosition - textSize / 2;

				drawList.AddRectFilled( textPosition - Settings.data.SubtitleBackgroundPadding, textPosition + textSize + Settings.data.SubtitleBackgroundPadding, Settings.data.SubtitleBackgroundColor, 10.0f );

				ImGui.SetCursorPos( textPosition );
				ImGui.PushTextWrapPos( textPosition.X + Settings.data.SubtitleMaxWidth );
				ImGui.TextWrapped( IRSDK.normalizedSession.chatLogData.text );
				ImGui.PopTextWrapPos();
				ImGui.PopFont();
			}

			// end

			ImGui.End();
			ImGui.PopStyleVar();

			imGuiRenderer.Render( graphicsDevice, commandList );

			commandList.End();

			graphicsDevice.SubmitCommands( commandList );

			graphicsDevice.SwapBuffers();
		}

		public static void DrawText( ImFontPtr font, Vector2 position, Vector4 color, string text, int mode = 0 )
		{
			ImGui.PushFont( font );

			var offset = Vector2.Zero;

			if ( mode == 1 || mode == 2 )
			{
				var size = ImGui.CalcTextSize( text );

				if ( mode == 1 )
				{
					offset = new Vector2( -size.X / 2, 0 );
				}
				else
				{
					offset = new Vector2( -size.X, 0 );
				}
			}

			ImGui.SetCursorPos( position + offset );
			ImGui.TextColored( color, text );
			ImGui.PopFont();
		}

		[GeneratedRegex( "[\\d]" )]
		private static partial Regex DigitsMatchRegex();
	}
}
