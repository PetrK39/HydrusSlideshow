using Hydrus_Slideshow.ViewModels;
using System.ComponentModel;
using System.Windows;

namespace Hydrus_Slideshow.Views
{
    public partial class ConfigView : Window
    {
        public ConfigView(ConfigViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.OnCloseRequest += (_, _) => Close();
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            DialogResult = startSlideshowCheckbox.IsChecked;
            base.OnClosing(e);
        }
    }
}
