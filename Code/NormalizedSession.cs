using irsdkSharp.Serialization.Enums.Fastest;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace iRacingTV
{
	internal partial class NormalizedSession
	{
		public const int MaxNumCars = 63;

		public int sessionID = -1;
		public int subSessionID = -1;
		public int sessionCount = -1;

		public float trackLengthInMeters = 0;

		public bool isReplay = false;
		public bool raceHasStarted = false;
		public bool displayIsMetric = false;
		public bool isUnderCaution = false;
		public bool isCheckeredFlag = false;
		public bool isTimedSession = false;
		public bool isPracticing = false;
		public bool isQualifying = false;

		public float deltaSessionTime = 0;
		public double sessionTime = 0;
		public int sessionTick = 0;

		public int replayFrameNum = 0;

		public int sessionLap = 0;
		public int sessionLapsTotal = 0;
		public int sessionLapsRemain = 0;

		public double sessionTimeTotal = 0;
		public double sessionTimeRemain = 0;

		public uint sessionFlags = 0;

		public int radioTransmitCarIdx = -1;

		public int camGroupNumber = -1;
		public int camCameraNumber = -1;
		public int camCarIdx = -1;

		public ChatLogData? chatLogData = null;

		public NormalizedCar[] normalizedCars = new NormalizedCar[ MaxNumCars ];
		public List<NormalizedCar> sortedNormalizedCars = new( MaxNumCars );

		public NormalizedSession()
		{
			for ( var i = 0; i < MaxNumCars; i++ )
			{
				normalizedCars[ i ] = new NormalizedCar();

				sortedNormalizedCars.Add( normalizedCars[ i ] );
			}
		}

		public void Initialize( bool forceResetRace )
		{
			if ( IRSDK.session == null ) { throw new Exception( "iRacing session data is missing." ); }
			if ( IRSDK.data == null ) { throw new Exception( "iRacing telemetry data is missing." ); }

			if ( ( sessionID != IRSDK.session.WeekendInfo.SessionID ) || ( subSessionID != IRSDK.session.WeekendInfo.SubSessionID ) || forceResetRace )
			{
				sessionID = IRSDK.session.WeekendInfo.SessionID;
				subSessionID = IRSDK.session.WeekendInfo.SubSessionID;
				sessionCount = IRSDK.session.SessionInfo.Sessions.Count;

				var match = TrackLengthRegex().Match( IRSDK.session.WeekendInfo.TrackLength );

				if ( match.Success )
				{
					var trackLengthInKilometers = float.Parse( match.Groups[ 1 ].Value, CultureInfo.InvariantCulture.NumberFormat );

					trackLengthInMeters = trackLengthInKilometers * 1000;
				}
				else
				{
					throw new Exception( "Failed to parse the track length string." );
				}

				isReplay = IRSDK.session.WeekendInfo.SimMode == "replay";
				raceHasStarted = false;
				isUnderCaution = false;
				isCheckeredFlag = false;

				sessionTime = IRSDK.data.SessionTime;

				chatLogData = null;

				for ( var carIdx = 0; carIdx < MaxNumCars; carIdx++ )
				{
					normalizedCars[ carIdx ].Initialize( carIdx, true );
				}
			}
			else
			{
				for ( var carIdx = 0; carIdx < MaxNumCars; carIdx++ )
				{
					normalizedCars[ carIdx ].Initialize( carIdx, false );
				}
			}
		}

		public void Update()
		{
			if ( IRSDK.session == null ) { throw new Exception( "iRacing session data is missing." ); }
			if ( IRSDK.data == null ) { throw new Exception( "iRacing telemetry data is missing." ); }

			if ( IRSDK.data.SessionNum == -1 )
			{
				return;
			}

			displayIsMetric = IRSDK.data.DisplayUnits == 1;
			isUnderCaution = ( sessionFlags & ( (uint) SessionFlags.CautionWaving | (uint) SessionFlags.Caution | (uint) SessionFlags.YellowWaving | (uint) SessionFlags.Yellow ) ) != 0;
			isTimedSession = IRSDK.data.SessionLapsTotal == 32767;

			isPracticing = IRSDK.session.SessionInfo.Sessions[ IRSDK.data.SessionNum ].SessionName == "PRACTICE";
			isQualifying = IRSDK.session.SessionInfo.Sessions[ IRSDK.data.SessionNum ].SessionName == "QUALIFY";

			if ( !isPracticing && !isQualifying )
			{
				isCheckeredFlag |= ( sessionFlags & ( (uint) SessionFlags.Checkered ) ) != 0;
				raceHasStarted |= ( sessionFlags & ( (uint) SessionFlags.StartGo | (uint) SessionFlags.Green ) ) != 0;

				if ( raceHasStarted && isTimedSession && ( IRSDK.data.SessionTimeRemain < 5 ) )
				{
					isCheckeredFlag = true;
				}
			}

			deltaSessionTime = (float) Math.Round( ( IRSDK.data.SessionTime - sessionTime ) / ( 1.0f / 60.0f ) ) * ( 1.0f / 60.0f );
			sessionTime = IRSDK.data.SessionTime;
			sessionTick = IRSDK.data.SessionTick;

			replayFrameNum = IRSDK.data.ReplayFrameNum;

			sessionLap = IRSDK.data.SessionLapsTotal - Math.Max( 0, IRSDK.data.SessionLapsRemain );
			sessionLapsTotal = IRSDK.data.SessionLapsTotal;
			sessionLapsRemain = Math.Max( 0, IRSDK.data.SessionLapsRemain );

			sessionTimeTotal = IRSDK.data.SessionTimeTotal;
			sessionTimeRemain = Math.Max( 0, IRSDK.data.SessionTimeRemain );

			radioTransmitCarIdx = IRSDK.data.RadioTransmitCarIdx;

			camGroupNumber = IRSDK.data.CamGroupNumber;
			camCameraNumber = IRSDK.data.CamCameraNumber;
			camCarIdx = IRSDK.data.CamCarIdx;

			if ( deltaSessionTime > 0 )
			{
				foreach ( var normalizedCar in normalizedCars )
				{
					normalizedCar.Update( this );
				}

				foreach ( var normalizedCar in normalizedCars )
				{
					normalizedCar.heat = 0;
					normalizedCar.distanceToCarInFrontInMeters = float.MaxValue;
					normalizedCar.distanceToCarBehindInMeters = float.MaxValue;

					foreach ( var otherNormalizedCar in normalizedCars )
					{
						if ( normalizedCar != otherNormalizedCar )
						{
							if ( normalizedCar.includeInLeaderboard && !normalizedCar.isOnPitRoad && !normalizedCar.isOutOfCar )
							{
								if ( otherNormalizedCar.includeInLeaderboard && !otherNormalizedCar.isOnPitRoad && !otherNormalizedCar.isOutOfCar )
								{
									var distanceToOtherCar = otherNormalizedCar.lapDistPct - normalizedCar.lapDistPct;

									if ( distanceToOtherCar > 0.5f )
									{
										distanceToOtherCar -= 1.0f;
									}
									else if ( distanceToOtherCar < -0.5f )
									{
										distanceToOtherCar += 1.0f;
									}

									var distanceToOtherCarInMeters = distanceToOtherCar * trackLengthInMeters;

									var heat = Math.Max( 0.0f, 50.0f - Math.Abs( distanceToOtherCarInMeters ) ) / 50.0f;

									normalizedCar.heat += (float) Math.Sqrt( heat );

									if ( distanceToOtherCarInMeters >= 0.0f )
									{
										normalizedCar.distanceToCarInFrontInMeters = Math.Min( normalizedCar.distanceToCarInFrontInMeters, distanceToOtherCarInMeters );
									}
									else
									{
										normalizedCar.distanceToCarBehindInMeters = Math.Min( normalizedCar.distanceToCarBehindInMeters, -distanceToOtherCarInMeters );
									}
								}
							}
						}
					}
				}

				sortedNormalizedCars.Sort( NormalizedCar.LapPositionComparison );

				var leaderboardPosition = 1;
				var leaderLapPosition = sortedNormalizedCars[ 0 ].lapPosition;

				foreach ( var normalizedCar in sortedNormalizedCars )
				{
					if ( isPracticing )
					{
						normalizedCar.leaderboardPosition = 1;
					}
					else if ( isQualifying )
					{
						normalizedCar.leaderboardPosition = normalizedCar.officialPosition;
					}
					else if ( isUnderCaution || isCheckeredFlag )
					{
						if ( normalizedCar.officialPosition >= 1 )
						{
							normalizedCar.leaderboardPosition = normalizedCar.officialPosition;
						}
						else
						{
							normalizedCar.leaderboardPosition = int.MaxValue;
						}
					}
					else if ( normalizedCar.hasCrossedStartLine )
					{
						normalizedCar.leaderboardPosition = leaderboardPosition++;
					}
					else if ( raceHasStarted )
					{
						if ( normalizedCar.officialPosition > 0 )
						{
							normalizedCar.leaderboardPosition = normalizedCar.officialPosition;
						}
						else
						{
							normalizedCar.leaderboardPosition = MaxNumCars;
						}
					}
					else
					{
						normalizedCar.leaderboardPosition = normalizedCar.qualifyingPosition;
					}

					normalizedCar.lapPositionRelativeToLeader = leaderLapPosition - normalizedCar.lapPosition;
				}

				sortedNormalizedCars.Sort( NormalizedCar.LeaderboardPositionComparison );

				leaderboardPosition = 1;

				foreach ( var normalizedCar in sortedNormalizedCars )
				{
					normalizedCar.leaderboardPosition = leaderboardPosition++;
				}

				raceHasStarted |= sortedNormalizedCars[ 0 ].raceHasStarted;

				if ( !isPracticing && !isQualifying )
				{
					isCheckeredFlag |= sortedNormalizedCars[ 0 ].lapPosition >= ( sessionLapsTotal - ( 200.0f / trackLengthInMeters ) );
				}
			}
		}

		public NormalizedCar? FindNormalizedCarByCarIdx( int carIdx )
		{
			foreach ( var normalizedCar in normalizedCars )
			{
				if ( normalizedCar.carIdx == carIdx )
				{
					return normalizedCar;
				}
			}

			return null;
		}

		[GeneratedRegex( "([-+]?[0-9]*\\.?[0-9]+)" )]
		public static partial Regex TrackLengthRegex();
	}
}
