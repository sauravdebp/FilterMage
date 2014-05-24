using FilterMage.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FilterMage.ViewModels
{
    public class Preview : INotifyPropertyChanged
    {
        public enum SCALE
        {
            FIT_SCREEN,
            FILL_SCREEN
        };

        private SCALE previewScale = SCALE.FIT_SCREEN;
        private Stream originalImageStream = null;
        private WriteableBitmap originalPreviewImage = null;
        public WriteableBitmap OriginalPreview
        {
            get { return originalPreviewImage; }
        }
        private int previewWidth;
        private int previewHeight;
        private int originalPreviewHeight;
        private int originalPreviewWidth;
        private List<WriteableBitmap> prevPreviews = new List<WriteableBitmap>(10);
        private List<Wrap_Filter> activeFilters = null;

        private WriteableBitmap fullResImage = null;
        public WriteableBitmap previewImage = null;
        private Effect eff = null;
        public Effect Effect
        {
            get { return eff; }
        }

        public int NoofFilters
        {
            get { return activeFilters.Count; }
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

        public Preview(Stream image, int width, int height)
        {
            originalImageStream = image;
            originalPreviewWidth = width;
            originalPreviewHeight = height;
            activeFilters = new List<Wrap_Filter>();
            eff = new Effect();
            CreatePreviewFromStream();
        }

        private WriteableBitmap CreatePreviewFromStream()
        {
            originalImageStream.Position = 0;
            originalPreviewImage = new WriteableBitmap(previewWidth, previewHeight);
            originalPreviewImage.SetSource(originalImageStream);

            ScalePreviewImageRes(SCALE.FIT_SCREEN);
            previewImage = new WriteableBitmap(originalPreviewImage);
            return originalPreviewImage;
        }

        private void ScalePreviewImageRes(SCALE scale)
        {
            previewScale = scale;
            int target = originalPreviewHeight;
            fixedDim fixedFlag = fixedDim.HEIGHT;
            if (originalPreviewImage.PixelWidth > originalPreviewImage.PixelHeight)
            {
                target = originalPreviewWidth;
                fixedFlag = fixedDim.WIDTH;
            }
            if (scale == SCALE.FILL_SCREEN)
            {
                if (fixedFlag == fixedDim.HEIGHT)
                    fixedFlag = fixedDim.WIDTH;
                else
                    fixedFlag = fixedDim.HEIGHT;
            }
            Resolution dim = new Resolution(originalPreviewImage.PixelWidth, originalPreviewImage.PixelHeight, target, fixedFlag);
            originalPreviewImage = originalPreviewImage.Resize(dim.width, dim.height, WriteableBitmapExtensions.Interpolation.Bilinear);
            previewWidth = dim.width;
            previewHeight = dim.height;
        }

        private async Task<WriteableBitmap> ApplyEffect()
        {
            WriteableBitmap tmp = new WriteableBitmap(previewWidth, previewHeight);
            await eff.ApplyEffect(originalPreviewImage, tmp);
            previewImage = tmp;
            return previewImage;
        }

        public async Task<WriteableBitmap> ApplyFilter(Wrap_Filter filter)
        {
            activeFilters.Add(filter);
            //prevPreviews.Add(previewImage);
            eff.addFilter(filter.filter);
            NotifyPropertyChanged("LastFilter");
            NotifyPropertyChanged("NoofFilters");
            return await ApplyEffect();
        }

        public async Task<WriteableBitmap> UndoLastFilter()
        {
            if (activeFilters.Count == 0)
                return null;
            activeFilters.RemoveAt(activeFilters.Count - 1);
            /*if (prevPreviews.Count > 0)
            {
                previewImage = prevPreviews.Last();
                prevPreviews.Remove(prevPreviews.Last());
            }*/
            //else
            {
                //previewImage = CreatePreviewFromStream();
                eff.remFilter();
                await ApplyEffect();
            }
            NotifyPropertyChanged("LastFilter");
            NotifyPropertyChanged("NoofFilters");
            return previewImage;
        }

        public Wrap_Filter GetLastFilter()
        {
            if(activeFilters.Count > 0)
                return activeFilters.Last();
            return null;
        }

        public async Task<WriteableBitmap> ClearAllFilters()
        {
            try
            {
                activeFilters.Clear();
                prevPreviews.Clear();
                eff.clearFilters();
                await ApplyEffect();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " in Preview.cs ClearAllFilters()");
            }
            NotifyPropertyChanged("LastFilter");
            NotifyPropertyChanged("NoofFilters");
            return originalPreviewImage;
        }

        public async Task<WriteableBitmap> CreateFullResPreview()
        {
            WriteableBitmap tempFullRes = new WriteableBitmap(0, 0);
            tempFullRes.SetSource(originalImageStream);
            if (fullResImage == null)
            {
                fullResImage = new WriteableBitmap(tempFullRes.PixelWidth, tempFullRes.PixelHeight);
            }
            fullResImage = await eff.ApplyEffect(tempFullRes , fullResImage);
            tempFullRes = null;
            return fullResImage;
        }

        public async Task<SCALE> TogglePreviewRes()
        {
            originalPreviewImage.SetSource(originalImageStream);
            if (previewScale == SCALE.FILL_SCREEN)
            {
                ScalePreviewImageRes(SCALE.FIT_SCREEN);
            }
            else
            {
                ScalePreviewImageRes(SCALE.FILL_SCREEN);
            }
            await ApplyEffect();
            return previewScale;
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
