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
        private int THREADS = Environment.ProcessorCount;                   // Obtains the number of threads the computer has

        Regex verifyLength = new Regex(@"^\d{1,3}");                        // Regex to verify if txtLength is properly typed in
        Regex verifyTimeStart = new Regex(@"^[0-6]\d:[0-6]\d:[0-6]\d");     // Regex to verify if txtStartTime is properly typed in
        Regex verifyWidth = new Regex(@"^\d{1,4}");                         // Regex to verify if txtWidth is properly typed in

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

        private void button3_Click(object sender, EventArgs e)
        {
            String command = "-y";
            String commandPass1 = null;
            String commandPass2 = null;
            String commandScale = null;

            // Verification boolean just incase the user messes up
            bool verified = true;

            // Validates if the user input a value for txtTimeStart
            if (!verifyTimeStart.IsMatch(txtTimeStart.Text))
            {
                verified = false;
                MessageBox.Show("The time format is messed up.\nPlease use HH:MM:SS", "Verification Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            // Validates if the user input a value for txtLength
            if (!verifyLength.IsMatch(txtLength.Text))
            {
                verified = false;
                MessageBox.Show("The length of the video is not properly set", "Verification Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            // Validates if the user input a value for txtWidth
            if (!verifyWidth.IsMatch(txtWidth.Text))
            {
                if (txtWidth.Text != "")
                {
                    verified = false;
                    MessageBox.Show("The width is not properly set", "Verification Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                commandScale = " -vf scale=" + txtWidth.Text + ":-1";
            }
            // Validates if the user input a value for txtInput
            if (txtInput.Text == "")
            {
                verified = false;
                MessageBox.Show("An input file needs to be selected", "Verification Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            // Validates if the user input a value for txtOutput
            if (txtOutput.Text == "")
            {
                verified = false;
                MessageBox.Show("An output file needs to be selected", "Verification Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // If everything is valid, continue with the conversion
            if (verified)
            {
                // Calculates the bitrate needed to hit txtMaxSize with length txtLength
                String bitrate = Helper.calcBitrate(txtMaxSize.Text, txtLength.Text);
                // Calculates the seconds from the time-code
                double seconds = Helper.convertToSeconds(txtTimeStart.Text);

                if (seconds > 30)
                {
                    command += " -ss " + Convert.ToString(seconds - 30) + " -i \"" + txtInput.Text + "\" -ss 30 -t " + txtLength.Text +
                    " -c:v libvpx -b:v " + bitrate + commandScale + " -threads " + THREADS +
                    " -quality best -auto-alt-ref 1 -lag-in-frames 16 -slices 8 -an ";
                    commandPass1 = command + "-pass 1 -f webm NUL";
                    commandPass2 = command + "-pass 2 \"" + txtOutput.Text + "\"";
                }
                else if (seconds <= 30)
                {
                    command += " -i \"" + txtInput.Text + "\" -ss " + seconds + " -t " + txtLength.Text +
                    " -c:v libvpx -b:v " + bitrate + commandScale + " -threads " + THREADS +
                    " -quality best -auto-alt-ref 1 -lag-in-frames 16 -slices 8 -an ";
                    commandPass1 = command + "-pass 1 -f webm NUL";
                    commandPass2 = command + "-pass 2 \"" + txtOutput.Text + "\"";
                }

                try
                {
                    // Pass 1
                    var pass1 = Process.Start("ffmpeg", commandPass1);
                    pass1.WaitForExit();

                    // Pass 2
                    var pass2 = Process.Start("ffmpeg", commandPass2);
                    pass2.WaitForExit();
                }
                catch (Win32Exception ex)
                {
                    MessageBox.Show("It appears you are missing ffmpeg. Please\ngo obtain a copy of it, and put it in the same\nfolder as this executable.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                txtInput.Text = openFileDialog1.FileName;
            }

        }

        private void button2_Click(object sender, EventArgs e)
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
        }
    }
}
