using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TwitchDownloaderCore;
using TwitchDownloaderCore.Extensions;
using TwitchDownloaderCore.Models;
using TwitchDownloaderCore.Options;
using TwitchDownloaderCore.Services;
using TwitchDownloaderCore.Tools;
using TwitchDownloaderCore.TwitchObjects.Gql;
using TwitchDownloaderWPF.Enums;
using TwitchDownloaderWPF.Models;
using TwitchDownloaderWPF.Properties;
using TwitchDownloaderWPF.Services;
using TwitchDownloaderWPF.Utils;

namespace TwitchDownloaderWPF.Views.ViewModels
{
    public class VodDownloadVM : ObservableObject
    {
        private bool _idle = true;
        private bool _IsAnalyzeComments = false;
        private ObservableCollection<VodCommentData> _vodCommentsData = new();
        private string _linkUrl = "";
        private double _statusProgressBarValue = 0.0d;
        private string _statusMessage = "";
        private string _logs = "";
        private string _oathText = "";
        private BitmapImage _imgThumb = null;
        private ObservableCollection<string> _comboQuality = new();
        private int _comboQualityIndex = 0;
        private string _textStreamer = "";
        private string _textTitle = "";
        private string _textCreatedAt = "";
        private bool _isCheckStart = false;
        private bool _isCheckEnd = false;
        private int _numDownloadThreads = 6;
        private double _numStartHour = 0;
        private double _numEndHour = 0;
        private double _numStartMinute = 0;
        private double _numEndMinute = 0;
        private double _numStartSecond = 0;
        private double _numEndSecond = 0;
        private TimeSpan vodLength;
        private string _vodlengthStr;

        public readonly Dictionary<string, (string url, int bandwidth)> videoQualities = new();
        public long currentVideoId;
        public DateTime currentVideoTime;
        public int viewCount;
        public string game;
        public string streamerId;
        private CancellationTokenSource _cancellationTokenSource;

        public VodDownloadVM()
        {
        }

        public ICommand OnGetVideoInfo => new RelayCommand(GetVideoInfoClick, () => _idle);
        public ICommand OnDownLoad => new RelayCommand(DownloadClick);
        public ICommand OnEnqueueDownload => new RelayCommand(EnququeDownload);


        public ObservableCollection<VodCommentData> VodCommentsData { get => _vodCommentsData; set => SetProperty(ref _vodCommentsData, value); }
        public bool IsAnalyzeComments { get => _IsAnalyzeComments; set => SetProperty(ref _IsAnalyzeComments, value); }
        public string LinkUrl { get => _linkUrl; set => SetProperty(ref _linkUrl, value); }
        public double StatusProgressBarValue { get => _statusProgressBarValue; set => SetProperty(ref _statusProgressBarValue, value); }
        public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }
        public string Logs { get => _logs; set => SetProperty(ref _logs, value); }
        public string OathText { get => _oathText; set => SetProperty(ref _oathText, value); }
        public BitmapImage ImgThumb { get => _imgThumb; set => SetProperty(ref _imgThumb, value); }
        public ObservableCollection<string> ComboQuality { get => _comboQuality; set => SetProperty(ref _comboQuality, value); }
        public int ComboQualityIndex { get => _comboQualityIndex; set => SetProperty(ref _comboQualityIndex, value); }
        public string TextStreamer { get => _textStreamer; set => SetProperty(ref _textStreamer, value); }
        public string TextTitle { get => _textTitle; set => SetProperty(ref _textTitle, value); }
        public string TextCreatedAt { get => _textCreatedAt; set => SetProperty(ref _textCreatedAt, value); }
        public bool IsCheckStart { get => _isCheckStart; set => SetProperty(ref _isCheckStart, value); }
        public bool IsCheckEnd { get => _isCheckEnd; set => SetProperty(ref _isCheckEnd, value); }
        public int NumDownloadThreads { get => _numDownloadThreads; set => SetProperty(ref _numDownloadThreads, value); }
        public double NumStartHour { get => _numStartHour; set => SetProperty(ref _numStartHour, value); }
        public double NumEndHour { get => _numEndHour; set => SetProperty(ref _numEndHour, value); }
        public double NumStartMinute { get => _numStartMinute; set => SetProperty(ref _numStartMinute, value); }
        public double NumEndMinute { get => _numEndMinute; set => SetProperty(ref _numEndMinute, value); }
        public double NumStartSecond { get => _numStartSecond; set => SetProperty(ref _numStartSecond, value); }
        public double NumEndSecond { get => _numEndSecond; set => SetProperty(ref _numEndSecond, value); }
        public TimeSpan VodLength
        {
            get => vodLength;
            private set
            {
                vodLength = value;
                VodLengthStr = value.ToString();
            }
        }
        public string VodLengthStr { get => _vodlengthStr; set => SetProperty(ref _vodlengthStr, value); }

