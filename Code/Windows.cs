using System;
using System.Runtime.InteropServices;

namespace iRacingTV
{
	internal static class Windows
	{
		public const int GWL_EXSTYLE = -20;

		public const int WS_EX_TOPMOST = 0x8;
		public const int WS_EX_TRANSPARENT = 0x20;
		public const int WS_EX_LAYERED = 0x80000;

		[StructLayout( LayoutKind.Sequential )]
		public struct MARGINS
		{
			public int Left;
			public int Right;
			public int Top;
			public int Bottom;
		}

		[DllImport( "user32.dll", SetLastError = true )]
		public static extern uint GetWindowLong( IntPtr hWnd, int nIndex );

		[DllImport( "user32.dll" )]
		public static extern int SetWindowLong( IntPtr hWnd, int nIndex, IntPtr dwNewLong );

		[DllImport( "user32.dll" )]
		public static extern bool SetLayeredWindowAttributes( IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags );

		[DllImport( "dwmapi.dll" )]
		public static extern int DwmExtendFrameIntoClientArea( IntPtr hwnd, ref MARGINS margins );
	}
}
