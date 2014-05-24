using FilterMage.Models;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FilterMage.ViewModels
{
    public class FilterThumbnail : INotifyPropertyChanged
    {
        private Effect effect = null;
        private WriteableBitmap originalImage = null;
        public Wrap_Filter wrapFilter = null;

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

        public FilterThumbnail(Wrap_Filter filter, WriteableBitmap originalImage)
        {
            effect = new Effect(filter.filter);
            this.wrapFilter = filter;
            this.filterName = filter.filterName;
            this.thumbWidth = originalImage.PixelWidth;
            this.thumbHeight = originalImage.PixelHeight;
            this.originalImage = originalImage;
            thumbnailImg = new WriteableBitmap(thumbWidth, thumbHeight);
            ApplyEffect();
        }

        private async void ApplyEffect()
        {
            try
            {
                thumbnailImg = await effect.ApplyEffect(originalImage, thumbnailImg);
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