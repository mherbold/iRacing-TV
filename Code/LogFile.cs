using System;
using System.IO;
using System.Threading;

namespace iRacingTV
{
	internal static class LogFile
	{
		public const string fileName = $"{Program.AppName}.log";

		public static ReaderWriterLock readerWriterLock = new();

		public static void Initialize()
		{
			if ( File.Exists( Program.appDataFolderPath + fileName ) )
			{
				File.Delete( Program.appDataFolderPath + fileName );
			}
		}

		public static void Write( string message )
		{
			MainWindow.instance?.AddToStatusTextBox( message );

			try
			{
				readerWriterLock.AcquireWriterLock( 250 );

				File.AppendAllText( Program.appDataFolderPath + fileName, message );
			}
			finally
			{
				readerWriterLock.ReleaseWriterLock();
			}
		}

		public static void WriteException( Exception exception )
		{
			Write( $"Exception caught!\r\n\r\n{exception.Message}\r\n\r\n{exception.StackTrace}\r\n\r\n" );
		}
	}
}
