using Nokia.Graphics.Imaging;
using Nokia.InteropServices.WindowsRuntime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FilterMage.Models
{
    public class Effect
    {
        private enum STATE
        {
            WAIT,
            APPLY,
            SCHEDULE
        };

        private STATE currentState = STATE.WAIT;
        /*public STATE CurrentState
        {
            get { return currentState; }
        }*/
        private List<IFilter> filters = null;

        public Effect()
        {
            filters = new List<IFilter>();
        }

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

        public void remFilter()
        {
            if (filters != null)
            {
                filters.RemoveAt(filters.Count - 1);
            }
            else
            {
                throw new Exception("Effect filters unassigned");
            }
        }


        public void clearFilters()
        {
            if (filters != null)
            {
                filters.Clear();
            }
            else
            {
                throw new Exception("Effect filters unassigned");
            }
        }
        public async Task<WriteableBitmap> ApplyEffect(WriteableBitmap sourceImage, WriteableBitmap effectImage)
        {
            try
            {
                using (var imageStream = new BitmapImageSource(sourceImage.AsBitmap()))
                using (var effect = new FilterEffect(imageStream))
                using (var renderer = new WriteableBitmapRenderer(effect, effectImage))
                {
                    //MessageBox.Show("Filters : " + filters.Count);  
                    if (canProceedWithRender())
                    {
                        effect.Filters = filters;
                        effectImage = await renderer.RenderAsync();
                        if (!renderFinished())
                        {
                            effectImage = await ApplyEffect(sourceImage, effectImage);
                        }
                    }
                    else
                    {
                        return sourceImage;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " in Effect::ApplyEffect()");
            }
            return effectImage;
        }

        public bool canProceedWithRender()
        {
            bool status = false;
            switch (currentState)
            {
                case STATE.WAIT :
                    currentState = STATE.APPLY;
                    status = true;
                    break;
                case STATE.APPLY:
                    currentState = STATE.SCHEDULE;
                    break;
                case STATE.SCHEDULE:
                    currentState = STATE.SCHEDULE;
                    break;
            }
            return status;
        }

        public bool renderFinished()
        {
            bool status = true;
            switch (currentState)
            {
                case STATE.APPLY:
                    currentState = STATE.WAIT;
                    break;
                case STATE.SCHEDULE:
                    currentState = STATE.WAIT;
                    status = false;
                    break;
            }
            return status;
        }

        public bool canProceed()
        {
            if (currentState == STATE.WAIT)
                return true;
            return false;
        }
    }
}
