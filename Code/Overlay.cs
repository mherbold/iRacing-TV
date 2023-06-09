﻿
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
			PositionSplitter,
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

			Windows.MARGINS margins = new() { Left = -1, Right = -1, Top = -1, Bottom = -1 };

			_ = Windows.DwmExtendFrameIntoClientArea( sdl2Window.Handle, ref margins );

			_ = Windows.SetWindowLong( sdl2Window.Handle, Windows.GWL_EXSTYLE, Windows.WS_EX_LAYERED | Windows.WS_EX_TRANSPARENT | Windows.WS_EX_TOPMOST );

			Windows.SetWindowPos( sdl2Window.Handle, Windows.HWND_TOPMOST, 0, 0, 0, 0, Windows.SWP_NOSIZE | Windows.SWP_NOMOVE );

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
			overlayTextures[ (int) TextureEnum.PositionSplitter ] = new OverlayTexture( Settings.data.PositionSplitterFileName );
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

			// race status background

			overlayTextures[ (int) TextureEnum.RaceStatus ]?.Draw( Settings.data.RaceStatusImagePosition, null, Settings.data.RaceStatusImageTint );

			// draw series image

			if ( IRSDK.normalizedSession.seriesOverlayTexture != null )
			{
				var heightRatio = Settings.data.SeriesImageSize.Y / IRSDK.normalizedSession.seriesOverlayTexture.texture.Height;
				var adjustedWidth = IRSDK.normalizedSession.seriesOverlayTexture.texture.Width * heightRatio;
				var xOffset = ( Settings.data.SeriesImageSize.X - adjustedWidth ) * 0.5f;

				IRSDK.normalizedSession.seriesOverlayTexture?.Draw( Settings.data.SeriesImagePosition + new Vector2( xOffset, 0 ), new Vector2( adjustedWidth, Settings.data.SeriesImageSize.Y ), Vector4.One );
			}

			// mode

			if ( !IRSDK.normalizedSession.isInRaceSession )
			{
				DrawText( fontB, Settings.data.ModeTextPosition, Settings.data.ModeTextColor, IRSDK.normalizedSession.sessionName, 0 );
			}

			// remaining laps / time

			if ( IRSDK.normalizedSession.isInTimedRace || !IRSDK.normalizedSession.isInRaceSession )
			{
				textString = GetTimeString( IRSDK.normalizedSession.sessionTimeRemain, false );

				DrawText( fontB, Settings.data.TimeRemainingTextPosition, Settings.data.TimeRemainingTextColor, textString, 2 );
			}
			else if ( IRSDK.normalizedSession.sessionLapsRemaining == 0 )
			{
				DrawText( fontB, Settings.data.LapsRemainingTextPosition, Settings.data.LapsRemainingTextColor, Settings.data.FinalLapString, 2 );
			}
			else
			{
				textString = Math.Min( IRSDK.normalizedSession.sessionLapsTotal, IRSDK.normalizedSession.sessionLapsRemaining + 1 ).ToString() + " " + Settings.data.LapsRemainingString;

				DrawText( fontB, Settings.data.LapsRemainingTextPosition, Settings.data.LapsRemainingTextColor, textString, 2 );
			}

			// race status light

			TextureEnum lightTextureEnum;

			if ( IRSDK.normalizedSession.isUnderCaution )
			{
				lightTextureEnum = TextureEnum.LightYellow;
			}
			else if ( IRSDK.normalizedSession.isInRaceSession && ( IRSDK.normalizedSession.sessionState != SessionState.StateRacing ) )
			{
				lightTextureEnum = TextureEnum.LightBlack;
			}
			else if ( IRSDK.normalizedSession.isInRaceSession && ( ( IRSDK.normalizedSession.sessionLapsRemaining == 0 ) || ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.White ) != 0 ) ) )
			{
				lightTextureEnum = TextureEnum.LightWhite;
			}
			else
			{
				lightTextureEnum = TextureEnum.LightGreen;
			}

			overlayTextures[ (int) lightTextureEnum ]?.Draw( Settings.data.LightImagePosition );

			// lap / time string

			if ( IRSDK.normalizedSession.isInTimedRace || !IRSDK.normalizedSession.isInRaceSession )
			{
				DrawText( fontB, Settings.data.TimeTextPosition, Settings.data.TimeTextColor, Settings.data.TimeString );
			}
			else
			{
				DrawText( fontB, Settings.data.LapTextPosition, Settings.data.LapTextColor, Settings.data.LapString );
			}

			// current and total laps / time

			if ( IRSDK.normalizedSession.isInTimedRace || !IRSDK.normalizedSession.isInRaceSession )
			{
				textString = GetTimeString( IRSDK.normalizedSession.sessionTimeTotal - IRSDK.normalizedSession.sessionTimeRemain, false ) + " | " + GetTimeString( IRSDK.normalizedSession.sessionTimeTotal, false );

				DrawText( fontA, Settings.data.CurrentTimeTextPosition, Settings.data.CurrentTimeTextColor, textString, 2 );
			}
			else
			{
				textString = IRSDK.normalizedSession.sessionLap.ToString() + " | " + IRSDK.normalizedSession.sessionLapsTotal.ToString();

				DrawText( fontA, Settings.data.CurrentLapTextPosition, Settings.data.CurrentLapTextColor, textString, 2 );
			}

			// leaderboard bottom split

			var bottomSplitCount = Settings.data.LeaderboardPlaceCount / 2;
			var bottomSplitLastPosition = Settings.data.LeaderboardPlaceCount;

			if ( IRSDK.normalizedSession.isInRaceSession && ( IRSDK.normalizedSession.sessionState == SessionState.StateRacing ) )
			{
				foreach ( var normalizedCar in IRSDK.normalizedSession.leaderboardSortedNormalizedCars )
				{
					if ( !normalizedCar.includeInLeaderboard )
					{
						break;
					}

					if ( IRSDK.normalizedSession.camCarIdx == normalizedCar.carIdx )
					{
						if ( normalizedCar.leaderboardPosition > bottomSplitLastPosition )
						{
							while ( bottomSplitLastPosition < normalizedCar.leaderboardPosition )
							{
								bottomSplitLastPosition += bottomSplitCount;
							}

							if ( bottomSplitLastPosition > IRSDK.normalizedSession.numCars )
							{
								bottomSplitLastPosition = IRSDK.normalizedSession.numCars;
							}

							break;
						}
					}
				}
			}

			var topSplitFirstPosition = 1;
			var topSplitLastPosition = Settings.data.LeaderboardPlaceCount - bottomSplitCount;
			var bottomSplitFirstPosition = bottomSplitLastPosition - bottomSplitCount + 1;

			// reset cars

			bool leaderboardBackgroundDrawn = false;

			foreach ( var normalizedCar in IRSDK.normalizedSession.leaderboardSortedNormalizedCars )
			{
				normalizedCar.visibleOnLeaderboard = false;
			}

			// leaderboard

			var carInFrontLapPosition = 0.0f;
			var leadCarBestLapTime = 0.0f;

			NormalizedCar? normalizedCarInFront = null;

			foreach ( var normalizedCar in IRSDK.normalizedSession.leaderboardSortedNormalizedCars )
			{
				// stop when we run out of cars to show

				if ( !normalizedCar.includeInLeaderboard )
				{
					break;
				}

				// lead car best lap time

				if ( leadCarBestLapTime == 0 )
				{
					leadCarBestLapTime = normalizedCar.bestLapTime;
				}

				// skip cars not visible on the leaderboard

				if ( ( ( normalizedCar.leaderboardPosition < topSplitFirstPosition ) || ( normalizedCar.leaderboardPosition > topSplitLastPosition ) ) && ( ( normalizedCar.leaderboardPosition < bottomSplitFirstPosition ) || ( normalizedCar.leaderboardPosition > bottomSplitLastPosition ) ) )
				{
					continue;
				}

				// stop when we get to cars that have not qualified yet (only during qualifying)

				if ( IRSDK.normalizedSession.isInQualifyingSession )
				{
					if ( normalizedCar.bestLapTime == 0 )
					{
						break;
					}
				}

				// leaderboard background

				if ( !leaderboardBackgroundDrawn )
				{
					leaderboardBackgroundDrawn = true;

					overlayTextures[ (int) TextureEnum.Leaderboard ]?.Draw( Settings.data.LeaderboardImagePosition, null, Settings.data.LeaderboardImageTint );

					if ( topSplitLastPosition + 1 != bottomSplitFirstPosition )
					{
						overlayTextures[ (int) TextureEnum.PositionSplitter ]?.Draw( Settings.data.PositionSplitterImagePosition, null, Settings.data.PositionSplitterImageTint );
					}
				}

				// visible

				normalizedCar.visibleOnLeaderboard = true;

				// index

				var leaderboardIndex = normalizedCar.leaderboardPosition - ( ( normalizedCar.leaderboardPosition >= bottomSplitFirstPosition ) ? bottomSplitFirstPosition - topSplitLastPosition : topSplitFirstPosition );

				// offset

				var targetOffsetPosition = new Vector2( 0.0f, Settings.data.LeaderboardPlaceSpacing * leaderboardIndex );

				if ( !normalizedCar.offsetPositionIsValid )
				{
					normalizedCar.offsetPositionIsValid = true;
					normalizedCar.offsetPosition = targetOffsetPosition;
					normalizedCar.offsetPositionVelocity = 0;
				}
				else if ( normalizedCar.offsetPosition != targetOffsetPosition )
				{
					var offsetVector = targetOffsetPosition - normalizedCar.offsetPosition;

					var remainingDistance = Vector2.Distance( Vector2.Zero, offsetVector );

					var acceleration = ( remainingDistance - 10.0f ) * 0.025f;

					normalizedCar.offsetPositionVelocity += acceleration;

					normalizedCar.offsetPositionVelocity = Math.Min( Math.Max( Math.Min( normalizedCar.offsetPositionVelocity, 10.0f ), 0.1f ), remainingDistance );

					normalizedCar.offsetPosition += Vector2.Normalize( offsetVector ) * normalizedCar.offsetPositionVelocity;
				}

				// place

				DrawText( fontC, Settings.data.PlaceTextPosition + normalizedCar.offsetPosition, Settings.data.PlaceTextColor, normalizedCar.leaderboardPosition.ToString(), 2 );

				// car number

				if ( normalizedCar.carNumberOverlayTexture != null )
				{
					if ( normalizedCar.carNumberOverlayTexture.texture.Height > 0 )
					{
						var carNumberPosition = Settings.data.CarNumberImagePosition + normalizedCar.offsetPosition;

						var heightRatio = Settings.data.CarNumberImageHeight / normalizedCar.carNumberOverlayTexture.texture.Height;
						var adjustedWidth = normalizedCar.carNumberOverlayTexture.texture.Width * heightRatio;
						var xOffset = adjustedWidth * -0.5f;

						normalizedCar.carNumberOverlayTexture.Draw( new Vector2( carNumberPosition.X + xOffset, carNumberPosition.Y ), new Vector2( adjustedWidth, Settings.data.CarNumberImageHeight ) );
					}
				}

				// driver name

				var driverNamePosition = Settings.data.DriverNameTextPosition + normalizedCar.offsetPosition;

				var driverNameTextColor = Settings.data.UseClassColorsForDriverNames ? Vector4.Lerp( Settings.data.DriverNameTextColor, normalizedCar.classColor, ( Settings.data.ClassColorStrength / 100.0f ) ) : Settings.data.DriverNameTextColor;

				driverNameTextColor.W = Settings.data.DriverNameTextColor.W;

				DrawText( fontD, driverNamePosition, driverNameTextColor, normalizedCar.abbrevName );

				// telemetry

				textString = string.Empty;
				textColor = Vector4.Zero;

				if ( IRSDK.normalizedSession.isInQualifyingSession )
				{
					if ( leadCarBestLapTime == normalizedCar.bestLapTime )
					{
						textString = $"{leadCarBestLapTime:0.000}";
					}
					else
					{
						var deltaTime = normalizedCar.bestLapTime - leadCarBestLapTime;

						textString = $"-{deltaTime:0.000}";
					}

					textColor = Settings.data.TelemetryTextColor;
				}
				else if ( normalizedCar.isOnPitRoad )
				{
					textString = Settings.data.PitString;
					textColor = Settings.data.PitTextColor;
				}
				else if ( normalizedCar.isOutOfCar )
				{
					textString = Settings.data.OutString;
					textColor = Settings.data.OutTextColor;
				}
				else if ( IRSDK.normalizedSession.isInRaceSession )
				{
					if ( ( IRSDK.normalizedSession.sessionState == SessionState.StateRacing ) && normalizedCar.hasCrossedStartLine )
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
									if ( ( normalizedCarInFront != null ) && ( normalizedCarInFront.checkpoints[ normalizedCar.checkpointIdx ] > 0 ) )
									{
										var deltaTime = normalizedCarInFront.checkpoints[ normalizedCar.checkpointIdx ] - normalizedCar.checkpoints[ normalizedCar.checkpointIdx ];

										var timeString = $"{deltaTime:0.00}";

										textString = timeString;
									}
								}
							}
						}

						textColor = Settings.data.TelemetryTextColor;
					}
				}

				carInFrontLapPosition = normalizedCar.lapPosition;

				if ( textString != string.Empty )
				{
					var lapsDownPosition = Settings.data.TelemetryTextPosition + normalizedCar.offsetPosition;

					DrawText( fontC, lapsDownPosition, textColor, textString, 2 );
				}

				// current target and speed

				if ( IRSDK.normalizedSession.isInRaceSession && ( IRSDK.normalizedSession.sessionState == SessionState.StateRacing ) )
				{
					if ( IRSDK.normalizedSession.camCarIdx == normalizedCar.carIdx )
					{
						var currentTargetPosition = Settings.data.CurrentTargetImagePosition + normalizedCar.offsetPosition;

						overlayTextures[ (int) TextureEnum.CurrentTarget ]?.Draw( currentTargetPosition );

						var speedPosition = currentTargetPosition + Settings.data.CurrentTargetSpeedTextOffset;

						textString = $"{normalizedCar.speedInMetersPerSecond * ( IRSDK.normalizedSession.displayIsMetric ? 3.6f : 2.23694f ):0} {( IRSDK.normalizedSession.displayIsMetric ? Settings.data.KphString : Settings.data.MphString )}";

						DrawText( fontC, speedPosition, Settings.data.CurrentTargetSpeedTextColor, textString, 2 );
					}
				}

				//

				if ( normalizedCarInFront == null || Settings.data.BetweenCars )
				{
					normalizedCarInFront = normalizedCar;
				}
			}

			// reset cars

			foreach ( var normalizedCar in IRSDK.normalizedSession.leaderboardSortedNormalizedCars )
			{
				if ( !normalizedCar.visibleOnLeaderboard )
				{
					normalizedCar.offsetPositionIsValid = false;
					normalizedCar.offsetPosition = Vector2.Zero;
					normalizedCar.offsetPositionVelocity = 0;
				}
			}

			// flags

			if ( ( IRSDK.normalizedSession.sessionState == SessionState.StateCheckered ) || ( IRSDK.normalizedSession.sessionState == SessionState.StateCoolDown ) )
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

			// intro

			if ( Settings.data.EnableIntro )
			{
				if ( IRSDK.normalizedSession.isInRaceSession )
				{
					var secondsFromStart = IRSDK.normalizedSession.sessionTime - Settings.data.IntroStartTime;
					var secondsRemaining = Settings.data.IntroDuration - secondsFromStart;

					var introPct = secondsFromStart / Settings.data.IntroDuration;

					if ( ( introPct >= 0.0f ) && ( introPct <= 1.0f ) )
					{
						if ( IRSDK.normalizedSession.largeTrackOverlayTexture != null )
						{
							var heightRatio = Settings.data.TrackImageSize.Y / IRSDK.normalizedSession.largeTrackOverlayTexture.texture.Height;
							var adjustedWidth = IRSDK.normalizedSession.largeTrackOverlayTexture.texture.Width * heightRatio;
							var xOffset = ( Settings.data.TrackImageSize.X - adjustedWidth ) * 0.5f;
							var alpha = (float) Math.Min( Math.Min( 1.0f, secondsFromStart ), secondsRemaining );

							IRSDK.normalizedSession.largeTrackOverlayTexture.Draw( Settings.data.TrackImagePosition + new Vector2( xOffset, 0 ), new Vector2( adjustedWidth, Settings.data.TrackImageSize.Y ), Vector4.One * alpha );

							if ( IRSDK.normalizedSession.trackLogoOverlayTexture != null )
							{
								var position = Settings.data.TrackLogoImagePosition - new Vector2( IRSDK.normalizedSession.trackLogoOverlayTexture.texture.Width, IRSDK.normalizedSession.trackLogoOverlayTexture.texture.Height ) * 0.5f;

								IRSDK.normalizedSession.trackLogoOverlayTexture.Draw( position, null, Vector4.One * alpha );
							}
						}

						var numCarsPerStartingGridGroup = 4;

						var numStartingGridGroups = (int) Math.Floor( (float) IRSDK.normalizedSession.numCars / numCarsPerStartingGridGroup ) + 1;

						var startingGridStartTime = 3.0f;
						var startingGridDuration = Settings.data.IntroDuration - 6.0f;

						var startingGridGroupDuration = startingGridDuration / numStartingGridGroups;

						if ( ( secondsFromStart >= startingGridStartTime ) && ( secondsFromStart <= startingGridStartTime + startingGridDuration ) )
						{
							var startingGridGroupPct = ( secondsFromStart - startingGridStartTime ) / startingGridGroupDuration;

							var currentStartingGridGroup = (int) Math.Floor( startingGridGroupPct );

							startingGridGroupPct -= Math.Floor( startingGridGroupPct );

							var startingGridGroupSecondsFromStart = startingGridGroupPct * startingGridGroupDuration;
							var startingGridGroupSecondsRemaining = startingGridGroupDuration - startingGridGroupSecondsFromStart;

							var firstSlotPosition = currentStartingGridGroup * numCarsPerStartingGridGroup + 1;
							var lastSlotPosition = currentStartingGridGroup * numCarsPerStartingGridGroup + numCarsPerStartingGridGroup;

							var bodyImageWidth = Settings.data.TrackImageSize.X / numCarsPerStartingGridGroup;

							foreach ( var normalizedCar in IRSDK.normalizedSession.leaderboardSortedNormalizedCars )
							{
								if ( ( normalizedCar.qualifyingPosition >= firstSlotPosition ) && ( normalizedCar.qualifyingPosition <= lastSlotPosition ) )
								{
									if ( normalizedCar.bodyOverlayTexture != null )
									{
										var slot = normalizedCar.qualifyingPosition - firstSlotPosition;

										var widthRatio = Settings.data.SeriesImageSize.X / normalizedCar.bodyOverlayTexture.texture.Width;
										var adjustedHeight = normalizedCar.bodyOverlayTexture.texture.Height * widthRatio;

										var alpha = (float) Math.Min( Math.Min( 1.0f, startingGridGroupSecondsFromStart - slot * 0.25f ), startingGridGroupSecondsRemaining - ( numCarsPerStartingGridGroup - slot - 1 ) * 0.25f );

										var imagePosition = Settings.data.TrackImagePosition + new Vector2( bodyImageWidth * slot, Settings.data.TrackImageSize.Y - adjustedHeight );
										var imageSize = new Vector2( bodyImageWidth, adjustedHeight );

										DrawText( fontA, imagePosition + Settings.data.IntroPositionTextOffset, Settings.data.IntroPositionTextColor * alpha, $"P{normalizedCar.qualifyingPosition}" );

										if ( normalizedCar.qualifyingTime > 1 )
										{
											textString = GetTimeString( normalizedCar.qualifyingTime, true );
										}
										else
										{
											textString = "DNQ";
										}

										DrawText( fontB, imagePosition + Settings.data.IntroQualifyingTimeTextOffset, Settings.data.IntroQualifyingTimeTextColor * alpha, $"{textString}" );

										DrawText( fontA, imagePosition + Settings.data.IntroDriverNameTextOffset, Settings.data.IntroDriverNameTextColor * alpha, normalizedCar.abbrevName );

										normalizedCar.bodyOverlayTexture.Draw( imagePosition, imageSize, Vector4.One * alpha );
									}
								}
							}
						}
					}
				}
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

				var tintColor = new Vector4( 1, 1, 1, t );

				overlayTextures[ (int) TextureEnum.VoiceOf ]?.Draw( voiceOfPosition, null, tintColor );

				DrawText( fontB, voiceOfPosition + Settings.data.VoiceOfTextOffset, Settings.data.VoiceOfTextColor * tintColor, Settings.data.VoiceOfString );

				var normalizedCar = IRSDK.normalizedSession.FindNormalizedCarByCarIdx( radioTransmitCarIdx );

				if ( normalizedCar != null )
				{
					textString = DigitsMatchRegex().Replace( normalizedCar.userName, string.Empty );

					DrawText( fontA, voiceOfPosition + Settings.data.VoiceOfDriverNameOffset, Settings.data.VoiceOfDriverNameColor * tintColor, textString );

					if ( normalizedCar.carOverlayTexture != null )
					{
						if ( normalizedCar.carOverlayTexture.texture.Height > 0 )
						{
							var heightRatio = (float) Settings.data.VoiceOfCarImageHeight / normalizedCar.carOverlayTexture.texture.Height;
							var adjustedWidth = normalizedCar.carOverlayTexture.texture.Width * heightRatio;

							normalizedCar.carOverlayTexture.Draw( voiceOfPosition + Settings.data.VoiceOfCarImageOffset, new Vector2( adjustedWidth, Settings.data.VoiceOfCarImageHeight ), tintColor );
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

			try
			{
				imGuiRenderer.Render( graphicsDevice, commandList );
			}
			catch ( Exception )
			{

			}

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

		public static string GetTimeString( double timeInSeconds, bool includeMilliseconds )
		{
			TimeSpan time = TimeSpan.FromSeconds( timeInSeconds );

			if ( time.Hours > 0 )
			{
				return time.ToString( @"h\:mm\:ss" );
			}
			else if ( includeMilliseconds )
			{
				if ( time.Minutes > 0 )
				{
					return time.ToString( @"m\:ss\.fff" );
				}
				else
				{
					return time.ToString( @"ss\.fff" );
				}
			}
			else
			{
				return time.ToString( @"m\:ss" );
			}
		}

		[GeneratedRegex( "[\\d]" )]
		private static partial Regex DigitsMatchRegex();
	}
}
