using Nokia.Graphics.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
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

        private ObservableCollection<IFilter> activeFilters = null;

        public Preview(Stream image, int height)
        {
            activeFilters = new ObservableCollection<IFilter>();
            previewImage = CreatePreviewFromStream(image, height);
        }

        private WriteableBitmap CreatePreviewFromStream(Stream image, int height)
        {
            image.Position = 0;
            previewImage = new WriteableBitmap(height, height);
            previewImage.SetSource(image);
            Resolution dim = new Resolution(previewImage.PixelWidth, previewImage.PixelHeight, height);
            previewImage = previewImage.Resize(dim.width, dim.height, WriteableBitmapExtensions.Interpolation.Bilinear);
            return previewImage;
        }

        public async Task<WriteableBitmap> ApplyFilters(List<IFilter> filters)
        {
            activeFilters.Concat<IFilter>(filters);
            Effect eff = new Effect(filters);
            WriteableBitmap newImage = new WriteableBitmap(previewImage.PixelWidth, previewImage.PixelHeight);
            newImage = await eff.ApplyEffect(previewImage, newImage);
            previewImage = newImage;
            return previewImage;
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
