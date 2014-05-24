using Nokia.Graphics.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace FilterMage.Models
{
    public class Property<T> : INotifyPropertyChanged
    {
        protected string _PropertyName;
        public string PropertyName
        {
            get { return _PropertyName; }
            set
            {
                if (_PropertyName != value)
                {
                    _PropertyName = value;
                    NotifyPropertyChanged("PropertyName");
                }
            }
        }

        private T _Value;
        public T Value
        {
            get { return _Value; }
            set
            {
                //if (_Value != value)
                {
                    _Value = value;
                    NotifyPropertyChanged("Value");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                if ((App.Current as App).preview.Effect.canProceed())
                {
                    handler(this, new PropertyChangedEventArgs(propertyName));
                    /*if (!(App.Current as App).preview.Effect.renderFinished())
                    {
                        handler(this, new PropertyChangedEventArgs(propertyName));
                    }*/
                }
            }
        }
    }

    public class RangeProperty : Property<double>
    {
        private double _Max;
        public double Max
        {
            get { return _Max; }
            set
            {
                if (_Max != value)
                {
                    _Max = value;
                    NotifyPropertyChanged("Max");
                }
            }
        }
        private double _Min;
        public double Min
        {
            get { return _Min; }
            set
            {
                if (_Min != value)
                {
                    _Min = value;
                    NotifyPropertyChanged("Min");
                }
            }
        }
        
        public RangeProperty(string name, double value, double max, double min)
        {
            PropertyName = name;
            this.Value = value;
            this.Max = max;
            this.Min = min;
        }
    }

    public class BoolProperty : Property<bool>
    {
        public BoolProperty(string name, bool value)
        {
            PropertyName = name;
            this.Value = value;
        }
    }

    public class EnumProperty : Property<double>
    {
        public List<KeyValuePair<int, string>> Enums {get; set;}
        public EnumProperty(string name, double value)
        {
            PropertyName = name;
            this.Value = value;
            Enums = new List<KeyValuePair<int, string>>();
        }

        public void AddEnum(int enumValue, string enumName) {
            Enums.Add(new KeyValuePair<int, string>(enumValue, enumName));
        }
    }

    public abstract class Wrap_Filter
    {
        public string filterName;
        public IFilter filter;
        public List<BoolProperty> BoolProperties = new List<BoolProperty>();
        public List<RangeProperty> RangeProperties = new List<RangeProperty>();
        public List<EnumProperty> EnumProperties = new List<EnumProperty>();

        public bool isEditable()
        {
            if (BoolProperties.Count == 0 && RangeProperties.Count == 0 && EnumProperties.Count == 0)
                return false;
            return true;
        }

        public delegate void FilterRefreshedEventHandler();
        public event FilterRefreshedEventHandler FilterRefreshed;
        protected void NotifyFilterRefreshed()
        {
            FilterRefreshedEventHandler handler = FilterRefreshed;
            if (handler != null)
                handler();
        }
    }

    public class Wrap_AntiqueFilter : Wrap_Filter
    {
        public Wrap_AntiqueFilter()
        {
            filterName = "Antique";
            filter = new AntiqueFilter();
        }
    }

    public class Wrap_AutoEnhanceFilter : Wrap_Filter
    {
        public BoolProperty IsContrastAndBrightnessEnhancementEnabled;
        public BoolProperty IsLocalBoostEnhancementEnabled;
        public Wrap_AutoEnhanceFilter()
        {
            filterName = "Auto Enhance";
            IsContrastAndBrightnessEnhancementEnabled = new BoolProperty("Contrast & Brightness Enhancement", true);
            IsLocalBoostEnhancementEnabled = new BoolProperty("Local Boost Enhancement", true);
            IsContrastAndBrightnessEnhancementEnabled.PropertyChanged += IsContrastAndBrightnessEnhancementEnabled_PropertyChanged;
            IsLocalBoostEnhancementEnabled.PropertyChanged += IsLocalBoostEnhancementEnabled_PropertyChanged;
            filter = new AutoEnhanceFilter(IsContrastAndBrightnessEnhancementEnabled.Value, IsLocalBoostEnhancementEnabled.Value);
            BoolProperties.Add(IsContrastAndBrightnessEnhancementEnabled);
            BoolProperties.Add(IsLocalBoostEnhancementEnabled);
        }

        void IsContrastAndBrightnessEnhancementEnabled_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                try
                {
                    (filter as AutoEnhanceFilter).IsContrastAndBrightnessEnhancementEnabled = IsContrastAndBrightnessEnhancementEnabled.Value;
                    NotifyFilterRefreshed();
                }
                catch (Exception)
                {
                    //Ignore exception
                }
            }
        }

        void IsLocalBoostEnhancementEnabled_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                try
                {
                    (filter as AutoEnhanceFilter).IsLocalBoostEnhancementEnabled = IsLocalBoostEnhancementEnabled.Value;
                    NotifyFilterRefreshed();
                }
                catch (Exception)
                {
                    //Ignore exception
                }
            }
        }
    }
    
    public class Wrap_BlurFilter : Wrap_Filter
    {
        public RangeProperty KernelSize;
        public Wrap_BlurFilter()
        {
            filterName = "Blur";
            KernelSize = new RangeProperty("Blur Intensity", 10, 20, 1);
            KernelSize.PropertyChanged += KernelSize_PropertyChanged;
            filter = new BlurFilter((int)KernelSize.Value);
            RangeProperties.Add(KernelSize);
        }

        private void KernelSize_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                Debug.WriteLine("PropertyChanged");
                try
                {
                    (filter as BlurFilter).KernelSize = (int)KernelSize.Value;
                    NotifyFilterRefreshed();
                }
                catch (Exception)
                {
                    //Ignore exception
                }
            }
        }
    }

    public class Wrap_BrightnessFilter : Wrap_Filter
    {
        public RangeProperty Level;
        public Wrap_BrightnessFilter()
        {
            filterName = "Brightness";
            Level = new RangeProperty("Brightness Level", 1.5, 2.0, 0);
            Level.PropertyChanged += Level_PropertyChanged;
            filter = new BrightnessFilter(Level.Value - 1);
            RangeProperties.Add(Level);
        }

        void Level_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                try
                {
                    (filter as BrightnessFilter).Level = Level.Value - 1;
                    NotifyFilterRefreshed();
                }
                catch (Exception)
                {
                    //Ignore exception
                }
            }
        }
    }

    public class Wrap_CartoonFilter : Wrap_Filter
    {
        public BoolProperty DistinctEdges;
        public Wrap_CartoonFilter()
        {
            filterName = "Cartoon";
            DistinctEdges = new BoolProperty("Edges", false);
            DistinctEdges.PropertyChanged += DistinctEdges_PropertyChanged;
            filter = new CartoonFilter(DistinctEdges.Value);
            BoolProperties.Add(DistinctEdges);
        }

        private void DistinctEdges_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                try
                {
                    (filter as CartoonFilter).DistinctEdges = DistinctEdges.Value;
                    NotifyFilterRefreshed();
                }
                catch (Exception)
                {
                    //Ignore exception
                }
            }
        }
    }

    public class Wrap_ColorBoostFilter : Wrap_Filter
    {
        public RangeProperty Gain;
        public Wrap_ColorBoostFilter()
        {
            filterName = "Color Boost";
            Gain = new RangeProperty("Gain", 6, 20, -1);
            Gain.PropertyChanged += Gain_PropertyChanged;
            filter = new ColorBoostFilter(Gain.Value);
            RangeProperties.Add(Gain);
        }

        void Gain_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                try
                {
                    (filter as ColorBoostFilter).Gain = Gain.Value;
                    NotifyFilterRefreshed();
                }
                catch (Exception)
                {
                    //Ignore exception
                }
            }
        }
    }

    public class Wrap_ContrastFilter : Wrap_Filter
    {
        public RangeProperty Level;
        public Wrap_ContrastFilter()
        {
            filterName = "Contrast";
            Level = new RangeProperty("Contrast Level", 1.5, 2.0, 0);       //Actual range is -1 to 1.
            Level.PropertyChanged += Level_PropertyChanged;
            filter = new ContrastFilter(Level.Value - 1);
            RangeProperties.Add(Level);
        }

        void Level_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                try
                {
                    (filter as ContrastFilter).Level = Level.Value - 1;
                    NotifyFilterRefreshed();
                }
                catch (Exception)
                {
                    //Ignore exception
                }
            }
        }
    }

    public class Wrap_DespeckleFilter : Wrap_Filter
    {
        public EnumProperty Level;
        public Wrap_DespeckleFilter()
        {
            filterName = "Despeckle";
            Level = new EnumProperty("Despeckle Level", (double)DespeckleLevel.High);
            Level.AddEnum((int)DespeckleLevel.Minimum, "Minimum");
            Level.AddEnum((int)DespeckleLevel.Low, "Low");
            Level.AddEnum((int)DespeckleLevel.High, "High");
            Level.AddEnum((int)DespeckleLevel.Maximum, "Maximum");
            Level.PropertyChanged += Level_PropertyChanged;
            filter = new DespeckleFilter((DespeckleLevel)Level.Value);
            EnumProperties.Add(Level);
        }

        void Level_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                try
                {
                    (filter as DespeckleFilter).DespeckleLevel = (DespeckleLevel)Level.Value;
                    NotifyFilterRefreshed();
                }
                catch (Exception)
                {
                    //Ignore exception
                }
            }
        }
    }

    public class Wrap_EmbossFilter : Wrap_Filter
    {
        public RangeProperty Level;
        public Wrap_EmbossFilter()
        {
            filterName = "Emboss";
            Level = new RangeProperty("Emboss Level", .4, 1, 0);
            Level.PropertyChanged += Level_PropertyChanged;
            filter = new EmbossFilter(Level.Value);
            RangeProperties.Add(Level);
        }

        void Level_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                try
                {
                    (filter as EmbossFilter).Level = Level.Value;
                    NotifyFilterRefreshed();
                }
                catch (Exception)
                {
                    //Ignore exception
                }
            }
        }
    }

    public class Wrap_ExposureFilter : Wrap_Filter
    {
        public RangeProperty Gain;
        public EnumProperty Mode;
        public Wrap_ExposureFilter()
        {
            filterName = "Exposure";
            Gain = new RangeProperty("Gain", .5, 3, 0); //Actual range is -1.5 to 1.5
            Mode = new EnumProperty("Exposure Mode", (int)ExposureMode.Natural);
            Mode.AddEnum((int)ExposureMode.Natural, "Natural");
            Mode.AddEnum((int)ExposureMode.Gamma, "Gamma");
            Gain.PropertyChanged += Gain_PropertyChanged;
            Mode.PropertyChanged += Mode_PropertyChanged;
            filter = new ExposureFilter((ExposureMode)Mode.Value, Gain.Value - 1.5);
            RangeProperties.Add(Gain);
            EnumProperties.Add(Mode);
        }

        void Mode_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                try
                {
                    (filter as ExposureFilter).ExposureMode = (ExposureMode)Mode.Value;
                    NotifyFilterRefreshed();
                }
                catch (Exception)
                {
                    //Ignore exception
                }
            }
        }

        void Gain_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                try
                {
                    (filter as ExposureFilter).Gain = Gain.Value - 1.5;
                    NotifyFilterRefreshed();
                }
                catch (Exception)
                {
                    //Ignore exception
                }
            }
        }
    }

    public class Wrap_FlipFilter : Wrap_Filter
    {
        public EnumProperty Mode;
        public Wrap_FlipFilter()
        {
            filterName = "Flip";
            Mode = new EnumProperty("Flip Mode", (int)FlipMode.Horizontal);
            Mode.AddEnum((int)FlipMode.None, "No flip");
            Mode.AddEnum((int)FlipMode.Vertical, "Vertical flip");
            Mode.AddEnum((int)FlipMode.Horizontal, "Horizontal flip");
            Mode.AddEnum((int)FlipMode.Both, "Both ways flip");
            Mode.PropertyChanged += Mode_PropertyChanged;
            filter = new FlipFilter((FlipMode)Mode.Value);
            EnumProperties.Add(Mode);
        }

        void Mode_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                try
                {
                    (filter as FlipFilter).FlipMode = (FlipMode)Mode.Value;
                    NotifyFilterRefreshed();
                }
                catch (Exception)
                {
                    //Ignore exception
                }
            }
        }
    }

    public class Wrap_FogFilter : Wrap_Filter
    {
        public Wrap_FogFilter()
        {
            filterName = "Foggy";
            filter = new FogFilter();
        }
    }

    public class Wrap_GrayscaleFilter : Wrap_Filter
    {
        public Wrap_GrayscaleFilter()
        {
            filterName = "Grayscale";
            filter = new GrayscaleFilter();
        }
    }

    public class Wrap_GrayscaleNegativeFilter : Wrap_Filter
    {
        public Wrap_GrayscaleNegativeFilter()
        {
            filterName = "Grayscale Negative";
            filter = new GrayscaleNegativeFilter();
        }
    }

    public class Wrap_LomoFilter : Wrap_Filter
    {
        public RangeProperty Brightness;
        public RangeProperty Saturation;
        public EnumProperty Style;
        public EnumProperty Vignetting;
        public Wrap_LomoFilter()
        {
            filterName = "Lomo";
            Brightness = new RangeProperty("Brightness", .3, 1, 0);
            Saturation = new RangeProperty("Saturation", .5, 1, 0);
            Style = new EnumProperty("Lomo Style", (double)LomoStyle.Neutral);
            Vignetting = new EnumProperty("Vignetting", (double)LomoVignetting.Medium);
            Style.AddEnum((int)LomoStyle.Neutral, "Neutral");
            Style.AddEnum((int)LomoStyle.Red, "Red");
            Style.AddEnum((int)LomoStyle.Green, "Green");
            Style.AddEnum((int)LomoStyle.Blue, "Blue");
            Style.AddEnum((int)LomoStyle.Yellow, "Yellow");
            Vignetting.AddEnum((int)LomoVignetting.Low, "Low");
            Vignetting.AddEnum((int)LomoVignetting.Medium, "Medium");
            Vignetting.AddEnum((int)LomoVignetting.High, "High");
            Brightness.PropertyChanged += Brightness_PropertyChanged;
            Saturation.PropertyChanged += Saturation_PropertyChanged;
            Style.PropertyChanged += Style_PropertyChanged;
            Vignetting.PropertyChanged += Vignetting_PropertyChanged;
            filter = new LomoFilter(Brightness.Value, Saturation.Value, (LomoVignetting)Vignetting.Value, (LomoStyle)Style.Value);
            RangeProperties.Add(Brightness);
            RangeProperties.Add(Saturation);
            EnumProperties.Add(Style);
            EnumProperties.Add(Vignetting);
        }

        void Vignetting_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                try
                {
                    (filter as LomoFilter).LomoVignetting = (LomoVignetting)Vignetting.Value;
                    NotifyFilterRefreshed();
                }
                catch (Exception)
                {
                    //Ignore exception
                }
            }
        }

        void Style_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                try
                {
                    (filter as LomoFilter).LomoStyle = (LomoStyle)Style.Value;
                    NotifyFilterRefreshed();
                }
                catch (Exception)
                {
                    //Ignore exception
                }
            }
        }

        void Saturation_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                try
                {
                    (filter as LomoFilter).Saturation = Saturation.Value;
                    NotifyFilterRefreshed();
                }
                catch (Exception)
                {
                    //Ignore exception
                }
            }
        }

        void Brightness_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                try
                {
                    (filter as LomoFilter).Brightness = Brightness.Value;
                    NotifyFilterRefreshed();
                }
                catch (Exception)
                {
                    //Ignore exception
                }
            }
        }
    }

    public class Wrap_MagicPenFilter : Wrap_Filter
    {
        public Wrap_MagicPenFilter()
        {
            filterName = "Magic Pen";
            filter = new MagicPenFilter();
        }
    }

    public class Wrap_MilkyFilter : Wrap_Filter
    {
        public Wrap_MilkyFilter()
        {
            filterName = "Milky";
            filter = new MilkyFilter();
        }
    }

    public class Wrap_MirrorFilter : Wrap_Filter
    {
        public Wrap_MirrorFilter()
        {
            filterName = "Mirror";
            filter = new MirrorFilter();
        }
    }

    public class Wrap_MoonlightFilter : Wrap_Filter
    {
        public RangeProperty Clock;
        public Wrap_MoonlightFilter()
        {
            filterName = "Moon Lighting";
            Clock = new RangeProperty("Time of night", 12, 23, 0);
            Clock.PropertyChanged += Clock_PropertyChanged;
            filter = new MoonlightFilter((int)Clock.Value);
            RangeProperties.Add(Clock);
        }

        void Clock_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                (filter as MoonlightFilter).Clock = (int)Clock.Value;
                NotifyFilterRefreshed();
            }
        }
    }

    public class Wrap_NegativeFilter : Wrap_Filter
    {
        public Wrap_NegativeFilter()
        {
            filterName = "Negate";
            filter = new NegativeFilter();
        }
    }

    public class Wrap_OilyFilter : Wrap_Filter
    {
        public Wrap_OilyFilter()
        {
            filterName = "Oily";
            filter = new OilyFilter();
        }
    }

    public class Wrap_PaintFilter : Wrap_Filter
    {
        public RangeProperty Level;
        public Wrap_PaintFilter()
        {
            filterName = "Paint";
            Level = new RangeProperty("Paint Level", 2, 4, 1);
            Level.PropertyChanged += Level_PropertyChanged;
            filter = new PaintFilter((int)Level.Value);
            RangeProperties.Add(Level);
        }

        void Level_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                try
                {
                    (filter as PaintFilter).Level = (int)Level.Value;
                    NotifyFilterRefreshed();
                }
                catch (Exception)
                {
                    //Ignore Exception
                }
            }
        }
    }

    public class Wrap_PosterizeFilter : Wrap_Filter
    {
        public RangeProperty ColorComponentValueCount;
        public Wrap_PosterizeFilter()
        {
            filterName = "Posterize";
            ColorComponentValueCount = new RangeProperty("Color component count", 6, 16, 2);
            ColorComponentValueCount.PropertyChanged += ColorComponentValueCount_PropertyChanged;
            filter = new PosterizeFilter((int)ColorComponentValueCount.Value);
            RangeProperties.Add(ColorComponentValueCount);
        }

        void ColorComponentValueCount_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                try
                {
                    (filter as PosterizeFilter).ColorComponentValueCount = (int)ColorComponentValueCount.Value;
                    NotifyFilterRefreshed();
                }
                catch (Exception)
                {
                    //Ignore exception
                }
            }
        }
    }

    public class Wrap_SepiaFilter : Wrap_Filter
    {
        public Wrap_SepiaFilter()
        {
            filterName = "Sepia";
            filter = new SepiaFilter();
        }
    }

    public class Wrap_SharpnessFilter : Wrap_Filter
    {
        public RangeProperty Level;
        public Wrap_SharpnessFilter()
        {
            filterName = "Sharper";
            Level = new RangeProperty("Sharpness", 3, 7, 0);
            Level.PropertyChanged += Level_PropertyChanged;
            filter = new SharpnessFilter((int)Level.Value);
            RangeProperties.Add(Level);
        }

        void Level_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                try
                {
                    (filter as SharpnessFilter).Level = (int)Level.Value;
                    NotifyFilterRefreshed();
                }
                catch (Exception)
                {
                    //Ignore exception
                }
            }
        }
    }
    
    public class Wrap_SketchFilter : Wrap_Filter
    {
        public EnumProperty Mode;
        public Wrap_SketchFilter()
        {
            filterName = "Sketchy";
            Mode = new EnumProperty("Sketch Mode", (double)SketchMode.Color);
            Mode.AddEnum((int)SketchMode.Gray, "Gray");
            Mode.AddEnum((int)SketchMode.Color, "Color");
            Mode.PropertyChanged += Mode_PropertyChanged;
            filter = new SketchFilter((SketchMode)Mode.Value);
            EnumProperties.Add(Mode);
        }

        void Mode_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                try
                {
                    (filter as SketchFilter).SketchMode = (SketchMode)Mode.Value;
                    NotifyFilterRefreshed();
                }
                catch (Exception)
                {
                    //Ignore exception
                }
            }
        }
    }

    public class Wrap_SolarizeFilter : Wrap_Filter
    {
        public RangeProperty Threshold;
        public Wrap_SolarizeFilter()
        {
            filterName = "Solarize";
            Threshold = new RangeProperty("Threshold", .3, 1, 0);
            Threshold.PropertyChanged += Threshold_PropertyChanged;
            filter = new SolarizeFilter(Threshold.Value);
            RangeProperties.Add(Threshold);
        }

        void Threshold_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                try
                {
                    (filter as SolarizeFilter).Threshold = Threshold.Value;
                    NotifyFilterRefreshed();
                }
                catch (Exception)
                {
                    //Ignore exception
                }
            }
        }
    }

    public class Wrap_StampFilter : Wrap_Filter
    {
        public RangeProperty Smoothness;
        public RangeProperty Threshold;
        public Wrap_StampFilter()
        {
            filterName = "Stamp";
            Smoothness = new RangeProperty("Smoothness", 2, 6, 0);
            Threshold = new RangeProperty("Threshold", .3, 1, 0);
            Smoothness.PropertyChanged += Smoothness_PropertyChanged;
            Threshold.PropertyChanged += Threshold_PropertyChanged;
            filter = new StampFilter((int)Smoothness.Value, Threshold.Value);
            RangeProperties.Add(Smoothness);
            RangeProperties.Add(Threshold);
        }

        void Threshold_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                try
                {
                    (filter as StampFilter).Threshold = Threshold.Value;
                    NotifyFilterRefreshed();
                }
                catch
                {
                    //Ignore exception
                }
            }
        }

        void Smoothness_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                try
                {
                    (filter as StampFilter).Smoothness = (int)Smoothness.Value;
                    NotifyFilterRefreshed();
                }
                catch
                {
                    //Ignore exception
                }
            }
        }
    }

    public class Wrap_TemperatureAndTintFilter : Wrap_Filter
    {
        public RangeProperty Temperature;
        public RangeProperty Tint;
        public Wrap_TemperatureAndTintFilter()
        {
            filterName = "Temperature & Tint";
            Temperature = new RangeProperty("Temperature", 1.4, 2, 0);   //Actual range is -1 to 1
            Tint = new RangeProperty("Tint", 1.4, 2, 0); //Actual range is -1 to 1
            Temperature.PropertyChanged += Temperature_PropertyChanged;
            Tint.PropertyChanged += Tint_PropertyChanged;
            filter = new TemperatureAndTintFilter(Temperature.Value - 1, Tint.Value - 1);
            RangeProperties.Add(Temperature);
            RangeProperties.Add(Tint);
        }

        void Tint_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                try
                {
                    (filter as TemperatureAndTintFilter).Tint = Tint.Value - 1;
                    NotifyFilterRefreshed();
                }
                catch (Exception)
                {
                    //Ignore exception
                }
            }
        }

        void Temperature_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                try
                {
                    (filter as TemperatureAndTintFilter).Temperature = Temperature.Value - 1;
                    NotifyFilterRefreshed();
                }
                catch (Exception)
                {
                    //Ignore exception
                }
            }
        }
    }

    public class Wrap_WatercolorFilter : Wrap_Filter
    {
        public RangeProperty ColorIntensity;
        public RangeProperty LightIntensity;
        public Wrap_WatercolorFilter()
        {
            filterName = "Water Color";
            ColorIntensity = new RangeProperty("Color Intensity", .5, 1, 0);
            LightIntensity = new RangeProperty("Light Intensity", .5, 1, 0);
            ColorIntensity.PropertyChanged += ColorIntensity_PropertyChanged;
            LightIntensity.PropertyChanged += LightIntensity_PropertyChanged;
            filter = new WatercolorFilter(LightIntensity.Value, ColorIntensity.Value);
            RangeProperties.Add(ColorIntensity);
            RangeProperties.Add(LightIntensity);
        }

        void LightIntensity_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                try
                {
                    (filter as WatercolorFilter).LightIntensity = LightIntensity.Value;
                    NotifyFilterRefreshed();
                }
                catch (Exception)
                {
                    //Ignore exception
                }
            }
        }

        void ColorIntensity_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                try
                {
                    (filter as WatercolorFilter).ColorIntensity = ColorIntensity.Value;
                    NotifyFilterRefreshed();
                }
                catch (Exception)
                {
                    //Ignore exception
                }
            }
        }
    }

    public class Wrap_WhiteboardEnhancementFilter : Wrap_Filter
    {
        public EnumProperty Mode;
        public Wrap_WhiteboardEnhancementFilter()
        {
            filterName = "Whiteboard Enhance";
            Mode = new EnumProperty("Enhancement Mode", (double)WhiteboardEnhancementMode.Hard);
            Mode.AddEnum((int)WhiteboardEnhancementMode.Hard, "Hard");
            Mode.AddEnum((int)WhiteboardEnhancementMode.Soft, "Soft");
            Mode.PropertyChanged += Mode_PropertyChanged;
            filter = new WhiteboardEnhancementFilter((WhiteboardEnhancementMode)Mode.Value);
            EnumProperties.Add(Mode);
        }

        void Mode_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                try
                {
                    (filter as WhiteboardEnhancementFilter).WhiteboardEnhancementMode = (WhiteboardEnhancementMode)Mode.Value;
                    NotifyFilterRefreshed();
                }
                catch (Exception)
                {
                    //Ignore Exception
                }
            }
        }
    }
}