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

		public bool SwitchCameraToTalkingDriver = true;
		public bool ShowLaps = false;
		public bool ShowDistance = false;
		public bool ShowTime = true;
		public bool BetweenCars = false;
		public int NumberOfCheckpoints = 100;

		public string FontAFileName = "Assets\\RevolutionGothic_ExtraBold.otf";
		public int FontASize = 31;

		public string FontBFileName = "Assets\\RevolutionGothic_ExtraBold_It.otf";
		public int FontBSize = 31;

		public string FontCFileName = "Assets\\RevolutionGothic_ExtraBold.otf";
		public int FontCSize = 25;

		public string FontDFileName = "Assets\\RevolutionGothic_ExtraBold_It.otf";
		public int FontDSize = 25;

		public string SubtitlesFontFileName = "Assets\\RevolutionGothic_ExtraBold.otf";
		public int SubtitlesFontSize = 65;

		public string LapString = "LAP";
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
		public Vector2 RaceStatusImagePosition = new( 44, 0 );
		public Vector4 RaceStatusImageTint = new( 1, 1, 1, 0.9f );

		public string LeaderboardImageFileName = "Assets\\leaderboard.png";
		public Vector2 LeaderboardImagePosition = new( 43, 206 );
		public Vector4 LeaderboardImageTint = new( 1, 1, 1, 0.9f );

		public string SponsorImageFileName = "Assets\\nascar-logo.png";
		public Vector2 SponsorImagePosition = new( 46, 2 );
		public Vector4 SponsorImageTint = new( 1, 1, 1, 1 );

		public string LightImageBlackFileName = "Assets\\light-black.png";
		public string LightImageGreenFileName = "Assets\\light-green.png";
		public string LightImageWhiteFileName = "Assets\\light-white.png";
		public string LightImageYellowFileName = "Assets\\light-yellow.png";
		public Vector2 LightImagePosition = new( 324, 112 );

		public string FlagImageCautionFileName = "Assets\\flag-caution-new.png";
		public string FlagImageCheckeredFileName = "Assets\\flag-checkered-new.png";
		public string FlagImageGreenFileName = "Assets\\flag-green-new.png";
		public Vector2 FlagImagePosition = new( 44, 0 );

		public Vector4 LapTextColor = new( 188 / 255.0f, 189 / 255.0f, 185 / 255.0f, 1.0f );
		public Vector2 LapTextPosition = new( 60, 155 );

		public Vector4 CurrentLapTextColor = new( 188 / 255.0f, 189 / 255.0f, 185 / 255.0f, 1.0f );
		public Vector2 CurrentLapTextPosition = new( 267, 155 );

		public Vector4 TotalLapsTextColor = new( 188 / 255.0f, 189 / 255.0f, 185 / 255.0f, 1.0f );
		public Vector2 TotalLapsTextPosition = new( 295, 155 );

		public Vector4 LapsRemainingTextColor = new( 245 / 255.0f, 245 / 255.0f, 243 / 255.0f, 1.0f );
		public Vector2 LapsRemainingTextPosition = new( 315, 107 );

		public int LeaderboardPlaceSpacing = 41;
		public int LeaderboardPlaceCount = 20;

		public Vector2 PlaceTextPosition = new( 72, 223 );
		public Vector4 PlaceTextColor = new( 176 / 255.0f, 181 / 255.0f, 177 / 255.0f, 1.0f );

		public Vector2 CarNumberImagePosition = new( 112, 218 );
		public int CarNumberImageHeight = 34;

		public Vector2 DriverNameTextPosition = new( 152, 223 );
		public Vector4 DriverNameTextColor = new( 176 / 255.0f, 181 / 255.0f, 177 / 255.0f, 1.0f );

		public Vector2 TelemetryTextPosition = new( 340, 223 );
		public Vector4 TelemetryTextColor = new( 176 / 255.0f, 181 / 255.0f, 177 / 255.0f, 1.0f );
		public Vector4 PitTextColor = new( 176 / 255.0f, 181 / 255.0f, 15 / 255.0f, 1.0f );
		public Vector4 OutTextColor = new( 176 / 255.0f, 15 / 255.0f, 15 / 255.0f, 1.0f );

		public string CurrentTargetImageFileName = "Assets\\current-target.png";
		public Vector2 CurrentTargetImagePosition = new( 43, 208 );
		public Vector2 CurrentTargetSpeedTextOffset = new( 400, 13 );
		public Vector4 CurrentTargetSpeedTextColor = new( 176 / 255.0f, 181 / 255.0f, 177 / 255.0f, 1.0f );

		public string VoiceOfImageFileName = "Assets\\voice-of.png";
		public Vector2 VoiceOfImageStartPosition = new( 1920, 41 );
		public Vector2 VoiceOfImageFinalPosition = new( 1484, 41 );
		public Vector4 VoiceOfImageTint = new( 1, 1, 1, 0.9f );
		public Vector2 VoiceOfTextOffset = new( 30, 10 );
		public Vector4 VoiceOfTextColor = new( 176 / 255.0f, 181 / 255.0f, 177 / 255.0f, 1.0f );
		public Vector2 VoiceOfDriverNameOffset = new( 30, 41 );
		public Vector4 VoiceOfDriverNameColor = new( 245 / 255.0f, 245 / 255.0f, 243 / 255.0f, 1.0f );
		public Vector2 VoiceOfCarImageOffset = new( 155, -37 );
		public int VoiceOfCarImageHeight = 160;

		public Vector2 SubtitleCenterPosition = new( 960, 800 );
		public Vector2 SubtitleBackgroundPadding = new( 20, 5 );
		public uint SubtitleBackgroundColor = 0xDD000000;
		public int SubtitleMaxWidth = 1100;
	}
}
