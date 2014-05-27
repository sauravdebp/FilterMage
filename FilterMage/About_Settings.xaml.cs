using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Windows.Navigation;

namespace FilterMage
{
    public partial class About_Settings : PhoneApplicationPage
    {
        public Settings s = new Settings();
        public About_Settings()
        {
            InitializeComponent();
            Rect_Thumb.DataContext = s;
            Slider_Thumb.DataContext = s;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if(PhoneApplicationService.Current.State.ContainsKey("RefreshThumbs"))
                s.Save();
            base.OnNavigatedFrom(e);
        }

        private void HyperlinkButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            s.SetTutorialOn();
            s.Save();
            NavigationService.Navigate(new Uri("/Tutorial.xaml", UriKind.Relative));
        }

        private void But_Rate_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            RateMyApp.Helpers.FeedbackHelper.Default.Review();
        }
    }

    public class Settings : INotifyPropertyChanged
    {
        private IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        public int ThumbHeight
        {
            get { return (App.Current as App).thumbnailHeight; }
            set
            {
                (App.Current as App).thumbnailHeight = value;
                NotifyPropertyChanged("ThumbHeight");
                PhoneApplicationService.Current.State["RefreshThumbs"] = null;
            }
        }

        public Settings()
        {
            if (settings.Contains("thumbHeight"))
            {
                ThumbHeight = (int)settings["thumbHeight"];
            }
        }

        public void SetTutorialOn()
        {
            settings["tutorialSkip"] = false;
        }

        public void Save()
        {
            if (settings.Contains("thumbHeight"))
            {
                settings["thumbHeight"] = ThumbHeight;
                settings.Save();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}