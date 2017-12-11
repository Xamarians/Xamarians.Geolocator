using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarians.GPS
{
   public static class GPSService
    {
        static IGPSService _instance;
        public static IGPSService Instance
        {
            get
            {
                return _instance;
            }
        }

        internal static void Init(IGPSService gps)
        {
            _instance = gps;
        }
    }
}
