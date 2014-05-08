namespace FilterMage.Models
{
    public class Resolution
    {
        public int width;
        public int height;

        public Resolution(int currentWidth, int currentHeight, int targetHeight)
        {
            height = targetHeight;
            float scale = (float)height / (float)currentHeight;
            width = (int)(currentWidth * scale);
        }
    }
}
