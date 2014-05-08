using FilterMage.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Nokia.Graphics.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using FilterMage.Models;
using Windows.Storage.Streams;

namespace FilterMage
{
    public partial class MainPage : PhoneApplicationPage
    {
        Stream chosenPhoto;
        private WriteableBitmap previewImage = null;
        private int previewImageWidth = 400;
        private int previewImageHeight = 350;
        private int thumbnailWidth = 200;
        private int thumbnailHeight = 200;
        public ObservableCollection<FilterThumbnail> filterThumbnails = null;
        public ObservableCollection<IFilter> activeFilters = null;
     
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            filterThumbnails = new ObservableCollection<FilterThumbnail>();
            activeFilters = new ObservableCollection<IFilter>();
            List_Thumbnails.DataContext = filterThumbnails;
        }
        
        private void SelectImage_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            PhotoChooserTask chooser = new PhotoChooserTask();
            chooser.Completed += chooser_Completed;
            chooser.Show();
        }

        private void chooser_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult != TaskResult.OK || e.ChosenPhoto == null)
                return;

            BitmapImage bmp = new BitmapImage();
            bmp.SetSource(e.ChosenPhoto);
            e.ChosenPhoto.Position = 0;
            previewImage = new WriteableBitmap(bmp.PixelWidth, bmp.PixelHeight);
            previewImage.SetSource(e.ChosenPhoto);
            e.ChosenPhoto.Position = 0;
            chosenPhoto = e.ChosenPhoto;
            
            Image_PreviewImage.Source = previewImage;
            SelectImage.Visibility = System.Windows.Visibility.Collapsed;

            WriteableBitmap chosenImage = new WriteableBitmap(thumbnailWidth, thumbnailHeight);
            chosenImage.SetSource(e.ChosenPhoto);
            addFilterThumbnails(chosenImage);
        }

        private void addFilterThumbnails(WriteableBitmap image)
        {
            try
            {
                var filters = (App.Current as App).supportedFilters;
                filterThumbnails.Clear();
                foreach (KeyValuePair<String, IFilter> filter in filters)
                {
                    filterThumbnails.Add(new FilterThumbnail(filter.Value, filter.Key, image));
                }
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

        private async void Image_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Image img = sender as Image;
            FilterThumbnail t = (FilterThumbnail)img.DataContext;
            Effect eff = new Effect(t.effect.filters);
            WriteableBitmap newImage = new WriteableBitmap(previewImageWidth, previewImageHeight);
            newImage = await eff.ApplyEffect(previewImage, newImage);
            previewImage = newImage;
            Image_PreviewImage.Source = previewImage;
            //filterThumbnails.Remove(t);
            WriteableBitmap thumbImg = new WriteableBitmap(previewImage);
            addFilterThumbnails(thumbImg);
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