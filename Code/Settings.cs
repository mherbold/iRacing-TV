using System.IO;
using System.Xml.Serialization;

namespace iRacingTV
{
	internal static class Settings
	{
		public const string SettingsFileName = "Settings.xml";

		public static SettingsData data = new();

		public static void Load()
		{
			LogFile.Write( "Loading settings..." );

			if ( File.Exists( Program.appDataFolderPath + SettingsFileName ) )
			{
				var xmlSerializer = new XmlSerializer( typeof( SettingsData ) );

				var fileStream = new FileStream( Program.appDataFolderPath + SettingsFileName, FileMode.Open );

				var deserializedObject = xmlSerializer.Deserialize( fileStream );

				if ( deserializedObject != null )
				{
					data = (SettingsData) deserializedObject;
				}

				fileStream.Close();
			}

			MainWindow.instance?.Initialize();

			LogFile.Write( " OK\r\n" );
		}

		public static void Save()
		{
			LogFile.Write( "Saving settings..." );

			var xmlSerializer = new XmlSerializer( typeof( SettingsData ) );

			var streamWriter = new StreamWriter( Program.appDataFolderPath + SettingsFileName );

			xmlSerializer.Serialize( streamWriter, data );

			streamWriter.Close();

			LogFile.Write( " OK\r\n" );
		}
	}
}
