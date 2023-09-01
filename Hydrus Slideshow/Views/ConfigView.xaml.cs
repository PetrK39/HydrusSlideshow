using Hydrus_Slideshow.ViewModels;
using System.Windows;

namespace Hydrus_Slideshow.Views
{
    public partial class ConfigView : Window
    {
        private bool isClosing = false;
        public ConfigView(ConfigViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Closing += (_, _) => isClosing = true;
            viewModel.OnCloseRequest += (_, _) => { if (!isClosing) Close(); };
        }
    }
}
