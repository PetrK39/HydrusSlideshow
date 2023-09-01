using Hydrus_Slideshow.Services;
using Hydrus_Slideshow.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Hydrus_Slideshow.Views
{
    public partial class SlideshowView : Window
    {
        private readonly IStoryboardProviderService storyboardProviderService;

        public SlideshowView(SlideshowViewModel viewModel, IStoryboardProviderService storyboardProviderService)
        {
            InitializeComponent();
            DataContext = viewModel;
            this.storyboardProviderService = storyboardProviderService;

            Slide.BeginStoryboard(storyboardProviderService.GetStoryboard(Canvas.RenderSize, viewModel.ImageSize, viewModel.IsPlaying));
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
            viewModel.OnExitRequired += ViewModel_OnExitRequired;
        }

        private void ViewModel_OnExitRequired(object? sender, bool e)
        {
            DialogResult = e;
            Close();
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var vm = DataContext as SlideshowViewModel;
            switch (e.PropertyName)
            {
                case nameof(vm.Image):
                case nameof(vm.IsPlaying):
                    Slide.BeginStoryboard(storyboardProviderService.GetStoryboard(Canvas.RenderSize, vm.ImageSize, vm.IsPlaying));
                    break;
            }
        }
    }
}
