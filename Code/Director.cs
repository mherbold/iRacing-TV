
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

			NormalizedCar? firstPlaceCar = null;
			NormalizedCar? firstVisibleCar = null;
			NormalizedCar? preferredCar = null;
			NormalizedCar? hottestCar = null;

			foreach ( var normalizedCar in IRSDK.normalizedSession.leaderboardSortedNormalizedCars )
			{
				if ( normalizedCar.includeInLeaderboard )
				{
					firstPlaceCar ??= normalizedCar;

					if ( ( firstVisibleCar == null ) && !normalizedCar.isOnPitRoad && !normalizedCar.isOutOfCar )
					{
						firstVisibleCar = normalizedCar;
					}

					if ( normalizedCar.carNumber == Settings.data.PreferredCarNumber )
					{
						preferredCar = normalizedCar;
					}

					if ( ( normalizedCar.heat > 0 ) && ( ( hottestCar == null ) || ( normalizedCar.heat > hottestCar.heat ) ) )
					{
						hottestCar = normalizedCar;
					}
				}
			}

			if ( ( IRSDK.normalizedSession.sessionState == SessionState.StateCoolDown ) && ( firstPlaceCar != null ) && !firstPlaceCar.isOutOfCar )
			{
				cameraGroup = IRSDK.CameraGroupEnum.Close;

				IRSDK.targetCameraCarIdx = firstPlaceCar.carIdx;
				IRSDK.targetCameraCarNumber = firstPlaceCar.carNumber;
				IRSDK.targetCameraReason = $"Race is cooling down and this car is the winner";
			}
			else if ( Settings.data.PreferredCarLockOnHeatEnabled && ( preferredCar != null ) && ( preferredCar.heat >= Settings.data.PreferredCarLockOnHeat ) && ( IRSDK.normalizedSession.sessionState == SessionState.StateRacing ) )
			{
				IRSDK.targetCameraReason = $"Preferred car heat is >= {Settings.data.PreferredCarLockOnHeat}";

				cameraGroup = ChooseCameraForCar( preferredCar );
			}
			else if ( IRSDK.normalizedSession.sessionState == SessionState.StateCheckered )
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

				IRSDK.cameraSwitchWaitTicksRemaining = IRSDK.sendMessageWaitTicksRemaining;

				driverWasTalking = false;
			}
			else if ( ( firstVisibleCar != null ) && ( ( IRSDK.normalizedSession.sessionState == SessionState.StateWarmup ) || ( IRSDK.normalizedSession.sessionState == SessionState.StateParadeLaps ) ) )
			{
				IRSDK.targetCameraCarIdx = firstVisibleCar.carIdx;
				IRSDK.targetCameraCarNumber = firstVisibleCar.carNumber;
				IRSDK.targetCameraReason = "The race is about to start and this is the lead car";

				driverWasTalking = false;
			}
			else if ( ( firstVisibleCar != null ) && ( ( IRSDK.normalizedSession.sessionFlags & ( (uint) SessionFlags.GreenHeld | (uint) SessionFlags.StartReady | (uint) SessionFlags.StartSet | (uint) SessionFlags.StartGo ) ) != 0 ) )
			{
				IRSDK.targetCameraCarIdx = firstVisibleCar.carIdx;
				IRSDK.targetCameraCarNumber = firstVisibleCar.carNumber;
				IRSDK.targetCameraReason = "Green flag is about to be shown or is waving";

				driverWasTalking = false;
			}
			else if ( Settings.data.SwitchCameraToTalkingDriver && ( IRSDK.normalizedSession.radioTransmitCarIdx != -1 ) )
			{
				cameraGroup = IRSDK.CameraGroupEnum.Close;

				var normalizedCar = IRSDK.normalizedSession.FindNormalizedCarByCarIdx( IRSDK.targetCameraCarIdx );

				IRSDK.targetCameraCarIdx = IRSDK.normalizedSession.radioTransmitCarIdx;
				IRSDK.targetCameraCarNumber = normalizedCar?.carNumber ?? "?!?";
				IRSDK.targetCameraReason = "Driver of this car is talking";

				IRSDK.cameraSwitchWaitTicksRemaining = IRSDK.sendMessageWaitTicksRemaining;

				driverWasTalking = true;
			}
			else if ( driverWasTalking )
			{
				driverWasTalking = false;

				IRSDK.cameraSwitchWaitTicksRemaining = IRSDK.PostChatCameraSwitchWaitTicks;
			}
			else if ( IRSDK.normalizedSession.sessionState == SessionState.StateGetInCar )
			{
				cameraGroup = IRSDK.CameraGroupEnum.Scenic;

				IRSDK.targetCameraCarIdx = 0;
				IRSDK.targetCameraCarNumber = string.Empty;
				IRSDK.targetCameraReason = "The race has not started yet";
			}
			else
			{
				cameraGroup = IRSDK.CameraGroupEnum.Medium;

				if ( hottestCar != null )
				{
					IRSDK.targetCameraReason = "This is the hottest car";

					cameraGroup = ChooseCameraForCar( hottestCar );
				}
				else if ( firstVisibleCar != null )
				{
					IRSDK.targetCameraReason = "There are no hot cars and this is the lead car";

					cameraGroup = ChooseCameraForCar( firstVisibleCar );
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & ( (uint) SessionFlags.CautionWaving | (uint) SessionFlags.YellowWaving ) ) != 0 )
				{
					cameraGroup = IRSDK.CameraGroupEnum.Far;
				}
				else if ( ( preferredCar != null ) && ( ( preferredCar.distanceToCarInFrontInMeters < 50 ) || ( preferredCar.distanceToCarBehindInMeters < 50 ) ) )
				{
					IRSDK.targetCameraReason = "This is the preferred car";

					cameraGroup = ChooseCameraForCar( preferredCar );
				}
			}

			IRSDK.targetCameraGroupNumber = IRSDK.cameraGroupNumbers[ (int) cameraGroup ];
			IRSDK.targetCameraGroup = cameraGroup;

			if ( ( IRSDK.targetCameraCarNumber != oldTargetCameraCarNumber ) || ( IRSDK.targetCameraGroup != oldTargetCameraGroup ) || ( IRSDK.targetCameraReason != oldTargetCameraReason ) )
			{
				MainWindow.instance?.UpdateTarget();
			}
		}

		private static IRSDK.CameraGroupEnum ChooseCameraForCar( NormalizedCar normalizedCar )
		{
			IRSDK.targetCameraCarIdx = normalizedCar.carIdx;
			IRSDK.targetCameraCarNumber = normalizedCar.carNumber;

			IRSDK.CameraGroupEnum cameraGroup;

			if ( IRSDK.normalizedSession.isUnderCaution )
			{
				cameraGroup = IRSDK.CameraGroupEnum.Far;

				IRSDK.targetCameraReason += " (under caution)";
			}
			else if ( ( normalizedCar.distanceToCarInFrontInMeters > 1 ) && ( normalizedCar.distanceToCarInFrontInMeters < 12 ) && ( IRSDK.currentCameraCarIdx == normalizedCar.carIdx ) )

			{
				cameraGroup = IRSDK.CameraGroupEnum.Inside;

				IRSDK.targetCameraReason += " (car in front within 12m)";
			}
			else
			{
				var nearestCarDistance = Math.Min( normalizedCar.distanceToCarInFrontInMeters, normalizedCar.distanceToCarBehindInMeters );

				if ( nearestCarDistance < 10 )
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

			return cameraGroup;
		}
	}
}
