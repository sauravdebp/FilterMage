using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using FilterMage.ViewModels;
using Nokia.Graphics.Imaging;
using System.Reflection;
using FilterMage.Models;
using System.Collections.ObjectModel;

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
            preview = (App.Current as App).tempPreview;
            wFilter = preview.GetLastFilter();
            List_BoolProps.DataContext = wFilter.BoolProperties;
            List_RangeProps.DataContext = wFilter.RangeProperties;
            List_EnumProps.DataContext = wFilter.EnumProperties;
            wFilter.FilterRefreshed += filter_FilterRefreshed;
            Image_PreviewImage.Source = preview.previewImage;
        }

        private async void filter_FilterRefreshed()
        {
            await preview.UndoLastFilter();
            Image_PreviewImage.Source = await preview.ApplyFilter(wFilter);
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
    }
}