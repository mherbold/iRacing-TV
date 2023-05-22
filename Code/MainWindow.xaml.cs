
using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace iRacingTV
{
	public partial class MainWindow : Window
	{
		public static MainWindow? instance = null;

		public const string StatusDisconnectedImageFileName = "Assets\\status-disconnected.png";
		public const string StatusConnectedImageFileName = "Assets\\status-connected.png";

		public readonly BitmapImage statusDisconnectedBitmapImage = new( new Uri( $"pack://application:,,,/{StatusDisconnectedImageFileName}" ) );
		public readonly BitmapImage statusConnectedBitmapImage = new( new Uri( $"pack://application:,,,/{StatusConnectedImageFileName}" ) );

		public MainWindow()
		{
			instance = this;

			try
			{
				InitializeComponent();

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
				OverlayX.Text = Settings.data.OverlayX.ToString();
				OverlayY.Text = Settings.data.OverlayY.ToString();
				OverlayWidth.Text = Settings.data.OverlayWidth.ToString();
				OverlayHeight.Text = Settings.data.OverlayHeight.ToString();

				InsideCameraTextBox.Text = Settings.data.InsideCameraGroupName.ToString();
				CloseCameraTextBox.Text = Settings.data.CloseCameraGroupName.ToString();
				MediumCameraTextBox.Text = Settings.data.MediumCameraGroupName.ToString();
				FarCameraTextBox.Text = Settings.data.FarCameraGroupName.ToString();
				BlimpCameraTextBox.Text = Settings.data.BlimpCameraGroupName.ToString();
				ScenicCameraTextBox.Text = Settings.data.ScenicCameraGroupName.ToString();

				PreferredCarNumber.Text = Settings.data.PreferredCarNumber;

				SwitchCameraToTalkingDriverCheckBox.IsChecked = Settings.data.SwitchCameraToTalkingDriver;

				ShowLapsRadioButton.IsChecked = Settings.data.ShowLaps;
				ShowDistanceRadioButton.IsChecked = Settings.data.ShowDistance;
				ShowTimeRadioButton.IsChecked = Settings.data.ShowTime;
				BetweenCarsCheckBox.IsChecked = Settings.data.BetweenCars;
				NumberOfCheckpointsSlider.Value = Settings.data.NumberOfCheckpoints;

				CarNumberImageColorOverrideATextBox.Text = Settings.data.CarNumberColorOverrideA;
				CarNumberImageColorOverrideBTextBox.Text = Settings.data.CarNumberColorOverrideB;
				CarNumberImageColorOverrideCTextBox.Text = Settings.data.CarNumberColorOverrideC;
				CarNumberImagePatternOverrideTextBox.Text = Settings.data.CarNumberPatternOverride;
				CarNumberImageSlantOverrideTextBox.Text = Settings.data.CarNumberSlantOverride;
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

		public void AddToStatusTextBox( string message )
		{
			Dispatcher.Invoke( () =>
			{
				StatusTextBox.Text += message;
				StatusTextBox.CaretIndex = StatusTextBox.Text.Length;
				StatusTextBox.ScrollToEnd();
			} );
		}

		private void ResetDataButton_Click( object sender, RoutedEventArgs e )
		{
			IRSDK.forceResetRace = true;
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

		private void OverlayX_TextChanged( object sender, System.Windows.Controls.TextChangedEventArgs e )
		{
			Settings.data.OverlayX = int.Parse( OverlayX.Text );
		}

		private void OverlayY_TextChanged( object sender, System.Windows.Controls.TextChangedEventArgs e )
		{
			Settings.data.OverlayY = int.Parse( OverlayY.Text );
		}

		private void OverlayWidth_TextChanged( object sender, System.Windows.Controls.TextChangedEventArgs e )
		{
			Settings.data.OverlayWidth = int.Parse( OverlayWidth.Text );
		}

		private void OverlayHeight_TextChanged( object sender, System.Windows.Controls.TextChangedEventArgs e )
		{
			Settings.data.OverlayHeight = int.Parse( OverlayHeight.Text );
		}

		private void InsideCameraTextBox_TextChanged( object sender, System.Windows.Controls.TextChangedEventArgs e )
		{
			Settings.data.InsideCameraGroupName = InsideCameraTextBox.Text;
		}

		private void CloseCameraTextBox_TextChanged( object sender, System.Windows.Controls.TextChangedEventArgs e )
		{
			Settings.data.CloseCameraGroupName = CloseCameraTextBox.Text;
		}

		private void MediumCameraTextBox_TextChanged( object sender, System.Windows.Controls.TextChangedEventArgs e )
		{
			Settings.data.MediumCameraGroupName = MediumCameraTextBox.Text;
		}

		private void FarCameraTextBox_TextChanged( object sender, System.Windows.Controls.TextChangedEventArgs e )
		{
			Settings.data.FarCameraGroupName = FarCameraTextBox.Text;
		}

		private void BlimpCameraTextBox_TextChanged( object sender, System.Windows.Controls.TextChangedEventArgs e )
		{
			Settings.data.BlimpCameraGroupName = BlimpCameraTextBox.Text;
		}

		private void ScenicCameraTextBox_TextChanged( object sender, System.Windows.Controls.TextChangedEventArgs e )
		{
			Settings.data.ScenicCameraGroupName = ScenicCameraTextBox.Text;
		}

		private void PreferredCarNumber_TextChanged( object sender, System.Windows.Controls.TextChangedEventArgs e )
		{
			Settings.data.PreferredCarNumber = PreferredCarNumber.Text;
		}

		private void SwitchCameraToTalkingDriverCheckBox_Click( object sender, RoutedEventArgs e )
		{
			Settings.data.SwitchCameraToTalkingDriver = SwitchCameraToTalkingDriverCheckBox.IsChecked ?? false;
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

		private void CarNumberImageColorOverrideA_TextChanged( object sender, System.Windows.Controls.TextChangedEventArgs e )
		{
			Settings.data.CarNumberColorOverrideA = CarNumberImageColorOverrideATextBox.Text;
		}

		private void CarNumberImageColorOverrideB_TextChanged( object sender, System.Windows.Controls.TextChangedEventArgs e )
		{
			Settings.data.CarNumberColorOverrideB = CarNumberImageColorOverrideBTextBox.Text;
		}

		private void CarNumberImageColorOverrideC_TextChanged( object sender, System.Windows.Controls.TextChangedEventArgs e )
		{
			Settings.data.CarNumberColorOverrideC = CarNumberImageColorOverrideCTextBox.Text;
		}

		private void CarNumberImagePatternOverride_TextChanged( object sender, System.Windows.Controls.TextChangedEventArgs e )
		{
			Settings.data.CarNumberPatternOverride = CarNumberImagePatternOverrideTextBox.Text;
		}

		private void CarNumberImageSlantOverride_TextChanged( object sender, System.Windows.Controls.TextChangedEventArgs e )
		{
			Settings.data.CarNumberSlantOverride = CarNumberImageSlantOverrideTextBox.Text;
		}

		private void ApplyChangesButton_Click( object sender, RoutedEventArgs e )
		{
			MainWindowTabControl.SelectedIndex = 0;

			Settings.Save();

			Program.forceReinitialize = true;
		}

		private void Window_Closed( object sender, EventArgs e )
		{
			Program.keepRunning = false;
		}
	}
}
