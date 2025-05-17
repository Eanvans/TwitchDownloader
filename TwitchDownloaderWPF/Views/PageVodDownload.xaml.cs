using System;
using System.Diagnostics;
using System.Net;
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
        public PageVodDownload()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void Page_Initialized(object sender, EventArgs e)
        {
            //SetEnabled(false);
            //SetEnabledTrimStart(false);
            //SetEnabledTrimEnd(false);
            WebRequest.DefaultWebProxy = null;
            //numDownloadThreads.Value = Settings.Default.VodDownloadThreads;
            //TextOauth.Text = Settings.Default.OAuth;
            //_ = (VideoTrimMode)Settings.Default.VodTrimMode switch
            //{
            //    VideoTrimMode.Exact => RadioTrimExact.IsChecked = true,
            //    _ => RadioTrimSafe.IsChecked = true,
            //};
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
            //statusImage.Visibility = Settings.Default.ReduceMotion ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            btnDonate.Visibility = Settings.Default.HideDonation ? Visibility.Collapsed : Visibility.Visible;
            //statusImage.Visibility = Settings.Default.ReduceMotion ? Visibility.Collapsed : Visibility.Visible;
        }


        //private void BtnCancel_Click(object sender, RoutedEventArgs e)
        //{
        //    statusMessage.Text = Translations.Strings.StatusCanceling;
        //    //SetImage("Images/ppStretch.gif", true);
        //    try
        //    {
        //        _cancellationTokenSource.Cancel();
        //    }
        //    catch (ObjectDisposedException) { }
        //}

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