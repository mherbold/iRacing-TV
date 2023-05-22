using irsdkSharp.Enums;
using System.Collections.Generic;
using System.Threading;

namespace iRacingTV
{
	internal static class IncidentScan
	{
		public const int IncidentFrameCount = 240;
		public const int IncidentPrerollFrameCount = 90;

		public static readonly List<IncidentData> incidentList = new();

		public static int currentSession = 0;
		public static int startOfRaceFrameNumber = 0;

		public static int settleStartingFrameNumber = 0;
		public static int settleTargetFrameNumber = 0;
		public static int settleLastFrameNumber = 0;
		public static int settleLoopCount = 0;

		public enum IncidentScanStateEnum
		{
			Startup,
			FindStartOfRace,
			LookAtPaceCarWithScenicCamera,
			WaitForLookAtPaceCarToComplete,
			SearchForNextIncident,
			AddIncidentToList,
			NoMoreIncidents,
			Complete,
			Idle,
			WaitForFrameNumberToSettle
		}

		public static IncidentScanStateEnum currentIncidentScanState = IncidentScanStateEnum.Idle;
		public static IncidentScanStateEnum nextIncidentScanState = IncidentScanStateEnum.Idle;

		public static IncidentData? GetCurrentIncident()
		{
			IncidentData? currentIncident = null;

			foreach ( var incident in incidentList )
			{
				if ( ( ( incident.frameNumber + IncidentFrameCount ) >= IRSDK.normalizedSession.replayFrameNum ) && ( ( incident.frameNumber - IncidentPrerollFrameCount ) <= IRSDK.normalizedSession.replayFrameNum ) )
				{
					if ( ( currentIncident == null ) || ( currentIncident.frameNumber < incident.frameNumber ) )
					{
						currentIncident = incident;
						break;
					}
				}
			}

			return currentIncident;
		}

		public static void Start()
		{
			currentIncidentScanState = IncidentScanStateEnum.Startup;

			Director.isEnabled = false;
			
			if ( Overlay.isVisible )
			{
				Overlay.ToggleVisibility();
			}

			MainWindow.instance?.Update();
		}

		public static bool IsRunning()
		{
			return currentIncidentScanState != IncidentScanStateEnum.Idle;
		}

