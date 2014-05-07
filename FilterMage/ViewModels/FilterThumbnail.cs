using Nokia.Graphics.Imaging;
using Nokia.InteropServices.WindowsRuntime;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FilterMage.ViewModels
{
    public class FilterThumbnail : INotifyPropertyChanged
    {
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
        private IFilter filter = null;
        private Stream originalImageStream = null;

        public FilterThumbnail(IFilter filter, Stream originalImage)
        {
            this.filter = filter;
            this.originalImageStream = originalImage;
            ApplyEffect();
        }

        private async void ApplyEffect()
        {
            originalImageStream.Position = 0;
            WriteableBitmap bmp = new WriteableBitmap(200, 200);
            bmp.SetSource(originalImageStream);
            try
            {
                using (var imageStream = new BitmapImageSource(bmp.AsBitmap()))
                {
                    using (var effect = new FilterEffect(imageStream))
                    {
                        List<IFilter> filters = new List<IFilter>();
                        filters.Add(filter);
                        effect.Filters = filters;
                        thumbnailImg = new WriteableBitmap(200, 200);
                        using (var renderer = new WriteableBitmapRenderer(effect, thumbnailImg))
                        {
                            thumbnailImg = await renderer.RenderAsync();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + " in FilterThumbnail");
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
            return "HELLO";
        }
    }
}