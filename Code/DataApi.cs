
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;

using Aydsko.iRacingData;
using Aydsko.iRacingData.Tracks;
using Aydsko.iRacingData.Common;

namespace iRacingTV
{
	class DataApi
	{
		public static IDataClient? dataClient = null;
		public static DataResponse<IReadOnlyDictionary<string, TrackAssets>>? trackAssetsDataResponse = null;
		public static TrackScreenshotService? trackScreenshotService = null;

		public static async Task InitializeAsync()
		{
			Dispose();

			if ( ( Settings.data.Username != string.Empty ) && ( Settings.data.Password != string.Empty ) )
			{
				LogFile.Write( "Connecting to iRacing data API for track images..." );

				var serviceCollection = new ServiceCollection();

				serviceCollection.AddIRacingDataApi( options =>
				{
					options.UserAgentProductName = Program.AppName;
					options.UserAgentProductVersion = typeof( Program ).Assembly.GetName().Version;
				} );

				var serviceProvider = serviceCollection.BuildServiceProvider();

				dataClient = serviceProvider.GetRequiredService<IDataClient>();

				dataClient.UseUsernameAndPassword( Settings.data.Username, Settings.data.Password );

				trackAssetsDataResponse = await dataClient.GetTrackAssetsAsync();

				trackScreenshotService = serviceProvider.GetRequiredService<TrackScreenshotService>();

				LogFile.Write( " OK!\r\n" );
			}
			else
			{
				LogFile.Write( "iRacing username and password were not provided so track images won't be available.\r\n" );
			}
		}

		public static void Dispose()
		{
			trackAssetsDataResponse = null;
			dataClient = null;
		}

		public static string GetLargeTrackImageUrl( int trackId )
		{
			var trackAssets = FindTrackAssets( trackId );

			return trackAssets?.LargeImageUrl.ToString() ?? string.Empty;
		}

		public static string GetTrackLogoUrl( int trackId )
		{
			var trackAssets = FindTrackAssets( trackId );

			return trackAssets?.LogoUrl.ToString() ?? string.Empty;
		}

		public static TrackAssets? FindTrackAssets( int trackId )
		{
			if ( trackAssetsDataResponse != null )
			{
				foreach ( var trackAssetsKeyValuePair in trackAssetsDataResponse.Data )
				{
					if ( trackAssetsKeyValuePair.Value.TrackId == trackId )
					{
						return trackAssetsKeyValuePair.Value;
					}
				}
			}

			return null;
		}

		public static async Task<IEnumerable<Uri>?> GetTrackScreenshotUrlsAsync( int trackId )
		{
			if ( trackScreenshotService != null )
			{
				return await trackScreenshotService.GetScreenshotLinksAsync( trackId );
			}

			return null;
		}
	}
}
