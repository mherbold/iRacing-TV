using System;
using System.IO;

namespace iRacingTV
{
	internal static class LogFile
	{
		public const string fileName = $"{Program.AppName}.log";

		public static void Initialize()
		{
			if ( File.Exists( Program.appDataFolderPath + fileName ) )
			{
				File.Delete( Program.appDataFolderPath + fileName );
			}
		}

		public static void Write( string message )
		{
			File.AppendAllText( Program.appDataFolderPath + fileName, message );

			MainWindow.instance?.AddToStatusTextBox( message );
		}

		public static void WriteException( Exception exception )
		{
			Write( $"Exception caught!\r\n\r\n{exception.Message}\r\n\r\n{exception.StackTrace}\r\n\r\n" );
		}
	}
}
