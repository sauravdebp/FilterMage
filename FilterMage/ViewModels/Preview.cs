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
        private List<WriteableBitmap> prevPreviews = new List<WriteableBitmap>(10);

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
        
        public string LastFilter
        {
            get
            {
                if (activeFilters.Count > 0)
                    return GetLastFilter().filterName;
                return "No filter applied";
            }
        }

        private List<Wrap_Filter> activeFilters = null;

        public Preview(Stream image, int width, int height)
        {
            originalImage = image;
            previewWidth = width;
            previewHeight = height;
            activeFilters = new List<Wrap_Filter>();
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

        private async Task<WriteableBitmap> ApplyFilters(List<IFilter> filters)
        {
            Effect eff = new Effect(filters);
            WriteableBitmap newImage = new WriteableBitmap(previewImage.PixelWidth, previewImage.PixelHeight);
            newImage = await eff.ApplyEffect(previewImage, newImage);
            previewImage = newImage;
            return previewImage;
        }

        public async Task<WriteableBitmap> ApplyFilter(Wrap_Filter filter)
        {
            activeFilters.Add(filter);
            noofFilters = activeFilters.Count;
            prevPreviews.Add(previewImage);
            List<IFilter> filters = new List<IFilter>();
            filters.Add(filter.filter);
            NotifyPropertyChanged("LastFilter");
            return await ApplyFilters(filters);
        }

        public async Task<WriteableBitmap> UndoLastFilter()
        {
            if (activeFilters.Count == 0)
                return null;
            activeFilters.RemoveAt(activeFilters.Count - 1);
            noofFilters = activeFilters.Count;
            if (prevPreviews.Count > 0)
            {
                previewImage = prevPreviews.Last();
                prevPreviews.Remove(prevPreviews.Last());
            }
            else
            {
                previewImage = CreatePreviewFromStream();
                List<IFilter> filters = new List<IFilter>();
                foreach (Wrap_Filter wFilter in activeFilters)
                {
                    filters.Add(wFilter.filter);
                }
                await ApplyFilters(filters);
            }
            NotifyPropertyChanged("LastFilter");
            return previewImage;
        }

        public Wrap_Filter GetLastFilter()
        {
            if(activeFilters.Count > 0)
                return activeFilters.Last();
            return null;
        }

        public WriteableBitmap ClearAllFilters()
        {
            try
            {
                activeFilters.Clear();
                prevPreviews.Clear();
                noofFilters = activeFilters.Count;
                previewImage = CreatePreviewFromStream();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " in Preview.cs ClearAllFilters()");
            }
            NotifyPropertyChanged("LastFilter");
            return previewImage;
        }

        public async Task<WriteableBitmap> CreateFullResPreview()
        {
            WriteableBitmap original = new WriteableBitmap(0, 0);
            original.SetSource(originalImage);
            WriteableBitmap source = new WriteableBitmap(original);
            WriteableBitmap dest = new WriteableBitmap(source.PixelWidth, source.PixelHeight);
            foreach (Wrap_Filter w_filter in activeFilters)
            {
                Effect eff = new Effect(w_filter.filter);
                await eff.ApplyEffect(source, dest);
                source = null;
                source = new WriteableBitmap(dest);
                dest = null;
                dest = new WriteableBitmap(source.PixelWidth, source.PixelHeight);
            }
            return source;
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
