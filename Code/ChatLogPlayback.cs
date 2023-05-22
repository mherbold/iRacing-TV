
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace iRacingTV
{
	internal static partial class ChatLogPlayback
	{
		public static string chatLogFileName = string.Empty;
		public static List<ChatLogData>? chatLogList = null;

		public static void Initialize()
		{
			if ( !IRSDK.normalizedSession.isReplay )
			{
				return;
			}

			var newChatLogFileName = $"{Program.sttDocumentsFolderPath}\\ChatLogs\\{IRSDK.normalizedSession.sessionID}-{IRSDK.normalizedSession.subSessionID}.csv";

			if ( newChatLogFileName != chatLogFileName )
			{
				chatLogFileName = newChatLogFileName;

				chatLogList = new List<ChatLogData>();

				if ( File.Exists( chatLogFileName ) )
				{
					LogFile.Write( "Loading chat log file..." );

					var streamReader = File.OpenText( chatLogFileName );

					var startSessionTime = 0.0;

					while ( true )
					{
						var line = streamReader.ReadLine();

						if ( line == null )
						{
							break;
						}

						var match = ChatLogCSVRegex().Match( line );

						if ( match.Success )
						{
							var sessionTime = float.Parse( match.Groups[ 2 ].Value, CultureInfo.InvariantCulture.NumberFormat );
							var eventId = int.Parse( match.Groups[ 3 ].Value );

							if ( eventId == 5 )
							{
								if ( startSessionTime == 0.0 )
								{
									startSessionTime = sessionTime;
								}
							}
							else if ( eventId == 6 )
							{
								if ( startSessionTime > 0 )
								{
									chatLogList.Add( new ChatLogData( startSessionTime - 1.6, sessionTime + 2, match.Groups[ 8 ].Value ) );

									startSessionTime = 0;
								}
							}
						}
					}

					LogFile.Write( " OK\r\n" );
				}

				chatLogList.Reverse();
			}
		}

		public static void Update()
		{
			IRSDK.normalizedSession.chatLogData = null;

			if ( chatLogList != null )
			{
				var sessionTime = double.MaxValue;

				foreach ( var chatLogData in chatLogList )
				{
					if ( chatLogData.startSessionTime > sessionTime )
					{
						break;
					}

					sessionTime = chatLogData.startSessionTime;

					if ( ( chatLogData.startSessionTime <= IRSDK.normalizedSession.sessionTime ) && ( chatLogData.endSessionTime > IRSDK.normalizedSession.sessionTime ) )
					{
						IRSDK.normalizedSession.chatLogData = chatLogData;
						break;
					}
				}
			}

		}

		[GeneratedRegex( "([^,]*),([^,]*),([^,]*),([^,]*)(,([^,]*))?(,\"([^\"]*)\")?" )]
		private static partial Regex ChatLogCSVRegex();
	}
}
