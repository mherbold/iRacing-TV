using System;
using System.Numerics;

namespace iRacingTV
{
	[Serializable]
	public class SettingsData
	{
		public int OverlayX = 0;
		public int OverlayY = 0;
		public int OverlayWidth = 1920;
		public int OverlayHeight = 1080;

		public string InsideCameraGroupName = "Roll Bar";
		public string CloseCameraGroupName = "TV1";
		public string MediumCameraGroupName = "TV2";
		public string FarCameraGroupName = "TV3";
		public string BlimpCameraGroupName = "Blimp";
		public string ScenicCameraGroupName = "Scenic";

		public string PreferredCarNumber = string.Empty;
		public bool PreferredCarLockOnHeatEnabled = true;
		public float PreferredCarLockOnHeat = 0.85f;

		public bool SwitchCameraToTalkingDriver = true;
		public bool ShowLaps = false;
		public bool ShowDistance = false;
		public bool ShowTime = true;
		public bool BetweenCars = false;
		public int NumberOfCheckpoints = 100;

		public string FontAFileName = "Assets\\RevolutionGothic_ExtraBold.otf";
		public int FontASize = 33;

		public string FontBFileName = "Assets\\RevolutionGothic_ExtraBold_It.otf";
		public int FontBSize = 33;

		public string FontCFileName = "Assets\\RevolutionGothic_ExtraBold.otf";
		public int FontCSize = 27;

		public string FontDFileName = "Assets\\RevolutionGothic_ExtraBold_It.otf";
		public int FontDSize = 27;

		public string SubtitlesFontFileName = "Assets\\RevolutionGothic_ExtraBold.otf";
		public int SubtitlesFontSize = 65;

		public string PracticeString = "PRACTICE";
		public string QualifyingString = "QUALIFYING";
		public string RaceString = string.Empty;
		public string LapString = "LAP";
		public string TimeString = "TIME";
		public string LapsRemainingString = "TO GO";
		public string DistanceLapPositionString = "L";
		public string DistanceFeetString = "FT";
		public string DistanceMetersString = "M";
		public string PitString = "PIT";
		public string OutString = "OUT";
		public string MphString = "MPH";
		public string KphString = "KPH";
		public string FinalLapString = "FINAL LAP";
		public string VoiceOfString = "VOICE OF";

		public string RaceStatusImageFileName = "Assets\\race-status.png";
		public Vector2 RaceStatusImagePosition = new( 44, 9 );
		public Vector4 RaceStatusImageTint = new( 1, 1, 1, 0.9f );

		public string LeaderboardImageFileName = "Assets\\leaderboard.png";
		public Vector2 LeaderboardImagePosition = new( 44, 244 );
		public Vector4 LeaderboardImageTint = new( 1, 1, 1, 0.9f );

		public Vector2 SeriesImagePosition = new( 51, 16 );
		public Vector2 SeriesImageSize = new( 305, 103 );

		public string LightImageBlackFileName = "Assets\\light-black.png";
		public string LightImageGreenFileName = "Assets\\light-green.png";
		public string LightImageWhiteFileName = "Assets\\light-white.png";
		public string LightImageYellowFileName = "Assets\\light-yellow.png";
		public Vector2 LightImagePosition = new( 324, 140 );

		public string FlagImageCautionFileName = "Assets\\flag-caution-new.png";
		public string FlagImageCheckeredFileName = "Assets\\flag-checkered-new.png";
		public string FlagImageGreenFileName = "Assets\\flag-green-new.png";
		public Vector2 FlagImagePosition = new( 44, 9 );

		public Vector4 ModeTextColor = new( 245 / 255.0f, 245 / 255.0f, 243 / 255.0f, 1.0f );
		public Vector2 ModeTextPosition = new( 62, 134 );

		public Vector4 LapsRemainingTextColor = new( 245 / 255.0f, 245 / 255.0f, 243 / 255.0f, 1.0f );
		public Vector2 LapsRemainingTextPosition = new( 313, 134 );

		public Vector4 TimeRemainingTextColor = new( 245 / 255.0f, 245 / 255.0f, 243 / 255.0f, 1.0f );
		public Vector2 TimeRemainingTextPosition = new( 313, 134 );

		public Vector4 LapTextColor = new( 188 / 255.0f, 189 / 255.0f, 185 / 255.0f, 1.0f );
		public Vector2 LapTextPosition = new( 62, 184 );

		public Vector4 TimeTextColor = new( 188 / 255.0f, 189 / 255.0f, 185 / 255.0f, 1.0f );
		public Vector2 TimeTextPosition = new( 64, 184 );

