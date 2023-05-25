
using System;
using System.IO;
using System.Numerics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Veldrid;
using Veldrid.ImageSharp;

using ImGuiNET;

namespace iRacingTV
{
	internal class OverlayTexture
	{
		public readonly Texture texture;
		public readonly TextureView textureView;
		public readonly nint textureId;

		public OverlayTexture( string imageFileName )
		{
			if ( Overlay.graphicsDevice == null ) { throw new Exception( "Graphics device was not initialized." ); }
			if ( Overlay.imGuiRenderer == null ) { throw new Exception( "ImGui renderer was not initialized." ); }

			var imageSharpTexture = new ImageSharpTexture( imageFileName, true, true );

			texture = imageSharpTexture.CreateDeviceTexture( Overlay.graphicsDevice, Overlay.graphicsDevice.ResourceFactory );

			var textureViewDescription = new TextureViewDescription( texture, PixelFormat.R8_G8_B8_A8_UNorm_SRgb );

			textureView = Overlay.graphicsDevice.ResourceFactory.CreateTextureView( textureViewDescription );

			textureId = Overlay.imGuiRenderer.GetOrCreateImGuiBinding( Overlay.graphicsDevice.ResourceFactory, textureView );
		}

		public OverlayTexture( Stream imageStream )
		{
			if ( Overlay.graphicsDevice == null ) { throw new Exception( "Graphics device was not initialized." ); }
			if ( Overlay.imGuiRenderer == null ) { throw new Exception( "ImGui renderer was not initialized." ); }

			var imageSharpTexture = new ImageSharpTexture( imageStream, true, true );

			texture = imageSharpTexture.CreateDeviceTexture( Overlay.graphicsDevice, Overlay.graphicsDevice.ResourceFactory );

			var textureViewDescription = new TextureViewDescription( texture, PixelFormat.R8_G8_B8_A8_UNorm_SRgb );

			textureView = Overlay.graphicsDevice.ResourceFactory.CreateTextureView( textureViewDescription );

			textureId = Overlay.imGuiRenderer.GetOrCreateImGuiBinding( Overlay.graphicsDevice.ResourceFactory, textureView );
		}

		~OverlayTexture()
		{
			if ( Overlay.imGuiRenderer == null ) { throw new Exception( "ImGui renderer was not initialized." ); }

			Overlay.imGuiRenderer.RemoveImGuiBinding( textureView );

			texture.Dispose();
		}

		public void Draw( Vector2 position, Vector2? size = null, Vector4? tintColor = null )
		{
			ImGui.SetCursorPos( position );

			ImGui.Image( textureId, size ?? new Vector2( texture.Width, texture.Height ), Vector2.Zero, Vector2.One, tintColor ?? Vector4.One );
		}

		public static async Task<OverlayTexture?> CreateViaUrlAsync( string url )
		{
			if ( url != string.Empty )
			{
				var attempts = 0;

				while ( attempts < 5 )
				{
					attempts++;

					try
					{
						var httpClient = new HttpClient();

						var stream = await httpClient.GetStreamAsync( url );

						return new OverlayTexture( stream );
					}
					catch ( Exception )
					{
						Thread.Sleep( 100 );
					}
				}

				LogFile.Write( $"Failed to get an overlay texture after {attempts} attempts!\r\n{url}\r\n" );
			}

			return null;
		}
	}
}
