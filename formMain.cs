using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MasterOfWebM
{
    public partial class formMain : Form
    {
        // ********************
        //      Variables
        // ********************
        private String THREADS = Environment.ProcessorCount.ToString();                   // Obtains the number of threads the computer has

        Regex verifyLength = new Regex(@"^\d{1,3}");                        // Regex to verify if txtLength is properly typed in
        Regex verifyTimeStart = new Regex(@"^[0-6]\d:[0-6]\d:[0-6]\d");     // Regex to verify if txtStartTime is properly typed in
        Regex verifyWidth = new Regex(@"^\d{1,4}");                         // Regex to verify if txtWidth is properly typed in
        Regex verifyMaxSize = new Regex(@"^\d{1,4}");                       // Regex to verify if txtMaxSize is properly typed in

        // ********************
        //      Functions
        // ********************
        public formMain()
        {
            InitializeComponent();
        }

        // As soon as the user clicks on txtTimeStart, get rid of the informational text
        private void txtTimeStart_Click(object sender, EventArgs e)
        {
            if (txtTimeStart.Text == "HH:MM:SS")
            {
                txtTimeStart.Text = "";
                txtTimeStart.ForeColor = Color.Black;
            }

        }

        // Check if the user clicks away without typing anything into txtTimeStart
        private void txtTimeStart_LostFocus(object sender, EventArgs e)
        {
            if (txtTimeStart.Text == "")
            {
                txtTimeStart.Text = "HH:MM:SS";
                txtTimeStart.ForeColor = Color.Silver;
            }
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            // Base command where each element gets replaced
            String baseCommand = "-y {time1} -i \"{input}\" {time2} -t {length} -c:v libvpx -b:v {bitrate} {scale} -threads {threads} -quality best -auto-alt-ref 1 -lag-in-frames 16 -slices 8 -an ";
            String commandPass1 = "-pass 1 -f webm NUL";
            String commandPass2 = "-pass 2 ";

            // Verification boolean just incase the user messes up
            bool verified = true;

            // Validates if the user input a value for txtInput
            if (txtInput.Text == "")
            {
                verified = false;
                MessageBox.Show("An input file needs to be selected", "Verification Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                baseCommand = baseCommand.Replace("{input}", txtInput.Text);
            }

            // Validates if the user input a value for txtOutput
            if (txtOutput.Text == "")
            {
                verified = false;
                MessageBox.Show("An output file needs to be selected", "Verification Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Validates if the user input a value for txtTimeStart
            if (!verifyTimeStart.IsMatch(txtTimeStart.Text))
            {
                verified = false;
                MessageBox.Show("The time format is messed up.\nPlease use HH:MM:SS", "Verification Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                // Calculates the seconds from the time-code
                double seconds = Helper.convertToSeconds(txtTimeStart.Text);

                if (seconds > 30)
                {
                    baseCommand = baseCommand.Replace("{time1}", "-ss " + Convert.ToString(seconds - 30));
                    baseCommand = baseCommand.Replace("{time2}", "-ss 30");
                }
                else
                {
                    baseCommand = baseCommand.Replace(" {time1}", "");
                    baseCommand = baseCommand.Replace("{time2}", "-ss " + seconds);
                }
            }

            // Validates if the user input a value for txtLength
            if (!verifyLength.IsMatch(txtLength.Text))
            {
                verified = false;
                MessageBox.Show("The length of the video is not properly set", "Verification Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                baseCommand = baseCommand.Replace("{length}", txtLength.Text);
            }

            // Validates if the user input a value for txtWidth
            if (!verifyWidth.IsMatch(txtWidth.Text))
            {
                if (txtWidth.Text != "")
                {
                    verified = false;
                    MessageBox.Show("The width is not properly set", "Verification Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    baseCommand = baseCommand.Replace(" {scale}", "");
                }
            }
            else
            {
                baseCommand = baseCommand.Replace("{scale}", "-vf scale=" + txtWidth.Text + ":-1");
            }

            // Validates if the user input a value for txtMaxSize
            if (!verifyMaxSize.IsMatch(txtMaxSize.Text))
            {
                verified = false;
                MessageBox.Show("The maxium file size is not properly set", "Verification Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                String bitrate = Helper.calcBitrate(txtMaxSize.Text, txtLength.Text);
                baseCommand = baseCommand.Replace("{bitrate}", bitrate);
            }

            // If everything is valid, continue with the conversion
            if (verified)
            {
                baseCommand = baseCommand.Replace("{threads}", THREADS);
                
                try
                {
                    // Pass 1
                    var pass1 = Process.Start("ffmpeg", baseCommand + commandPass1);
                    pass1.WaitForExit();

                    // Pass 2
                    var pass2 = Process.Start("ffmpeg", baseCommand + commandPass2 + "\"" + txtOutput.Text + "\"");
                    pass2.WaitForExit();
                }
                catch (Win32Exception ex)
                {
                    MessageBox.Show("It appears you are missing ffmpeg. Please\ngo obtain a copy of it, and put it in the same\nfolder as this executable.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Debug.WriteLine(ex);
                }
            }

        }

        private void btnInput_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                txtInput.Text = openFileDialog1.FileName;
            }

        }

        private void btnOutput_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                txtOutput.Text = saveFileDialog1.FileName;
            }
        }

        private void formMain_Load(object sender, EventArgs e)
        {
            lblThreads.Text = "Threads: " + THREADS;
            comboQuality.SelectedIndex = 0;
            Debug.WriteLine("Started");
        }
    }
}
