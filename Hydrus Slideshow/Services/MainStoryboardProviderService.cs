using PreferenceManagerLibrary.PreferenceStorage;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Hydrus_Slideshow.Services
{
    public class MainStoryboardProviderService : IStoryboardProviderService
    {
        private readonly IConfigService configService;
        private readonly Random rnd;
        public MainStoryboardProviderService(IConfigService configService)
        {
            this.configService = configService;
            rnd = new();
        }
        public Storyboard GetStoryboard(Size canvasSize, Size imageSize, bool isPlaying)
        {
            var sb = new Storyboard();

            if (canvasSize.IsEmpty || canvasSize.Width == 0 || canvasSize.Height == 0) return sb;
            if (imageSize.IsEmpty || imageSize.Width == 0 || imageSize.Height == 0) return sb;

            var canvasRatio = canvasSize.Width / canvasSize.Height;
            var imageRatio = imageSize.Width / imageSize.Height;

            #region random
            bool zoomDirection = rnd.NextDouble() >= 0.5;
            bool slideDirection = rnd.NextDouble() >= 0.5;
            #endregion

            #region preferences
            int timerInterval = configService.SlideInterval;
            double maxTallFit = configService.FitHeightMax;
            double maxWideFit = configService.FitWidthMax;
            double zoomToMultiplyer = configService.Zoom;

            bool isFadeEnabled = configService.IsFadeEnabled;
            bool isZoomEnabled = configService.IsZoomEnabled;
            bool isSlideEnabled = configService.IsSlideEnabled;

            if (!isPlaying)
            {
                isZoomEnabled = false;
                isSlideEnabled = false;

                maxTallFit = 1d;
                maxWideFit = 1d;
            }

            if (!isSlideEnabled)
                slideDirection = true;
            #endregion

            #region fade
            if (isFadeEnabled)
            {
                var fade = new DoubleAnimationUsingKeyFrames()
                {
                    Duration = TimeSpan.FromSeconds(timerInterval / 2.0),
                    KeyFrames = new DoubleKeyFrameCollection
                    {
                        new DiscreteDoubleKeyFrame(0, KeyTime.FromPercent(0.0)),
                        new LinearDoubleKeyFrame(1, KeyTime.FromPercent(0.20)),
                        new DiscreteDoubleKeyFrame(1, KeyTime.FromPercent(1))
                    }
                };

                if (isPlaying)
                {
                    fade.RepeatBehavior = new RepeatBehavior(1);
                    fade.AutoReverse = true;
                }

                Storyboard.SetTargetProperty(fade, new PropertyPath(UIElement.OpacityProperty));

                sb.Children.Add(fade);
            }
            #endregion

            #region zoom
            bool zoomMode;
            if (isPlaying)
                zoomMode = imageRatio <= canvasRatio;
            else
                zoomMode = imageRatio >= canvasRatio;

            var zoomFrom = (zoomMode) ? (canvasSize.Width * maxTallFit) / imageSize.Width : (canvasSize.Height * maxWideFit) / imageSize.Height;
            var zoomTo = zoomFrom;

            if (isZoomEnabled)
            {
                zoomTo *= zoomToMultiplyer;
            }

            if (zoomDirection) //swap zoom direction
            {
                (zoomTo, zoomFrom) = (zoomFrom, zoomTo);
            }

            var oldSize = new Size(imageSize.Width * zoomFrom, imageSize.Height * zoomFrom);
            var newSize = new Size(imageSize.Width * zoomTo, imageSize.Height * zoomTo);

            var zoomAnimationX = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(timerInterval),
                From = oldSize.Width,
                To = newSize.Width
            };

            var zoomAnimationY = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(timerInterval),
                From = oldSize.Height,
                To = newSize.Height
            };

            Storyboard.SetTargetProperty(zoomAnimationX, new PropertyPath(FrameworkElement.WidthProperty));
            Storyboard.SetTargetProperty(zoomAnimationY, new PropertyPath(FrameworkElement.HeightProperty));

            sb.Children.Add(zoomAnimationX);
            sb.Children.Add(zoomAnimationY);
            #endregion

            #region slide

            Point fromPoint, toPoint = new();

            if (imageRatio <= canvasRatio) // if taller than screen
            {
                fromPoint.X = (double)(canvasSize.Width / 2 - zoomAnimationX.From / 2);
                fromPoint.Y = (slideDirection) ? 0 : (double)(canvasSize.Height - zoomAnimationY.From);

                if (isSlideEnabled)
                {
                    toPoint.X = (double)(canvasSize.Width / 2 - zoomAnimationX.To / 2);
                    toPoint.Y = (slideDirection) ? (double)(canvasSize.Height - zoomAnimationY.To) : 0;
                }
                else
                {
                    toPoint = fromPoint;
                }
            }
            else
            {
                fromPoint.X = (slideDirection) ? 0 : (double)(canvasSize.Width - zoomAnimationX.From);
                fromPoint.Y = (double)(canvasSize.Height / 2 - zoomAnimationY.From / 2);

                if (isSlideEnabled)
                {
                    toPoint.X = (slideDirection) ? (double)(canvasSize.Width - zoomAnimationX.To) : 0;
                    toPoint.Y = (double)(canvasSize.Height / 2 - zoomAnimationY.To / 2);
                }
                else
                {
                    toPoint = fromPoint;
                }
            }

            var transitionX = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(timerInterval),
                From = fromPoint.X,
                To = toPoint.X
            };

            var transitionY = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(timerInterval),
                From = fromPoint.Y,
                To = toPoint.Y
            };

            Storyboard.SetTargetProperty(transitionX, new PropertyPath(Canvas.LeftProperty));
            Storyboard.SetTargetProperty(transitionY, new PropertyPath(Canvas.TopProperty));

            sb.Children.Add(transitionX);
            sb.Children.Add(transitionY);

            #endregion

            sb.FillBehavior = FillBehavior.HoldEnd;

            return sb;
        }
    }
}
