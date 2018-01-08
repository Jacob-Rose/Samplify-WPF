using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveFormRendererLib;

namespace Samplify
{
    [Serializable]
    public struct SampleInfo
    {
        public long fileSize;
        public PeakInfo[] peakInfo;
        public SampleInfo(long fileSize, PeakInfo[] peakInfo)
        {
            this.fileSize = fileSize;
            this.peakInfo = peakInfo;
        }
    }
}
