using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterOfWebM
{
    class Helper
    {
        /// <summary>
        /// This function intakes the time format, so it can convert it to flat seconds
        /// </summary>
        /// <param name="input">A string that is formatted as HH:MM:SS</param>
        /// <returns>The seconds.</returns>
        public static double convertToSeconds(String input) {
            string[] time = input.Split(':');
            return Convert.ToDouble(time[0]) * 3600 + Convert.ToDouble(time[1]) * 60 + Convert.ToDouble(time[2]);
        }
    }
}
