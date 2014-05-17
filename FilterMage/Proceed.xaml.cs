using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.Xna.Framework.Media;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Phone.Tasks;
using FilterMage.ViewModels;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Media.PhoneExtensions;

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
            preview = (App.Current as App).tempPreview;
            Image_FullImage.Source = preview.previewImage;
            RenderImage();
        }

        private async void RenderImage()
        {
            finalImage = await preview.CreateFullResPreview();
            Image_FullImage.Source = finalImage;
            Text_Resolution.Text = finalImage.PixelWidth.ToString() + " X " + finalImage.PixelHeight.ToString();
            Progress_Rendering.Visibility = System.Windows.Visibility.Collapsed;
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
            MessageBox.Show("Image saved");
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
                return lib.SavePicture(imageName, jpeg).GetPath();
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
    }
}