		public Vector4 CurrentLapTextColor = new( 188 / 255.0f, 189 / 255.0f, 185 / 255.0f, 1.0f );
		public Vector2 CurrentLapTextPosition = new( 342, 184 );

		public Vector4 CurrentTimeTextColor = new( 188 / 255.0f, 189 / 255.0f, 185 / 255.0f, 1.0f );
		public Vector2 CurrentTimeTextPosition = new( 342, 184 );

		public int LeaderboardPlaceSpacing = 41;
		public int LeaderboardPlaceCount = 20;

		public Vector2 PlaceTextPosition = new( 87, 256 );
		public Vector4 PlaceTextColor = new( 176 / 255.0f, 181 / 255.0f, 177 / 255.0f, 1.0f );

		public string CarNumberColorOverrideA = string.Empty;
		public string CarNumberColorOverrideB = string.Empty;
		public string CarNumberColorOverrideC = string.Empty;
		public string CarNumberPatternOverride = string.Empty;
		public string CarNumberSlantOverride = string.Empty;
		public Vector2 CarNumberImagePosition = new( 120, 254 );
		public int CarNumberImageHeight = 28;

		public Vector2 DriverNameTextPosition = new( 152, 256 );
		public Vector4 DriverNameTextColor = new( 176 / 255.0f, 181 / 255.0f, 177 / 255.0f, 1.0f );

		public Vector2 TelemetryTextPosition = new( 342, 256 );
		public Vector4 TelemetryTextColor = new( 176 / 255.0f, 181 / 255.0f, 177 / 255.0f, 1.0f );
		public Vector4 PitTextColor = new( 176 / 255.0f, 181 / 255.0f, 15 / 255.0f, 1.0f );
		public Vector4 OutTextColor = new( 176 / 255.0f, 15 / 255.0f, 15 / 255.0f, 1.0f );

		public string CurrentTargetImageFileName = "Assets\\current-target.png";
		public Vector2 CurrentTargetImagePosition = new( 44, 244 );
		public Vector2 CurrentTargetSpeedTextOffset = new( 402, 12 );
		public Vector4 CurrentTargetSpeedTextColor = new( 176 / 255.0f, 181 / 255.0f, 177 / 255.0f, 1.0f );

		public string VoiceOfImageFileName = "Assets\\voice-of.png";
		public Vector2 VoiceOfImageStartPosition = new( 1920, 41 );
		public Vector2 VoiceOfImageFinalPosition = new( 1484, 41 );
		public Vector4 VoiceOfImageTint = new( 1, 1, 1, 0.9f );
		public Vector2 VoiceOfTextOffset = new( 30, 10 );
		public Vector4 VoiceOfTextColor = new( 35 / 255.0f, 31 / 255.0f, 32 / 255.0f, 1.0f );
		public Vector2 VoiceOfDriverNameOffset = new( 30, 41 );
		public Vector4 VoiceOfDriverNameColor = new( 35 / 255.0f, 31 / 255.0f, 32 / 255.0f, 1.0f );
		public Vector2 VoiceOfCarImageOffset = new( 155, -37 );
		public int VoiceOfCarImageHeight = 160;

		public Vector2 SubtitleCenterPosition = new( 1146, 939 );
		public Vector2 SubtitleBackgroundPadding = new( 20, 5 );
		public uint SubtitleBackgroundColor = 0xDD000000;
		public int SubtitleMaxWidth = 1210;

		public string Username = string.Empty;
		public string Password = string.Empty;

		public Vector2 TrackImagePosition = new( 541, 165 );
		public Vector2 TrackImageSize = new( 1210, 680 );

		public Vector2 TrackLogoImagePosition = new( 1147, 282 );

		public bool EnableIntro = true;
		public double IntroStartTime = 10;
		public double IntroDuration = 100;

		public Vector2 IntroPositionTextOffset = new( 30, 20 );
		public Vector4 IntroPositionTextColor = new( 1, 1, 1, 1 );
		public Vector2 IntroQualifyingTimeTextOffset = new( 30, 50 );
		public Vector4 IntroQualifyingTimeTextColor = new( 176 / 255.0f, 181 / 255.0f, 177 / 255.0f, 1.0f );
		public Vector2 IntroDriverNameTextOffset = new( 30, 80 );
		public Vector4 IntroDriverNameTextColor = new( 35 / 255.0f, 31 / 255.0f, 32 / 255.0f, 1.0f );
	}
}
