
using System;
using System.Collections.Generic;
using System.Linq;

using irsdkSharp;
using irsdkSharp.Enums;
using irsdkSharp.Serialization;
using irsdkSharp.Serialization.Models.Data;
using irsdkSharp.Serialization.Models.Session;

namespace iRacingTV
{
	internal static class IRSDK
	{
		public static readonly IRacingSDK iRacingSdk = new();

		public static bool isConnected = false;
		public static bool wasConnected = false;

		public static int sessionInfoUpdate = -1;
		public static int sessionNum = -1;
		public static bool sessionResetRequested = false;

		public static IRacingSessionModel? session = null;
		public static DataModel? data = null;

		public static NormalizedSession normalizedSession = new();

		public enum CameraGroupEnum
		{
			None,
			Inside,
			Close,
			Medium,
			Far,
			Blimp,
			Scenic,
			NumCameraGroups
		};

		public static int[] cameraGroupNumbers = new int[ (int) CameraGroupEnum.NumCameraGroups ];

		public const int MinimumSendMessageWaitTicks = 30; // half second
		public const int MinimumCameraSwitchWaitTicks = 180; // three seconds
		public const int PostChatCameraSwitchWaitTicks = 60; // one second

		public static int sendMessageWaitTicksRemaining = 0;
		public static int cameraSwitchWaitTicksRemaining = 0;

		public static int currentCameraGroupNumber = 0;
		public static int currentCameraNumber = 0;
		public static int currentCameraCarIdx = 0;

		public static bool cameraSwitchingEnabled = false;
		public static int targetCameraCarIdx = 0;
		public static string targetCameraCarNumber = string.Empty;
		public static int targetCameraGroupNumber = 0;
		public static CameraGroupEnum targetCameraGroup = CameraGroupEnum.None;
		public static string targetCameraReason = string.Empty;

		public static readonly List<Message> messageBuffer = new();

		public static void Update()
		{
			isConnected = iRacingSdk.IsConnected();

			if ( isConnected != wasConnected )
			{
				wasConnected = isConnected;

				MainWindow.instance?.Update();
			}

			if ( isConnected )
			{
				UpdateData();

				SessionFlagsPlayback.Update();
				ChatLogPlayback.Update();

				SendMessages();
			}
			else
			{
				sessionInfoUpdate = -1;
				sessionNum = -1;
				sessionResetRequested = false;

				session = null;
				data = null;

				sendMessageWaitTicksRemaining = 0;
				cameraSwitchWaitTicksRemaining = 0;

				currentCameraGroupNumber = 0;
				currentCameraNumber = 0;
				currentCameraCarIdx = 0;

				cameraSwitchingEnabled = false;
				targetCameraCarIdx = 0;
				targetCameraCarNumber = string.Empty;
				targetCameraGroupNumber = 0;
				targetCameraGroup = CameraGroupEnum.None;
				targetCameraReason = string.Empty;

				messageBuffer.Clear();
			}
		}

		public static void UpdateData()
		{
			data = iRacingSdk.GetSerializedData().Data;

			if ( !IncidentScan.IsRunning() )
			{
				if ( sessionNum != data.SessionNum && ( data.SessionNum != -1 ) )
				{
					LogFile.Write( $"The session number has changed - we are now in session #{data.SessionNum + 1}.\r\n" );

					sessionResetRequested = true;
				}
			}

			if ( ( sessionInfoUpdate != iRacingSdk.Header.SessionInfoUpdate ) || sessionResetRequested )
			{
				if ( sessionResetRequested )
				{
					LogFile.Write( "Session reset has been requested, doing a hard reinitialize.\r\n" );
				}
				else
				{
					LogFile.Write( "Session information has been updated, doing a soft reinitialize.\r\n" );
				}

				sessionInfoUpdate = iRacingSdk.Header.SessionInfoUpdate;
				sessionNum = data.SessionNum;

				session = iRacingSdk.GetSerializedSessionInfo();

				normalizedSession.Initialize( sessionResetRequested );

				UpdateCameraGroupNumbers();

				SessionFlagsPlayback.Initialize();

				ChatLogPlayback.Initialize();

				MainWindow.instance?.Update();

				sessionResetRequested = false;
			}

			normalizedSession.Update();
		}

