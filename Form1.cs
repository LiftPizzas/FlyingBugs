using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
            numCritters--;
            if (numCritters < 1) numCritters = 1;
            labelnumCritters.Text = numCritters.ToString();
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

        private void trackBarPolygonLength_Scroll(object sender, EventArgs e)
        {
            polygonLength = trackBarPolygonLength.Value;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            numCritters++;
            if (numCritters > 60) numCritters = 60;
            labelnumCritters.Text = numCritters.ToString();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            edgesOn = checkBox1.Checked;
        }

        private void trackBarTrailLength_Scroll(object sender, EventArgs e)
        {
            int t = trackBarTrailLength.Value;
            //1-16 are direct
            if (t < 21)
            {
                trailLength = t;
            }
            else if (t < 41)
            {
                trailLength = t * 2;
            }
            else if (t < 61)
            {
                trailLength = t * 4;
            }
            else if (t < 81)
            {
                trailLength = t * 8;
            }
            else
            {
                trailLength = t * 16;
            }
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        { 
            rainbowMode = listBox2.SelectedIndex;
        }

        private void trackBarRainbowSpeed_Scroll(object sender, EventArgs e)
        {
            rainbowSpeed = trackBarRainbowSpeed.Value;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            brightnessMode = checkBox3.Checked;
        }

        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            brightnessCycleSpeed = (float)trackBar4.Value / 200f;
        }

        private void trackBar6_Scroll(object sender, EventArgs e)
        {
            lineWidth = (float)trackBar6.Value;
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {

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
    }
}
