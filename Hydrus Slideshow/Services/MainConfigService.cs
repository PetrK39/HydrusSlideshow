using PreferenceManagerLibrary.Manager;
using PreferenceManagerLibrary.Preferences;
using PreferenceManagerLibrary.PreferenceStorage;
using PreferenceManagerLibrary.Validation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydrus_Slideshow.Services
{
    public class MainConfigService : IConfigService
    {
        public bool Fullscreen => PreferenceManager.FindPreferenceByKey<BoolPreference>("slideshow.fullscreen").Value;
        public int SlideInterval => int.Parse(PreferenceManager.FindPreferenceByKey<InputPreference>("slideshow.slideInterval").Value);

        public bool IsFadeEnabled => PreferenceManager.FindPreferenceByKey<BoolPreference>("slideshow.isFadeEnabled").Value;
        public bool IsSlideEnabled => PreferenceManager.FindPreferenceByKey<BoolPreference>("slideshow.isSlideEnabled").Value;
        public bool IsZoomEnabled => PreferenceManager.FindPreferenceByKey<BoolPreference>("slideshow.isZoomEnabled").Value;

        public double FitWidthMax => double.Parse(PreferenceManager.FindPreferenceByKey<InputPreference>("slideshow.fitWidthMax").Value);
        public double FitHeightMax => double.Parse(PreferenceManager.FindPreferenceByKey<InputPreference>("slideshow.fitHeightMax").Value);
        public double Zoom => double.Parse(PreferenceManager.FindPreferenceByKey<InputPreference>("slideshow.zoom").Value);

        public string HydrusUrl => PreferenceManager.FindPreferenceByKey<InputPreference>("hydrus.url").Value;
        public string HydrusToken => PreferenceManager.FindPreferenceByKey<InputPreference>("hydrus.token").Value;
        public string HydrusQuerry => PreferenceManager.FindPreferenceByKey<InputPreference>("hydrus.querry").Value;


        public PreferenceManager PreferenceManager { get; init; }

        public MainConfigService()
        {
            PreferenceManager = new(new XMLPreferenceStorage("config.xml"));

            var tabHydrus = new PreferenceCollection("hydrus", "Hydrus");

            var uriValidator = new StringValidator().AddCustom(s =>
            {
                if (Uri.TryCreate(s, new UriCreationOptions(), out _)) return string.Empty;
                else return $"Failed to parse URL";
            });
            var hyUrl = new InputPreference("hydrus.url", "Hydrus address", defaultValue: "http://localhost:45869/", valueValidator: uriValidator );
            var tokenValidator = new StringValidator().AddEqualsTo("Not valid token length", 64);
            var hyToken = new InputPreference("hydrus.token", "Hydrus token", valueValidator: tokenValidator);
            var hyQuerry = new InputPreference("hydrus.querry", "Hydrus querry", defaultValue: "system:archive");

            tabHydrus.ChildrenPreferences.Add(hyUrl);
            tabHydrus.ChildrenPreferences.Add(hyToken);
            tabHydrus.ChildrenPreferences.Add(hyQuerry);

            var tabSlideshow = new PreferenceCollection("slideshow", "Slideshow");

            var fullscreenPref = new BoolPreference("slideshow.fullscreen", "Fullscreen on startup", defaultValue: true);

            var slideIntervalPref = new InputPreference("slideshow.slideInterval", "Slide Interval", "Slideshow interval in seconds", "15", new NumberValidator<int>().AddGreaterOrEqualsThan(3).AddLessOrEqualsThan(300));

            var isFadePref = new BoolPreference("slideshow.isFadeEnabled", "Enable Fade", "Enable fade effect for slide", true);
            var isSlidePref = new BoolPreference("slideshow.isSlideEnabled", "Enable Animation", "Enable slide animation", true);
            var isZoomPref = new BoolPreference("slideshow.isZoomEnabled", "Enable Zoom", "Enable zoom in/out animation", true);

            var fitWidthPref = new InputPreference("slideshow.fitWidthMax", "Max Width Fit", "Set the preferred width fit where 1.0 is horizontal fit", "1", new NumberValidator<double>().AddGreaterThan(0).AddLessOrEqualsThan(1).SetRejectInfinity().SetRejectNaN());
            var fitHeightPref = new InputPreference("slideshow.fitHeightMax", "Max Height Fit", "Set the preferred height fit where 1.0 is vertical fit", "1", new NumberValidator<double>().AddGreaterThan(0).AddLessOrEqualsThan(1).SetRejectInfinity().SetRejectNaN());
            var zoomPref = new InputPreference("slideshow.zoom", "Zoom", "Set the preferred zoom in/out amount where 1.0 is no zoom", (1.2).ToString(), new NumberValidator<double>().SetRejectInfinity().SetRejectNaN());

            tabSlideshow.ChildrenPreferences.Add(fullscreenPref);
            tabSlideshow.ChildrenPreferences.Add(slideIntervalPref);

            tabSlideshow.ChildrenPreferences.Add(isFadePref);
            tabSlideshow.ChildrenPreferences.Add(isSlidePref);
            tabSlideshow.ChildrenPreferences.Add(isZoomPref);

            tabSlideshow.ChildrenPreferences.Add(fitWidthPref);
            tabSlideshow.ChildrenPreferences.Add(fitHeightPref);
            tabSlideshow.ChildrenPreferences.Add(zoomPref);

            PreferenceManager.Preferences.Add(tabHydrus);
            PreferenceManager.Preferences.Add(tabSlideshow);

            PreferenceManager.LoadPreferences();
        }

        public void SetFullscreen(bool value)
        {
            PreferenceManager.BeginEdit();
            PreferenceManager.FindPreferenceByKey<BoolPreference>("slideshow.fullscreen").EditableValue = value;
            PreferenceManager.EndEdit();
            PreferenceManager.SavePreferences();
        }
    }
}