		public static void UpdateCameraGroupNumbers()
		{
			if ( session == null ) { throw new Exception( "iRacing session data is missing." ); }

			for ( var i = 0; i < cameraGroupNumbers.Length; i++ )
			{
				cameraGroupNumbers[ i ] = 0;
			}

			foreach ( var cameraGroup in session.CameraInfo.Groups )
			{
				if ( cameraGroup.GroupName == Settings.data.InsideCameraGroupName )
				{
					cameraGroupNumbers[ (int) CameraGroupEnum.Inside ] = cameraGroup.GroupNum;
				}
				else if ( cameraGroup.GroupName == Settings.data.CloseCameraGroupName )
				{
					cameraGroupNumbers[ (int) CameraGroupEnum.Close ] = cameraGroup.GroupNum;
				}
				else if ( cameraGroup.GroupName == Settings.data.MediumCameraGroupName )
				{
					cameraGroupNumbers[ (int) CameraGroupEnum.Medium ] = cameraGroup.GroupNum;
				}
				else if ( cameraGroup.GroupName == Settings.data.FarCameraGroupName )
				{
					cameraGroupNumbers[ (int) CameraGroupEnum.Far ] = cameraGroup.GroupNum;
				}
				else if ( cameraGroup.GroupName == Settings.data.BlimpCameraGroupName )
				{
					cameraGroupNumbers[ (int) CameraGroupEnum.Blimp ] = cameraGroup.GroupNum;
				}
				else if ( cameraGroup.GroupName == Settings.data.ScenicCameraGroupName )
				{
					cameraGroupNumbers[ (int) CameraGroupEnum.Scenic ] = cameraGroup.GroupNum;
				}
			}
		}

		public static void AddMessage( BroadcastMessageTypes msg, int var1, int var2, int var3 )
		{
			messageBuffer.Add( new Message( msg, var1, var2, var3 ) );
		}

		public static void SendMessages()
		{
			cameraSwitchWaitTicksRemaining--;

			if ( ( normalizedSession.camGroupNumber != currentCameraGroupNumber ) || ( normalizedSession.camCameraNumber != currentCameraNumber ) || ( normalizedSession.camCarIdx != currentCameraCarIdx ) )
			{
				currentCameraGroupNumber = normalizedSession.camGroupNumber;
				currentCameraNumber = normalizedSession.camCameraNumber;
				currentCameraCarIdx = normalizedSession.camCarIdx;

				cameraSwitchWaitTicksRemaining = MinimumCameraSwitchWaitTicks;
			}

			sendMessageWaitTicksRemaining--;

			if ( sendMessageWaitTicksRemaining <= 0 )
			{
				if ( messageBuffer.Count > 0 )
				{
					var message = messageBuffer.First();

					messageBuffer.RemoveAt( 0 );

					// LogFile.Write( $"Sending message to iRacing: {message.msg}, {message.var1}, {message.var2}, {message.var3}\r\n" );

					iRacingSdk.BroadcastMessage( message.msg, message.var1, message.var2, message.var3 );

					sendMessageWaitTicksRemaining = MinimumSendMessageWaitTicks;
				}
				else
				{
					if ( Director.isEnabled )
					{
						if ( ( cameraSwitchWaitTicksRemaining <= 0 ) && ( ( currentCameraCarIdx != targetCameraCarIdx ) || ( currentCameraGroupNumber != targetCameraGroupNumber ) ) )
						{
							var normalizedCar = normalizedSession.FindNormalizedCarByCarIdx( targetCameraCarIdx );

							var carNumberRaw = normalizedCar?.carNumberRaw ?? 0;

							iRacingSdk.BroadcastMessage( BroadcastMessageTypes.CamSwitchNum, carNumberRaw, targetCameraGroupNumber, 0 );

							// LogFile.Write( $"Sending message to iRacing: {BroadcastMessageTypes.CamSwitchNum}, {carNumberRaw}, {targetCameraGroupNumber}, 0\r\n" );

							sendMessageWaitTicksRemaining = MinimumSendMessageWaitTicks;
							cameraSwitchWaitTicksRemaining = MinimumCameraSwitchWaitTicks;
						}
					}
				}
			}
		}
	}
}
