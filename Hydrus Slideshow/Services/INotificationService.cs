using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydrus_Slideshow.Services
{
    public interface INotificationService
    {
        public void ShowClipboardNotification(int hashesCount);
        public void ShowFilesLoadedNotification(int count);
        public void ShowSentToHydrusNotification();
    }
}
