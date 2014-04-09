using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterOfWebM
{
    class Helper
    {
        private const int BITCONVERSION = 8 * 1024;                         // Converts the filesize to Kilobits

        /// <summary>
        /// This function intakes the time format, so it can convert it to flat seconds
        /// </summary>
        /// <param name="input">A string that is formatted as HH:MM:SS</param>
        /// <returns>The seconds.</returns>
        public static double convertToSeconds(String input) {
            string[] time = input.Split(':');
            return Convert.ToDouble(time[0]) * 3600 + Convert.ToDouble(time[1]) * 60 + Convert.ToDouble(time[2]);
        }

        /// <summary>
        /// This function calculates the bitrate required to fit into a file size.
        /// </summary>
        /// <param name="size">The requested file size in MB</param>
        /// <param name="length">The length of the footage in seconds</param>
        /// <returns></returns>
        public static String calcBitrate(String size, String length)
        {
            return Convert.ToString(Math.Floor(Convert.ToDouble(size) * BITCONVERSION / Convert.ToDouble(length))) + "K";
        }
    }
}
