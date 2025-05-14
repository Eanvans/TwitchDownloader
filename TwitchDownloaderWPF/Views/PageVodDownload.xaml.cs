using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using TwitchDownloaderCore.Tools;
using TwitchDownloaderWPF.Properties;

namespace TwitchDownloaderWPF
{
    /// <summary>
    /// Interaction logic for PageVodDownload.xaml
    /// </summary>
    public partial class PageVodDownload : Page
    {

        public long currentVideoId;
        public DateTime currentVideoTime;
        public TimeSpan vodLength;
        public int viewCount;
        public string game;
        public string streamerId;
        private CancellationTokenSource _cancellationTokenSource;

        public PageVodDownload()
        {
            InitializeComponent();
        }

        private void SetEnabled(bool isEnabled)
        {
            //comboQuality.IsEnabled = isEnabled;
            checkStart.IsEnabled = isEnabled;
            checkEnd.IsEnabled = isEnabled;
            SplitBtnDownload.IsEnabled = isEnabled;
            MenuItemEnqueue.IsEnabled = isEnabled;
            RadioTrimSafe.IsEnabled = isEnabled;
            RadioTrimExact.IsEnabled = isEnabled;
            SetEnabledTrimStart(isEnabled & checkStart.IsChecked.GetValueOrDefault());
            SetEnabledTrimEnd(isEnabled & checkEnd.IsChecked.GetValueOrDefault());
        }

        private void SetEnabledTrimStart(bool isEnabled)
        {
            numStartHour.IsEnabled = isEnabled;
            numStartMinute.IsEnabled = isEnabled;
            numStartSecond.IsEnabled = isEnabled;
        }

        private void SetEnabledTrimEnd(bool isEnabled)
        {
            numEndHour.IsEnabled = isEnabled;
            numEndMinute.IsEnabled = isEnabled;
            numEndSecond.IsEnabled = isEnabled;
        }


        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void UpdateActionButtons(bool isDownloading)
        {
            if (isDownloading)
            {
                SplitBtnDownload.Visibility = Visibility.Collapsed;
                BtnCancel.Visibility = Visibility.Visible;
                return;
            }
            SplitBtnDownload.Visibility = Visibility.Visible;
            BtnCancel.Visibility = Visibility.Collapsed;
        }

        private void Page_Initialized(object sender, EventArgs e)
        {
            //SetEnabled(false);
            //SetEnabledTrimStart(false);
            //SetEnabledTrimEnd(false);
            WebRequest.DefaultWebProxy = null;
            //numDownloadThreads.Value = Settings.Default.VodDownloadThreads;
            TextOauth.Text = Settings.Default.OAuth;
            _ = (VideoTrimMode)Settings.Default.VodTrimMode switch
            {
                VideoTrimMode.Exact => RadioTrimExact.IsChecked = true,
                _ => RadioTrimSafe.IsChecked = true,
            };
        }

        private void numDownloadThreads_ValueChanged(object sender, HandyControl.Data.FunctionEventArgs<double> e)
        {
            //if (this.IsInitialized && numDownloadThreads.IsEnabled)
            //{
            //    Settings.Default.VodDownloadThreads = (int)numDownloadThreads.Value;
            //    Settings.Default.Save();
            //}
        }

        private void TextOauth_TextChanged(object sender, RoutedEventArgs e)
        {
            if (this.IsInitialized)
            {
                Settings.Default.OAuth = TextOauth.Text;
                Settings.Default.Save();
            }
        }

        private void btnDonate_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://www.buymeacoffee.com/lay295") { UseShellExecute = true });
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            var settings = new WindowSettings
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            settings.ShowDialog();
            btnDonate.Visibility = Settings.Default.HideDonation ? Visibility.Collapsed : Visibility.Visible;
            statusImage.Visibility = Settings.Default.ReduceMotion ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            btnDonate.Visibility = Settings.Default.HideDonation ? Visibility.Collapsed : Visibility.Visible;
            statusImage.Visibility = Settings.Default.ReduceMotion ? Visibility.Collapsed : Visibility.Visible;
        }

        private void checkStart_OnCheckStateChanged(object sender, RoutedEventArgs e)
        {
            SetEnabledTrimStart(checkStart.IsChecked.GetValueOrDefault());

            //UpdateVideoSizeEstimates();
        }

        private void checkEnd_OnCheckStateChanged(object sender, RoutedEventArgs e)
        {
            SetEnabledTrimEnd(checkEnd.IsChecked.GetValueOrDefault());

            //UpdateVideoSizeEstimates();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            statusMessage.Text = Translations.Strings.StatusCanceling;
            //SetImage("Images/ppStretch.gif", true);
            try
            {
                _cancellationTokenSource.Cancel();
            }
            catch (ObjectDisposedException) { }
        }

        private void MenuItemEnqueue_Click(object sender, RoutedEventArgs e)
        {

        }

        private void numEndHour_ValueChanged(object sender, HandyControl.Data.FunctionEventArgs<double> e)
        {
            //UpdateVideoSizeEstimates();
        }

        private void numEndMinute_ValueChanged(object sender, HandyControl.Data.FunctionEventArgs<double> e)
        {
            //UpdateVideoSizeEstimates();
        }

        private void numEndSecond_ValueChanged(object sender, HandyControl.Data.FunctionEventArgs<double> e)
        {
            //UpdateVideoSizeEstimates();
        }

        private void numStartHour_ValueChanged(object sender, HandyControl.Data.FunctionEventArgs<double> e)
        {
            //UpdateVideoSizeEstimates();
        }

        private void numStartMinute_ValueChanged(object sender, HandyControl.Data.FunctionEventArgs<double> e)
        {
            //UpdateVideoSizeEstimates();
        }

        private void numStartSecond_ValueChanged(object sender, HandyControl.Data.FunctionEventArgs<double> e)
        {
            //UpdateVideoSizeEstimates();
        }

        private async void TextUrl_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //await GetVideoInfo();
                e.Handled = true;
            }
        }

        private void RadioTrimSafe_OnCheckedStateChanged(object sender, RoutedEventArgs e)
        {
            if (IsInitialized)
            {
                Settings.Default.VodTrimMode = (int)VideoTrimMode.Safe;
                Settings.Default.Save();
            }
        }

        private void RadioTrimExact_OnCheckedStateChanged(object sender, RoutedEventArgs e)
        {
            if (IsInitialized)
            {
                Settings.Default.VodTrimMode = (int)VideoTrimMode.Exact;
                Settings.Default.Save();
            }
        }

    }
}