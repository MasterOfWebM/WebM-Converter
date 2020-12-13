using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace MasterOfWebM
{
    class Helper
    {
        private const int BITCONVERSION = 8 * 1024;                         // Converts the filesize to Kilobits

        // Version check variables
        private static String downloadUrl = "";
        private static Version newVersion = null;
        private static String xmlUrl = "https://raw.githubusercontent.com/MasterOfWebM/WebM-Converter/master/update.xml";
        private static XmlTextReader reader = null;

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

        /// <summary>
        /// Checks to see if ffmpeg has a font.conf installed, and if it doesn't
        /// it will install one for the user to support subtitles
        /// </summary>
        /// <returns>If the current FFmpeg installation has a font config installed</returns>
        public static bool checkFFmpegFontConfig()
        {
            // Spawn process to check if ffmpeg is installed and find out where it is
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "cmd";
            p.StartInfo.Arguments = "/k where ffmpeg & exit";
            p.StartInfo.CreateNoWindow = true;
            p.Start();

            string output = p.StandardOutput.ReadToEnd();
            int ffmpegCount = output.Count(f => f == '.');

            if(ffmpegCount >=2) // Multiple paths found
            {
                int indexOfSecondPath = output.LastIndexOf("C:\\");
                string newOutput = output.Substring(0, indexOfSecondPath);
                output = newOutput;

                MessageBox.Show("Multiple FFmpeg instances found. Defaulted to Root.\n" +
                                "Check root folder and PATH folder for duplicates.", "Warning",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            p.WaitForExit();

            if (output == "")
            {
                MessageBox.Show("FFmpeg is not installed, please either put it in the same directory\n"+
                                "as this program or in your 'PATH' Environment Variable.", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
            else
            {
                // Get rid of the newline at the end of the output
                output = output.Replace(Environment.NewLine, "");

                // Get the root directory of ffmpeg
                output = Path.GetDirectoryName(@output);

                if (File.Exists(output + "\\fonts\\fonts.conf"))
                {
                    return true;
                }
                else
                {
                    if (Directory.Exists(output + "\\fonts"))
                    {
                        // If the directory actually exists, just write the config file
                        try
                        {
                            File.Copy("fonts\\fonts.conf", output + "\\fonts\\fonts.conf");

                            return true;
                        }
                        catch (Exception ex)
                        {
                            if (ex is UnauthorizedAccessException)
                            {
                                MessageBox.Show("Failed to create the fonts config due to\n" +
                                    "Unathorized Access. Please start this program with Administrator\n" +
                                    "privileges.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                                return false;
                            }
                            else
                            {
                                MessageBox.Show("Something went wrong with writing the config\n" +
                                "file. Please message Master Of WebM to figure it out.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                                return false;
                            }
                        }
                    }
                    else
                    {
                        // If neither the directory, or file exists, then create them both
                        try
                        {
                            Directory.CreateDirectory(output + "\\fonts");
                            File.Copy("fonts\\fonts.conf", output + "\\fonts\\fonts.conf");

                            return true;
                        }
                        catch (Exception ex)
                        {
                            if (ex is UnauthorizedAccessException)
                            {
                                MessageBox.Show("Failed to create the fonts config due to\n" +
                                    "Unathorized Access. Please start this program with Administrator\n" +
                                    "privileges.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                                return false;
                            }
                            else
                            {
                                MessageBox.Show("Something went wrong with writing the config\n" +
                                "file. Please message Master Of WebM to figure it out.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                                return false;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Verifies the version of the program.
        /// It will prompt the user if the program is
        /// outdated.
        /// </summary>
        public static void checkUpdate()
        {
            try
            {
                reader = new XmlTextReader(xmlUrl);
                reader.MoveToContent();
                string elementName = "";

                if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "webmconverter"))
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            elementName = reader.Name;
                        }
                        else
                        {
                            if ((reader.NodeType == XmlNodeType.Text) && (reader.HasValue))
                            {
                                switch (elementName)
                                {
                                    case "version":
                                        newVersion = new Version(reader.Value);
                                        break;
                                    case "url":
                                        downloadUrl = reader.Value;
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Error out to not disrupt the user
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }

            // Current version of the application
            Version appVersion = Assembly.GetExecutingAssembly().GetName().Version;

            if (appVersion.CompareTo(newVersion) < 0)
            {
                if (MessageBox.Show("You are currently out of date.\nWould you like to update now?", "Version out of date", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    var update = Process.Start(downloadUrl);
                }
            }
        }

        /// <summary>
        /// Checks if a subtitle (subs.ass | subs.srt) exists
        /// and deletes it.
        /// </summary>
        public static void subsCheck()
        {
            if(File.Exists("subs.ass"))
            {
                File.Delete("subs.ass");
            }
            else if (File.Exists("subs.srt"))
            {
                File.Delete("subs.srt");
            }
        }
    }
}
