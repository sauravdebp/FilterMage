using FilterMage.Models;
using FilterMage.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Diagnostics;
using System.Windows.Navigation;

namespace FilterMage
{
    public partial class FineTuneFilter : PhoneApplicationPage
    {
        private Preview preview = null;
        private Wrap_Filter wFilter = null;
        public FineTuneFilter()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            preview = (App.Current as App).preview;
            wFilter = preview.GetLastFilter();
            List_BoolProps.DataContext = wFilter.BoolProperties;
            List_RangeProps.DataContext = wFilter.RangeProperties;
            List_EnumProps.DataContext = wFilter.EnumProperties;
            wFilter.FilterRefreshed += filter_FilterRefreshed;
            Image_PreviewImage.Source = preview.previewImage;
        }

        private async void filter_FilterRefreshed()
        {
            Debug.WriteLine("FilterRefreshed()");
            //wFilter.FilterRefreshed -= filter_FilterRefreshed;
            await preview.UndoLastFilter();
            Image_PreviewImage.Source = await preview.ApplyFilter(wFilter);
            //wFilter.FilterRefreshed += filter_FilterRefreshed;
        }

        private void AppBarBut_Proceed_Click(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            PhoneApplicationService.Current.State["RefreshThumbs"] = null;
            wFilter.FilterRefreshed -= filter_FilterRefreshed;
            base.OnNavigatedFrom(e);
        }

        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/About_Settings.xaml", UriKind.Relative));
        }
    }
}