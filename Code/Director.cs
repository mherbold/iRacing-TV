
using System;

using irsdkSharp.Serialization.Enums.Fastest;

namespace iRacingTV
{
	internal static class Director
	{
		public static bool isEnabled = false;
		public static bool driverWasTalking = false;

		public static void Update()
		{
			if ( !isEnabled )
			{
				return;
			}

			var oldTargetCameraCarNumber = IRSDK.targetCameraCarNumber;
			var oldTargetCameraGroup = IRSDK.targetCameraGroup;
			var oldTargetCameraReason = IRSDK.targetCameraReason;

			IncidentData? currentIncident = IncidentScan.GetCurrentIncident();

			IRSDK.targetCameraReason = string.Empty;

			var cameraGroup = IRSDK.CameraGroupEnum.Far;

			NormalizedCar? preferredCar = null;

			foreach ( var normalizedCar in IRSDK.normalizedSession.normalizedCars )
			{
				if ( normalizedCar.includeInLeaderboard && ( normalizedCar.carNumber == Settings.data.PreferredCarNumber ) )
				{
					preferredCar = normalizedCar;
					break;
				}
			}

			if ( Settings.data.PreferredCarLockOnHeatEnabled && ( preferredCar.heat >= Settings.data.PreferredCarLockOnHeat ) )
			{
				IRSDK.targetCameraCarIdx = preferredCar.carIdx;
				IRSDK.targetCameraCarNumber = preferredCar.carNumber;
				IRSDK.targetCameraReason = $"Preferred car heat is >= {Settings.data.PreferredCarLockOnHeat}";
			}
			else if ( IRSDK.normalizedSession.isCheckeredFlag )
			{
				cameraGroup = IRSDK.CameraGroupEnum.Medium;

				var highestLapPosition = 0.0f;

				foreach ( var normalizedCar in IRSDK.normalizedSession.normalizedCars )
				{
					if ( normalizedCar.includeInLeaderboard )
					{
						if ( normalizedCar.lapPosition < IRSDK.normalizedSession.sessionLapsTotal + ( 20.0f / IRSDK.normalizedSession.trackLengthInMeters ) )
						{
							if ( normalizedCar.lapPosition > highestLapPosition )
							{
								highestLapPosition = normalizedCar.lapPosition;

								IRSDK.targetCameraCarIdx = normalizedCar.carIdx;
								IRSDK.targetCameraCarNumber = normalizedCar.carNumber;
								IRSDK.cameraSwitchWaitTicksRemaining = IRSDK.sendMessageWaitTicksRemaining;
								IRSDK.targetCameraReason = "Checkered flag and this car is closest to the finish line";
							}
						}
					}
				}

				driverWasTalking = false;
			}
			else if ( currentIncident != null )
			{
				cameraGroup = IRSDK.CameraGroupEnum.Medium;

				var normalizedCar = IRSDK.normalizedSession.FindNormalizedCarByCarIdx( currentIncident.carIdx );

				IRSDK.targetCameraCarIdx = currentIncident.carIdx;
				IRSDK.targetCameraCarNumber = normalizedCar?.carNumber ?? "?!?";
				IRSDK.targetCameraReason = $"Incident at frame {currentIncident.frameNumber} involving this car";

				driverWasTalking = false;
			}
			else if ( ( IRSDK.normalizedSession.sessionFlags & ( (uint) SessionFlags.GreenHeld | (uint) SessionFlags.StartReady | (uint) SessionFlags.StartSet | (uint) SessionFlags.StartGo ) ) != 0 )
			{
				var normalizedCar = IRSDK.normalizedSession.leaderboardSortedNormalizedCars[ 0 ];

				IRSDK.targetCameraCarIdx = normalizedCar.carIdx;
				IRSDK.targetCameraCarNumber = normalizedCar.carNumber;
				IRSDK.targetCameraReason = "The race is about to start and this is the lead car";

				driverWasTalking = false;
			}
			else if ( Settings.data.SwitchCameraToTalkingDriver && ( IRSDK.normalizedSession.radioTransmitCarIdx != -1 ) )
			{
				cameraGroup = IRSDK.CameraGroupEnum.Close;

				var normalizedCar = IRSDK.normalizedSession.FindNormalizedCarByCarIdx( IRSDK.targetCameraCarIdx );

				IRSDK.targetCameraCarIdx = IRSDK.normalizedSession.radioTransmitCarIdx;
				IRSDK.targetCameraCarNumber = normalizedCar?.carNumber ?? "?!?";
				IRSDK.cameraSwitchWaitTicksRemaining = IRSDK.sendMessageWaitTicksRemaining;
				IRSDK.targetCameraReason = "Driver of this car is talking";

				driverWasTalking = true;
			}
			else if ( driverWasTalking )
			{
				driverWasTalking = false;

				IRSDK.cameraSwitchWaitTicksRemaining = IRSDK.PostChatCameraSwitchWaitTicks;
			}
			else if ( !IRSDK.normalizedSession.raceHasStarted )
			{
				cameraGroup = IRSDK.CameraGroupEnum.Scenic;

				IRSDK.targetCameraCarIdx = 0;
				IRSDK.targetCameraCarNumber = string.Empty;
				IRSDK.targetCameraReason = "The race has not started yet";
			}
			else
			{
				var highestHeat = 0.0f;

				foreach ( var normalizedCar in IRSDK.normalizedSession.normalizedCars )
				{
					if ( normalizedCar.includeInLeaderboard )
					{
						if ( normalizedCar.heat > highestHeat )
						{
							highestHeat = normalizedCar.heat;

							IRSDK.targetCameraCarIdx = normalizedCar.carIdx;
							IRSDK.targetCameraCarNumber = normalizedCar.carNumber;
							IRSDK.targetCameraReason = "This is the hottest car";
						}
					}
				}

				if ( highestHeat == 0 )
				{
					var normalizedCar = IRSDK.normalizedSession.leaderboardSortedNormalizedCars[ 0 ];

					IRSDK.targetCameraCarIdx = normalizedCar.carIdx;
					IRSDK.targetCameraCarNumber = normalizedCar.carNumber;
					IRSDK.targetCameraReason = "There are no hot cars and this is the lead car";
				}

				cameraGroup = IRSDK.CameraGroupEnum.Medium;

				if ( ( IRSDK.normalizedSession.sessionFlags & ( (uint) SessionFlags.CautionWaving | (uint) SessionFlags.YellowWaving ) ) != 0 )
				{
					cameraGroup = IRSDK.CameraGroupEnum.Far;

					IRSDK.targetCameraReason += " (caution waving)";
				}
				else
				{
					foreach ( var normalizedCar in IRSDK.normalizedSession.normalizedCars )
					{
						if ( normalizedCar.includeInLeaderboard )
						{
							if ( normalizedCar.carNumber == Settings.data.PreferredCarNumber )
							{
								var nearestCarDistance = Math.Min( normalizedCar.distanceToCarInFrontInMeters, normalizedCar.distanceToCarBehindInMeters );

								if ( nearestCarDistance < 50 )
								{
									IRSDK.targetCameraCarIdx = normalizedCar.carIdx;
									IRSDK.targetCameraCarNumber = normalizedCar.carNumber;
									IRSDK.targetCameraReason = "This is the preferred car";

									if ( IRSDK.normalizedSession.isUnderCaution )
									{
										cameraGroup = IRSDK.CameraGroupEnum.Far;

										IRSDK.targetCameraReason += " (under caution)";
									}
									else if ( normalizedCar.distanceToCarInFrontInMeters < 10 )
									{
										cameraGroup = IRSDK.CameraGroupEnum.Inside;

										IRSDK.targetCameraReason += " (car in front < 10m)";
									}
									else if ( nearestCarDistance < 10 )
									{
										cameraGroup = IRSDK.CameraGroupEnum.Close;

										IRSDK.targetCameraReason += " (car within 10m)";
									}
									else if ( nearestCarDistance < 20 )
									{
										cameraGroup = IRSDK.CameraGroupEnum.Medium;

										IRSDK.targetCameraReason += " (car within 20m)";
									}
									else if ( nearestCarDistance < 30 )
									{
										cameraGroup = IRSDK.CameraGroupEnum.Far;

										IRSDK.targetCameraReason += " (car within 30m)";
									}
									else
									{
										cameraGroup = IRSDK.CameraGroupEnum.Blimp;

										IRSDK.targetCameraReason += " (car within 50m)";
									}
								}

								break;
							}
						}
					}
				}
			}

			IRSDK.targetCameraGroupNumber = IRSDK.cameraGroupNumbers[ (int) cameraGroup ];
			IRSDK.targetCameraGroup = cameraGroup;

			if ( ( IRSDK.targetCameraCarNumber != oldTargetCameraCarNumber ) || ( IRSDK.targetCameraGroup != oldTargetCameraGroup ) || ( IRSDK.targetCameraReason != oldTargetCameraReason ) )
			{
				MainWindow.instance?.UpdateTarget();
			}
		}
	}
}
