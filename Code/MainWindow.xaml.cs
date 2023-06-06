
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using Microsoft.WindowsAPICodePack.Dialogs;

using irsdkSharp.Serialization.Enums.Fastest;
using System.Globalization;

namespace iRacingTV
{
	public partial class MainWindow : Window
	{
		public static MainWindow? instance = null;

		public const string StatusDisconnectedImageFileName = "Assets\\status-disconnected.png";
		public const string StatusConnectedImageFileName = "Assets\\status-connected.png";

		public readonly BitmapImage statusDisconnectedBitmapImage = new( new Uri( $"pack://application:,,,/{StatusDisconnectedImageFileName}" ) );
		public readonly BitmapImage statusConnectedBitmapImage = new( new Uri( $"pack://application:,,,/{StatusConnectedImageFileName}" ) );

		public static TextBox[] heatTextBoxes = new TextBox[ 12 ];

		public MainWindow()
		{
			instance = this;

			try
			{
				InitializeComponent();

				heatTextBoxes[ 0 ] = Debug_H1;
				heatTextBoxes[ 1 ] = Debug_H2;
				heatTextBoxes[ 2 ] = Debug_H3;
				heatTextBoxes[ 3 ] = Debug_H4;
				heatTextBoxes[ 4 ] = Debug_H5;
				heatTextBoxes[ 5 ] = Debug_H6;
				heatTextBoxes[ 6 ] = Debug_H7;
				heatTextBoxes[ 7 ] = Debug_H8;
				heatTextBoxes[ 8 ] = Debug_H9;
				heatTextBoxes[ 9 ] = Debug_H10;
				heatTextBoxes[ 10 ] = Debug_H11;
				heatTextBoxes[ 11 ] = Debug_H12;

				Program.Initialize();
			}
			catch ( Exception exception )
			{
				LogFile.WriteException( exception );
			}
		}

		public void Initialize()
		{
			Dispatcher.Invoke( () =>
			{
				// overlay

				OverlayX.Text = Settings.data.OverlayX.ToString();
				OverlayY.Text = Settings.data.OverlayY.ToString();
				OverlayWidth.Text = Settings.data.OverlayWidth.ToString();
				OverlayHeight.Text = Settings.data.OverlayHeight.ToString();

				// leaderboard

				ShowLapsRadioButton.IsChecked = Settings.data.ShowLaps;
				ShowDistanceRadioButton.IsChecked = Settings.data.ShowDistance;
				ShowTimeRadioButton.IsChecked = Settings.data.ShowTime;
				BetweenCarsCheckBox.IsChecked = Settings.data.BetweenCars;
				NumberOfCheckpointsSlider.Value = Settings.data.NumberOfCheckpoints;

				UseClassColorsForDriverNamesCheckBox.IsChecked = Settings.data.UseClassColorsForDriverNames;
				ClassColorStrengthSlider.Value = Settings.data.ClassColorStrength;

				CarNumberImageColorOverrideATextBox.Text = Settings.data.CarNumberColorOverrideA;
				CarNumberImageColorOverrideBTextBox.Text = Settings.data.CarNumberColorOverrideB;
				CarNumberImageColorOverrideCTextBox.Text = Settings.data.CarNumberColorOverrideC;
				CarNumberImagePatternOverrideTextBox.Text = Settings.data.CarNumberPatternOverride;
				CarNumberImageSlantOverrideTextBox.Text = Settings.data.CarNumberSlantOverride;

				// director

				InsideCameraTextBox.Text = Settings.data.InsideCameraGroupName.ToString();
				CloseCameraTextBox.Text = Settings.data.CloseCameraGroupName.ToString();
				MediumCameraTextBox.Text = Settings.data.MediumCameraGroupName.ToString();
				FarCameraTextBox.Text = Settings.data.FarCameraGroupName.ToString();
				BlimpCameraTextBox.Text = Settings.data.BlimpCameraGroupName.ToString();
				ScenicCameraTextBox.Text = Settings.data.ScenicCameraGroupName.ToString();

				CarLengthTextBox.Text = $"{Settings.data.CarLength:0.00}";
				HeatFalloffTextBox.Text = $"{Settings.data.HeatFalloff:0.00}";
				HeatBiasTextBox.Text = $"{Settings.data.HeatBias:0.00}";

				PreferredCarNumberTextBox.Text = Settings.data.PreferredCarNumber;
				PreferredCarLockOnHeatEnabledCheckBox.IsChecked = Settings.data.PreferredCarLockOnHeatEnabled;
				PreferredCarLockOnHeatTextBox.Text = Settings.data.PreferredCarLockOnHeat.ToString();

				SwitchCameraToTalkingDriverCheckBox.IsChecked = Settings.data.SwitchCameraToTalkingDriver;

				// incidents

				IncidentPrerollFramesTextBox.Text = Settings.data.IncidentPrerollFrames.ToString();
				IncidentFramesTextBox.Text = Settings.data.IncidentFrames.ToString();
				IncidentOffsetFramesTextBox.Text = Settings.data.IncidentOffsetFrames.ToString();

				// intro

				EnableIntroCheckBox.IsChecked = Settings.data.EnableIntro;
				IntroStartTimeTextBox.Text = Settings.data.IntroStartTime.ToString();
				IntroDurationTextBox.Text = Settings.data.IntroDuration.ToString();

				// iRacing

				UsernameTextBox.Text = Settings.data.Username;
				PasswordTextBox.Password = Settings.data.Password;
				CustomPaintsDirectoryTextBox.Text = Settings.data.CustomPaintsDirectory;
				MinimumCommandRateTextBox.Text = Settings.data.MinimumCommandRate.ToString();
			} );
		}

