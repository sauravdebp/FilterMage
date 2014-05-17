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

namespace FilterMage
{
    public partial class Proceed : PhoneApplicationPage
    {
        public WriteableBitmap fullResImg;
        public Proceed()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            fullResImg = (App.Current as App).Image;
            if (fullResImg == null)
            {
                MessageBox.Show("Illegal Navigation to page!");
            }
            else
            {
                Image_FullImage.Source = fullResImg;
            }
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

        private void AppBarBut_Save_Click(object sender, EventArgs e)
        {
            (sender as ApplicationBarIconButton).IsEnabled = false;
            MemoryStream jpeg = new MemoryStream();
            fullResImg.SaveJpeg(jpeg, fullResImg.PixelWidth, fullResImg.PixelHeight, 0, 100);
            jpeg.Seek(0, SeekOrigin.Begin);
            try
            {
                MediaLibrary lib = new MediaLibrary();
                string imageName = String.Format("FilterMesh_{0:G}", DateTime.Now);
                lib.SavePicture(imageName, jpeg);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            MessageBox.Show("Image saved");
            //(sender as ApplicationBarIconButton).IsEnabled = true;
        }

        private void AppBarBut_Share_Click(object sender, EventArgs e)
        {
            ShareLinkTask share = new ShareLinkTask();
            share.Title = "Share Photo";
            share.LinkUri = new Uri("http://www.facebook.com", UriKind.Absolute);
            share.Show();
        }
    }
}