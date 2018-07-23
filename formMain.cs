using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MasterOfWebM
{
    public partial class formMain : Form
    {
        // ********************
        //      Variables
        // ********************
        private String THREADS = Environment.ProcessorCount.ToString();             // Obtains the number of threads the computer has
        private String runningDirectory = AppDomain.CurrentDomain.BaseDirectory;    // Obtains the root directory

        Regex verifyLength = new Regex(@"^\d{1,3}");                                // Regex to verify if txtLength is properly typed in
        Regex verifyTimeStart = new Regex(@"^[0-6]\d:[0-6]\d:[0-6]\d");             // Regex to verify if txtStartTime is properly typed in
        Regex verifyWidth = new Regex(@"^\d{1,4}");                                 // Regex to verify if txtWidth is properly typed in
        Regex verifyMaxSize = new Regex(@"^\d{1,4}");                               // Regex to verify if txtMaxSize is properly typed in
        Regex verifyCrop = new Regex(@"^\d{1,4}:\d{1,4}:\d{1,4}:\d{1,4}");          // Regex to verify if txtCrop is properly typed in

        // ********************
        //      Functions
        // ********************
        public formMain()
        {
            InitializeComponent();
        }

        // As soon as the user clicks on txtTimeStart, get rid of the informational text
        private void txtTimeStart_Enter(object sender, EventArgs e)
        {
            if (txtTimeStart.Text == "HH:MM:SS")
            {
                txtTimeStart.Text = "";
                txtTimeStart.ForeColor = Color.Black;
            }

        }

        // Check if the user clicks away without typing anything into txtTimeStart
        private void txtTimeStart_Leave(object sender, EventArgs e)
        {
            if (txtTimeStart.Text == "")
            {
                txtTimeStart.Text = "HH:MM:SS";
                txtTimeStart.ForeColor = Color.Silver;
            }
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            // Delete any existing temp subtitle file
            Helper.subsCheck();

            // Disable btnConvert so user's cant click on it multiple times
            btnConvert.Enabled = false;

            // Base command where each element gets replaced
            String baseCommand = "-y {time1} -i \"{input}\" {time2} -t {length} -c:v libvpx -b:v {bitrate} {scale} -threads {threads} {metadata} {quality} {audio} ";
            String filterCommands = null;

            // Verification boolean just incase the user messes up
            bool verified = true;
            bool filters = false;

            double bitrate = 0;

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
                    if (txtSubs.Text == "")
                    {
                        // If not subtitles exist
                        baseCommand = baseCommand.Replace("{time1}", "-ss " + Convert.ToString(seconds - 30));
                        baseCommand = baseCommand.Replace("{time2}", "-ss 30");
                    }
                    else
                    {
                        // If subtitles exist
                        baseCommand = baseCommand.Replace(" {time1}", "");
                        baseCommand = baseCommand.Replace("{time2}", "-ss " + Convert.ToString(seconds));
                    }
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

            // Validates if the user input a value for txtCrop
            if (!verifyCrop.IsMatch(txtCrop.Text))
            {
                if (txtCrop.Text != "o_w:o_h:x:y")
                {
                    verified = false;
                    MessageBox.Show("The crop field is not properly set\nSyntax:\nout_x:out_y:x:y\n\nout_x & out_y is the output size\nx & y are where you begin your crop",
                        "Verification Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                filters = true;
                filterCommands += filterCommands == null ? "crop=" + txtCrop.Text : ",crop=" + txtCrop.Text;
            }

            // Check if we need to add subtitles
            if (txtSubs.Text != "")
            {
                switch (Path.GetExtension(txtSubs.Text))
                {
                    case ".ass":
                        filters = true;
                        File.Copy(txtSubs.Text, runningDirectory + "subs.ass");
                        filterCommands += filterCommands == null ? "ass=subs.ass" : ",ass=subs.ass";
                        break;
                    case ".srt":
                        filters = true;
                        File.Copy(txtSubs.Text, runningDirectory + "subs.srt");
                        filterCommands += filterCommands == null ? "subtitles=subs.srt" : ",subtitles=subs.srt";
                        break;
                }
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
                filters = true;
                filterCommands += filterCommands == null ? "scale=" + txtWidth.Text + ":-1" : ",scale=" + txtWidth.Text + ":-1";
            }

            // Validates if the user input a value for txtMaxSize
            if (!verifyMaxSize.IsMatch(txtMaxSize.Text))
            {
                verified = false;
                MessageBox.Show("The maxium file size is not properly set", "Verification Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                try
                {
                    bitrate = Helper.calcBitrate(txtMaxSize.Text, txtLength.Text);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                // If audio is requested
                if (checkAudio.Checked)
                {
                    // TODO: Give bitrate options for audio (currently enforcing 48k)
                    // TODO: Disable audio encoding on first pass to speed up encoding
                    bitrate -= 48;
                    baseCommand = baseCommand.Replace("{audio}", "-c:a libvorbis -b:a 48k");
                }
                else
                {
                    baseCommand = baseCommand.Replace("{audio}", "-an");
                }

                // Changes the quality to what the user selected
                switch (comboQuality.Text)
                {
                    case "Good":
                        baseCommand = baseCommand.Replace("{quality}", "-quality good -cpu-used 0");
                        baseCommand = baseCommand.Replace("{bitrate}", bitrate.ToString() + "K");
                        break;
                    case "Best":
                        baseCommand = baseCommand.Replace("{quality}", "-quality best -auto-alt-ref 1 -lag-in-frames 16 -slices 8");
                        baseCommand = baseCommand.Replace("{bitrate}", bitrate.ToString() + "K");
                        break;
                    case "Ultra":
                        baseCommand = baseCommand.Replace("{quality}", "-quality best -auto-alt-ref 1 -lag-in-frames 16 -slices 8");
                        bitrate = Convert.ToDouble(bitrate) * 1024;
                        baseCommand = baseCommand.Replace("{bitrate}", bitrate.ToString());
                        break;
                }
            }

            // If any filters are being used, add them to baseCommand
            if (filters)
            {
                filterCommands = "-vf " + filterCommands;
                baseCommand = baseCommand.Replace("{scale}", filterCommands);
            }
            else
            {
                baseCommand = baseCommand.Replace(" {scale}", "");
            }

            // Validates if the user input a value for txtTitle
            if (txtTitle.Text != "")
            {
                baseCommand = baseCommand.Replace("{metadata}", string.Format("-metadata title=\"{0}\"", txtTitle.Text.Replace("\"", "\\\"")));
            }
            else
            {
                baseCommand = baseCommand.Replace("{metadata}", "");
            }

            // If everything is valid, continue with the conversion
            if (verified)
            {
                baseCommand = baseCommand.Replace("{threads}", THREADS);

                try
                {
                    Helper.encodeVideo(baseCommand, txtOutput.Text);
                }
                catch (Win32Exception ex)
                {
                    MessageBox.Show("It appears you are missing ffmpeg. Please\ngo obtain a copy of it, and put it in the same\nfolder as this executable.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Debug.WriteLine(ex);
                }

                double fileSize = Helper.getFileSize(txtOutput.Text);

                if (fileSize < Convert.ToDouble(txtMaxSize.Text) * 1024)
                {
                    // Clears the output box so user's don't overwrite their previous output
                    txtOutput.Text = "";
                    if (txtSubs.Text != "")
                    {
                        switch (Path.GetExtension(txtSubs.Text))
                        {
                            case ".ass":
                                File.Delete(runningDirectory + "\\subs.ass");
                                break;
                            case ".srt":
                                File.Delete(runningDirectory + "\\subs.srt");
                                break;
                        }
                        txtSubs.Text = "";
                    }
                }
                else
                {
                    if (comboQuality.Text == "Ultra")
                    {
                        /*
                         * Automatically attempt to create a smaller file
                         * If it doesn't work within 10 attempts, recommend
                         * 'Best' quality
                         */
                        int passes = 0;

                        while (fileSize > Convert.ToDouble(txtMaxSize.Text) * 1024 && passes <= 10)
                        {
                            // Lowers the bitrate by 1k
                            bitrate -= 1000;

                            // Replacing the whole command just in case the file name contains the same numbers
                            baseCommand = baseCommand.Replace("-b:v " + (bitrate + 1000), "-b:v " + bitrate);

                            Helper.encodeVideo(baseCommand, txtOutput.Text);
                            passes++;

                            // Gets the filesize after encoding
                            fileSize = Helper.getFileSize(txtOutput.Text);
                        }

                        if (fileSize < Convert.ToDouble(txtMaxSize.Text) * 1024)
                        {
                            txtOutput.Text = "";

                            if (txtSubs.Text != "")
                            {
                                switch (Path.GetExtension(txtSubs.Text))
                                {
                                    case ".ass":
                                        File.Delete(runningDirectory + "\\subs.ass");
                                        break;
                                    case ".srt":
                                        File.Delete(runningDirectory + "\\subs.srt");
                                        break;
                                }
                                txtSubs.Text = "";
                            }
                        }
                        else
                            MessageBox.Show("Could not get the file size below " + txtMaxSize.Text + "MB.\n" +
                                "Try using 'Best' quality, and if that doesn't work,\n" +
                                "you must reduce your resolution and/or shorten the length.",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show("The final clip is larger than " + txtMaxSize.Text + "MB.\n" +
                            "This occured because the clip's resolution was too large,\n" +
                            "and/or because the clip was too long for the inputted size.");
                    }
                }
            }

            // Re-enable the button after a run
            btnConvert.Enabled = true;
        }

        private void btnInput_Click(object sender, EventArgs e)
        {
            if (inputFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtInput.Text = inputFileDialog.FileName;
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

            // Calls the font config checker, and if something went wrong, it disables converting
            if (!Helper.checkFFmpegFontConfig())
            {
                btnConvert.Enabled = false;
            }

            Helper.checkUpdate();
        }

        // Handles when the user focuses txtCrop
        private void txtCrop_Enter(object sender, EventArgs e)
        {
            txtCrop.Left -= 46;
            txtCrop.Size = new System.Drawing.Size(115, 20);
            txtCrop.ForeColor = Color.Black;

            if (txtCrop.Text == "o_w:o_h:x:y")
            {
                txtCrop.Text = "";
            }
        }

        // Handles when the user unfocuses txtCrop
        private void txtCrop_Leave(object sender, EventArgs e)
        {
            txtCrop.Left += 46;
            txtCrop.Size = new System.Drawing.Size(69, 20);

            if (txtCrop.Text == "")
            {
                txtCrop.Text = "o_w:o_h:x:y";
                txtCrop.ForeColor = Color.Silver;
            }
        }

        private void btnSubs_Click(object sender, EventArgs e)
        {
            if (subsFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtSubs.Text = subsFileDialog.FileName;
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtInput.Text = txtOutput.Text = txtSubs.Text = txtLength.Text = txtWidth.Text = "";
            txtTimeStart.Text = "HH:MM:SS";
            txtTimeStart.ForeColor = Color.Silver;
            txtMaxSize.Text = "3";
            txtCrop.Text = "o_w:o_h:x:y";
            txtCrop.ForeColor = Color.Silver;
            txtTitle.Text = "";
            comboQuality.SelectedIndex = 0;
            checkAudio.Checked = false;
        }

        private void comboQuality_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboQuality.Text == "Ultra")
                MessageBox.Show("Ultra quality will try getting just under your\n" +
                                "'Max Size'. This program will run ffmpeg up to 11 \n" +
                                "times and currently there is no way of stopping it.\n\n" +
                                "USE AT YOUR OWN RISK.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void formMain_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void formMain_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            txtInput.Text = files[0];
        }
    }
}
