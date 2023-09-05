using System;
using System.Threading.Tasks;
using System.Timers;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Hydrus_Slideshow.Services
{
    public class MainRealTimeProviderService : ObservableObject, IRealTimeProviderService
    {
        public string TimeString => DateTime.Now.ToShortTimeString();
        private readonly Timer timer;

        public MainRealTimeProviderService()
        {
            timer = new Timer
            {
                AutoReset = true,
                Interval = 60 * 1000
            };

            timer.Elapsed += (_, _) => OnPropertyChanged(nameof(TimeString));

            Task.Run(async () =>
            {
                await Task.Delay((60 - DateTime.Now.Second) * 1000);
                OnPropertyChanged(nameof(TimeString));
                timer.Start();
            });
        }
    }
}
