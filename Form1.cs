using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SphereDivision
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            GetConfigList();
            chooseRandomConfig();

            //Random r = new Random();
            //for (int i = 1; i < 1000000; i++) {
            //    if(r.Next(3) >2) 
            //        Console.WriteLine("THREE!"); 
            //}


            //            [System.Runtime.InteropServices.DllImport("User32.dll")]
            //            private static extern bool SetForegroundWindow(IntPtr handle);

            //private IntPtr handle;
        }

        public void updateClock(int time)
        {
            label1.Text = "Next in: " + (time/1000).ToString();
        }

        public void chooseRandomConfig()
        {
            listBox6.SelectedIndex = -1;
            Random r = new Random();
            listBox6.SelectedIndex = r.Next(listBox6.Items.Count);
            label2.Text = "Preset: " + listBox6.SelectedItem;
        }


        public void createRandomSettings()
        {
            label2.Text = "Random";
            Random r = new Random();

            checkBox7.Checked = (r.NextDouble() > 0.5);
            checkBox2.Checked = (r.NextDouble() > 0.25);
            trackBar8.Value = r.Next(1, 17);
            trackBar3.Value = r.Next(1, 101);
            listBox3.SelectedIndex = r.Next(3);

            int total;
            do
            {
                listBoxDrawingStyle.SelectedIndex = r.Next(5);
                trackBar1.Value = r.Next(trackBar1.Minimum, (int)trackBar1.Maximum); //numcritters
                trackBarTrailLength.Value = r.Next(2, 130);
                total = numCritters * trailLength * 3;
                if (checkBox7.Checked) total += trackBar8.Value;
                if (listBoxDrawingStyle.SelectedIndex == 1) total *= 4;
                if (listBoxDrawingStyle.SelectedIndex == 2) total *= 16;
                if (listBoxDrawingStyle.SelectedIndex == 4) total *= 4;
            }
            while (total > 1000000);
            label2.Text = "Random " + total.ToString();


            if (r.NextDouble() > 0.5) checkBox1.Checked = true; else checkBox1.Checked = false;

            
            trackBarPolygonLength.Value = r.Next(2, 25);


            if (r.NextDouble() < 0.1) trackBar7.Value = r.Next(1, 100); else trackBar7.Value = 1;
            listBox2.SelectedIndex = r.Next(4);
            trackBarRainbowSpeed.Value = r.Next(0, 100);

            checkBox3.Checked = (r.NextDouble() > 0.1);
            checkBox4.Checked = (r.NextDouble() > 0.5);
            trackBar4.Value = r.Next(1, 11);

            checkBox9.Checked = (r.NextDouble() > 0.3);
            checkBox8.Checked = (r.NextDouble() > 0.5);
            trackBar9.Value = r.Next(1, 101);

            trackBar6.Value = r.Next(1, 10);
            if (r.NextDouble() < 0.05) trackBar6.Value = 20;
            
            


            restartNow = true;
        }


        private void GetConfigList()
        {
            string[] dir = Directory.GetFiles(Application.StartupPath + "\\configs", "*.bug");
            listBox6.Items.Clear();
            for (int i = 0; i < dir.Length; i++)
            {
                listBox6.Items.Add(Path.GetFileNameWithoutExtension(dir[i]));
            }
        }


        public int numCritters = 4;
        public int polygonLength = 5;
        public int connectMode = 0;
        public bool edgesOn = false;
        public int trailLength = 8;
        public int rainbowSpeed = 5;
        public int rainbowMode = 3;
        public bool brightnessMode = true;
        public float brightnessCycleSpeed = 0.1f;
        public float lineWidth = 4f;
        public bool brightnessSynch = false;
        public bool restartNow = false;
        public bool paused = false;
        public bool requestExit = false;
        public int trailSkip = 1;
        public bool gravMode = true;
        public bool showGravPoints = true; //draw them visibly
        public int numGravPoints = 4;
        public float gravPointStrength = 0.05f;
        public int gravFalloff = 1;
        public bool thicknessMode = true;
        public float thicknessCycleSpeed = 1f;
        public bool thicknessSynch = false;
        public float mouseGravStrength = 0.1f;
        public int MouseGrav = 0;

        public bool newCycleTime = true;
        public int cycleTime = 30000;
        public bool usePresets = true;
        public bool useRandom = true;
        public bool reExplode = true;

        /// <summary>
        /// random starting positions
        /// explosion starting position
        /// multi-explode
        /// rain starting position
        /// 
        /// optimize trails, use positional pointer (start, finish) and for loops.
        /// 
        /// Background image
        /// Remove "on top" from window
        /// </summary>
        /// <param name="fps"></param>
        /// <param name="deltaTime"></param>

        public void showFPS(int fps, long deltaTime)
        {
            labelFPS.Text = fps.ToString() + " fps";
            labelMS.Text = deltaTime.ToString() + " ms";
        }

        /// <summary>
        /// changes values of all controls so they match the internal settings
        /// </summary>
        public void setControlsToMatchVars()
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //numCritters--;
            //if (numCritters < 1) numCritters = 1;
            //labelnumCritters.Text = numCritters.ToString();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listBoxDrawingStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            connectMode = listBoxDrawingStyle.SelectedIndex;

            if (((string)listBoxDrawingStyle.Items[listBoxDrawingStyle.SelectedIndex] == "Mystify") ||
                    ((string)listBoxDrawingStyle.Items[listBoxDrawingStyle.SelectedIndex] == "Bowties"))
            {
                groupBox21.Visible = true;
            }
            else
            {
                groupBox21.Visible = false;
            }
        }

        private void changePolygonLength(object sender, EventArgs e)
        {
            polygonLength = trackBarPolygonLength.Value;
            groupBox21.Text = "Polygon Length " + polygonLength.ToString();
        }



        private void changeMouseGrav(object sender, EventArgs e)
        {
            mouseGravStrength = trackBar5.Value * 0.1f;
            if (MouseGrav == 1) mouseGravStrength *= 0.1f;
        }



        private void changeCycleTime(object sender, EventArgs e)
        {
            cycleTime = trackBar2.Value * 1000;
            if (trackBar2.Value == 0)
            {
                groupBox23.Text = "Cycle Time (disabled)";
            }
            else
            {
                groupBox23.Text = "Cycle Time (" + trackBar2.Value + " sec)";
                newCycleTime = true;
            }
        }


        private void changeTrailLength(object sender, EventArgs e)
        {
            int t = trackBarTrailLength.Value;
            if (t < 21)
            {
                trailLength = t;
            }
            else if (t < 41)
            {
                trailLength = 20 + ((t - 20) * 2);
            }
            else if (t < 61)
            {
                trailLength = 60 + ((t - 40) * 4);
            }
            else if (t < 81)
            {
                trailLength = 140 + ((t - 60) * 8);
            }
            else if (t < 111)
            {
                trailLength = 300 + ((t - 80) * 16);
            }
            else
            {
                trailLength = 620 + ((t - 100) * 32);
            }

            if (trailLength < 3) trailLength = 3;
            groupBox8.Text = "Trail Length " + (trailLength - 2).ToString();
        }

        private void changeTrailSkip(object sender, EventArgs e)
        {
            trailSkip = trackBar7.Value;
            if (trailSkip > (trailLength - 3))
            {
                trailSkip = trailLength - 3;
                if (trailSkip < 1) trailSkip = 1;
                trackBar7.Value = trailSkip;
            }
            groupBox24.Text = "Trail Skip " + trailSkip.ToString();
        }


        private void changeRainbowSpeed(object sender, EventArgs e)
        {
            rainbowSpeed = trackBarRainbowSpeed.Value;
            groupBox7.Text = "Speed " + rainbowSpeed.ToString();
        }

        private void changeBrightnessSpeed(object sender, EventArgs e)
        {
            brightnessCycleSpeed = trackBar4.Value / 200f;
            groupBox13.Text = "Speed " + trackBar4.Value.ToString();
        }



        private void changeLineThickness(object sender, EventArgs e)
        {
            lineWidth = (float)trackBar6.Value;
            groupBox19.Text = "Line Thickness " + lineWidth.ToString();
        }



        private void changeGravStrength(object sender, EventArgs e)
        {
            gravPointStrength = trackBar3.Value * 0.01f;
            if (gravFalloff == 0) gravPointStrength *= 0.1f;
        }


        private void changeThicknessSpeed(object sender, EventArgs e)
        {
            thicknessCycleSpeed = trackBar9.Value / 200f;
            groupBox26.Text = "Speed " + trackBar9.Value.ToString();
        }
        private void changeGravPoints(object sender, EventArgs e)
        {
            numGravPoints = trackBar8.Value;
            groupBox2.Text = numGravPoints.ToString() + "Auto Grav Points";
        }

        private void changeCritters(object sender, EventArgs e)
        {
            int t = trackBar1.Value;
            //1-16 are direct
            if (t < 11)
            {
                numCritters = t;
            }
            else if (t < 21)
            {
                numCritters = t * 2;
            }
            else if (t < 31)
            {
                numCritters = t * 4;
            }
            else if (t < 41)
            {
                numCritters = t * 8;
            }
            else if (t < 51)
            {
                numCritters = t * 16;
            }
            else if (t < 61)
            {
                numCritters = t * 32;
            }
            else if (t < 71)
            {
                numCritters = t * 64;
            }
            else if (t < 81)
            {
                numCritters = t * 128;
            }
            else if (t < 91)
            {
                numCritters = t * 256;
            }
            else
            {
                numCritters = t * 512;
            }
            groupBox1.Text = numCritters.ToString() + " Critters ";
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            brightnessSynch = checkBox4.Checked;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            restartNow = true;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (paused)
            {
                paused = false;
                button6.Text = "Pause";
            }
            else
            {
                paused = true;
                button6.Text = "Continue";
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //System.Windows.Forms.Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            requestExit = true;
            e.Cancel = true;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            //TEST BUTTON
            createRandomSettings();
        }


        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            edgesOn = checkBox1.Checked;
        }
        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            rainbowMode = listBox2.SelectedIndex;
        }
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            brightnessMode = checkBox3.Checked;
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            gravMode = checkBox7.Checked;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            showGravPoints = checkBox2.Checked;
        }


        private void listBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            gravFalloff = listBox3.SelectedIndex;
            gravPointStrength = trackBar3.Value * 0.01f;
            if (gravFalloff == 0) gravPointStrength *= 0.1f;
        }



        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {
            thicknessMode = checkBox9.Checked;
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            thicknessSynch = checkBox8.Checked;
        }



        private void button7_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "Bugs files (*.bug)|*.bug|All files (*.*)|*.*";
            saveFileDialog1.InitialDirectory = Application.StartupPath + "\\configs";
            saveFileDialog1.FileName = "";
            if (!Directory.Exists(saveFileDialog1.InitialDirectory))
            {
                Directory.CreateDirectory(saveFileDialog1.InitialDirectory);
                if (!Directory.Exists(saveFileDialog1.InitialDirectory))
                {
                    MessageBox.Show("Cannot create Directory \n" + saveFileDialog1.InitialDirectory + "\nConfig NOT saved!", "Save Error!");
                    return;
                }
            }

            {
                /*public int numCritters = 4;
                //public bool edgesOn = false;
                //public int connectMode = 0;
                //public int polygonLength = 5;
                //public int trailLength = 8;
                //public int trailSkip = 1;
                //public int rainbowMode = 3;
                //public int rainbowSpeed = 5;
                //public bool brightnessMode = true;
                //public bool brightnessSynch = false;
                //public float brightnessCycleSpeed = 0.1f;
                //public bool thicknessMode = true;
                //public bool thicknessSynch = false;
                //public float thicknessCycleSpeed = 1f;
                //public float lineWidth = 4f;
                //public bool gravMode = true;
                //public bool showGravPoints = true; //draw them visibly
                //public int numGravPoints = 4;
                //public float gravPointStrength = 0.05f;
                //public int gravFalloff = 1;
                */
            }

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                using (BinaryWriter binWriter = new BinaryWriter(File.Open(saveFileDialog1.FileName, FileMode.Create)))
                {
                    binWriter.Write(trackBar1.Value);
                    binWriter.Write(edgesOn);
                    binWriter.Write(connectMode);
                    binWriter.Write(polygonLength);
                    binWriter.Write(trackBarTrailLength.Value);
                    binWriter.Write(trailSkip);
                    binWriter.Write(rainbowMode);
                    binWriter.Write(rainbowSpeed);
                    binWriter.Write(brightnessMode);
                    binWriter.Write(brightnessSynch);
                    binWriter.Write(trackBar4.Value);
                    binWriter.Write(thicknessMode);
                    binWriter.Write(thicknessSynch);
                    binWriter.Write(trackBar9.Value);
                    binWriter.Write(trackBar6.Value);
                    binWriter.Write(gravMode);
                    binWriter.Write(showGravPoints);
                    binWriter.Write(numGravPoints);
                    binWriter.Write(trackBar3.Value);
                    binWriter.Write(gravFalloff);
                }
                //clear the config list and refresh it
                GetConfigList();
            }
        }

        private void listBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox6.SelectedIndex == -1) return; //this is used to trigger the preset load in case of duplicates
            string dir = Path.Combine(Application.StartupPath, "configs");
            if (!Directory.Exists(dir))
            {
                MessageBox.Show("Cannot open Config folder!\n" + dir, "Load Error!");
                return;
            }
            string fileName = Path.Combine(dir, listBox6.SelectedItem + ".bug");
            if (!File.Exists(fileName))
            {
                MessageBox.Show("Cannot open Config File!\n" + fileName, "Load Error!");
                return;
            }

            using (BinaryReader binaryReader = new BinaryReader(File.OpenRead(fileName)))
            {
                numCritters = binaryReader.ReadInt32();
                edgesOn = binaryReader.ReadBoolean();
                connectMode = binaryReader.ReadInt32();
                polygonLength = binaryReader.ReadInt32();
                trailLength = binaryReader.ReadInt32();
                trailSkip = binaryReader.ReadInt32();
                rainbowMode = binaryReader.ReadInt32();
                rainbowSpeed = binaryReader.ReadInt32();
                brightnessMode = binaryReader.ReadBoolean();
                brightnessSynch = binaryReader.ReadBoolean();
                int bcs = binaryReader.ReadInt32();
                thicknessMode = binaryReader.ReadBoolean();
                thicknessSynch = binaryReader.ReadBoolean();
                int tcs = binaryReader.ReadInt32();
                int lw = binaryReader.ReadInt32();
                gravMode = binaryReader.ReadBoolean();
                showGravPoints = binaryReader.ReadBoolean();
                numGravPoints = binaryReader.ReadInt32();
                int gps = binaryReader.ReadInt32();
                gravFalloff = binaryReader.ReadInt32();

                trackBar1.Value = numCritters;
                checkBox1.Checked = edgesOn;
                listBoxDrawingStyle.SelectedIndex = connectMode;
                trackBarPolygonLength.Value = polygonLength;
                trackBarTrailLength.Value = trailLength;

                trackBar7.Value = trailSkip;
                listBox2.SelectedIndex = rainbowMode;
                trackBarRainbowSpeed.Value = rainbowSpeed;

                checkBox3.Checked = brightnessMode;
                checkBox4.Checked = brightnessSynch;
                trackBar4.Value = bcs;

                checkBox9.Checked = thicknessMode;
                checkBox8.Checked = thicknessSynch;
                trackBar9.Value = tcs;

                trackBar6.Value = lw;

                checkBox7.Checked = gravMode;
                checkBox2.Checked = showGravPoints;
                trackBar8.Value = numGravPoints;
                trackBar3.Value = gps;
                listBox3.SelectedIndex = gravFalloff;
            }
            groupBox20.Text = "Presets (" + listBox6.SelectedItem + ")";
        }

        private void listBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            MouseGrav = listBox4.SelectedIndex;
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            usePresets = checkBox5.Checked;
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            useRandom = checkBox6.Checked;
        }

        private void checkBox10_CheckedChanged(object sender, EventArgs e)
        {
            reExplode = checkBox10.Checked;
        }
    }
}
