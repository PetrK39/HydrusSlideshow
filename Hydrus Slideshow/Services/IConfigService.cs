using PreferenceManagerLibrary.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydrus_Slideshow.Services
{
    public interface IConfigService
    {
        public bool Fullscreen { get; }
        public int SlideInterval { get; }

        public bool IsFadeEnabled { get; }
        public bool IsSlideEnabled { get; }
        public bool IsZoomEnabled { get; }

        public double FitWidthMax { get; }
        public double FitHeightMax { get; }
        public double Zoom { get; }

        public string HydrusUrl { get; }
        public string HydrusToken { get; }
        public string HydrusQuerry { get; }

        public PreferenceManager PreferenceManager { get; }

        public void SetFullscreen(bool value);
    }
}
