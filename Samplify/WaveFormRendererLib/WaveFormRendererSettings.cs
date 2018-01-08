using System.Windows.Media;

namespace WaveFormRendererLib
{
    public class WaveFormRendererSettings
    {
        protected WaveFormRendererSettings()
        {
            Width = 800;
            PixelsPerPeak = 1;
            SpacerPixels = 0;
            //BackgroundColor = Color.Beige;
        }

        // for display purposes only
        public string Name { get; set; }

        public int Width { get; set; }

        public int PixelsPerPeak { get; set; }
        public int SpacerPixels { get; set; }
        public bool DecibelScale { get; set; }

        //public Image BackgroundImage { get; set; }
        
    }
}