        protected TimeSpan StartTime => new TimeSpan((int)NumStartHour, (int)NumStartMinute, (int)NumStartSecond);
        protected TimeSpan EndTime => new TimeSpan((int)NumEndHour, (int)NumEndMinute, (int)NumEndSecond);


        private async Task SelectChatInfo()
        {
            // file not been saved
            ChatDownloadOptions downloadOptions = GetChatOptions("");

            var downloadProgress = new WpfTaskProgress((LogLevel)Settings.Default.LogLevels, SetPercent, SetStatus, AppendLog);
            var currentDownload = new ChatDownloader(downloadOptions, downloadProgress);

            CancellationTokenSource c = new CancellationTokenSource();

            try
            {
                var chatRoot = await currentDownload.DownloadChat(c.Token);

                var rst = DataAnalyzeService.FindHotCommentsTimeline(chatRoot);

                VodCommentsData = new(rst);

                downloadProgress.SetStatus(Translations.Strings.StatusDone);
            }
            catch (Exception ex) when (ex is OperationCanceledException or TaskCanceledException && c.IsCancellationRequested)
            {
                downloadProgress.SetStatus(Translations.Strings.StatusCanceled);
            }
            catch (Exception ex)
            {
                downloadProgress.SetStatus(Translations.Strings.StatusError);
            }
            c.Dispose();
        }

