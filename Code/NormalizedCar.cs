using System;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace iRacingTV
{
	internal partial class NormalizedCar
	{
		public int driverIdx = 0;
		public int carIdx = 0;

		public string userName = string.Empty;
		public string abbrevName = string.Empty;

		public string carNumber = string.Empty;
		public int carNumberRaw = 0;

		public bool hasCrossedStartLine = false;
		public bool raceHasStarted = false;
		public bool isOnPitRoad = false;
		public bool isOutOfCar = false;

		public int lapNumber = 0;

		public float deltaLapDistPct = 0;
		public float lapDistPct = 0;
		public float distanceMovedInMeters = 0;
		public float speedInMetersPerSecond = 0;
		public float lapPosition = 0;
		public float lapPositionRelativeToLeader = 0;
		public float distanceToCarInFrontInMeters = float.MaxValue;
		public float distanceToCarBehindInMeters = float.MaxValue;
		public float heat = 0;

		public int checkpointIdx = -1;
		public double[] checkpoints = new double[ 100 ];

		public bool includeInLeaderboard = false;
		public int leaderboardPosition = int.MaxValue;
		public int qualifyingPosition = int.MaxValue;
		public int officialPosition = int.MaxValue;

		public OverlayTexture? carOverlayTexture = null;
		public OverlayTexture? carNumberOverlayTexture = null;

		public async void Initialize( int driverIdx )
		{
			if ( IRSDK.session == null ) { throw new Exception( "iRacing session data is missing." ); }

			if ( driverIdx < IRSDK.session.DriverInfo.Drivers.Count )
			{
				var driver = IRSDK.session.DriverInfo.Drivers[ driverIdx ];

				this.driverIdx = driverIdx;
				carIdx = driver.CarIdx;

				userName = driver.UserName;
				abbrevName = driver.AbbrevName;

				carNumber = driver.CarNumber;
				carNumberRaw = driver.CarNumberRaw;

				includeInLeaderboard = driver.IsSpectator == 0 && driver.CarIsPaceCar == "0";

				var numberDesignMatch = CarNumberDesignStringRegex().Match( driver.CarNumberDesignStr );

				if ( numberDesignMatch.Success )
				{
					try
					{
						var carNumberUrl = $"http://localhost:32034/pk_number.png?size={Settings.data.CarNumberImageHeight}&view=0&number={carNumber}&numPat={numberDesignMatch.Groups[ 1 ].Value}&numCol={numberDesignMatch.Groups[ 3 ].Value},{numberDesignMatch.Groups[ 4 ].Value},{numberDesignMatch.Groups[ 5 ].Value}&numSlnt={numberDesignMatch.Groups[ 2 ].Value}";

						var httpClient = new HttpClient();

						var stream = await httpClient.GetStreamAsync( carNumberUrl );

						carNumberOverlayTexture = new OverlayTexture( stream );
					}
					catch ( Exception )
					{

					}
				}

				var carDesignMatch = CarDesignStringRegex().Match( driver.CarDesignStr );

				if ( numberDesignMatch.Success && carDesignMatch.Success )
				{
					try
					{
						var licColor = driver.LicColor[ 2.. ];
						var carPath = driver.CarPath.Replace( " ", "%5C" );

						var carUrl = $"http://localhost:32034/pk_car.png?size=2&view=1&licCol={licColor}&club={driver.ClubID}&sponsors={driver.CarSponsor_1},{driver.CarSponsor_2}&numPat={numberDesignMatch.Groups[ 1 ].Value}&numCol={numberDesignMatch.Groups[ 3 ].Value},{numberDesignMatch.Groups[ 4 ].Value},{numberDesignMatch.Groups[ 5 ].Value}&numSlnt={numberDesignMatch.Groups[ 2 ].Value}&number={carNumber}&carPath={carPath}&carPat={carDesignMatch.Groups[ 1 ].Value}&carCol={carDesignMatch.Groups[ 2 ].Value},{carDesignMatch.Groups[ 3 ].Value},{carDesignMatch.Groups[ 4 ].Value}&carRimType=2&carRimCol={carDesignMatch.Groups[ 5 ].Value}";

						var httpClient = new HttpClient();

						var stream = await httpClient.GetStreamAsync( carUrl );

						carOverlayTexture = new OverlayTexture( stream );
					}
					catch ( Exception )
					{

					}
				}
			}
			else
			{
				this.driverIdx = -1;
				carIdx = -1;

				userName = string.Empty;
				abbrevName = string.Empty;

				carNumber = string.Empty;
				carNumberRaw = 0;

				includeInLeaderboard = false;

				carOverlayTexture = null;
				carNumberOverlayTexture = null;
			}

			hasCrossedStartLine = false;
			raceHasStarted = false;
			isOnPitRoad = false;
			isOutOfCar = false;

			lapNumber = 0;

			deltaLapDistPct = 0;
			lapDistPct = 0;

			distanceMovedInMeters = 0;
			speedInMetersPerSecond = 0;
			lapPosition = 0;
			lapPositionRelativeToLeader = 0;
			distanceToCarInFrontInMeters = float.MaxValue;
			distanceToCarBehindInMeters = float.MaxValue;
			heat = 0;

			checkpointIdx = -1;

			for ( var i = 0; i < checkpoints.Length; i++ )
			{
				checkpoints[ i ] = 0;
			}

			leaderboardPosition = int.MaxValue;
			qualifyingPosition = int.MaxValue;
			officialPosition = int.MaxValue;

			if ( includeInLeaderboard )
			{
				foreach ( var s in IRSDK.session.SessionInfo.Sessions )
				{
					if ( s.SessionName == "QUALIFY" && s.ResultsPositions != null )
					{
						foreach ( var position in s.ResultsPositions )
						{
							if ( position.CarIdx == carIdx )
							{
								qualifyingPosition = position.Position;
								break;
							}
						}
					}
				}
			}
		}

		public void Update( NormalizedSession normalizedSession )
		{
			if ( !includeInLeaderboard )
			{
				return;
			}

			if ( IRSDK.data == null ) { throw new Exception( "iRacing telemetry data is missing." ); }

			var car = IRSDK.data.Cars[ carIdx ];

			if ( !hasCrossedStartLine )
			{
				if ( car.CarIdxLap >= 1 && car.CarIdxLapDistPct >= 0.0f && car.CarIdxLapDistPct < 0.1f )
				{
					hasCrossedStartLine = true;
				}
			}

			raceHasStarted = car.CarIdxLap >= 1;
			isOnPitRoad = car.CarIdxOnPitRoad;
			isOutOfCar = car.CarIdxLapDistPct == -1;

			if ( car.CarIdxLapDistPct >= 0 )
			{
				deltaLapDistPct = car.CarIdxLapDistPct - lapDistPct;
				lapDistPct = car.CarIdxLapDistPct;
			}
			else
			{
				deltaLapDistPct = 0;
			}

			if ( hasCrossedStartLine )
			{
				if ( deltaLapDistPct < -0.9f )
				{
					deltaLapDistPct += 1;

					lapNumber++;
				}

				lapPosition = lapNumber + lapDistPct - 1;

				var checkpointIdx = (int) Math.Floor( lapDistPct * Settings.data.NumberOfCheckpoints ) % Settings.data.NumberOfCheckpoints;

				if ( checkpointIdx != this.checkpointIdx )
				{
					this.checkpointIdx = checkpointIdx;

					checkpoints[ checkpointIdx ] = normalizedSession.sessionTime;
				}
			}
			else
			{
				lapPosition = 0;
			}

			officialPosition = car.CarIdxPosition;

			distanceMovedInMeters = deltaLapDistPct * normalizedSession.trackLengthInMeters;
			speedInMetersPerSecond = distanceMovedInMeters / normalizedSession.deltaSessionTime;
		}

		public static Comparison<NormalizedCar> LapPositionComparison = delegate ( NormalizedCar object1, NormalizedCar object2 )
		{
			return object2.lapPosition.CompareTo( object1.lapPosition );
		};

		public static Comparison<NormalizedCar> LeaderboardPositionComparison = delegate ( NormalizedCar object1, NormalizedCar object2 )
		{
			return object1.leaderboardPosition.CompareTo( object2.leaderboardPosition );
		};

		[GeneratedRegex( "(\\d+),(.{6}),(.{6}),(.{6}),?(.{6})?" )]
		public static partial Regex CarDesignStringRegex();

		[GeneratedRegex( "(\\d+),(\\d+),(.{6}),(.{6}),(.{6})" )]
		public static partial Regex CarNumberDesignStringRegex();
	}
}
