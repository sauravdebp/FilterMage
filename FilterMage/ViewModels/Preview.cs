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
using System.Windows;

namespace FilterMage.ViewModels
{
    public class Preview : INotifyPropertyChanged
    {
        private Stream originalImage = null;
        private int previewWidth;
        private int previewHeight;

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

        private int _noofFilters;
        public int noofFilters
        {
            get { return _noofFilters; }
            set
            {
                _noofFilters = value;
                NotifyPropertyChanged("noofFilters");
            }
        }

        private List<IFilter> activeFilters = null;

        public Preview(Stream image, int width, int height)
        {
            originalImage = image;
            previewWidth = width;
            previewHeight = height;
            activeFilters = new List<IFilter>();
            previewImage = CreatePreviewFromStream();
        }

        private WriteableBitmap CreatePreviewFromStream()
        {
            originalImage.Position = 0;
            previewImage = new WriteableBitmap(previewWidth, previewHeight);
            previewImage.SetSource(originalImage);

            int target = previewHeight;
            fixedDim fixedFlag = fixedDim.HEIGHT;
            if (previewImage.PixelWidth > previewImage.PixelHeight)
            {
                target = previewWidth;
                fixedFlag = fixedDim.WIDTH;
            }
            Resolution dim = new Resolution(previewImage.PixelWidth, previewImage.PixelHeight, target, fixedFlag);
            previewImage = previewImage.Resize(dim.width, dim.height, WriteableBitmapExtensions.Interpolation.Bilinear);
            return previewImage;
        }

        public async Task<WriteableBitmap> ApplyFilters(List<IFilter> filters, bool storeFilter=true)
        {
            if(storeFilter)
                activeFilters.AddRange(filters);
            noofFilters = activeFilters.Count;
            Effect eff = new Effect(filters);
            WriteableBitmap newImage = new WriteableBitmap(previewImage.PixelWidth, previewImage.PixelHeight);
            newImage = await eff.ApplyEffect(previewImage, newImage);
            previewImage = newImage;
            return previewImage;
        }

        public async Task<WriteableBitmap> UndoLastFilter()
        {
            if (activeFilters.Count == 0)
                return null;
            int lastIndex = activeFilters.Count - 1;
            activeFilters.RemoveAt(lastIndex);
            noofFilters = activeFilters.Count;
            previewImage = CreatePreviewFromStream();
            return await ApplyFilters(activeFilters, false);
        }

        public IFilter GetLastFilter()
        {
            var lastFilter = activeFilters[activeFilters.Count - 1];
            return lastFilter;
        }

        public WriteableBitmap ClearAllFilters()
        {
            activeFilters.Clear();
            noofFilters = activeFilters.Count;
            previewImage = CreatePreviewFromStream();
            return previewImage;
        }

        public async Task<WriteableBitmap> CreateFullResPreview()
        {
            WriteableBitmap source = new WriteableBitmap(0, 0);
            source.SetSource(originalImage);
            Effect eff = new Effect(activeFilters);
            WriteableBitmap fullRes = new WriteableBitmap(source.PixelWidth, source.PixelHeight);
            fullRes = await eff.ApplyEffect(source, fullRes);
            return fullRes;
            //MessageBox.Show("Full Res Image : " + fullResImg.PixelWidth + " X " + fullResImg.PixelHeight);
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
