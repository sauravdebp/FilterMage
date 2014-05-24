using FilterMage.Models;
using FilterMage.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace FilterMage
{
    public partial class MainPage : PhoneApplicationPage
    {
        Stream chosenPhoto = null;
        private int previewWidth = 0;
        private int previewHeight = 0;
        public Preview preview = null;
        public ObservableCollection<FilterThumbnail> filterThumbnails = null;
        private enum States { INITIAL, IMAGE_LOADED, THUMB_LOADED, FILTER_APPLIED, EDITABLE_FILTER_APPLIED, OPENED_VIA_EDIT };
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
            PageState = States.INITIAL;
            Settings();
        }

        private bool canSkipTutorial()
        {
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
            if (settings.Contains("tutorialSkip") && (bool)settings["tutorialSkip"] == true)
            {
                return true;
            }
            return false;
        }

        private void Settings()
        {
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
            if (settings.Contains("thumbHeight"))
            {
                (App.Current as App).thumbnailHeight = (int)settings["thumbHeight"];
            }
            else
            {
                settings.Add("thumbHeight", (App.Current as App).thumbnailHeight);
                settings.Save();
            }
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
            if (PageState == States.INITIAL)
                Grid_Preview.Tap -= SelectImage_Tap;
            loadChosenPhotoThumbs();
        }

        private void loadChosenPhotoThumbs()
        {
            WriteableBitmap chosenImage = new WriteableBitmap((App.Current as App).thumbnailHeight, (App.Current as App).thumbnailHeight);
            chosenImage.SetSource(chosenPhoto);
            chosenPhoto.Position = 0;
            addFilterThumbnails(chosenImage);
            if (PageState == States.INITIAL || PageState == States.OPENED_VIA_EDIT)
                PageState = States.THUMB_LOADED;
            else
                loadPreview();
        }

        private void loadPreview()
        {
            preview = new Preview(chosenPhoto, previewWidth, previewHeight);
            (App.Current as App).preview = preview;
            Image_PreviewImage.Source = preview.OriginalPreview;
            Grid_MemoryAid.DataContext = preview;
            SelectImage.Visibility = System.Windows.Visibility.Collapsed;

            PageState = States.IMAGE_LOADED;
        }

        private void Grid_Preview_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (PageState == States.THUMB_LOADED)
            {
                SetPreviewAreaDim();
                loadPreview();
            }
            else if (PageState == States.OPENED_VIA_EDIT)
            {
                SetPreviewAreaDim();
                loadPreview();
            }
        }

        private void SetPreviewAreaDim()
        {
            previewWidth = (int)Grid_Preview.ActualWidth;
            previewHeight = (int)Grid_Preview.ActualHeight;
            if (PageState == States.OPENED_VIA_EDIT)
            {
                loadChosenPhotoThumbs();
                Progress_Rendering.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void addFilterThumbnails(WriteableBitmap image)
        {
            try
            {
                Resolution dim = new Resolution(image.PixelWidth, image.PixelHeight, (App.Current as App).thumbnailHeight, fixedDim.HEIGHT);
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
            Progress_Rendering.Visibility = System.Windows.Visibility.Visible;
            Image img = sender as Image;
            FilterThumbnail selectedThumb = (FilterThumbnail)img.DataContext;
            Image_PreviewImage.Source = await preview.ApplyFilter(selectedThumb.wrapFilter);
            WriteableBitmap thumbImg = new WriteableBitmap(preview.previewImage);
            addFilterThumbnails(thumbImg);
            if (preview.NoofFilters > 0)
            {
                if (selectedThumb.wrapFilter.isEditable())
                    PageState = States.EDITABLE_FILTER_APPLIED;
                else
                    PageState = States.FILTER_APPLIED;
            }
            Progress_Rendering.Visibility = System.Windows.Visibility.Collapsed;
        }

        private async void AppBarBut_Undo_Click(object sender, EventArgs e)
        {
            Progress_Rendering.Visibility = System.Windows.Visibility.Visible;
            (sender as ApplicationBarIconButton).IsEnabled = false;
            Image_PreviewImage.Source = await preview.UndoLastFilter();
            WriteableBitmap thumbImg = new WriteableBitmap(preview.previewImage);
            addFilterThumbnails(thumbImg);
            (sender as ApplicationBarIconButton).IsEnabled = true;
            if (preview.NoofFilters == 0)
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
            Progress_Rendering.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void AppBarBut_Proceed_Click(object sender, EventArgs e)
        {
            (sender as ApplicationBarIconButton).IsEnabled = false;            
            NavigationService.Navigate(new Uri("/Proceed.xaml", UriKind.Relative));
            (sender as ApplicationBarIconButton).IsEnabled = true;
        }

        private async void AppBarBut_Clear_Click(object sender, EventArgs e)
        {
            Progress_Rendering.Visibility = System.Windows.Visibility.Visible;
            (sender as ApplicationBarIconButton).IsEnabled = false;
            Image_PreviewImage.Source = await preview.ClearAllFilters();
            WriteableBitmap thumbImg = new WriteableBitmap(preview.OriginalPreview);
            addFilterThumbnails(thumbImg);
            (sender as ApplicationBarIconButton).IsEnabled = true;
            PageState = States.IMAGE_LOADED;
            Progress_Rendering.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void AppBarBut_Edit_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/FineTuneFilter.xaml", UriKind.Relative));
        }
        
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (!canSkipTutorial())
            {
                NavigationService.Navigate(new Uri("/Tutorial.xaml", UriKind.Relative));
                NavigationService.RemoveBackEntry();
            }
            if(PhoneApplicationService.Current.State.ContainsKey("RefreshThumbs"))
            {
                PhoneApplicationService.Current.State.Clear();
                if (preview == null)
                    return;
                Image_PreviewImage.Source = preview.previewImage;
                WriteableBitmap thumbImg = new WriteableBitmap(preview.previewImage);
                addFilterThumbnails(thumbImg);
            }
            else if (NavigationContext.QueryString.ContainsKey("FileId"))
            {
                MediaLibrary library = new MediaLibrary();
                Picture photo = library.GetPictureFromToken(NavigationContext.QueryString["FileId"]);
                chosenPhoto = photo.GetImage();
                library.Dispose();
                photo.Dispose();
                SelectImage.Visibility = System.Windows.Visibility.Collapsed;
                PageState = States.OPENED_VIA_EDIT;
                Grid_Preview.Tap -= SelectImage_Tap;
                NavigationContext.QueryString.Clear();
                loadChosenPhotoThumbs();
            }
        }

        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/About_Settings.xaml", UriKind.Relative));
            
        }

        private async void Image_PreviewImage_DoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Progress_Rendering.Visibility = System.Windows.Visibility.Visible;
            if (await preview.TogglePreviewRes() == Preview.SCALE.FIT_SCREEN)
            {
                Scroll_PreviewImage.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                Scroll_PreviewImage.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            }
            else
            {
                Scroll_PreviewImage.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                Scroll_PreviewImage.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            }
            Image_PreviewImage.Source = preview.previewImage;
            Progress_Rendering.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void AppBar_SelectNewImage_Click(object sender, EventArgs e)
        {
            SelectImage_Tap(sender, new System.Windows.Input.GestureEventArgs());
        }
    }
}