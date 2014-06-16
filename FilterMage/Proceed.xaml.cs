using FilterMage.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Media.PhoneExtensions;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace FilterMage
{
    public partial class Proceed : PhoneApplicationPage
    {
        public Preview preview;
        public WriteableBitmap finalImage;
        private string imagePath = null;
        public Proceed()
        {
            InitializeComponent();
            preview = (App.Current as App).preview;
            Image_FullImage.Source = preview.previewImage;
            RenderImage();
        }

        private async void RenderImage()
        {
            finalImage = await preview.CreateFullResPreview();
            //finalImage = preview.GetFullResImage();
            Image_FullImage.Source = finalImage;
            Text_Resolution.Text = finalImage.PixelWidth.ToString() + " X " + finalImage.PixelHeight.ToString();
            Progress_Rendering.Visibility = System.Windows.Visibility.Collapsed;
            (ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = true;
            (ApplicationBar.Buttons[1] as ApplicationBarIconButton).IsEnabled = true;
        }

        protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            if (e.Orientation == PageOrientation.LandscapeLeft || e.Orientation == PageOrientation.LandscapeRight)
            {
                TitlePanel.Visibility = System.Windows.Visibility.Collapsed;
                ApplicationBar.IsVisible = false;
            }
            else
            {
                TitlePanel.Visibility = System.Windows.Visibility.Visible;
                ApplicationBar.IsVisible = true;
            }
            base.OnOrientationChanged(e);
        }

        private async void AppBarBut_Save_Click(object sender, EventArgs e)
        {
            (sender as ApplicationBarIconButton).IsEnabled = false;
            Progress_Rendering.Visibility = System.Windows.Visibility.Visible;
            if (imagePath == null)
            {
                imagePath = await Task.Run(() => SaveImage());
            }
            Progress_Rendering.Visibility = System.Windows.Visibility.Collapsed;
            MessageBox.Show("Image saved");// in saved pictures folder");
        }

        private string SaveImage()
        {
            MemoryStream jpeg = new MemoryStream();
            finalImage.SaveJpeg(jpeg, finalImage.PixelWidth, finalImage.PixelHeight, 0, 100);
            jpeg.Seek(0, SeekOrigin.Begin);
            try
            {
                MediaLibrary lib = new MediaLibrary();
                string imageName = String.Format("FilterMesh_{0:G}", DateTime.Now);
                return lib.SavePictureToCameraRoll(imageName, jpeg).GetPath();
                //return lib.SavePicture(imageName, jpeg).GetPath();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return "";
        }

        private async void AppBarBut_Share_Click(object sender, EventArgs e)
        {
            (sender as ApplicationBarIconButton).IsEnabled = false;
            Progress_Rendering.Visibility = System.Windows.Visibility.Visible;
            if(imagePath == null)
                imagePath = await Task.Run(() => SaveImage());
            ShareMediaTask share = new ShareMediaTask();
            share.FilePath = imagePath;
            share.Show();
            Progress_Rendering.Visibility = System.Windows.Visibility.Collapsed;
            (sender as ApplicationBarIconButton).IsEnabled = true;
        }

        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/About_Settings.xaml", UriKind.Relative));
        }
    }
}