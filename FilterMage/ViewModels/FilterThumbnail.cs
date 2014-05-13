using Nokia.Graphics.Imaging;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Imaging;
using FilterMage.Models;
using System.Reflection;

namespace FilterMage.ViewModels
{
    public class FilterThumbnail : INotifyPropertyChanged
    {
        public Effect effect = null;

        public int thumbHeight;
        public int thumbWidth;

        private WriteableBitmap _thumbnailImg = null;
        public WriteableBitmap thumbnailImg
        {
            get { return _thumbnailImg; }
            set
            {
                _thumbnailImg = value;
                NotifyPropertyChanged("thumbnailImg");
            }
        }

        private string _filterName;
        public string filterName
        {
            get { return _filterName; }
            set
            {
                _filterName = value;
                NotifyPropertyChanged("filterName");
            }
        }

        private int _rotateAngle;
        public int rotateAngle
        {
            get { return _rotateAngle; }
            set
            {
                _rotateAngle = value;
                NotifyPropertyChanged("rotateAngle");
            }
        }

        public PropertyInfo[] GetFilterProperties()
        {
            IFilter filter = effect.filters[0];
            PropertyInfo[] props = filter.GetType().GetProperties();
            return props;
        }

        public FilterThumbnail(IFilter filter, string filterName, WriteableBitmap originalImage)
        {
            effect = new Effect(filter);
            this.filterName = filterName;
            this.thumbWidth = originalImage.PixelWidth;
            this.thumbHeight = originalImage.PixelHeight;
            ApplyEffect(originalImage);
        }

        private async void ApplyEffect(WriteableBitmap sourceImg)
        {
            try
            {
                //MessageBox.Show(thumbWidth + " X " + thumbHeight);
                thumbnailImg = new WriteableBitmap(thumbWidth, thumbHeight);
                thumbnailImg = await effect.ApplyEffect(sourceImg, thumbnailImg);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + " in FilterThumbnail::ApplyEffect()");
            }
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

        public override string ToString()
        {
            return filterName;
        }
    }
}