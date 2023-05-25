
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace iRacingTV
{
	internal static class Program
	{
		public const string AppName = "iRacing-TV";
		public const string STTAppName = "iRacing-STT-VR";

		public static string appDataFolderPath = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ) + $"\\{AppName}\\";
		public static string documentsFolderPath = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ) + $"\\{AppName}\\";
		public static string sttDocumentsFolderPath = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ) + $"\\{STTAppName}\\";

		public static DispatcherTimer dispatcherTimer = new( DispatcherPriority.Render );

		public static bool keepRunning = true;
		public static bool applySettingChangesRequested = false;
		public static int tickMutex = 0;

		public static void Initialize()
		{
			LogFile.Initialize();

			LogFile.Write( $"{MainWindow.instance?.Title} is starting up!\r\n\r\n" );

			Settings.Load();

			Task.Run( () => ProgramAsync() );
		}

		private static async Task ProgramAsync()
		{
			try
			{
				Overlay.Initialize();

				await DataApi.InitializeAsync();

				dispatcherTimer.Tick += async ( sender, e ) => await TickAsync( sender, e );
				dispatcherTimer.Interval = TimeSpan.FromSeconds( 1 / 60.0f );
				dispatcherTimer.Start();

				while ( keepRunning )
				{
					Thread.Sleep( 250 );
				}

				dispatcherTimer.Stop();

				Overlay.Dispose();
			}
			catch ( Exception exception )
			{
				LogFile.WriteException( exception );
			}
		}

		private static async Task TickAsync( object? sender, EventArgs e )
		{
			int tickMutex = Interlocked.Increment( ref Program.tickMutex );

			if ( tickMutex == 1 )
			{
				if ( applySettingChangesRequested )
				{
					LogFile.Write( "Reinitializing..." );

					applySettingChangesRequested = false;

					Overlay.isVisible = false;

					MainWindow.instance?.Update();

					Overlay.Initialize();

					await DataApi.InitializeAsync();

					IRSDK.sessionResetRequested = true;
				}

				IRSDK.Update();
				Director.Update();
				IncidentScan.Update();
				Overlay.Update();
				MainWindow.instance?.UpdateDebug();
			}

			Interlocked.Decrement( ref Program.tickMutex );
		}
	}
}
