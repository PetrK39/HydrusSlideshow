using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using Hydrus_Slideshow.Services;
using Hydrus_Slideshow.ViewModels;
using Hydrus_Slideshow.Views;

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

        private readonly IConfigService configService;
        public App()
        {
            configService = new MainConfigService();
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
            var msps = new MainStoryboardProviderService(configService);
            var meds = new MainErrorDialogService();
            var mcbs = new MainClipboardService();
            var mns = new MainNotificationService();

            if (string.IsNullOrWhiteSpace(configService.HydrusToken))
            {
                meds.ShowExceptionDialog(new ArgumentNullException(nameof(configService.HydrusToken)));
                nextState = States.Config;
                return;
            }
            var svm = new SlideshowViewModel(configService, meds, mcbs, mns);
            var sv = new SlideshowView(svm, msps);

            if (sv.ShowDialog() is true)
                nextState = States.Config;
            else
                nextState = States.Exit;
        }
        private void ShowConfig()
        {
            var cvm = new ConfigViewModel(configService);
            var cv = new ConfigView(cvm);

            cv.ShowDialog();
            nextState = States.Slideshow;
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