		public void Update()
		{
			Dispatcher.Invoke( () =>
			{
				var incidentScanIsRunning = IncidentScan.IsRunning();

				ShowOverlayButton.Content = Overlay.isVisible ? "Hide Overlay" : "Show Overlay";
				ScanForIncidentsButton.Content = incidentScanIsRunning ? "Incident Scan is Running..." : "Scan for Incidents";
				EnableAIDirectorButton.Content = Director.isEnabled ? "Disable AI Director" : "Enable AI Director";

				if ( IRSDK.isConnected )
				{
					StatusImage.Source = statusConnectedBitmapImage;

					ShowOverlayButton.IsEnabled = !incidentScanIsRunning;
					ScanForIncidentsButton.IsEnabled = !incidentScanIsRunning && IRSDK.normalizedSession.isReplay;
					EnableAIDirectorButton.IsEnabled = !incidentScanIsRunning;
					ResetDataButton.IsEnabled = !incidentScanIsRunning;
				}
				else
				{
					StatusImage.Source = statusDisconnectedBitmapImage;

					ShowOverlayButton.IsEnabled = false;
					ScanForIncidentsButton.IsEnabled = false;
					EnableAIDirectorButton.IsEnabled = false;
					ResetDataButton.IsEnabled = false;
				}
			} );
		}

		public void UpdateDebug()
		{
			Dispatcher.Invoke( () =>
			{
				Debug_Frame.Text = IRSDK.normalizedSession.replayFrameNum.ToString();
				Debug_Time.Text = $"{IRSDK.normalizedSession.sessionTime:0.0}";
				Debug_Mode.Text = IRSDK.normalizedSession.sessionName;
				Debug_State.Text = IRSDK.normalizedSession.sessionState.ToString();

				Debug_IS_S.Text = IncidentScan.currentIncidentScanState.ToString();
				Debug_IS_SSFN.Text = IncidentScan.settleStartingFrameNumber.ToString();
				Debug_IS_STFN.Text = IncidentScan.settleTargetFrameNumber.ToString();
				Debug_IS_SLFN.Text = IncidentScan.settleLastFrameNumber.ToString();
				Debug_IS_SLC.Text = IncidentScan.settleLoopCount.ToString();
				Debug_IS_TOTAL.Text = IncidentScan.incidentList.Count.ToString();
			} );
		}

