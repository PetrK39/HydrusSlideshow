using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Hydrus_Slideshow.Services
{
    public class MainErrorDialogService : IErrorDialogService
    {
        public void ShowExceptionDialog(Exception e)
        {
            MessageBox.Show(e.Message, "Fatal error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public bool ShowSendToPageErrorDialog()
        {
            return MessageBox.Show("Can't find 'Hydrus Slideshow' page.\r\nDo you want to copy hash to clipboard instead?\r\nUse Ctrl + C next time or create 'file search' page named 'Hydrus Slideshow'", "Page was not found", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) is MessageBoxResult.Yes;
        }
    }
}
