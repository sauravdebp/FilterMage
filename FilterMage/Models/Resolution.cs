namespace FilterMage.Models
{
    public enum fixedDim { WIDTH, HEIGHT };
    public class Resolution
    {
        public int width;
        public int height;
        
        public Resolution(int currentWidth, int currentHeight, int target, fixedDim mode)
        {
            if (mode == fixedDim.HEIGHT)
            {
                height = target;
                float scale = (float)height / (float)currentHeight;
                width = (int)(currentWidth * scale);
            }
            else
            {
                width = target;
                float scale = (float)width / (float)currentWidth;
                height = (int)(currentHeight * scale);
            }
        }
    }
}