		public void UpdateSessionFlags()
		{
			Dispatcher.Invoke( () =>
			{
				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.Checkered ) != 0 )
				{
					SF_Checkered.Background = Brushes.White;
				}
				else
				{
					SF_Checkered.Background = null;
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.White ) != 0 )
				{
					SF_White.Background = Brushes.White;
				}
				else
				{
					SF_White.Background = null;
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.Green ) != 0 )
				{
					SF_Green.Background = Brushes.Green;
					SF_Green.Foreground = Brushes.White;
				}
				else
				{
					SF_Green.Background = null;
					SF_Green.Foreground = Brushes.Black;
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.Yellow ) != 0 )
				{
					SF_Yellow.Background = Brushes.Yellow;
				}
				else
				{
					SF_Yellow.Background = null;
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.Red ) != 0 )
				{
					SF_Red.Background = Brushes.Red;
					SF_Red.Foreground = Brushes.White;
				}
				else
				{
					SF_Red.Background = null;
					SF_Red.Foreground = Brushes.Black;
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.Blue ) != 0 )
				{
					SF_Blue.Background = Brushes.Blue;
					SF_Blue.Foreground = Brushes.White;
				}
				else
				{
					SF_Blue.Background = null;
					SF_Blue.Foreground = Brushes.Black;
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.Debris ) != 0 )
				{
					SF_Debris.Background = Brushes.Yellow;
				}
				else
				{
					SF_Debris.Background = null;
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.Crossed ) != 0 )
				{
					SF_Crossed.Background = Brushes.White;
				}
				else
				{
					SF_Crossed.Background = null;
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.YellowWaving ) != 0 )
				{
					SF_YellowWaving.Background = Brushes.Yellow;
				}
				else
				{
					SF_YellowWaving.Background = null;
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.OneLapToGreen ) != 0 )
				{
					SF_OneLapToGreen.Background = Brushes.Green;
					SF_OneLapToGreen.Foreground = Brushes.White;
				}
				else
				{
					SF_OneLapToGreen.Background = null;
					SF_OneLapToGreen.Foreground = Brushes.Black;
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.GreenHeld ) != 0 )
				{
					SF_GreenHeld.Background = Brushes.Green;
					SF_GreenHeld.Foreground = Brushes.White;
				}
				else
				{
					SF_GreenHeld.Background = null;
					SF_GreenHeld.Foreground = Brushes.Black;
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.TenToGo ) != 0 )
				{
					SF_TenToGo.Background = Brushes.White;
				}
				else
				{
					SF_TenToGo.Background = null;
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.FiveToGo ) != 0 )
				{
					SF_FiveToGo.Background = Brushes.White;
				}
				else
				{
					SF_FiveToGo.Background = null;
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.RandomWaving ) != 0 )
				{
					SF_RandomWaving.Background = Brushes.White;
				}
				else
				{
					SF_RandomWaving.Background = null;
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.Caution ) != 0 )
				{
					SF_Caution.Background = Brushes.Yellow;
				}
				else
				{
					SF_Caution.Background = null;
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.CautionWaving ) != 0 )
				{
					SF_CautionWaving.Background = Brushes.Yellow;
				}
				else
				{
					SF_CautionWaving.Background = null;
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.Black ) != 0 )
				{
					SF_Black.Background = Brushes.Black;
					SF_Black.Foreground = Brushes.White;
				}
				else
				{
					SF_Black.Background = null;
					SF_Black.Foreground = Brushes.Black;
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.Disqualify ) != 0 )
				{
					SF_Black.Background = Brushes.Black;
					SF_Black.Foreground = Brushes.White;
				}
				else
				{
					SF_Black.Background = null;
					SF_Black.Foreground = Brushes.Black;
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.Servicible ) != 0 )
				{
					SF_Black.Background = Brushes.Black;
					SF_Black.Foreground = Brushes.White;
				}
				else
				{
					SF_Black.Background = null;
					SF_Black.Foreground = Brushes.Black;
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.Furled ) != 0 )
				{
					SF_Black.Background = Brushes.Black;
					SF_Black.Foreground = Brushes.White;
				}
				else
				{
					SF_Black.Background = null;
					SF_Black.Foreground = Brushes.Black;
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.Repair ) != 0 )
				{
					SF_Black.Background = Brushes.Black;
					SF_Black.Foreground = Brushes.White;
				}
				else
				{
					SF_Black.Background = null;
					SF_Black.Foreground = Brushes.Black;
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.StartHidden ) != 0 )
				{
					SF_StartHidden.Background = Brushes.Green;
					SF_StartHidden.Foreground = Brushes.White;
				}
				else
				{
					SF_StartHidden.Background = null;
					SF_StartHidden.Foreground = Brushes.Black;
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.StartReady ) != 0 )
				{
					SF_StartReady.Background = Brushes.Green;
					SF_StartReady.Foreground = Brushes.White;
				}
				else
				{
					SF_StartReady.Background = null;
					SF_StartReady.Foreground = Brushes.Black;
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.StartSet ) != 0 )
				{
					SF_StartSet.Background = Brushes.Green;
					SF_StartSet.Foreground = Brushes.White;
				}
				else
				{
					SF_StartSet.Background = null;
					SF_StartSet.Foreground = Brushes.Black;
				}

				if ( ( IRSDK.normalizedSession.sessionFlags & (uint) SessionFlags.StartGo ) != 0 )
				{
					SF_StartGo.Background = Brushes.Green;
					SF_StartGo.Foreground = Brushes.White;
				}
				else
				{
					SF_StartGo.Background = null;
					SF_StartGo.Foreground = Brushes.Black;
				}
			} );
		}

		public void UpdateTarget()
		{
			Dispatcher.Invoke( () =>
			{
				Debug_TargetCar.Text = IRSDK.targetCameraCarNumber;
				Debug_TargetCameraGroup.Text = IRSDK.targetCameraGroup.ToString();
				Debug_TargetReason.Text = IRSDK.targetCameraReason;
			} );
		}

		public void UpdateHottestCars()
		{
			Dispatcher.Invoke( () =>
			{
				for ( var i = 0; i < 12; i++ )
				{
					var text = string.Empty;

					var normalizedCar = IRSDK.normalizedSession.attackingHeatSortedNormalizedCars[ i ];

					if ( ( normalizedCar.includeInLeaderboard ) && ( normalizedCar.attackingHeat > 0 ) )
					{
						text = $"#{normalizedCar.carNumber}:{normalizedCar.attackingHeat:0.00}";
					}

					heatTextBoxes[ i ].Text = text;
				}
			} );
		}

		public void AddToStatusTextBox( string message )
		{
			Dispatcher.Invoke( () =>
			{
				StatusTextBox.Text += message;
				StatusTextBox.CaretIndex = StatusTextBox.Text.Length;
				StatusTextBox.ScrollToEnd();
			} );
		}

		// status

		private void ResetDataButton_Click( object sender, RoutedEventArgs e )
		{
			IRSDK.sessionResetRequested = true;
		}

		private void ScanForIncidentsButton_Click( object sender, RoutedEventArgs e )
		{
			IncidentScan.Start();

			Update();
		}

		private void ShowOverlayButton_Click( object sender, RoutedEventArgs e )
		{
			Overlay.ToggleVisibility();

			Update();
		}

		private void EnableAIDirectorButton_Click( object sender, RoutedEventArgs e )
		{
			Director.isEnabled = !Director.isEnabled;

			Update();
		}

		// overlay

		private void OverlayX_TextChanged( object sender, TextChangedEventArgs e )
		{
			if ( !int.TryParse( OverlayX.Text, out Settings.data.OverlayX ) )
			{
				Settings.data.OverlayX = 0;
			}
		}

		private void OverlayY_TextChanged( object sender, TextChangedEventArgs e )
		{
			if ( !int.TryParse( OverlayY.Text, out Settings.data.OverlayY ) )
			{
				Settings.data.OverlayY = 0;
			}
		}

		private void OverlayWidth_TextChanged( object sender, TextChangedEventArgs e )
		{
			if ( !int.TryParse( OverlayWidth.Text, out Settings.data.OverlayWidth ) )
			{
				Settings.data.OverlayWidth = 1920;
			}
		}

		private void OverlayHeight_TextChanged( object sender, TextChangedEventArgs e )
		{
			if ( !int.TryParse( OverlayHeight.Text, out Settings.data.OverlayHeight ) )
			{
				Settings.data.OverlayHeight = 1080;
			}
		}

		// leaderboard

		private void UseClassColorsForDriverNamesCheckBox_Click( object sender, RoutedEventArgs e )
		{
			Settings.data.UseClassColorsForDriverNames = UseClassColorsForDriverNamesCheckBox.IsChecked ?? false;
		}

		private void ClassColorStrengthSlider_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e )
		{
			Settings.data.ClassColorStrength = (int) ClassColorStrengthSlider.Value;
		}

		private void ShowLapsRadioButton_Click( object sender, RoutedEventArgs e )
		{
			Settings.data.ShowLaps = true;
			Settings.data.ShowDistance = false;
			Settings.data.ShowTime = false;
		}

		private void ShowDistanceRadioButton_Click( object sender, RoutedEventArgs e )
		{
			Settings.data.ShowLaps = false;
			Settings.data.ShowDistance = true;
			Settings.data.ShowTime = false;
		}

		private void ShowTimeRadioButton_Click( object sender, RoutedEventArgs e )
		{
			Settings.data.ShowLaps = false;
			Settings.data.ShowDistance = false;
			Settings.data.ShowTime = true;
		}

		private void BetweenCarsCheckBox_Click( object sender, RoutedEventArgs e )
		{
			Settings.data.BetweenCars = BetweenCarsCheckBox.IsChecked ?? false;
		}

		private void NumberOfCheckpointsSlider_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e )
		{
			Settings.data.NumberOfCheckpoints = (int) NumberOfCheckpointsSlider.Value;
		}

		private void CarNumberImageColorOverrideA_TextChanged( object sender, TextChangedEventArgs e )
		{
			Settings.data.CarNumberColorOverrideA = CarNumberImageColorOverrideATextBox.Text;
		}

		private void CarNumberImageColorOverrideB_TextChanged( object sender, TextChangedEventArgs e )
		{
			Settings.data.CarNumberColorOverrideB = CarNumberImageColorOverrideBTextBox.Text;
		}

		private void CarNumberImageColorOverrideC_TextChanged( object sender, TextChangedEventArgs e )
		{
			Settings.data.CarNumberColorOverrideC = CarNumberImageColorOverrideCTextBox.Text;
		}

		private void CarNumberImagePatternOverride_TextChanged( object sender, TextChangedEventArgs e )
		{
			Settings.data.CarNumberPatternOverride = CarNumberImagePatternOverrideTextBox.Text;
		}

		private void CarNumberImageSlantOverride_TextChanged( object sender, TextChangedEventArgs e )
		{
			Settings.data.CarNumberSlantOverride = CarNumberImageSlantOverrideTextBox.Text;
		}

		// director

		private void InsideCameraTextBox_TextChanged( object sender, TextChangedEventArgs e )
		{
			Settings.data.InsideCameraGroupName = InsideCameraTextBox.Text;
		}

		private void CloseCameraTextBox_TextChanged( object sender, TextChangedEventArgs e )
		{
			Settings.data.CloseCameraGroupName = CloseCameraTextBox.Text;
		}

		private void MediumCameraTextBox_TextChanged( object sender, TextChangedEventArgs e )
		{
			Settings.data.MediumCameraGroupName = MediumCameraTextBox.Text;
		}

		private void FarCameraTextBox_TextChanged( object sender, TextChangedEventArgs e )
		{
			Settings.data.FarCameraGroupName = FarCameraTextBox.Text;
		}

		private void BlimpCameraTextBox_TextChanged( object sender, TextChangedEventArgs e )
		{
			Settings.data.BlimpCameraGroupName = BlimpCameraTextBox.Text;
		}

		private void ScenicCameraTextBox_TextChanged( object sender, TextChangedEventArgs e )
		{
			Settings.data.ScenicCameraGroupName = ScenicCameraTextBox.Text;
		}

		private void CarLengthTextBoxTextBox_TextChanged( object sender, TextChangedEventArgs e )
		{
			if ( !float.TryParse( CarLengthTextBox.Text, CultureInfo.InvariantCulture.NumberFormat, out Settings.data.CarLength ) )
			{
				Settings.data.CarLength = 4.91f;
			}
		}

		private void HeatFalloffTextBoxTextBox_TextChanged( object sender, TextChangedEventArgs e )
		{
			if ( !float.TryParse( HeatFalloffTextBox.Text, CultureInfo.InvariantCulture.NumberFormat, out Settings.data.HeatFalloff ) )
			{
				Settings.data.HeatFalloff = 10.0f;
			}
		}

		private void HeatBiasTextBoxTextBox_TextChanged( object sender, TextChangedEventArgs e )
		{
			if ( !float.TryParse( HeatBiasTextBox.Text, CultureInfo.InvariantCulture.NumberFormat, out Settings.data.HeatBias ) )
			{
				Settings.data.HeatBias = 0.5f;
			}
		}

		private void PreferredCarNumberTextBox_TextChanged( object sender, TextChangedEventArgs e )
		{
			Settings.data.PreferredCarNumber = PreferredCarNumberTextBox.Text;
		}

		private void PreferredCarLockOnHeatEnabledCheckBox_Click( object sender, RoutedEventArgs e )
		{
			Settings.data.PreferredCarLockOnHeatEnabled = PreferredCarLockOnHeatEnabledCheckBox.IsChecked ?? false;
		}

		private void PreferredCarLockOnHeatTextBox_TextChanged( object sender, TextChangedEventArgs e )
		{
			if ( !float.TryParse( PreferredCarLockOnHeatTextBox.Text, CultureInfo.InvariantCulture.NumberFormat, out Settings.data.PreferredCarLockOnHeat ) )
			{
				Settings.data.PreferredCarLockOnHeat = 0.85f;
			}
		}

		private void SwitchCameraToTalkingDriverCheckBox_Click( object sender, RoutedEventArgs e )
		{
			Settings.data.SwitchCameraToTalkingDriver = SwitchCameraToTalkingDriverCheckBox.IsChecked ?? false;
		}

		// incidents

		private void IncidentPrerollFramesTextBox_TextChanged( object sender, TextChangedEventArgs e )
		{
			if ( !int.TryParse( IncidentPrerollFramesTextBox.Text, out Settings.data.IncidentPrerollFrames ) )
			{
				Settings.data.IncidentPrerollFrames = 30;
			}
		}

		private void IncidentFramesTextBox_TextChanged( object sender, TextChangedEventArgs e )
		{
			if ( !int.TryParse( IncidentFramesTextBox.Text, out Settings.data.IncidentFrames ) )
			{
				Settings.data.IncidentFrames = 240;
			}
		}

		private void IncidentOffsetFramesTextBox_TextChanged( object sender, TextChangedEventArgs e )
		{
			if ( !int.TryParse( IncidentOffsetFramesTextBox.Text, out Settings.data.IncidentOffsetFrames ) )
			{
				Settings.data.IncidentOffsetFrames = 90;
			}
		}

		// intro

		private void EnableIntroCheckBox_Click( object sender, RoutedEventArgs e )
		{
			Settings.data.EnableIntro = EnableIntroCheckBox.IsChecked ?? false;
		}

		private void IntroStartTimeTextBox_TextChanged( object sender, TextChangedEventArgs e )
		{
			if ( !double.TryParse( IntroStartTimeTextBox.Text, CultureInfo.InvariantCulture.NumberFormat, out Settings.data.IntroStartTime ) )
			{
				Settings.data.IntroStartTime = 35;
			}
		}

		private void IntroDurationTextBox_TextChanged( object sender, TextChangedEventArgs e )
		{
			if ( !double.TryParse( IntroDurationTextBox.Text, CultureInfo.InvariantCulture.NumberFormat, out Settings.data.IntroDuration ) )
			{
				Settings.data.IntroDuration = 80;
			}
		}

		// iRacing

		private void UsernameTextBox_TextChanged( object sender, TextChangedEventArgs e )
		{
			Settings.data.Username = UsernameTextBox.Text;
		}

		private void PasswordTextBox_PasswordChanged( object sender, RoutedEventArgs e )
		{
			Settings.data.Password = PasswordTextBox.Password;
		}

		private void CustomPaintsDirectoryTextBox_TextChanged( object sender, TextChangedEventArgs e )
		{
			Settings.data.CustomPaintsDirectory = CustomPaintsDirectoryTextBox.Text;
		}

		private void CustomPaintsDirectory_Click( object sender, RoutedEventArgs e )
		{
			var commonOpenFileDialog = new CommonOpenFileDialog
			{
				Title = "Choose the iRacing Custom Paints Folder",
				IsFolderPicker = true,
				InitialDirectory = Settings.data.CustomPaintsDirectory,
				AddToMostRecentlyUsedList = false,
				AllowNonFileSystemItems = false,
				DefaultDirectory = Settings.data.CustomPaintsDirectory,
				EnsureFileExists = true,
				EnsurePathExists = true,
				EnsureReadOnly = false,
				EnsureValidNames = true,
				Multiselect = false,
				ShowPlacesList = true
			};

			if ( commonOpenFileDialog.ShowDialog() == CommonFileDialogResult.Ok )
			{
				Settings.data.CustomPaintsDirectory = commonOpenFileDialog.FileName;

				CustomPaintsDirectoryTextBox.Text = Settings.data.CustomPaintsDirectory;
			}
		}

		private void MinimumCommandRateTextBox_TextChanged( object sender, TextChangedEventArgs e )
		{
			if ( !int.TryParse( MinimumCommandRateTextBox.Text, out Settings.data.MinimumCommandRate ) )
			{
				Settings.data.MinimumCommandRate = 30;
			}
		}

		// save and apply changes

		private void ApplyChangesButton_Click( object sender, RoutedEventArgs e )
		{
			MainWindowTabControl.SelectedIndex = 0;

			Settings.Save();

			Program.applySettingChangesRequested = true;
		}

		// window closed

		private void Window_Closed( object sender, EventArgs e )
		{
			Program.keepRunning = false;
		}
	}
}
