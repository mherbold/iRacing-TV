
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using irsdkSharp.Serialization.Enums.Fastest;

namespace iRacingTV
{
	internal static partial class SessionFlagsPlayback
	{
		public static List<SessionFlagsData>? sessionFlagsDataList = null;

		public static string sessionFlagsFileName = string.Empty;
		public static StreamWriter? streamWriter = null;
		public static uint sessionFlags = 0;

		public static void Initialize()
		{
			var newSessionFlagsFileName = $"{Program.documentsFolderPath}\\SessionFlags\\{IRSDK.normalizedSession.sessionID}-{IRSDK.normalizedSession.subSessionID}.csv";

			if ( newSessionFlagsFileName != sessionFlagsFileName )
			{
				sessionFlagsFileName = newSessionFlagsFileName;

				sessionFlagsDataList = new List<SessionFlagsData>();

				if ( IRSDK.normalizedSession.isReplay )
				{
					if ( File.Exists( sessionFlagsFileName ) )
					{
						LogFile.Write( "Loading session flags file..." );

						var streamReader = File.OpenText( sessionFlagsFileName );

						while ( true )
						{
							var line = streamReader.ReadLine();

							if ( line == null )
							{
								break;
							}

							var match = SessionFlagsCSVRegex().Match( line );

							if ( match.Success )
							{
								sessionFlagsDataList.Add( new SessionFlagsData( int.Parse( match.Groups[ 1 ].Value ), float.Parse( match.Groups[ 2 ].Value, CultureInfo.InvariantCulture.NumberFormat ), uint.Parse( match.Groups[ 3 ].Value, NumberStyles.HexNumber ) ) );
							}
						}

						LogFile.Write( " OK\r\n" );
					}

					sessionFlagsDataList.Reverse();
				}
				else
				{
					if ( streamWriter != null )
					{
						streamWriter.Close();

						streamWriter = null;
					}

					LogFile.Write( "Opening session flags file for writing..." );

					Directory.CreateDirectory( $"{Program.documentsFolderPath}\\SessionFlags" );

					streamWriter = File.AppendText( sessionFlagsFileName );

					LogFile.Write( " OK\r\n" );
				}
			}
		}

		public static void Update()
		{
			if ( IRSDK.normalizedSession.isReplay )
			{
				IRSDK.normalizedSession.sessionFlags = 0;

				if ( sessionFlagsDataList != null )
				{
					foreach ( var sessionFlagData in sessionFlagsDataList )
					{
						if ( IRSDK.normalizedSession.sessionTime >= sessionFlagData.sessionTime )
						{
							IRSDK.normalizedSession.sessionFlags = sessionFlagData.sessionFlags;

							break;
						}
					}
				}
			}
			else
			{
				if ( sessionFlags != IRSDK.normalizedSession.sessionFlags )
				{
					if ( streamWriter != null )
					{
						var sessionFlagsAsHex = IRSDK.normalizedSession.sessionFlags.ToString( "X8" );

						streamWriter.WriteLine( $"{IRSDK.normalizedSession.sessionTick},{IRSDK.normalizedSession.sessionTime:0.000},0x{sessionFlagsAsHex}" );

						streamWriter.Flush();
					}
				}
			}

			if ( sessionFlags != IRSDK.normalizedSession.sessionFlags )
			{
				var sessionFlagsString = string.Empty;

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.Checkered ) != 0 )
				{
					sessionFlagsString += $" {SessionFlags.Checkered}";
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.White ) != 0 )
				{
					sessionFlagsString += $" {SessionFlags.White}";
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.Green ) != 0 )
				{
					sessionFlagsString += $" {SessionFlags.Green}";
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.Yellow ) != 0 )
				{
					sessionFlagsString += $" {SessionFlags.Yellow}";
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.Red ) != 0 )
				{
					sessionFlagsString += $" {SessionFlags.Red}";
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.Blue ) != 0 )
				{
					sessionFlagsString += $" {SessionFlags.Blue}";
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.Debris ) != 0 )
				{
					sessionFlagsString += $" {SessionFlags.Debris}";
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.Crossed ) != 0 )
				{
					sessionFlagsString += $" {SessionFlags.Crossed}";
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.YellowWaving ) != 0 )
				{
					sessionFlagsString += $" {SessionFlags.YellowWaving}";
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.OneLapToGreen ) != 0 )
				{
					sessionFlagsString += $" {SessionFlags.OneLapToGreen}";
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.GreenHeld ) != 0 )
				{
					sessionFlagsString += $" {SessionFlags.GreenHeld}";
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.TenToGo ) != 0 )
				{
					sessionFlagsString += $" {SessionFlags.TenToGo}";
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.FiveToGo ) != 0 )
				{
					sessionFlagsString += $" {SessionFlags.FiveToGo}";
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.RandomWaving ) != 0 )
				{
					sessionFlagsString += $" {SessionFlags.RandomWaving}";
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.Caution ) != 0 )
				{
					sessionFlagsString += $" {SessionFlags.Caution}";
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.CautionWaving ) != 0 )
				{
					sessionFlagsString += $" {SessionFlags.CautionWaving}";
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.Black ) != 0 )
				{
					sessionFlagsString += $" {SessionFlags.Black}";
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.Disqualify ) != 0 )
				{
					sessionFlagsString += $" {SessionFlags.Disqualify}";
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.Servicible ) != 0 )
				{
					sessionFlagsString += $" {SessionFlags.Servicible}";
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.Furled ) != 0 )
				{
					sessionFlagsString += $" {SessionFlags.Furled}";
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.Repair ) != 0 )
				{
					sessionFlagsString += $" {SessionFlags.Repair}";
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.StartHidden ) != 0 )
				{
					sessionFlagsString += $" {SessionFlags.StartHidden}";
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.StartReady ) != 0 )
				{
					sessionFlagsString += $" {SessionFlags.StartReady}";
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.StartSet ) != 0 )
				{
					sessionFlagsString += $" {SessionFlags.StartSet}";
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.StartGo ) != 0 )
				{
					sessionFlagsString += $" {SessionFlags.StartGo}";
				}

				LogFile.Write( $"Session flags ={sessionFlagsString}\r\n" );
			}

			sessionFlags = IRSDK.normalizedSession.sessionFlags;
		}

		[GeneratedRegex( "(.*),(.*),0x(.*)" )]
		private static partial Regex SessionFlagsCSVRegex();
	}
}
