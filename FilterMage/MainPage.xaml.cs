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

namespace FilterMage
{
    public partial class MainPage : PhoneApplicationPage
    {
        Stream chosenPhoto;
        private int thumbnailHeight = 200;
        private int previewHeight = 400;
        public Preview preview = null;
        public ObservableCollection<FilterThumbnail> filterThumbnails = null;
     
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            filterThumbnails = new ObservableCollection<FilterThumbnail>();
            List_Thumbnails.DataContext = filterThumbnails;
            Image_PreviewImage.DataContext = preview;
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

            chosenPhoto = e.ChosenPhoto;
            preview = new Preview(e.ChosenPhoto, previewHeight);
            Image_PreviewImage.Source = preview.previewImage;

            SelectImage.Visibility = System.Windows.Visibility.Collapsed;

            WriteableBitmap chosenImage = new WriteableBitmap(thumbnailHeight, thumbnailHeight);
            chosenImage.SetSource(e.ChosenPhoto);
            Resolution dim = new Resolution(chosenImage.PixelWidth, chosenImage.PixelHeight, thumbnailHeight);
            chosenImage = chosenImage.Resize(dim.width , dim.height, WriteableBitmapExtensions.Interpolation.Bilinear);
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

        private async void Image_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Image img = sender as Image;
            FilterThumbnail t = (FilterThumbnail)img.DataContext;
            Image_PreviewImage.Source = await preview.ApplyFilters(t.effect.filters);
            WriteableBitmap thumbImg = new WriteableBitmap(preview.previewImage);
            addFilterThumbnails(thumbImg);
        }
    }
}