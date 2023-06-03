using System;
using System.Runtime.InteropServices;

namespace iRacingTV
{
	internal static class Windows
	{
		public struct MARGINS
		{
			public int Left;
			public int Right;
			public int Top;
			public int Bottom;
		}

		[DllImport( "user32.dll" )]
		public static extern IntPtr GetActiveWindow();

		[DllImport( "user32.dll" )]
		public static extern int SetWindowLong( IntPtr hWnd, int nIndex, uint dwNewLong );

		[DllImport( "user32.dll" )]
		public static extern bool SetWindowPos( IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags );

		[DllImport( "Dwmapi.dll" )]
		public static extern uint DwmExtendFrameIntoClientArea( IntPtr hWnd, ref MARGINS margins );

		public const int GWL_EXSTYLE = -20;

		public const uint WS_EX_TOPMOST = 0x00000008;
		public const uint WS_EX_TRANSPARENT = 0x00000020;
		public const uint WS_EX_LAYERED = 0x00080000;

		public static readonly IntPtr HWND_TOPMOST = new IntPtr( -1 );

		public const uint SWP_NOSIZE = 0x0001;
		public const uint SWP_NOMOVE = 0x0002;
	}
}
