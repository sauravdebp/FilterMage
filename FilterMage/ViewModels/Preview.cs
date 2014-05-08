using Nokia.Graphics.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using FilterMage.Models;

namespace FilterMage.ViewModels
{
    public class Preview : INotifyPropertyChanged
    {
        private WriteableBitmap _previewImage = null;
        public WriteableBitmap previewImage
        {
            get { return _previewImage; }
            set
            {
                _previewImage = value;
                NotifyPropertyChanged("previewImage");
            }
        }

        public ObservableCollection<IFilter> activeFilters = null;

        public Preview(Stream image)
        {
            image.Position = 0;
            BitmapImage bmp = new BitmapImage();
            bmp.SetSource(image);
            previewImage = new WriteableBitmap(bmp.PixelWidth, bmp.PixelHeight);
            previewImage.SetSource(image);
            activeFilters = new ObservableCollection<IFilter>();
        }

        public async void ApplyFilters(List<IFilter> filters)
        {
            activeFilters.Concat<IFilter>(filters);
            Effect eff = new Effect(filters);
            WriteableBitmap newImage = new WriteableBitmap(previewImage.PixelWidth, previewImage.PixelHeight);
            newImage = await eff.ApplyEffect(previewImage, newImage);
            previewImage = newImage;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
