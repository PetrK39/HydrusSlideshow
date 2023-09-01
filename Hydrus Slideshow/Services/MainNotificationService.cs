using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Notification.Wpf;

namespace Hydrus_Slideshow.Services
{
    public class MainNotificationService : INotificationService
    {
        private readonly NotificationManager notificationManager;
        public MainNotificationService()
        {
            notificationManager = new NotificationManager();
        }
        public void ShowClipboardNotification(int hashesCount)
        {
            notificationManager.Show($"{hashesCount} hashes copied to clipboard", areaName: "notifArea");
        }

        public void ShowFilesLoadedNotification(int count)
        {
            notificationManager.Show($"{count} images was loaded from Hydrus", areaName: "notifArea");
        }

        public void ShowSentToHydrusNotification()
        {
            notificationManager.Show($"Image was sent to hydrus", areaName: "notifArea");
        }
    }
}
