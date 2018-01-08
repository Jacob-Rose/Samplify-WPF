using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using WaveFormRendererLib;

namespace Samplify
{
    class PeakToPointConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            PeakInfo[] waveform = value[0] as PeakInfo[];

            FrameworkElement element = (value[1]) as FrameworkElement;

            double multiplier = (double)value[2];
           
            PointCollection points = new PointCollection();

            for(int i = 0; i < waveform.Length; i++)
            {
                
                Point p = new Point();
                p.X = (element.ActualWidth / waveform.Length) * i;
                if (multiplier > 0)
                {
                    p.Y = (element.ActualHeight / 2) + (waveform[i].Max * element.ActualHeight / 2);
                }
                else
                {
                    p.Y = (element.ActualHeight / 2) + (waveform[i].Min * element.ActualHeight / 2);
                }
                
                
                points.Add(p);
            }
            return points;

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
