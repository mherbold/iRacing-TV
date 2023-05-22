
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

			IncidentData? currentIncident = IncidentScan.GetCurrentIncident();

			IRSDK.targetCameraReason = string.Empty;

			var cameraGroup = IRSDK.CameraGroupEnum.Far;

			if ( IRSDK.normalizedSession.isCheckeredFlag )
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

								IRSDK.cameraSwitchWaitTicksRemaining = IRSDK.sendMessageWaitTicksRemaining;

								IRSDK.targetCameraReason = $"Checkered flag, car #{normalizedCar.carNumber} is closest to the finish line.";
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

				IRSDK.targetCameraReason = $"Incident at frame {currentIncident.frameNumber} involving {normalizedCar?.userName ?? "<Unknown driver>"} in car #{normalizedCar?.carNumber ?? "<Unknown car number>"}.";

				driverWasTalking = false;
			}
			else if ( ( IRSDK.normalizedSession.sessionFlags & ( (uint) SessionFlags.GreenHeld | (uint) SessionFlags.StartReady | (uint) SessionFlags.StartSet | (uint) SessionFlags.StartGo ) ) != 0 )
			{
				var normalizedCar = IRSDK.normalizedSession.normalizedCars[ 0 ];

				IRSDK.targetCameraCarIdx = normalizedCar.carIdx;

				IRSDK.targetCameraReason = $"Session flags = GreenHeld or StartReady or StartSet or StartGo, looking at lead car #{normalizedCar.carNumber}.";

				driverWasTalking = false;
			}
			else if ( Settings.data.SwitchCameraToTalkingDriver && ( IRSDK.normalizedSession.radioTransmitCarIdx != -1 ) )
			{
				cameraGroup = IRSDK.CameraGroupEnum.Close;

				var normalizedCar = IRSDK.normalizedSession.FindNormalizedCarByCarIdx( IRSDK.targetCameraCarIdx );

				IRSDK.targetCameraCarIdx = IRSDK.normalizedSession.radioTransmitCarIdx;

				IRSDK.cameraSwitchWaitTicksRemaining = IRSDK.sendMessageWaitTicksRemaining;

				IRSDK.targetCameraReason = $"{normalizedCar?.userName ?? "<Unknown driver>"} is talking.";

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

				IRSDK.targetCameraReason = "Race has not started yet.";
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

							IRSDK.targetCameraReason = $"Hottest car is #{normalizedCar.carNumber}.";
						}
					}
				}

				if ( highestHeat == 0 )
				{
					var normalizedCar = IRSDK.normalizedSession.normalizedCars[ 0 ];

					IRSDK.targetCameraCarIdx = normalizedCar.carIdx;

					IRSDK.targetCameraReason = $"No hot cars - focusing on leader car #{normalizedCar.carNumber}.";
				}

				cameraGroup = IRSDK.CameraGroupEnum.Medium;

				if ( ( IRSDK.normalizedSession.sessionFlags & ( (uint) SessionFlags.CautionWaving | (uint) SessionFlags.YellowWaving ) ) != 0 )
				{
					cameraGroup = IRSDK.CameraGroupEnum.Far;

					IRSDK.targetCameraReason += " (Caution waving)";
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

									IRSDK.targetCameraReason = $"Preferred car is #{normalizedCar.carNumber}.";

									if ( IRSDK.normalizedSession.isUnderCaution )
									{
										cameraGroup = IRSDK.CameraGroupEnum.Far;

										IRSDK.targetCameraReason += " (Under caution)";
									}
									else if ( normalizedCar.distanceToCarInFrontInMeters < 10 )
									{
										cameraGroup = IRSDK.CameraGroupEnum.Inside;

										IRSDK.targetCameraReason += " (Car in front < 10m)";
									}
									else if ( nearestCarDistance < 10 )
									{
										cameraGroup = IRSDK.CameraGroupEnum.Close;

										IRSDK.targetCameraReason += " (Car within 10m)";
									}
									else if ( nearestCarDistance < 20 )
									{
										cameraGroup = IRSDK.CameraGroupEnum.Medium;

										IRSDK.targetCameraReason += " (Car within 20m)";
									}
									else if ( nearestCarDistance < 30 )
									{
										cameraGroup = IRSDK.CameraGroupEnum.Far;

										IRSDK.targetCameraReason += " (Car within 30m)";
									}
									else
									{
										cameraGroup = IRSDK.CameraGroupEnum.Blimp;

										IRSDK.targetCameraReason += " (Car within 50m)";
									}
								}

								break;
							}
						}
					}
				}
			}

			IRSDK.targetCameraGroupNumber = IRSDK.cameraGroupNumbers[ (int) cameraGroup ];

			IRSDK.targetCameraReason += $" ({cameraGroup})";
		}
	}
}
