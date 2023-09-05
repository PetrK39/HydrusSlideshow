using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hydrus_Slideshow.Models;
using Hydrus_Slideshow.Services;
using Hydrus_Slideshow.Utils;
using static Hydrus_Slideshow.Models.HydrusClient;
using Timer = System.Timers.Timer;

namespace Hydrus_Slideshow.ViewModels
{
    [INotifyPropertyChanged]
    public partial class SlideshowViewModel
    {
        private readonly IConfigService configService;
        private readonly IErrorDialogService errorDialogService;
        private readonly IClipboardService clipboardService;
        private readonly INotificationService notificationService;
        private readonly IRealTimeProviderService realTimeProviderService;
        private readonly Dispatcher dispatcher;

        private readonly HydrusClient hydrus;
        private CircularEnumerator<SimpleHydrusFile> enumerator;

        private Timer timer;

        [ObservableProperty]
        private bool isPlaying = true;

        [ObservableProperty]
        private BitmapImage? image;
        public Size ImageSize { get; private set; }

        [ObservableProperty]
        private bool displayHelp = false;

        [ObservableProperty]
        private bool isFullscreen;
        public string TimeString => realTimeProviderService.TimeString;

        public event EventHandler<bool>? OnExitRequired;

        public SlideshowViewModel(IConfigService configService, IErrorDialogService errorDialogService, IClipboardService clipboardService, INotificationService notificationService, IRealTimeProviderService realTimeProviderService)
        {
            this.configService = configService;
            this.errorDialogService = errorDialogService;
            this.clipboardService = clipboardService;
            this.notificationService = notificationService;
            this.realTimeProviderService = realTimeProviderService;
            this.dispatcher = Dispatcher.CurrentDispatcher;

            this.realTimeProviderService.PropertyChanged += (_, e) => dispatcher.Invoke(() => PropertyChanged?.Invoke(this, e));

            hydrus = new HydrusClient(new Uri(this.configService.HydrusUrl), this.configService.HydrusToken);

            Initialise();

            IsFullscreen = configService.Fullscreen;
        }
        private async void Initialise()
        {
            var tags = configService.HydrusQuerry.Split(new[] { ", " }, StringSplitOptions.None)
                .Concat(new[] { $"system:limit={1000}", "system:filetype=image" })
                .ToArray();

            // fetch images
            List<SimpleHydrusFile> coll;
            try
            {
                coll = await hydrus.SearchFilesSimple(new(FileSortTypes.Random), tags).ToListAsync();
                if (coll.Count == 0) throw new Exception("No images in your Hydrus was found using following querry:\r\n" + string.Join(", ", tags));
                notificationService.ShowFilesLoadedNotification(coll.Count);
            }
            catch (Exception e)
            {
                errorDialogService.ShowExceptionDialog(e);
                Exit(true);
                return;
            }

            enumerator = new CircularEnumerator<SimpleHydrusFile>(coll);

            // initial image
            Image = await CreateImageAsync(enumerator.GetCurrent());

            // image update
            timer = new Timer
            {
                AutoReset = false,
                Interval = configService.SlideInterval * 1000
            };
            timer.Elapsed += (_, _) => NextImage();
            timer.Start();
        }

        [RelayCommand]
        public async void NextImage()
        {
            timer.Stop();

            var img = await CreateImageAsync(enumerator.GetNext());
            dispatcher.Invoke(() => Image = img);

            if (IsPlaying) timer.Start();
        }
        [RelayCommand]
        public async void PrevImage()
        {
            timer.Stop();

            var img = await CreateImageAsync(enumerator.GetPrev());
            dispatcher.Invoke(() => Image = img);
            GC.Collect();

            if (IsPlaying) timer.Start();
        }
        [RelayCommand]
        public void PlayPause()
        {
            IsPlaying = !IsPlaying;

            if (IsPlaying) timer.Start();
            else timer.Stop();
        }
        [RelayCommand]
        public async void SendCurrentToHyrus()
        {
            var result = await hydrus.TrySendHashToPage(enumerator.GetCurrent().Hash, "Hydrus Slideshow");

            if (result)
            {
                notificationService.ShowSentToHydrusNotification();
                return;
            }


            var timerState = IsPlaying;
            if (timerState) PlayPause(); // pause if IsPlaying was true

            if (errorDialogService.ShowSendToPageErrorDialog())
            {
                SendCurrentToClipboard();
            }
            if (timerState) PlayPause(); // resume if IsPlaying was true
        }
        [RelayCommand]
        public void SendCurrentToClipboard()
        {
            var count = clipboardService.AddLine(enumerator.GetCurrent().Hash);
            notificationService.ShowClipboardNotification(count);
        }
        [RelayCommand]
        public void ToggleHelp()
        {
            DisplayHelp = !DisplayHelp;
        }
        [RelayCommand]
        public void ToggleFullscreen()
        {
            IsFullscreen = !IsFullscreen;

            configService.SetFullscreen(IsFullscreen);
        }
        [RelayCommand]
        public void Exit(bool showConfig)
        {
            OnExitRequired?.Invoke(this, showConfig);
        }
        private async Task<BitmapImage> CreateImageAsync(SimpleHydrusFile hydrusFile)
        {
            try
            {
                int width = (int)(1920 * configService.FitWidthMax * configService.Zoom);

                var stream = await hydrus.GetFile(hydrusFile);

                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.DecodePixelWidth = width;
                image.EndInit();
                image.Freeze();

                ImageSize = new Size(image.PixelWidth, image.PixelHeight);
                return image;
            }
            catch (Exception e)
            {
                errorDialogService.ShowExceptionDialog(e);
                throw;
            }
        }
    }
}
