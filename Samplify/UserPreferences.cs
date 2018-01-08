using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Samplify
{
    public class UserPreferences
    {
        public static Brush defaultSampleColor = Brushes.Cyan;
        public static Brush baseUIColor = Brushes.Black;
        public static Brush baseTextColor = Brushes.White;
        
        public static int waveformPointCount = 128;
    }
}
