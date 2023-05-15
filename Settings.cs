using System;
using System.Numerics;

namespace iRacingTV
{
	[Serializable]
	public class Settings
	{
		public int _overlayX { get; set; } = 0;
		public int _overlayY { get; set; } = 0;
		public int _overlayWidth { get; set; } = 1920;
		public int _overlayHeight { get; set; } = 1080;

		public string _insideCameraGroupName { get; set; } = "Roll Bar";
		public string _closeCameraGroupName { get; set; } = "TV1";
		public string _mediumCameraGroupName { get; set; } = "TV2";
		public string _farCameraGroupName { get; set; } = "TV3";
		public string _blimpCameraGroupName { get; set; } = "Blimp";
		public string _scenicCameraGroupName { get; set; } = "Scenic";

		public string _preferredCarNumber { get; set; } = string.Empty;

		public bool _switchToTalkingDriver { get; set; } = true;
		public bool _showDistances { get; set; } = true;
		public bool _betweenCars { get; set; } = false;

		public string _fontAFileName { get; set; } = "RevolutionGothic_ExtraBold.otf";
		public int _fontASize { get; set; } = 31;

		public string _fontBFileName { get; set; } = "RevolutionGothic_ExtraBold_It.otf";
		public int _fontBSize { get; set; } = 31;

		public string _fontCFileName { get; set; } = "RevolutionGothic_ExtraBold.otf";
		public int _fontCSize { get; set; } = 25;

		public string _fontDFileName { get; set; } = "RevolutionGothic_ExtraBold_It.otf";
		public int _fontDSize { get; set; } = 25;

		public string _lapString { get; set; } = "LAP";
		public string _lapsRemainingString { get; set; } = "TO GO";
		public string _distanceLapPositionString { get; set; } = "L";
		public string _distanceFeetString { get; set; } = "FT";
		public string _distanceMetersString { get; set; } = "M";
		public string _pitString { get; set; } = "PIT";
		public string _outString { get; set; } = "OUT";
		public string _mphString { get; set; } = "MPH";
		public string _kphString { get; set; } = "KPH";
		public string _finalLapString { get; set; } = "FINAL LAP";
		public string _voiceOfString { get; set; } = "VOICE OF";

		public string _raceStatusFileName { get; set; } = "race-status.png";
		public Vector2 _raceStatusPosition { get; set; } = new( 44, 0 );
		public Vector4 _raceStatusTint { get; set; } = new( 1, 1, 1, 0.9f );

		public string _leaderboardFileName { get; set; } = "leaderboard.png";
		public Vector2 _leaderboardPosition { get; set; } = new( 43, 206 );
		public Vector4 _leaderboardTint { get; set; } = new( 1, 1, 1, 0.9f );

		public string _sponsorFileName { get; set; } = "nascar-logo.png";
		public Vector2 _sponsorPosition { get; set; } = new( 46, 2 );

		public string _lightBlackFileName { get; set; } = "light-black.png";
		public string _lightGreenFileName { get; set; } = "light-green.png";
		public string _lightWhiteFileName { get; set; } = "light-white.png";
		public string _lightYellowFileName { get; set; } = "light-yellow.png";
		public Vector2 _lightPosition { get; set; } = new( 324, 112 );

		public string _flagCautionFileName { get; set; } = "flag-caution-new.png";
		public string _flagCheckeredFileName { get; set; } = "flag-checkered-new.png";
		public string _flagGreenFileName { get; set; } = "flag-green-new.png";
		public Vector2 _flagPosition { get; set; } = new( 44, 0 );

		public Vector4 _lapColor { get; set; } = new( 188 / 255.0f, 189 / 255.0f, 185 / 255.0f, 1.0f );
		public Vector2 _lapPosition { get; set; } = new( 60, 155 );

		public Vector4 _currentLapColor { get; set; } = new( 188 / 255.0f, 189 / 255.0f, 185 / 255.0f, 1.0f );
		public Vector2 _currentLapPosition { get; set; } = new( 267, 155 );

		public Vector4 _totalLapsColor { get; set; } = new( 188 / 255.0f, 189 / 255.0f, 185 / 255.0f, 1.0f );
		public Vector2 _totalLapsPosition { get; set; } = new( 295, 155 );

		public Vector4 _lapsRemainingColor { get; set; } = new( 245 / 255.0f, 245 / 255.0f, 243 / 255.0f, 1.0f );
		public Vector2 _lapsRemainingPosition { get; set; } = new( 315, 107 );

		public int _placeSpacing { get; set; } = 41;
		public int _placeCount { get; set; } = 20;

		public Vector2 _placePosition { get; set; } = new( 72, 223 );
		public Vector4 _placeColor { get; set; } = new( 176 / 255.0f, 181 / 255.0f, 177 / 255.0f, 1.0f );

		public Vector2 _carNumberPosition { get; set; } = new( 92, 220 );
		public Vector2 _carNumberSize { get; set; } = new( 41, 29 );

		public Vector2 _driverNamePosition { get; set; } = new( 152, 223 );
		public Vector4 _driverNameColor { get; set; } = new( 176 / 255.0f, 181 / 255.0f, 177 / 255.0f, 1.0f );

		public Vector2 _lapsDownPosition { get; set; } = new( 340, 223 );
		public Vector4 _lapsDownColor { get; set; } = new( 176 / 255.0f, 181 / 255.0f, 177 / 255.0f, 1.0f );
		public Vector4 _pitColor { get; set; } = new( 176 / 255.0f, 181 / 255.0f, 15 / 255.0f, 1.0f );
		public Vector4 _outColor { get; set; } = new( 176 / 255.0f, 15 / 255.0f, 15 / 255.0f, 1.0f );

		public string _currentTargetFileName { get; set; } = "current-target.png";
		public Vector2 _currentTargetPosition { get; set; } = new( 43, 208 );
		public Vector2 _currentTargetSpeedOffset { get; set; } = new( 400, 13 );
		public Vector4 _currentTargetSpeedColor { get; set; } = new( 176 / 255.0f, 181 / 255.0f, 177 / 255.0f, 1.0f );

		public string _voiceOfFileName { get; set; } = "voice-of.png";
		public Vector2 _voiceOfStartPosition { get; set; } = new( 1920, 41 );
		public Vector2 _voiceOfFinalPosition { get; set; } = new( 1698, 41 );
		public Vector4 _voiceOfTint { get; set; } = new( 1, 1, 1, 0.9f );
		public Vector2 _voiceOfStringOffset { get; set; } = new( 30, 10 );
		public Vector4 _voiceOfStringColor { get; set; } = new( 176 / 255.0f, 181 / 255.0f, 177 / 255.0f, 1.0f );
		public Vector2 _voiceOfNameOffset { get; set; } = new( 30, 41 );
		public Vector4 _voiceOfNameColor { get; set; } = new( 245 / 255.0f, 245 / 255.0f, 243 / 255.0f, 1.0f );
		public Vector2 _voiceOfNumberOffset { get; set; } = new( 119, 11 );
		public Vector2 _voiceOfNumberSize { get; set; } = new( 41, 29 );
	}
}
