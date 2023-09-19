using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using Hydrus_Slideshow.Services;
using Hydrus_Slideshow.ViewModels;
using Hydrus_Slideshow.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Hydrus_Slideshow
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private enum States
        {
            Startup,
            Slideshow,
            Config,
            Exit
        }
        private States currentState = States.Startup;
        private States nextState = States.Exit;

        private readonly IHost host;
        public App()
        {
            host = Host.CreateDefaultBuilder()
           .ConfigureServices((services) =>
           {
               services.AddTransient<IClipboardService, MainClipboardService>();
               services.AddSingleton<IConfigService, MainConfigService>();
               services.AddTransient<IErrorDialogService, MainErrorDialogService>();
               services.AddTransient<INotificationService, MainNotificationService>();
               services.AddSingleton<IRealTimeProviderService, MainRealTimeProviderService>();
               services.AddTransient<IStoryboardProviderService, MainStoryboardProviderService>();

               services.AddSingleton<SlideshowViewModel>();
               services.AddSingleton<ConfigViewModel>();

               services.AddSingleton<SlideshowView>();
               services.AddSingleton<ConfigView>();
           })
           .Build();
        }
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            while (currentState != States.Exit)
            {
                switch (currentState)
                {
                    case States.Startup:
                        ParseArgs();
                        break;
                    case States.Slideshow:
                        ShowSlideshow();
                        break;
                    case States.Config:
                        ShowConfig();
                        break;
                }
                currentState = nextState;
            }
            Shutdown();
        }
        private void ParseArgs()
        {
            var args = Environment.GetCommandLineArgs().Skip(1).Select(s => s.ToLower()).ToList();

            if (args.Count == 0 || args[0] == "/s")
            {
                nextState = States.Slideshow;
            }
            else if (args[0] == "/c")
            {
                nextState = States.Config;
            }
            else if (args[0] == "/p" && args[1] is string hwnd)
            {
                nextState = States.Exit;
                // preview mode is not supported
            }
            else
            {
                DisplayHelp(args);
                nextState = States.Exit;
            }
        }
        private void ShowSlideshow()
        {
            var configService = host.Services.GetRequiredService<IConfigService>();
            var dialogService = host.Services.GetRequiredService<IErrorDialogService>();

            if (string.IsNullOrWhiteSpace(configService.HydrusToken))
            {
                dialogService.ShowExceptionDialog(new ArgumentNullException(nameof(configService.HydrusToken)));
                nextState = States.Config;
                return;
            }

            var sv = host.Services.GetRequiredService<SlideshowView>();

            if (sv.ShowDialog() is true)
                nextState = States.Config;
            else
                nextState = States.Exit;
        }
        private void ShowConfig()
        {
            var cv = host.Services.GetRequiredService<ConfigView>();

            if (cv.ShowDialog() is true)
                nextState = States.Slideshow;
            else
                nextState = States.Exit;
        }
        private static void DisplayHelp(IEnumerable<string> args)
        {
            MessageBox.Show(
$@"Can't recognize '{string.Join(" ", args)}' args
Usage:
ScreenSaver    - Run the Screen Saver.
ScreenSaver /s - Run the Screen Saver.
ScreenSaver /c - Show the Settings dialog box.");
        }
    }
}
