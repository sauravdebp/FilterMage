using Nokia.Graphics.Imaging;
using Nokia.InteropServices.WindowsRuntime;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FilterMage.Models
{
    public class Effect
    {
        public List<IFilter> filters = null;

        public Effect(IFilter filter)
        {
            filters = new List<IFilter>();
            filters.Add(filter);
        }

        public Effect(List<IFilter> effectFilters = null)
        {
            filters = new List<IFilter>(effectFilters);
        }

        public void addFilter(IFilter filter)
        {
            if (filters == null)
                filters = new List<IFilter>();
            filters.Add(filter);
        }

        public async Task<WriteableBitmap> ApplyEffect(WriteableBitmap sourceImage, WriteableBitmap effectImage)
        {
            try
            {
                using (var imageStream = new BitmapImageSource(sourceImage.AsBitmap()))
                {
                    using (var effect = new FilterEffect(imageStream))
                    {
                        effect.Filters = filters;
                        using (var renderer = new WriteableBitmapRenderer(effect, effectImage))
                        {
                            effectImage = await renderer.RenderAsync();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + " in Effect::ApplyEffect()");
            }
            return effectImage;
        }
    }
}
