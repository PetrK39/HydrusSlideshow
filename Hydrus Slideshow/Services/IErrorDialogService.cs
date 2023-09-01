using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydrus_Slideshow.Services
{
    public interface IErrorDialogService
    {
        void ShowExceptionDialog(Exception e);
        bool ShowSendToPageErrorDialog();
    }
}
