
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
			var newSessionFlagsFileName = $"{Program.documentsFolderPath}SessionFlags\\{IRSDK.normalizedSession.sessionID}-{IRSDK.normalizedSession.subSessionID}.csv";

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

					Directory.CreateDirectory( $"{Program.documentsFolderPath}SessionFlags" );

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

						streamWriter.WriteLine( $"{IRSDK.sessionNum},{IRSDK.normalizedSession.sessionTime:0.000},0x{sessionFlagsAsHex}" );

						streamWriter.Flush();
					}
				}
			}

			if ( sessionFlags != IRSDK.normalizedSession.sessionFlags )
			{
				sessionFlags = IRSDK.normalizedSession.sessionFlags;

				MainWindow.instance?.UpdateSessionFlags();
			}
		}

		[GeneratedRegex( "(.*),(.*),0x(.*)" )]
		private static partial Regex SessionFlagsCSVRegex();
	}
}
