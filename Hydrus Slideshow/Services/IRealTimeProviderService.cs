using System.ComponentModel;

namespace Hydrus_Slideshow.Services
{
    public interface IRealTimeProviderService : INotifyPropertyChanged
    {
        public string TimeString { get; }
    }
}
