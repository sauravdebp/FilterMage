using Microsoft.Phone.Controls;
using System;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace FilterMage
{
    public partial class Tutorial : PhoneApplicationPage
    {
        IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        int chapters = 7;
        int curChapter = 1;
        string chapterImgSource = "/Tutorial ScreenShots/";
        string imgExtn = ".png";
        BitmapImage chapterImage = new BitmapImage();
        public Tutorial()
        {
            InitializeComponent();
            Image_Chapter.Source = chapterImage;
            chapterImage.UriSource = new Uri(chapterImgSource + curChapter + imgExtn, UriKind.Relative);
            But_Next.IsEnabled = true;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            /*if (canSkipTutorial())
            {
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                NavigationService.RemoveBackEntry();
                return;
            }*/
            //settings.Add("tutorialSkip", false);
            
        }

        /*private bool canSkipTutorial()
        {
            if (settings.Contains("tutorialSkip") && (bool)settings["tutorialSkip"] == true)
            {
                return true;
            }
            return false;
        }*/

        private void But_Next_Click(object sender, RoutedEventArgs e)
        {
            if (curChapter == chapters)
            {
                MessageBox.Show("The quick start guide is complete.");
                settings["tutorialSkip"] = true;
                settings.Save();
                NavigationService.RemoveBackEntry();
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                NavigationService.RemoveBackEntry();
                return;
            }            
            chapterImage.UriSource = new Uri(chapterImgSource + (++curChapter) + imgExtn, UriKind.Relative);
            if (curChapter == 2)
            {
                But_Prev.IsEnabled = true;
            }
        }

        private void But_Prev_Click(object sender, RoutedEventArgs e)
        {
            chapterImage.UriSource = new Uri(chapterImgSource + (--curChapter) + imgExtn, UriKind.Relative);
            if (curChapter == 1)
            {
                But_Prev.IsEnabled = false;
            }
        }
    }
}