using FilterMage.Resources;
using FilterMage.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Nokia.Graphics.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace FilterMage
{
    public partial class MainPage : PhoneApplicationPage
    {
        BitmapImage originalImage = null;
        Stream ActivePhoto = null;
        public ObservableCollection<FilterThumbnail> filterThumbnails = null;
     
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            filterThumbnails = new ObservableCollection<FilterThumbnail>();
            List_Thumbnails.DataContext = filterThumbnails;
            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
            /*PhotoChooserTask chooser = new PhotoChooserTask();
            chooser.Completed += chooser_Completed;
            chooser.Show();
            SelectImage.Text = "Loading Image...";*/
        }
        
        private void SelectImage_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            PhotoChooserTask chooser = new PhotoChooserTask();
            chooser.Completed += chooser_Completed;
            chooser.Show();
            SelectImage.Text = "Loading Image...";
        }

        private void chooser_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult != TaskResult.OK || e.ChosenPhoto == null)
                return;

            ActivePhoto = e.ChosenPhoto;
            originalImage = new BitmapImage();
            originalImage.SetSource(ActivePhoto);
            ActivePhoto.Position = 0;
            e.ChosenPhoto.Position = 0;
            
            EditImage.Source = originalImage;
            SelectImage.Visibility = System.Windows.Visibility.Collapsed;
            EditImage.Visibility = System.Windows.Visibility.Visible;

            /*var imageStream = new StreamImageSource(ActivePhoto);
            FilterEffect effect = new FilterEffect(imageStream);
            var filter = new AntiqueFilter();
            effect.Filters = new[] { filter };
            WriteableBitmap filterImg = new WriteableBitmap(100, 100);
            var renderer = new WriteableBitmapRenderer(effect, filterImg);
            filterImg = await renderer.RenderAsync();

            FilterImage.Source = filterImg;
            */

            //FilterThumbnail thumb = new FilterThumbnail(new CartoonFilter(), ActivePhoto);
            //FilterImage.Source = await thumb.ApplyEffect();
            addFilterThumbnails();
        }

        private void addFilterThumbnails()
        {
            try
            {
                filterThumbnails.Add(new FilterThumbnail(new AntiqueFilter(), ActivePhoto));
                filterThumbnails.Add(new FilterThumbnail(new CartoonFilter(), ActivePhoto));
                filterThumbnails.Add(new FilterThumbnail(new ContrastFilter(), ActivePhoto));
                filterThumbnails.Add(new FilterThumbnail(new GrayscaleFilter(), ActivePhoto));
                filterThumbnails.Add(new FilterThumbnail(new GrayscaleNegativeFilter(), ActivePhoto));
                filterThumbnails.Add(new FilterThumbnail(new DespeckleFilter(), ActivePhoto));
                filterThumbnails.Add(new FilterThumbnail(new ColorBoostFilter(), ActivePhoto));
                filterThumbnails.Add(new FilterThumbnail(new ColorSwapFilter(), ActivePhoto));
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + " in addFilterThumbnails");
            }
        }

        protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            base.OnOrientationChanged(e);
        }
        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}