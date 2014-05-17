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
using Microsoft.Phone.Shell;
using FilterMage.Resources;
using Microsoft.Xna.Framework.Media;
using System.Windows.Data;

namespace FilterMage
{
    public partial class MainPage : PhoneApplicationPage
    {
        Stream chosenPhoto;
        private int thumbnailHeight = 200;
        private int previewWidth = 0;
        private int previewHeight = 0;
        public Preview preview = null;
        public ObservableCollection<FilterThumbnail> filterThumbnails = null;
        private enum States { INITIAL, IMAGE_LOADED, FILTER_APPLIED, EDITABLE_FILTER_APPLIED };
        private States _PageState;
        private States PageState
        {
            get { return _PageState; }
            set
            {
                if (value != _PageState)
                {
                    _PageState = value;
                    switch (_PageState)
                    {
                        case States.INITIAL:
                            foreach (ApplicationBarIconButton but in ApplicationBar.Buttons)
                            {
                                but.IsEnabled = false;
                            }
                            break;

                        case States.IMAGE_LOADED:
                            goto case States.INITIAL;

                        case States.FILTER_APPLIED:
                            foreach (ApplicationBarIconButton but in ApplicationBar.Buttons)
                            {
                                if (but.Text != "edit last filter")
                                {
                                    but.IsEnabled = true;
                                }
                                else if(PageState == States.FILTER_APPLIED)
                                {
                                    but.IsEnabled = false;
                                }
                            }
                            break;

                        case States.EDITABLE_FILTER_APPLIED:
                            ((ApplicationBarIconButton)ApplicationBar.Buttons[2]).IsEnabled = true;
                            goto case States.FILTER_APPLIED;

                        default:
                            break;
                    }
                }
            }
        }
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            filterThumbnails = new ObservableCollection<FilterThumbnail>();
            List_Thumbnails.DataContext = filterThumbnails;
            Image_PreviewImage.DataContext = preview;
            
            PageState = States.INITIAL;
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
            preview = new Preview(e.ChosenPhoto, previewWidth, previewHeight);
            
            Image_PreviewImage.Source = preview.previewImage;
            Grid_MemoryAid.DataContext = preview;
            SelectImage.Visibility = System.Windows.Visibility.Collapsed;

            WriteableBitmap chosenImage = new WriteableBitmap(thumbnailHeight, thumbnailHeight);
            chosenImage.SetSource(e.ChosenPhoto);
            addFilterThumbnails(chosenImage);
            
            PageState = States.IMAGE_LOADED;
        }

        private void addFilterThumbnails(WriteableBitmap image)
        {
            try
            {
                Resolution dim = new Resolution(image.PixelWidth, image.PixelHeight, thumbnailHeight, fixedDim.HEIGHT);
                image = image.Resize(dim.width, dim.height, WriteableBitmapExtensions.Interpolation.Bilinear);
                var filters = (App.Current as App).supportedFilters;
                filterThumbnails.Clear();
                foreach (Wrap_Filter filter in filters)
                {
                    filterThumbnails.Add(new FilterThumbnail(filter, image));
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
            FilterThumbnail selectedThumb = (FilterThumbnail)img.DataContext;
            Image_PreviewImage.Source = await preview.ApplyFilter(selectedThumb.wrapFilter);
            WriteableBitmap thumbImg = new WriteableBitmap(preview.previewImage);
            addFilterThumbnails(thumbImg);
            if (preview.noofFilters > 0)
            {
                if (selectedThumb.wrapFilter.isEditable())
                    PageState = States.EDITABLE_FILTER_APPLIED;
                else
                    PageState = States.FILTER_APPLIED;
            }
            //setThumbAngle();
        }

        private async void AppBarBut_Undo_Click(object sender, EventArgs e)
        {
            (sender as ApplicationBarIconButton).IsEnabled = false;
            Image_PreviewImage.Source = await preview.UndoLastFilter();
            WriteableBitmap thumbImg = new WriteableBitmap(preview.previewImage);
            addFilterThumbnails(thumbImg);
            (sender as ApplicationBarIconButton).IsEnabled = true;
            if (preview.noofFilters == 0)
            {
                PageState = States.IMAGE_LOADED;
            }
            else
            {
                Wrap_Filter lastFilter = preview.GetLastFilter();
                if (!lastFilter.isEditable())
                {
                    PageState = States.FILTER_APPLIED;
                }
                else
                {
                    PageState = States.EDITABLE_FILTER_APPLIED;
                }
            }
        }

        private void AppBarBut_Proceed_Click(object sender, EventArgs e)
        {
            (sender as ApplicationBarIconButton).IsEnabled = false;
            (App.Current as App).tempPreview = preview;
            NavigationService.Navigate(new Uri("/Proceed.xaml", UriKind.Relative));
            (sender as ApplicationBarIconButton).IsEnabled = true;
        }

        private void AppBarBut_Clear_Click(object sender, EventArgs e)
        {
            (sender as ApplicationBarIconButton).IsEnabled = false;
            Image_PreviewImage.Source = preview.ClearAllFilters();
            WriteableBitmap thumbImg = new WriteableBitmap(preview.previewImage);
            addFilterThumbnails(thumbImg);
            (sender as ApplicationBarIconButton).IsEnabled = true;
            PageState = States.IMAGE_LOADED;
        }
        
        private void Grid_Preview_Loaded(object sender, RoutedEventArgs e)
        {
            if (previewWidth == 0 && previewHeight == 0)
            {
                previewWidth = (int)Grid_Preview.ActualWidth;
                previewHeight = (int)Grid_Preview.ActualHeight;
            }
        }

        private void AppBarBut_Edit_Click(object sender, EventArgs e)
        {
            (App.Current as App).tempPreview = preview;
            NavigationService.Navigate(new Uri("/FineTuneFilter.xaml", UriKind.Relative));
        }
        
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if(PhoneApplicationService.Current.State.ContainsKey("RefreshThumbs"))
            {
                PhoneApplicationService.Current.State.Clear();
                Image_PreviewImage.Source = preview.previewImage;
                WriteableBitmap thumbImg = new WriteableBitmap(preview.previewImage);
                addFilterThumbnails(thumbImg);
            }
        }

        /*protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            if (e.Orientation == PageOrientation.LandscapeLeft)
            {
                Anim_RotatePreview.To = 90;
            }
            else if (e.Orientation == PageOrientation.PortraitUp)
            {
                Anim_RotatePreview.To = 0;
            }
            Story_RotateGrid.Begin();
            setThumbAngle();
            //base.OnOrientationChanged(e);
        }

        private void setThumbAngle()
        {
            foreach (FilterThumbnail thumb in filterThumbnails)
            {
                thumb.rotateAngle = (int)Anim_RotatePreview.To;
            }
        }*/
    }
}