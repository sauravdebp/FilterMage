using Nokia.Graphics.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterMage.Models
{
    public enum PropType { RANGE, BOOL, ENUM };

    public class FilterProperty
    {
        protected PropType PropType;
    }

    public class BlurFilterProperty : FilterProperty, IFilter
    {
        public int KernelSize = 100;
        public int max;
        public int min;
        public BlurFilterProperty()
        {
            PropType = PropType.RANGE;
        }
    }

    public class CartoonFilterProperty : FilterProperty
    {
        public bool DistinctEdges = false;
        public CartoonFilterProperty()
        {
            PropType = PropType.BOOL;
        }
    }

    public class ColorBoostFilterProperty : FilterProperty
    {
        public double Gain = 10.0;
        public double max;
        public double min;
        public ColorBoostFilterProperty()
        {
            PropType = PropType.RANGE;
        }
    }

    public class ContrastFilterProperty : FilterProperty
    {
        public double ContrastLevel = .5;
        public double max;
        public double min;
        public ContrastFilterProperty()
        {
            PropType = PropType.RANGE;
        }
    }

    public class DespeckleFilterProperty : FilterProperty
    {
        public DespeckleLevel DespeckleLevel = DespeckleLevel.High;
        public DespeckleFilterProperty()
        {
            PropType = PropType.ENUM;
        }
    }

    public class EmbossFilterProperty : FilterProperty
    {
        public double Level = .5;
        public double max;
        public double min;
        public EmbossFilterProperty()
        {
            PropType = PropType.RANGE;
        }
    }

    public class FlipFilterProperty : FilterProperty
    {
        public FlipMode FlipMode = FlipMode.Horizontal;
        public FlipFilterProperty()
        {
            PropType = PropType.ENUM;
        }
    }
}
