using Hydrus_Slideshow.ViewModels;
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
    }
}