        private async Task GetVideoInfo()
        {
            long videoId = CommUtils.ValidateUrl(LinkUrl.Trim());
            if (videoId <= 0)
            {
                System.Windows.MessageBox.Show(System.Windows.Application.Current.MainWindow!,
                    Translations.Strings.InvalidVideoLinkIdMessage.Replace(@"\n", Environment.NewLine),
                    Translations.Strings.InvalidVideoLinkId, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            currentVideoId = videoId;
            try
            {
                Task<GqlVideoResponse> taskVideoInfo = TwitchHelper.GetVideoInfo(videoId);
                Task<GqlVideoTokenResponse> taskAccessToken = TwitchHelper.GetVideoToken(videoId, OathText);
                await Task.WhenAll(taskVideoInfo, taskAccessToken);

                if (taskAccessToken.Result.data.videoPlaybackAccessToken is null)
                {
                    throw new NullReferenceException("Invalid VOD, deleted/expired VOD possibly?");
                }

                var thumbUrl = taskVideoInfo.Result.data.video.thumbnailURLs.FirstOrDefault();
                if (!ThumbnailService.TryGetThumb(thumbUrl, out var image))
                {
                    AppendLog(Translations.Strings.ErrorLog + Translations.Strings.UnableToFindThumbnail);
                    _ = ThumbnailService.TryGetThumb(ThumbnailService.THUMBNAIL_MISSING_URL, out image);
                }
                ImgThumb = image;

                ComboQuality.Clear();
                videoQualities.Clear();

                var playlistString = await TwitchHelper.GetVideoPlaylist(videoId, taskAccessToken.Result.data.videoPlaybackAccessToken.value, taskAccessToken.Result.data.videoPlaybackAccessToken.signature);
                if (playlistString.Contains("vod_manifest_restricted") || playlistString.Contains("unauthorized_entitlements"))
                {
                    throw new NullReferenceException(Translations.Strings.InsufficientAccessMayNeedOauth);
                }

                M3U8 videoPlaylist = M3U8.Parse(playlistString);
                videoPlaylist.SortStreamsByQuality();

                //Add video qualities to combo quality
                foreach (var stream in videoPlaylist.Streams)
                {
                    var userFriendlyName = stream.GetResolutionFramerateString();
                    if (!videoQualities.ContainsKey(userFriendlyName))
                    {
                        videoQualities.Add(userFriendlyName, (stream.Path, stream.StreamInfo.Bandwidth));
                        ComboQuality.Add(userFriendlyName);
                    }
                }
                ComboQualityIndex = 0;

                VodLength = TimeSpan.FromSeconds(taskVideoInfo.Result.data.video.lengthSeconds);
                TextStreamer = taskVideoInfo.Result.data.video.owner?.displayName ?? Translations.Strings.UnknownUser;
                streamerId = taskVideoInfo.Result.data.video.owner?.id;
                TextTitle = taskVideoInfo.Result.data.video.title;
                var videoCreatedAt = taskVideoInfo.Result.data.video.createdAt;
                TextCreatedAt = Settings.Default.UTCVideoTime ? videoCreatedAt.ToString(CultureInfo.CurrentCulture) : videoCreatedAt.ToLocalTime().ToString(CultureInfo.CurrentCulture);
                currentVideoTime = Settings.Default.UTCVideoTime ? videoCreatedAt : videoCreatedAt.ToLocalTime();
                var urlTimeCodeMatch = TwitchRegex.UrlTimeCode.Match(LinkUrl);
                if (urlTimeCodeMatch.Success)
                {
                    var time = UrlTimeCode.Parse(urlTimeCodeMatch.ValueSpan);
                    IsCheckStart = true;
                    NumStartHour = time.Hours;
                    NumStartMinute = time.Minutes;
                    NumStartSecond = time.Seconds;
                }
                else
                {
                    NumStartHour = 0;
                    NumStartMinute = 0;
                    NumStartSecond = 0;
                }

                // set the maximum
                //if (vodLength > TimeSpan.Zero)
                //{
                //    NumStartHour.Maximum = (int)vodLength.TotalHours;
                //    numEndHour.Maximum = (int)vodLength.TotalHours;
                //}
                //else
                //{
                //    numStartHour.Maximum = 48;
                //    numEndHour.Maximum = 48;
                //}

                NumEndHour = (int)VodLength.TotalHours;
                NumEndMinute = VodLength.Minutes;
                NumEndSecond = VodLength.Seconds;
                //labelLength = vodLength.ToString("c");
                viewCount = taskVideoInfo.Result.data.video.viewCount;
                game = taskVideoInfo.Result.data.video.game?.displayName ?? Translations.Strings.UnknownGame;

                UpdateVideoSizeEstimates();

                //SetEnabled(true);
            }
            catch (Exception ex)
            {
                //btnGetInfo.IsEnabled = true;
                AppendLog(Translations.Strings.ErrorLog + ex.Message);
                MessageBox.Show(Application.Current.MainWindow!, Translations.Strings.UnableToGetVideoInfo, Translations.Strings.UnableToGetInfo, MessageBoxButton.OK, MessageBoxImage.Error);
                if (Settings.Default.VerboseErrors)
                {
                    MessageBox.Show(Application.Current.MainWindow!, ex.ToString(),
                        Translations.Strings.VerboseErrorOutput,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public ChatDownloadOptions GetChatOptions(string filename)
        {
            ChatDownloadOptions options = new ChatDownloadOptions();

            options.DownloadFormat = ChatFormat.Json;

            // TODO: Support non-json chat compression
            options.Compression = ChatCompression.None;

            if (IsCheckStart)
            {
                options.TrimBeginning = true;
                TimeSpan start = StartTime;
                options.TrimBeginningTime = (int)start.TotalSeconds;
            }

            if (IsCheckEnd)
            {
                options.TrimEnding = true;
                TimeSpan end = EndTime;
                options.TrimEndingTime = (int)end.TotalSeconds;
            }

            options.TimeFormat = TimestampFormat.Utc;

            options.Id = currentVideoId.ToString();
            options.Filename = filename;
            options.DownloadThreads = 8;
            return options;
        }

        public VideoDownloadOptions GetOptions(string filename, string folder)
        {
            VideoDownloadOptions options = new VideoDownloadOptions
            {
                DownloadThreads = NumDownloadThreads,
                ThrottleKib = Settings.Default.DownloadThrottleEnabled
                    ? Settings.Default.MaximumBandwidthKib
                    : -1,
                Filename = filename ?? Path.Combine(folder, FilenameService.GetFilename(Settings.Default.TemplateVod, TextTitle, currentVideoId.ToString(),
                    currentVideoTime, TextStreamer, streamerId,
                    IsCheckStart ? StartTime : TimeSpan.Zero,
                    IsCheckEnd ? EndTime : VodLength,
                    VodLength, viewCount, game) + FilenameService.GuessVodFileExtension(ComboQuality[ComboQualityIndex])),
                Oauth = OathText,
                Quality = GetQualityWithoutSize(ComboQuality[ComboQualityIndex]),
                Id = currentVideoId,
                TrimBeginning = IsCheckStart,
                TrimBeginningTime = StartTime,
                TrimEnding = IsCheckEnd,
                TrimEndingTime = EndTime,
                FfmpegPath = "ffmpeg",
                TempFolder = Settings.Default.TempPath
            };

            //if (RadioTrimSafe.IsChecked == true)
            //    options.TrimMode = VideoTrimMode.Safe;
            //else if (RadioTrimExact.IsChecked == true)
            //    options.TrimMode = VideoTrimMode.Exact;

            return options;
        }

        private static string GetQualityWithoutSize(string qualityWithSize)
        {
            var qualityIndex = qualityWithSize.LastIndexOf(" - ", StringComparison.Ordinal);
            return qualityIndex == -1
                ? qualityWithSize
                : qualityWithSize[..qualityIndex];
        }

        private void UpdateVideoSizeEstimates()
        {
            int selectedIndex = ComboQualityIndex;

            var trimStart = IsCheckStart ? StartTime : TimeSpan.Zero;
            var trimEnd = IsCheckEnd ? EndTime : VodLength;

            for (var i = 0; i < ComboQuality.Count; i++)
            {
                var qualityWithSize = (string)ComboQuality[i];
                var quality = GetQualityWithoutSize(qualityWithSize);
                var bandwidth = videoQualities[quality].bandwidth;

                var sizeInBytes = VideoSizeEstimator.EstimateVideoSize(bandwidth, trimStart, trimEnd);
                if (sizeInBytes == 0)
                {
                    ComboQuality[i] = quality;
                }
                else
                {
                    var newVideoSize = VideoSizeEstimator.StringifyByteCount(sizeInBytes);
                    ComboQuality[i] = $"{quality} - {newVideoSize}";
                }
            }

            ComboQualityIndex = selectedIndex;
        }

        private async void GetVideoInfoClick()
        {
            _idle = false;
            await GetVideoInfo();

            if (IsAnalyzeComments)
            {
                await SelectChatInfo();
            }

            _idle = true;
        }

        private async void DownloadClick()
        {
            //if (((HandyControl.Controls.SplitButton)sender).IsDropDownOpen)
            //{
            //    return;
            //}

            if (!ValidateInputs())
            {
                AppendLog(Translations.Strings.ErrorLog + Translations.Strings.InvalidTrimInputs);
                return;
            }

            System.Windows.Forms.SaveFileDialog saveFileDialog = new()
            {
                Filter = ComboQuality.Contains("Audio") ? "M4A Files | *.m4a" : "MP4 Files | *.mp4",
                FileName = FilenameService.GetFilename(Settings.Default.TemplateVod, TextTitle, currentVideoId.ToString(),
                currentVideoTime, TextStreamer, streamerId,
                    IsCheckStart == true ? StartTime : TimeSpan.Zero,
                    IsCheckEnd == true ? EndTime : VodLength,
                    VodLength, viewCount, game) + FilenameService.GuessVodFileExtension(ComboQuality[ComboQualityIndex])
            };

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }

            //SetEnabled(false);
            //btnGetInfo.IsEnabled = false;
            _idle = false;

            VideoDownloadOptions options = GetOptions(saveFileDialog.FileName, null);
            options.CacheCleanerCallback = HandleCacheCleanerCallback;

            var downloadProgress = new WpfTaskProgress((LogLevel)Settings.Default.LogLevels, SetPercent, SetStatus, AppendLog);
            VideoDownloader currentDownload = new VideoDownloader(options, downloadProgress);
            _cancellationTokenSource = new CancellationTokenSource();

            SetImage("Images/ppOverheat.gif", true);
            StatusMessage = Translations.Strings.StatusDownloading;
            //UpdateActionButtons(true);
            try
            {
                await currentDownload.DownloadAsync(_cancellationTokenSource.Token);
                downloadProgress.SetStatus(Translations.Strings.StatusDone);
                SetImage("Images/ppHop.gif", true);
            }
            catch (Exception ex) when (ex is OperationCanceledException or TaskCanceledException && _cancellationTokenSource.IsCancellationRequested)
            {
                downloadProgress.SetStatus(Translations.Strings.StatusCanceled);
                SetImage("Images/ppHop.gif", true);
            }
            catch (Exception ex)
            {
                downloadProgress.SetStatus(Translations.Strings.StatusError);
                SetImage("Images/peepoSad.png", false);
                AppendLog(Translations.Strings.ErrorLog + ex.Message);
                if (Settings.Default.VerboseErrors)
                {
                    MessageBox.Show(Application.Current.MainWindow!, ex.ToString(), Translations.Strings.VerboseErrorOutput, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            _idle = true;
            downloadProgress.ReportProgress(0);
            _cancellationTokenSource.Dispose();
            //UpdateActionButtons(false);

            GC.Collect();
        }

        private void EnququeDownload()
        {
            if (ValidateInputs())
            {
                var queueOptions = new WindowQueueOptions(PageEnum.VOD_DOWNLOAD)
                {
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                queueOptions.ShowDialog();
            }
            else
            {
                AppendLog(Translations.Strings.ErrorLog + Translations.Strings.InvalidTrimInputs);
            }
        }

        public void SetImage(string imageUri, bool isGif)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(imageUri, UriKind.Relative);
            image.EndInit();
            //if (isGif)
            //{
            //    ImageBehavior.SetAnimatedSource(statusImage, image);
            //}
            //else
            //{
            //    ImageBehavior.SetAnimatedSource(statusImage, null);
            //    statusImage.Source = image;
            //}
        }

        private DirectoryInfo[] HandleCacheCleanerCallback(DirectoryInfo[] directories)
        {
            var window = new WindowOldVideoCacheManager(directories)
            {
                Owner = System.Windows.Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            window.ShowDialog();

            return window.GetItemsToDelete();
        }


        private void SetPercent(int percent)
        {
            StatusProgressBarValue = percent;
        }

        private void SetStatus(string message)
        {
            StatusMessage = message;
        }

        private void AppendLog(string message)
        {
            Logs += message + Environment.NewLine;
        }

        public bool ValidateInputs()
        {
            if (IsCheckStart)
            {
                var beginTime = StartTime;
                if (VodLength > TimeSpan.Zero && beginTime.TotalSeconds >= VodLength.TotalSeconds)
                {
                    return false;
                }

                if (IsCheckEnd)
                {
                    var endTime = EndTime;
                    if (endTime.TotalSeconds < beginTime.TotalSeconds)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
