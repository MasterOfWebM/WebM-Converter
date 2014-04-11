using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        /// <returns>The bitrate in kilobits</returns>
        public static double calcBitrate(String size, String length)
        {
            return Math.Floor(Convert.ToDouble(size) * BITCONVERSION / Convert.ToDouble(length));
        }

        /// <summary>
        /// Obtains the file size of a given file
        /// </summary>
        /// <param name="file">The file that needs to be calculated</param>
        /// <returns>The file size of a given file</returns>
        public static double getFileSize(String file)
        {
            FileInfo fi = new FileInfo(@file);

            double fileSize = fi.Length;

            return Math.Round(fileSize / 1024, 2);
        }

        /// <summary>
        /// Calls ffmpeg to encode the video
        /// </summary>
        /// <param name="command">The base command string (passes are entered automatically by this class)</param>
        /// <param name="fileOutput">The path to the output</param>
        public static void encodeVideo(String command, String fileOutput)
        {
            String commandPass1 = "-pass 1 -f webm NUL";
            String commandPass2 = "-pass 2 ";

            // Pass 1
            var pass1 = Process.Start("ffmpeg", command + commandPass1);
            pass1.WaitForExit();

            // Pass 2
            var pass2 = Process.Start("ffmpeg", command + commandPass2 + "\"" + fileOutput + "\"");
            pass2.WaitForExit();
        }
    }
}