		public static void Update()
		{
			switch ( currentIncidentScanState )
			{
				case IncidentScanStateEnum.Startup:

					incidentList.Clear();

					LogFile.Write( "Rewinding to the start of the replay...\r\n" );

					IRSDK.AddMessage( BroadcastMessageTypes.ReplaySetPlaySpeed, 0, 0, 0 );
					IRSDK.AddMessage( BroadcastMessageTypes.ReplaySetPlayPosition, 0, 1, 0 );

					currentSession = 1;

					LogFile.Write( "Finding the start of the race event...\r\n" );

					WaitForFrameNumberToSettleState( IncidentScanStateEnum.FindStartOfRace, 1 );

					break;

				case IncidentScanStateEnum.FindStartOfRace:

					if ( currentSession < IRSDK.normalizedSession.sessionCount )
					{
						LogFile.Write( "Jumping to the next event...\r\n" );

						currentSession++;

						IRSDK.AddMessage( BroadcastMessageTypes.ReplaySearch, (int) ReplaySearchModeTypes.NextSession, 0, 0 );

						WaitForFrameNumberToSettleState( IncidentScanStateEnum.FindStartOfRace, 0 );
					}
					else
					{
						LogFile.Write( $"Start of race found at frame {IRSDK.normalizedSession.replayFrameNum}.\r\n" );

						startOfRaceFrameNumber = IRSDK.normalizedSession.replayFrameNum;

						currentIncidentScanState = IncidentScanStateEnum.LookAtPaceCarWithScenicCamera;
					}

					break;

				case IncidentScanStateEnum.LookAtPaceCarWithScenicCamera:

					IRSDK.AddMessage( BroadcastMessageTypes.CamSwitchNum, 0, IRSDK.cameraGroupNumbers[ (int) IRSDK.CameraGroupEnum.Scenic ], 0 );

					currentIncidentScanState = IncidentScanStateEnum.WaitForLookAtPaceCarToComplete;

					break;

				case IncidentScanStateEnum.WaitForLookAtPaceCarToComplete:

					if ( ( IRSDK.currentCameraCarIdx == 0 ) && ( IRSDK.currentCameraGroupNumber == IRSDK.cameraGroupNumbers[ (int) IRSDK.CameraGroupEnum.Scenic ] ) )
					{
						currentIncidentScanState = IncidentScanStateEnum.SearchForNextIncident;
					}

					break;

				case IncidentScanStateEnum.SearchForNextIncident:

					IRSDK.AddMessage( BroadcastMessageTypes.ReplaySearch, (int) ReplaySearchModeTypes.NextIncident, 0, 0 );

					WaitForFrameNumberToSettleState( IncidentScanStateEnum.AddIncidentToList, 0 );

					break;

				case IncidentScanStateEnum.AddIncidentToList:

					var normalizedCar = IRSDK.normalizedSession.FindNormalizedCarByCarIdx( IRSDK.normalizedSession.camCarIdx );

					LogFile.Write( $"New incident found at frame {IRSDK.normalizedSession.replayFrameNum} involving car #{normalizedCar?.carNumber ?? "<Unknown car>"}.\r\n" );

					incidentList.Add( new IncidentData( IRSDK.normalizedSession.camCarIdx, IRSDK.normalizedSession.replayFrameNum ) );

					currentIncidentScanState = IncidentScanStateEnum.SearchForNextIncident;

					break;

				case IncidentScanStateEnum.NoMoreIncidents:

					LogFile.Write( "Done with finding incidents, rewinding to the start of the replay.\r\n" );

					IRSDK.AddMessage( BroadcastMessageTypes.ReplaySetPlayPosition, 0, startOfRaceFrameNumber, 0 );

					incidentList.Reverse();

					WaitForFrameNumberToSettleState( IncidentScanStateEnum.Complete, startOfRaceFrameNumber );

					break;

				case IncidentScanStateEnum.Complete:

					currentIncidentScanState = IncidentScanStateEnum.Idle;

					MainWindow.instance?.Update();

					IRSDK.forceResetRace = true;

					break;

				case IncidentScanStateEnum.WaitForFrameNumberToSettle:

					// LogFile.Write( $"SFN={settleStartingFrameNumber}, TFN={settleTargetFrameNumber}, LFN={settleLastFrameNumber}, LOOP={settleLoopCount}\r\n" );

					if ( settleTargetFrameNumber != 0 )
					{
						if ( IRSDK.normalizedSession.replayFrameNum == settleTargetFrameNumber )
						{
							currentIncidentScanState = nextIncidentScanState;
						}
					}
					else
					{
						if ( ( IRSDK.normalizedSession.replayFrameNum != 0 ) && ( IRSDK.normalizedSession.replayFrameNum != settleStartingFrameNumber ) )
						{
							if ( ( settleLastFrameNumber == 0 ) || ( settleLastFrameNumber != IRSDK.normalizedSession.replayFrameNum ) )
							{
								settleLastFrameNumber = IRSDK.normalizedSession.replayFrameNum;
								settleLoopCount = 0;
							}
							else
							{
								settleLoopCount++;

								if ( settleLoopCount == IRSDK.MinimumSendMessageWaitTicks )
								{
									currentIncidentScanState = nextIncidentScanState;
								}
							}
						}
						else
						{
							settleLoopCount++;

							if ( settleLoopCount == IRSDK.MinimumSendMessageWaitTicks * 10 )
							{
								currentIncidentScanState = IncidentScanStateEnum.NoMoreIncidents;
							}
						}
					}

					break;
			}
		}

		public static void WaitForFrameNumberToSettleState( IncidentScanStateEnum nextIncidentScanState, int targetFrameNumber )
		{
			settleStartingFrameNumber = IRSDK.normalizedSession.replayFrameNum;
			settleTargetFrameNumber = targetFrameNumber;
			settleLastFrameNumber = 0;
			settleLoopCount = 0;

			currentIncidentScanState = IncidentScanStateEnum.WaitForFrameNumberToSettle;

			IncidentScan.nextIncidentScanState = nextIncidentScanState;
		}
	}
}
