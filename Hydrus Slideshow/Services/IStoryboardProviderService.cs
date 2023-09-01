using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace Hydrus_Slideshow.Services
{
    public interface IStoryboardProviderService
    {
        public Storyboard GetStoryboard(Size canvasSize, Size imageSize, bool isPlaying);
    }
}
