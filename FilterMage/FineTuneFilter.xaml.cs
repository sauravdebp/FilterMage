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

namespace FilterMage
{
    public partial class FineTuneFilter : PhoneApplicationPage
    {
        private Preview preview;
        private FilterThumbnail thumb;
        public FineTuneFilter()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            preview = (Preview)PhoneApplicationService.Current.State["preview"];
            thumb = (FilterThumbnail)PhoneApplicationService.Current.State["latestThumb"];
            Image_PreviewImage.Source = preview.previewImage;
            CreateFineTuneControls(thumb.GetFilterProperties());
        }

        private void CreateFineTuneControls(PropertyInfo[] props)
        {
            foreach (PropertyInfo prop in props)
            {
                StackPanel Stack = new StackPanel();
                TextBlock label = new TextBlock();
                label.Text = prop.Name;
                Stack.Children.Add(label);
                Stack_FineControls.Children.Add(Stack);
                switch (prop.PropertyType.ToString())
                {
                    case "System.Boolean":
                        ToggleSwitch tSwitch = new ToggleSwitch();
                        tSwitch.Name = prop.Name;
                        Stack.Children.Add(tSwitch);
                        break;
                }
            }
        }
    }
}