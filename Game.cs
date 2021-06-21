using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Windows.Forms;
using System.IO;
using System.Numerics;
using Vector3 = System.Numerics.Vector3;
using Vector2 = System.Numerics.Vector2;

//using SharpDX.XAudio2;
//using SharpDX.Multimedia;
//using SharpDX.IO;
//to be added through nuget:
//SharpDX 4.2.0 (Alexandre Mutel)
//SharpDX.Xaudio2 4.2.0(Alexandre Mutel)

namespace SphereDivision
{

    /*
	Features to add:
	quad faces
	face colors for different tri types
	sounds based on face area, alternative to line length
	Auto-show? (automatically randomly change settings)
	dividing a hypersphere(4d, 5d?)
	*/

    class Game : GameWindow
    {
        /// New Vars
        /// 
        // up to 12 critters can exist.
        // each critter can have up to 16384 points
        // each point has a position and velocity

        int numCritters = 4;
        const int maxCritters = 23200;
        const int maxPoints = 1601;
        Vector2[,] bPos = new Vector2[maxCritters, maxPoints];
        float[,] zPos = new float[maxCritters, maxPoints];
        Vector2[] bVel = new Vector2[maxCritters];
        float[,] circleSize = new float[maxCritters, maxPoints]; //only for drawing circles, tracks size to avoid later recalculation
        //Vector3[,] bHSL = new Vector3[maxCritters, maxPoints]; //expressed in HSL, needs to be looked up when newly calculated
        System.Numerics.Vector3[,] bColor = new System.Numerics.Vector3[maxCritters, maxPoints]; //this is the actual draw color, expressed in RGB
        int[] currentHue = new int[maxCritters]; //desired hue to reach
        int[] hueTarget = new int[maxCritters]; //desired hue to reach
        int[] hueSpeed = new int[maxCritters]; //how fast to go toward this hue
        int[] hueTimer = new int[maxCritters]; //how fast to go toward this hue

        float maxVel = 2f;
        float minVel = 0.1f;

        int connectMode = 0;
        //0 = individual bugs (lines)
        //1 = Individual squares
        //2 = Individual circles
        //3 = mystify style (each 4 critters form a loop)

        //brightness cycle mode, size cycle mode

        float lineWidth = 4f;
        bool edgesOn = false;
        int trailLength = 6; //number from 1 to maxpoints, how many trailing points are drawn and tracked
        int trailSkip = 1;
        public int polygonLength = 4; //for mystify and bowtie drawing styles

        //for tracking trail positions, instead of copying the entire trail each frame
        //we just move a pointer along.
        int trailStart = 1;
        int trailEnd = 2;
        int trailPrev = 0;

        System.Numerics.Vector3[] lookupHSL = new System.Numerics.Vector3[768]; //Hue is looked up, Saturation and Lightness (if <1) are calculated
        // gives full saturation of indexed hue 0-767

        float mouseGravStrength = 0.1f;
        int MouseGrav = 0;// Mouse cursor acts as an attractor.
        //0= off
        //1= directional (distance doesn't matter)
        //2= linear falloff
        //3= Squared Falloff

        //color cycling:
        //totally random: each critter goes from one random hue to another
        // Synched Random: each critter is its own different hue and timing
        // Grayscale: stays on white/back
        // Rainbow: cycles in order

        int rainbowMode = 3;
        int rainbowPosition = 0; //current rainbow color to lookup in hue chart
        int rainbowSpeed = 5; //how fast the colors cycle
        bool brightnessMode = true;
        float[] brightnessValue = new float[maxCritters];
        float[] brightnessDir = new float[maxCritters];
        float brightnessCycleSpeed = 0.1f;
        bool brightnessSynch = false; //if true, all brightness is set to the same value all the time

        bool thicknessMode = true;
        float[,] thicknessValue = new float[maxCritters, maxPoints];
        float[] thicknessDir = new float[maxCritters];
        float thicknessCycleSpeed = 01f;
        bool thicknessSynch = false;
        bool attractorMode = false;

        // gravpoints have their own separate physics params and routines
        bool gravMode = true;
        bool showGravPoints = true; //draw them visibly
        int numGravPoints = 4;
        const int maxGravPoints = 16;
        Vector2[] gPos = new Vector2[maxGravPoints];
        Vector2[] gVel = new Vector2[maxGravPoints];
        float gravPointStrength = 0.05f;
        int gravFalloff = 1; //0 = Directional, 1 = 1/dist, 2 = 1/dist^2
        float maxGravSpeed = 0.5f;
        float minGravSpeed = 0.05f;

        //for slow explosions, so many critters are released per tick to make a stream instead.
        int explosionPoint;
        int explosionSpeed;

        float[] explosionX = new float[16]; //XY location of explosions
        float[] explosionY = new float[16];

        //Completed:
        //Tickonlyonce (while paused)
        //Maxdeltatime & speed scaling

        /// <summary>
        /// Gravpoint graphics (Dot, Plus, Square, Circle)
        //grav point size change with strength
        //wormholes (how to handle trail jumping?)

        //adjust min/max critter speed (advanced options)
        //adjust min/max grav speed

        //Path jitter
        //Ellipse/rectangle path rendering

        //draw every nth frame (calculate n frames each tick)
        


        //Add sound reactance

        //Add "streamed" explosions that take place over N seconds
        //multiple explosion locations
        //control screen to enable different explosion types
        //alternate transition: when going from lower to higher number of critters, copy critter locations of all higher numbers to existing ones to make them split off
        //reverse all velocities button and randomly make it happen (can tie to sound reactance too)
        /// </summary>

        float spinAngle;
        float spinAngleY;
        float viewScale;

        float cycleTime = 1000f;

        bool SoundReaction = false;
        string soundFile = ""; //sound file to play and react to




        // my vars
        Random random = new Random();

        long lastTime, curTime; //milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        float deltaTime;
        int tempTicks; //number of ticks per loop
        int fps; //for counting how many frames are rendered each second
        int fpsCount; //to be displayed, updated once per second
        float fpsTime;

        long tickCount = 0;


        Vector2 lastMousePos = new Vector2();
        bool lastButtonState = false; //previous mouse button state so we can tell when it's first pressed
        int lastWheel;
        KeyboardState keyboard, lastkeyboard;
        bool[] keysHeld = new bool[5]; //set to true when this key is to be activated by timer
        int[] keyTimers = new int[5]; //tracks how long since last keypress
        enum keysUsed
        {
            Right, Left, PageUp, PageDown, Enter
        }




        //sound stuff
        //        XAudio2 xaudio;
        //        WaveFormat waveFormat;
        //        AudioBuffer buffer;
        //        SoundStream soundstream;
        //        SourceVoice[] sourceVoice;
        int soundOn = 0;
        int currentVoice = 0;
        int voiceLoop = 0;

        public Game()
            : base(1920, 1080, new OpenTK.Graphics.GraphicsMode(32, 24, 0, 4))
        {

        }

        Form1 settingsForm;



        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //this.WindowState = WindowState.Minimized;
            settingsForm = new Form1();
            settingsForm.Show();


            //for 4d testing:
            //binocular = false;
            //findFaces = 0;
            //lineLevel = 0;

            //this.Cursor = OpenTK.MouseCursor.Cross;
            //initAudio();

            //initProgram();
            // hide max,min and close button at top right of Window
            //FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            // fill the screen
            initHSL();
            initcirclePoints();
            initCritters();
            //this.WindowState = WindowState.Maximized;
            this.WindowBorder = WindowBorder.Hidden;
            Bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            Title = "Flying Bugs";
            GL.ClearColor(System.Drawing.Color.CornflowerBlue);
            GL.PointSize(5f);
        }

        int maxCirclePoints = 16;
        Vector2[] circlePoints = new Vector2[64];
        void initcirclePoints()
        {
            int pNum = 0;
            double factor = Math.PI * 2d / (double)maxCirclePoints;
            for (int i = 0; i < maxCirclePoints; i++)
            {
                circlePoints[pNum].X = (float)Math.Sin((double)i * factor);
                circlePoints[pNum].Y = (float)-Math.Cos((double)i * factor);
                pNum++;
            }
        }

        void initHSL()
        {
            for (int i = 0; i < 128; i++)
            {
                lookupHSL[i].X = 1.0f;
                lookupHSL[i].Y = (float)i / 128f;
                lookupHSL[i].Z = 0.0f;
            }
            for (int i = 128; i < 256; i++)
            {
                lookupHSL[i].X = 1f - ((float)(i - 128) / 128f);
                lookupHSL[i].Y = 1.0f;
                lookupHSL[i].Z = 0.0f;
            }
            for (int i = 256; i < 384; i++)
            {
                lookupHSL[i].X = 0.0f;
                lookupHSL[i].Y = 1.0f;
                lookupHSL[i].Z = (float)(i - 256) / 128f;
            }
            for (int i = 384; i < 512; i++)
            {
                lookupHSL[i].X = 0.0f;
                lookupHSL[i].Y = 1f - ((float)(i - 384) / 128f);
                lookupHSL[i].Z = 1.0f;
            }
            for (int i = 512; i < 640; i++)
            {
                lookupHSL[i].X = (float)(i - 512) / 128f;
                lookupHSL[i].Y = 0f;
                lookupHSL[i].Z = 1f;
            }
            for (int i = 640; i < 768; i++)
            {
                lookupHSL[i].X = 1f;
                lookupHSL[i].Y = 0f;
                lookupHSL[i].Z = 1f - ((float)(i - 640) / 128f);
            }
        }

        //void initAudio()
        //{
        //	xaudio = new XAudio2();
        //	MasteringVoice masteringsound = new MasteringVoice(xaudio);
        //	NativeFileStream nativefilestream = new NativeFileStream(@"ding.wav", NativeFileMode.Open, NativeFileAccess.Read, NativeFileShare.Read);

        //	soundstream = new SoundStream(nativefilestream);
        //	waveFormat = soundstream.Format;
        //	buffer = new AudioBuffer
        //	{
        //		Stream = soundstream.ToDataStream(),
        //		AudioBytes = (int)soundstream.Length,
        //		Flags = BufferFlags.EndOfStream
        //	};

        //	sourceVoice = new SourceVoice[32];
        //	for (int i = 0; i < sourceVoice.Length; i++)
        //	{
        //		sourceVoice[i] = new SourceVoice(xaudio, waveFormat, VoiceFlags.None, 4f);
        //	}

        //}


        void BounceOffEdges()
        {

        }



        void LimitVelocity(int critter)
        {
            float vel = (float)Math.Sqrt((bVel[critter].X * bVel[critter].X) + (bVel[critter].Y + bVel[critter].Y));
            if (vel > maxVel)
            {
                bVel[critter].X = (bVel[critter].X / vel) * maxVel;
                bVel[critter].Y = (bVel[critter].Y / vel) * maxVel;
            }
            if (vel < minVel)
            {
                bVel[critter].X = (bVel[critter].X / vel) * minVel;
                bVel[critter].Y = (bVel[critter].Y / vel) * minVel;
            }
        }


        void getSettingsFromForm()
        {
            if (settingsForm.requestExit) Exit();

            numCritters = settingsForm.numCritters;
            polygonLength = settingsForm.polygonLength;
            connectMode = settingsForm.connectMode;
            edgesOn = settingsForm.edgesOn;
            trailLength = settingsForm.trailLength;
            rainbowSpeed = settingsForm.rainbowSpeed;
            rainbowMode = settingsForm.rainbowMode;
            brightnessMode = settingsForm.brightnessMode;
            brightnessCycleSpeed = settingsForm.brightnessCycleSpeed;
            lineWidth = settingsForm.lineWidth;
            trailSkip = settingsForm.trailSkip;
            gravMode = settingsForm.gravMode;
            showGravPoints = settingsForm.showGravPoints;
            numGravPoints = settingsForm.numGravPoints;
            gravPointStrength = settingsForm.gravPointStrength;
            gravFalloff = settingsForm.gravFalloff;

            thicknessMode = settingsForm.thicknessMode;
            thicknessCycleSpeed = settingsForm.thicknessCycleSpeed;
            thicknessSynch = settingsForm.thicknessSynch;

            mouseGravStrength = settingsForm.mouseGravStrength;
            MouseGrav = settingsForm.MouseGrav;

            if (settingsForm.newCycleTime)
            {
                settingsForm.newCycleTime = false; //reset each time it's changed
                cycleTime = (float)settingsForm.cycleTime;
            }


            if (settingsForm.restartNow)
            {
                settingsForm.restartNow = false;
                initCritters();
            }

        }


        private void makeNewSettings()
        {
            if (settingsForm.cycleTime == 0) return; //don't change settings if user hasn't selected either or if timer is set to zero

            //generate random settings or choose a random preset
            int choice = 0;
            if (settingsForm.usePresets) choice = 1;
            if (settingsForm.useRandom) choice += 2;

            switch (choice)
            {
                case 0: //no checkboxes selected
                    if (settingsForm.reExplode) initCritters();
                    break;
                case 1: //usepresets only
                    settingsForm.chooseRandomConfig();
                    break;
                case 2: //use random only
                    settingsForm.createRandomSettings();
                    break;
                case 3: //use either/or
                    if (random.NextDouble() > 0.5) settingsForm.chooseRandomConfig();
                    else settingsForm.createRandomSettings();
                    break;
            }


            cycleTime = (float)settingsForm.cycleTime;
        }


        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            getSettingsFromForm();

            //if (soundOn>0) playSounds();

            curTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            deltaTime = (float)(curTime - lastTime);
            fps++;
            fpsTime += deltaTime;
            if (fpsTime >= 1000)
            {
                fpsTime -= 1000;
                fpsCount = fps;
                fps = 0;
            }
            lastTime = curTime;
            settingsForm.showFPS(fpsCount, deltaTime);

            if (settingsForm.paused)
            {
                if (settingsForm.tickOnce) settingsForm.tickOnce = false;
                else return;
            }

            cycleTime -= deltaTime;
            settingsForm.updateClock((int)cycleTime);
            if (cycleTime <= 0f)
            {
                cycleTime = 0f;
                makeNewSettings();
            }
            if (deltaTime > (float)settingsForm.deltaTimeLimit) deltaTime = (float)settingsForm.deltaTimeLimit;

            deltaTime *= settingsForm.timeScale;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //set up the viewport's drawing scale so that everything shows up
            //get the window's aspect ratio
            float targetAspectRatio = (float)this.Width / this.Height; // 1.777
            GL.ClearColor(System.Drawing.Color.Black);
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();
            float maxDepth = 1000f;
            if (maxDepth < 1000) maxDepth = 1000f;
            GL.Ortho(-1000 * targetAspectRatio, 1000 * targetAspectRatio, 1000, -1000, maxDepth, -maxDepth);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.Blend);
            //GL.Enable(EnableCap.AlphaTest);
            //GL.LineStipple(1, 0x0101);

            float screenLeft = -1000f * targetAspectRatio;
            float screenRight = 1000f * targetAspectRatio;
            float screenTop = -1000f;
            float screenBot = 1000f;

            //Advance trailing points and start pointer
            trailPrev = trailStart;
            trailStart--;
            if (trailStart < 1) trailStart += 1600;
            trailEnd = trailStart + trailLength;// + 1;
            if (trailEnd > 1601) trailEnd -= 1601;


            //apply Mouse Gravity (if enabled)
            if (MouseGrav > 0)
            {
                //convert mouse screen coords to GL coordinates
                //Console.WriteLine(lastMousePos.X + "," + lastMousePos.Y);
                GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.Lines);
                GL.Color3(0.5f, 0.5f, 0.5f);
                GL.Vertex2(lastMousePos.X - 8, lastMousePos.Y);
                GL.Vertex2(lastMousePos.X + 8, lastMousePos.Y);
                GL.End();
                GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.Lines);
                GL.Vertex2(lastMousePos.X, lastMousePos.Y - 8);
                GL.Vertex2(lastMousePos.X, lastMousePos.Y + 8);
                GL.End();

                for (int c = 0; c < numCritters; c++)
                {
                    //get vector pointing from point to mouse
                    Vector2 g = lastMousePos - bPos[c, trailPrev];
                    float dist = g.Length();

                    if (dist >= 1f)
                    {
                        Vector2 direction = g / dist;
                        switch (MouseGrav)
                        {
                            case 1: //directional only
                                bVel[c] += (direction * mouseGravStrength);
                                break;
                            case 2:
                                //dist = g.Length;
                                bVel[c] += (direction * mouseGravStrength / dist);
                                break;
                            case 3:
                                dist = dist * dist;
                                bVel[c] += (direction * mouseGravStrength / dist);
                                break;
                        }

                    }
                }
            }

            //apply Points Gravity (if enabled)
            if (gravMode)
            {
                for (int g = 0; g < numGravPoints; g++)
                {
                    //Move them forward by their velocity
                    gPos[g] += gVel[g] * deltaTime;

                    //keep them on-screen
                    if (gPos[g].X < screenLeft)
                    {
                        gPos[g].X = screenLeft + (screenLeft - gPos[g].X);
                        gVel[g].X = minGravSpeed + (float)random.NextDouble() * maxGravSpeed;
                    }
                    else if (gPos[g].X > screenRight)
                    {
                        gPos[g].X = screenRight - (gPos[g].X - screenRight);
                        gVel[g].X = -minGravSpeed - (float)random.NextDouble() * maxGravSpeed;
                    }

                    if (gPos[g].Y < screenTop)
                    {
                        gPos[g].Y = screenTop + (screenTop - gPos[g].Y);
                        gVel[g].Y = minGravSpeed + (float)random.NextDouble() * maxGravSpeed;
                    }
                    else if (gPos[g].Y > screenBot)
                    {
                        gPos[g].Y = screenBot - (gPos[g].Y - screenBot);
                        gVel[g].Y = -minGravSpeed - (float)random.NextDouble() * maxGravSpeed;
                    }
                }
                if (showGravPoints)
                {
                    for (int g = 0; g < numGravPoints; g++)
                    {
                        GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.Lines);
                        GL.Color3(0.5f, 0.5f, 0.5f);
                        GL.Vertex2(gPos[g].X - 6, gPos[g].Y);
                        GL.Vertex2(gPos[g].X + 6, gPos[g].Y);
                        GL.End();
                        GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.Lines);
                        GL.Vertex2(gPos[g].X, gPos[g].Y - 6);
                        GL.Vertex2(gPos[g].X, gPos[g].Y + 6);
                        GL.End();
                    }
                }
                for (int c = 0; c < numCritters; c++)
                {
                    for (int g = 0; g < numGravPoints; g++)
                    {
                        //get vector pointing from point to gravwell
                        Vector2 gravVector = gPos[g] - bPos[c, trailPrev];
                        float dist = gravVector.Length();

                        if (dist > 10f)
                        {
                            Vector2 direction = gravVector / dist;
                            switch (gravFalloff)
                            {
                                case 0:
                                    bVel[c] += (direction * gravPointStrength * deltaTime);
                                    break;
                                case 1:
                                    bVel[c] += (direction * gravPointStrength * deltaTime / dist);
                                    break;
                                case 2:
                                    dist = dist * dist;
                                    bVel[c] += (direction * gravPointStrength * deltaTime / dist);
                                    break;
                                case 3:
                                    bVel[c] -= (direction * gravPointStrength * deltaTime / dist);
                                    break;
                                case 4:
                                    bVel[c] -= (direction * gravPointStrength * deltaTime);
                                    break;
                            }

                        }
                    }
                    LimitVelocity(c); //don't let gravpoints fling them too fast!
                }
            }


            if (attractorMode) // do cool chaotic attractor mode with lorenz system
            {
                float sigma = 10f;
                float rho = 28f;
                float beta = 8f / 3f;
                float time = 0.025f;
                for (int c = 0; c < numCritters; c++)
                {
                    float xt = 0, yt = 0, zt = 0;

                    //normalize/scale to a number less than 10:
                    //Vector3 totalSize = new Vector3(bPos[c, 0].X, bPos[c, 0].Y, zPos[c, 0]);
                    //float len = totalSize.Length;
                    //if (len > 1)
                    //{
                    //    Console.WriteLine("length " + len + "exceeds limit");
                    //    totalSize.Normalize();
                    //    totalSize *= 1f;
                    //    bPos[c, 0].X = totalSize.X;
                    //    bPos[c, 0].Y = totalSize.Y;
                    //    zPos[c, 0] = totalSize.Z;
                    //}




                    float xVel = sigma * (bPos[c, trailStart].Y - bPos[c, trailStart].X);
                    float yVel = (bPos[c, trailStart].X * (rho - zPos[c, trailStart])) - bPos[c, trailStart].Y;
                    float zVel = (bPos[c, trailStart].X * bPos[c, trailStart].Y) - (beta * zPos[c, trailStart]);

                    bPos[c, trailStart].X += xVel * time;
                    bPos[c, trailStart].Y += yVel * time;
                    zPos[c, trailStart] += zVel * time;

                    ////rescale to fit on screen
                    //if (bPos[c, 0].X > largest) largest = bPos[c, 0].X;
                    //if (bPos[c, 0].Y > largest) largest = bPos[c, 0].Y;

                    //float scale =  screenBot / largest;
                    //bPos[c, 0].X *= scale;
                    //bPos[c, 0].Y *= scale;
                    //zPos[c, 0] *= scale;

                    //Console.WriteLine(bPos[c, 0].X + ", " + bPos[c, 0].Y + ", " + zPos[c, 0]);


                    if (rainbowMode == 3)
                    {
                        bColor[c, trailStart] = lookupHSL[rainbowPosition] * brightnessValue[c];
                    }
                    else
                    {
                        bColor[c, trailStart] = new Vector3(1f, 1f, 1f) * brightnessValue[c];
                    }
                }
            }
            else //Regular Newtonian Physics for particle movement
            {

                if (edgesOn)
                {
                    //update point locations and velocities
                    for (int c = 0; c < numCritters; c++)
                    {
                        bPos[c, trailStart] = bPos[c, trailPrev] + bVel[c] * deltaTime;

                        //Edge Detection
                        if (bPos[c, trailStart].X < screenLeft)
                        {
                            bPos[c, trailStart].X = screenLeft + (screenLeft - bPos[c, trailStart].X);
                            bVel[c].X = (float)random.NextDouble() * maxVel;
                            LimitVelocity(c);
                        }
                        else if (bPos[c, trailStart].X > screenRight)
                        {
                            bPos[c, trailStart].X = screenRight - (bPos[c, trailStart].X - screenRight);
                            bVel[c].X = -(float)random.NextDouble() * maxVel;
                            LimitVelocity(c);
                        }

                        if (bPos[c, trailStart].Y < screenTop)
                        {
                            bPos[c, trailStart].Y = screenTop + (screenTop - bPos[c, trailStart].Y);
                            bVel[c].Y = (float)random.NextDouble() * maxVel;
                            LimitVelocity(c);
                        }
                        else if (bPos[c, trailStart].Y > screenBot)
                        {
                            bPos[c, trailStart].Y = screenBot - (bPos[c, trailStart].Y - screenBot);
                            bVel[c].Y = -(float)random.NextDouble() * maxVel;
                            LimitVelocity(c);
                        }
                    }
                }
                else //if edges are off, we just let things fly to double the screensize instead
                {
                    float lLeft = screenLeft * 2f;
                    float lRight = screenRight * 2f;
                    float lTop = screenTop * 2f;
                    float lBottom = screenBot * 2f;
                    //update point locations and velocities
                    for (int c = 0; c < numCritters; c++)
                    {
                        bPos[c, trailStart] = bPos[c, trailPrev] + bVel[c] * deltaTime;

                        //Edge Detection

                        if (bPos[c, trailStart].X < lLeft)
                        {
                            bPos[c, trailStart].X = lLeft + (lLeft - bPos[c, trailStart].X);
                            bVel[c].X = (float)random.NextDouble() * maxVel;
                            LimitVelocity(c);
                        }
                        else if (bPos[c, trailStart].X > lRight)
                        {
                            bPos[c, trailStart].X = lRight - (bPos[c, trailStart].X - lRight);
                            bVel[c].X = -(float)random.NextDouble() * maxVel;
                            LimitVelocity(c);
                        }

                        if (bPos[c, trailStart].Y < lTop)
                        {
                            bPos[c, trailStart].Y = lTop + (lTop - bPos[c, trailStart].Y);
                            bVel[c].Y = (float)random.NextDouble() * maxVel;
                            LimitVelocity(c);
                        }
                        else if (bPos[c, trailStart].Y > lBottom)
                        {
                            bPos[c, trailStart].Y = lBottom - (bPos[c, trailStart].Y - lBottom);
                            bVel[c].Y = -(float)random.NextDouble() * maxVel;
                            LimitVelocity(c);
                        }
                    }
                }

            }


            //deal with colors and hues:
            if (rainbowMode == 0) //random hues
            {
                for (int c = 0; c < numCritters; c++)
                {
                    hueTimer[c] -= 1;
                    if (hueTimer[c] <= 0)
                    {
                        //time to pick a new hue and set up speed/direction
                        hueTarget[c] = random.Next(0, 768);
                        int dir;
                        if (hueTarget[c] > currentHue[c]) dir = 1; else dir = -1;
                        if (Math.Abs(hueTarget[c] - currentHue[c]) > 384)
                        {
                            dir *= -1;
                        }
                        hueSpeed[c] = random.Next(1, 100) * dir;
                        hueTimer[c] = random.Next(1, 10) * 60;
                    }
                    if (hueSpeed[c] != 0)
                    {
                        currentHue[c] += hueSpeed[c];
                        if (currentHue[c] < 0) currentHue[c] += 768;
                        else if (currentHue[c] > 767) currentHue[c] -= 768;
                        if (Math.Abs(currentHue[c] - hueTarget[c]) <= Math.Abs(hueSpeed[c]))
                        {
                            currentHue[c] = hueTarget[c];
                            hueSpeed[c] = 0;
                        }
                    }

                    bColor[c, trailStart] = lookupHSL[currentHue[c]] * brightnessValue[c];
                }

            }
            else if (rainbowMode == 1) //synched random hues
            { //same as above except we only need to do this for a single critter.
                hueTimer[0] -= 1;
                if (hueTimer[0] <= 0)
                {
                    //time to pick a new hue and set up speed/direction
                    hueTarget[0] = random.Next(0, 768);
                    int dir;
                    if (hueTarget[0] > currentHue[0]) dir = 1; else dir = -1;
                    if (Math.Abs(hueTarget[0] - currentHue[0]) > 384)
                    {
                        dir *= -1;
                    }
                    hueSpeed[0] = random.Next(1, 100) * dir;
                    hueTimer[0] = random.Next(1, 10) * 60;
                }
                if (hueSpeed[0] != 0)
                {
                    currentHue[0] += hueSpeed[0];
                    if (currentHue[0] < 0) currentHue[0] += 768;
                    else if (currentHue[0] > 767) currentHue[0] -= 768;
                    if (Math.Abs(currentHue[0] - hueTarget[0]) <= Math.Abs(hueSpeed[0]))
                    {
                        currentHue[0] = hueTarget[0];
                        hueSpeed[0] = 0;
                    }
                }
                for (int c = 0; c < numCritters; c++)
                {
                    bColor[c, trailStart] = lookupHSL[currentHue[0]] * brightnessValue[c];
                }
            }
            else if (rainbowMode == 2) //grayscale
            {
                for (int c = 0; c < numCritters; c++)
                {
                    bColor[c, trailStart] = new Vector3(1f, 1f, 1f) * brightnessValue[c];
                }
            }
            else// (rainbowMode == 3)
            {
                for (int c = 0; c < numCritters; c++)
                {
                    bColor[c, trailStart] = lookupHSL[rainbowPosition] * brightnessValue[c];
                }
            }


            Vector3 rainbowColor;
            rainbowColor = lookupHSL[rainbowPosition];
            rainbowPosition += rainbowSpeed;
            if (rainbowPosition > 767) rainbowPosition = 0;


            if (brightnessMode)
            {
                if (brightnessSynch)
                {
                    brightnessValue[0] += brightnessDir[0];
                    if (brightnessValue[0] > 1f)
                    {
                        brightnessValue[0] = 1f - (brightnessValue[0] - 1f);
                        brightnessDir[0] = -brightnessCycleSpeed;
                        if (brightnessValue[0] < 0f) brightnessValue[0] = 1f;
                    }
                    else if (brightnessValue[0] < 0f)
                    {
                        brightnessDir[0] = brightnessCycleSpeed;
                        brightnessValue[0] = -brightnessValue[0];
                        if (brightnessValue[0] > 1f) brightnessValue[0] = 0f;
                    }
                    for (int c = 0; c < numCritters; c++)
                    {
                        brightnessValue[c] = brightnessValue[0];
                    }

                }
                else
                {
                    // to cycle brightness values
                    for (int c = 0; c < numCritters; c++)
                    {
                        //Console.WriteLine(brightnessValue[c].ToString() + " " + brightnessDir[c].ToString());
                        brightnessValue[c] += brightnessDir[c];
                        if (brightnessValue[c] > 1f)
                        {
                            brightnessValue[c] = 1f - (brightnessValue[c] - 1f);
                            brightnessDir[c] = -brightnessCycleSpeed;
                            if (brightnessValue[c] < 0f) brightnessValue[c] = 1f;
                        }
                        else if (brightnessValue[c] < 0f)
                        {
                            brightnessDir[c] = brightnessCycleSpeed;
                            brightnessValue[c] = -brightnessValue[c];
                            if (brightnessValue[c] > 1f) brightnessValue[c] = 0f;
                        }
                    }
                }
            }
            else
            {
                for (int c = 0; c < numCritters; c++)
                {
                    brightnessValue[c] = 1f;
                }
            }

            if (thicknessMode)
            {
                if (thicknessSynch)
                {
                    thicknessValue[0, 0] += thicknessDir[0];
                    if (thicknessValue[0, 0] > 10f)
                    {
                        thicknessValue[0, 0] = 10f - (thicknessValue[0, 0] - 10f);
                        thicknessDir[0] = -thicknessCycleSpeed;
                        if (thicknessValue[0, 0] < 0f) thicknessValue[0, 0] = 10f;
                    }
                    else if (thicknessValue[0, 0] < 0f)
                    {
                        thicknessDir[0] = thicknessCycleSpeed;
                        thicknessValue[0, 0] = -thicknessValue[0, 0];
                        if (thicknessValue[0, 0] > 10f) thicknessValue[0, 0] = 0f;
                    }
                    float t = thicknessValue[0, 0];
                    for (int c = 0; c < numCritters; c++)
                    {
                        thicknessValue[c, trailStart] = t;
                    }

                }
                else
                {
                    // to cycle thickness values
                    for (int c = 0; c < numCritters; c++)
                    {
                        //Console.WriteLine(thicknessValue[c].ToString() + " " + thicknessDir[c].ToString());
                        thicknessValue[c, 0] += thicknessDir[c];
                        if (thicknessValue[c, 0] > 10f)
                        {
                            thicknessValue[c, 0] = 10f - (thicknessValue[c, 0] - 10f);
                            thicknessDir[c] = -thicknessCycleSpeed;
                            if (thicknessValue[c, 0] < 0f) thicknessValue[c, 0] = 10f;
                        }
                        else if (thicknessValue[c, 0] < 0f)
                        {
                            thicknessDir[c] = thicknessCycleSpeed;
                            thicknessValue[c, 0] = -thicknessValue[c, 0];
                            if (thicknessValue[c, 0] > 10f) thicknessValue[c, 0] = 0f;
                        }
                        thicknessValue[c, trailStart] = thicknessValue[c, 0];
                    }
                }
            }
            else
            {
                for (int c = 0; c < numCritters; c++)
                {
                    thicknessValue[c, trailStart] = lineWidth;
                }
            }



            //if (thicknessMode || connectMode > 0)
            //{
            //    //advance each trailing point
            //    for (int c = 0; c < numCritters; c++)
            //    {
            //        for (int p = trailLength; p > 0; p--)
            //        {
            //            bPos[c, p] = bPos[c, p - 1];
            //            bColor[c, p] = bColor[c, p - 1];
            //            thicknessValue[c, p] = thicknessValue[c, p - 1];
            //        }
            //    }
            //}
            //else
            //{
            //    //advance each trailing point
            //    for (int c = 0; c < numCritters; c++)
            //    {
            //        for (int p = trailLength; p > 0; p--)
            //        {
            //            bPos[c, p] = bPos[c, p - 1];
            //            bColor[c, p] = bColor[c, p - 1];
            //        }
            //    }
            //}

            if (connectMode == 0)
            {
                //Console.WriteLine(bPos[0, trailStart].X.ToString() + ", " + bPos[0, trailStart].Y.ToString());
                if (trailEnd > trailStart)
                {
                    // draw each point's position
                    for (int c = 0; c < numCritters; c++)
                    {
                        GL.LineWidth(thicknessValue[c, trailStart]);
                        GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.LineStrip);

                        for (int p = trailStart; p < trailEnd; p += trailSkip)
                        {
                            GL.Color3(bColor[c, p].X, bColor[c, p].Y, bColor[c, p].Z);
                            GL.Vertex2(bPos[c, p].X, bPos[c, p].Y);
                        }
                        GL.End();
                    }
                }
                else//otherwise we need to wrap around
                {
                    for (int c = 0; c < numCritters; c++)
                    {
                        GL.LineWidth(thicknessValue[c, trailStart]);
                        GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.LineStrip);
                        for (int p = trailStart; p < 1601; p += trailSkip)
                        {
                            GL.Color3(bColor[c, p].X, bColor[c, p].Y, bColor[c, p].Z);
                            GL.Vertex2(bPos[c, p].X, bPos[c, p].Y);
                        }
                        //now that we've passed the threshold, we need to wrap around accounting for trailskip size

                        int wrap = trailSkip - ((maxPoints - trailStart) % trailSkip);

                        for (int p = wrap; p < trailEnd; p += trailSkip)
                        {
                            GL.Color3(bColor[c, p].X, bColor[c, p].Y, bColor[c, p].Z);
                            GL.Vertex2(bPos[c, p].X, bPos[c, p].Y);
                        }
                        GL.End();
                    }

                }
            }

            else if (connectMode == 1) //Squares
            {
                //Console.WriteLine(trailEnd);
                int tEnd = trailEnd - 1;
                if (tEnd < 1) { tEnd = maxPoints - 1; }
                if (tEnd >= trailStart)
                {

                    // draw each point's position
                    for (int c = 0; c < numCritters; c++)
                    {
                        for (int p = trailStart; p < tEnd; p += trailSkip)
                        {
                            int prev = p + 1;
                            if (prev >= maxPoints) prev -= maxPoints;
                            GL.LineWidth(thicknessValue[c, p]);
                            GL.Color3(bColor[c, p].X, bColor[c, p].Y, bColor[c, p].Z);
                            GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.LineLoop);
                            GL.Vertex2(bPos[c, p].X, bPos[c, p].Y);
                            GL.Vertex2(bPos[c, p].X, bPos[c, prev].Y);
                            GL.Vertex2(bPos[c, prev].X, bPos[c, prev].Y);
                            GL.Vertex2(bPos[c, prev].X, bPos[c, p].Y);
                            GL.End();
                        }
                    }
                }
                else
                {
                    for (int c = 0; c < numCritters; c++)
                    {
                        //need to integrate skipping into this, too
                        for (int p = trailStart; p < maxPoints - 1; p += trailSkip)
                        {
                            //int prev = p - 1;
                            //if (prev < 0) prev += maxPoints;
                            int prev = p + 1;
                            if (prev >= maxPoints) prev -= maxPoints;
                            GL.LineWidth(thicknessValue[c, p]);
                            GL.Color3(bColor[c, p].X, bColor[c, p].Y, bColor[c, p].Z);
                            GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.LineLoop);
                            GL.Vertex2(bPos[c, p].X, bPos[c, p].Y);
                            GL.Vertex2(bPos[c, p].X, bPos[c, prev].Y);
                            GL.Vertex2(bPos[c, prev].X, bPos[c, prev].Y);
                            GL.Vertex2(bPos[c, prev].X, bPos[c, p].Y);
                            GL.End();
                        }

                        int wrap = trailSkip - ((maxPoints - trailStart) % trailSkip);

                        for (int p = wrap; p < tEnd; p += trailSkip)
                        {
                            int prev = p + 1;
                            if (prev >= maxPoints) prev -= maxPoints;
                            GL.LineWidth(thicknessValue[c, p]);
                            GL.Color3(bColor[c, p].X, bColor[c, p].Y, bColor[c, p].Z);
                            GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.LineLoop);
                            GL.Vertex2(bPos[c, p].X, bPos[c, p].Y);
                            GL.Vertex2(bPos[c, p].X, bPos[c, prev].Y);
                            GL.Vertex2(bPos[c, prev].X, bPos[c, prev].Y);
                            GL.Vertex2(bPos[c, prev].X, bPos[c, p].Y);
                            GL.End();
                        }
                    }
                }

            }
            else if (connectMode == 2) //Circles
            {
                int tEnd = trailEnd - 1;
                if (tEnd < 1) { tEnd = maxPoints; }
                //Console.WriteLine(trailEnd + ", " + deltaTime);

                if (tEnd >= trailStart)
                {
                    // draw each point's position
                    for (int c = 0; c < numCritters; c++)
                    {

                        circleSize[c, trailStart] = new Vector2(bPos[c, trailStart].X - bPos[c, trailPrev].X, bPos[c, trailStart].Y - bPos[c, trailPrev].Y).Length() / 2f;

                        for (int p = trailStart; p < tEnd; p += trailSkip)
                        {
                            float sSize = circleSize[c, p];
                            GL.LineWidth(thicknessValue[c, p]);
                            GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.LineLoop);
                            GL.Color3(bColor[c, p].X, bColor[c, p].Y, bColor[c, p].Z);

                            for (int i = 0; i < maxCirclePoints; i++)
                            {
                                GL.Vertex2(bPos[c, p].X + (circlePoints[i].X * sSize), bPos[c, p].Y + (circlePoints[i].Y * sSize));
                            }
                            GL.End();
                        }
                    }
                }
                else
                {

                    for (int c = 0; c < numCritters; c++)
                    {
                        circleSize[c, trailStart] = new Vector2(bPos[c, trailStart].X - bPos[c, trailPrev].X, bPos[c, trailStart].Y - bPos[c, trailPrev].Y).Length() / 2f;

                        for (int p = trailStart; p < maxPoints; p += trailSkip)
                        {
                            float sSize = circleSize[c, p];
                            GL.LineWidth(thicknessValue[c, p]);
                            GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.LineLoop);
                            GL.Color3(bColor[c, p].X, bColor[c, p].Y, bColor[c, p].Z);

                            for (int i = 0; i < maxCirclePoints; i++)
                            {
                                GL.Vertex2(bPos[c, p].X + (circlePoints[i].X * sSize), bPos[c, p].Y + (circlePoints[i].Y * sSize));
                            }
                            GL.End();
                        }

                        int wrap = trailSkip - ((maxPoints - trailStart) % trailSkip);

                        for (int p = wrap; p < tEnd; p += trailSkip)
                        {
                            float sSize = circleSize[c, p];
                            GL.LineWidth(thicknessValue[c, p]);
                            GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.LineLoop);
                            GL.Color3(bColor[c, p].X, bColor[c, p].Y, bColor[c, p].Z);

                            for (int i = 0; i < maxCirclePoints; i++)
                            {
                                GL.Vertex2(bPos[c, p].X + (circlePoints[i].X * sSize), bPos[c, p].Y + (circlePoints[i].Y * sSize));
                            }
                            GL.End();
                        }
                    }
                }
            }
            else if (connectMode == 3) //Mystify
            {
                // draw each point's position
                int maxUsed = (int)(numCritters / polygonLength) * polygonLength;
                if (maxUsed == 0)
                { //we ended up in a situation where the numcritters is too low
                    numCritters = polygonLength;
                    settingsForm.numCritters = numCritters;
                    maxUsed = polygonLength;
                    settingsForm.fixCritterCount(polygonLength);
                    initCritters(); //reset just in case
                }

                //for (int c = 0; c < maxUsed; c += polygonLength)
                //{

                //    for (int p = 0; p < trailLength; p += trailSkip)
                //    {
                //        GL.LineWidth(thicknessValue[c, p]);
                //        GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.LineLoop);
                //        GL.Color3(bColor[c, p].X, bColor[c, p].Y, bColor[c, p].Z);

                //        for (int v = 0; v < polygonLength; v++)
                //        {
                //            GL.Vertex2(bPos[c + v, p]);
                //        }
                //        GL.End();
                //    }
                //}

                if (trailEnd > trailStart)
                {
                    // draw each point's position
                    for (int c = 0; c < maxUsed; c += polygonLength)
                    {

                        for (int p = trailStart; p < trailEnd; p += trailSkip)
                        {
                            GL.LineWidth(thicknessValue[c, p]);
                            GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.LineLoop);
                            GL.Color3(bColor[c, p].X, bColor[c, p].Y, bColor[c, p].Z);
                            for (int v = 0; v < polygonLength; v++)
                            {
                                GL.Vertex2(bPos[c + v, p].X, bPos[c + v, p].Y);
                            }
                            GL.End();
                        }
                    }
                }
                else//otherwise we need to wrap around
                {
                    for (int c = 0; c < maxUsed; c += polygonLength)
                    {

                        for (int p = trailStart; p < 1601; p += trailSkip)
                        {
                            GL.LineWidth(thicknessValue[c, p]);
                            GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.LineLoop);
                            GL.Color3(bColor[c, p].X, bColor[c, p].Y, bColor[c, p].Z);
                            for (int v = 0; v < polygonLength; v++)
                            {
                                GL.Vertex2(bPos[c + v, p].X, bPos[c + v, p].Y);
                            }
                            GL.End();
                        }
                        //now that we've passed the threshold, we need to wrap around accounting for trailskip size

                        int wrap = trailSkip - ((maxPoints - trailStart) % trailSkip);

                        for (int p = wrap; p < trailEnd; p += trailSkip)
                        {
                            GL.LineWidth(thicknessValue[c, p]);
                            GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.LineLoop);
                            GL.Color3(bColor[c, p].X, bColor[c, p].Y, bColor[c, p].Z);
                            for (int v = 0; v < polygonLength; v++)
                            {
                                GL.Vertex2(bPos[c + v, p].X, bPos[c + v, p].Y);
                            }
                            GL.End();
                        }
                    }

                }





            }
            else if (connectMode == 4) //Solid Mystify (Bowties)
            {

                // draw each point's position
                int maxUsed = (int)(numCritters / polygonLength) * polygonLength;
                if (maxUsed == 0)
                { //we ended up in a situation where the numcritters is too low
                    numCritters = polygonLength;
                    settingsForm.numCritters = numCritters;
                    maxUsed = polygonLength;
                    settingsForm.fixCritterCount(polygonLength);
                    initCritters(); //reset just in case
                }
                //for (int c = 0; c < maxUsed; c += polygonLength)
                //{

                //    for (int p = 0; p < trailLength; p += trailSkip)
                //    {
                //        for (int v = 0; v < polygonLength - 1; v++)
                //        {
                //            GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.Polygon);
                //            GL.Color3(bColor[c, p].X, bColor[c, p].Y, bColor[c, p].Z);
                //            GL.LineWidth(thicknessValue[c, p]);
                //            GL.Vertex2(bPos[c + v, p]);
                //            GL.Vertex2(bPos[c + v + 1, p]);
                //            GL.Vertex2(bPos[c + v + 1, p + 1]);
                //            GL.Vertex2(bPos[c + v, p + 1]);
                //            GL.End();

                //        }
                //        GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.Polygon);
                //        GL.Color3(bColor[c, p].X, bColor[c, p].Y, bColor[c, p].Z);
                //        GL.LineWidth(thicknessValue[c, p]);
                //        GL.Vertex2(bPos[c + polygonLength - 1, p]);
                //        GL.Vertex2(bPos[c, p].X,bPos[c,p].Y);
                //        GL.Vertex2(bPos[c, p + 1]);
                //        GL.Vertex2(bPos[c + polygonLength - 1, p + 1]);
                //        GL.End();

                //    }
                //}

                //Console.WriteLine(trailStart);
                int tEnd = trailEnd - 1;
                if (tEnd < 1) { tEnd = maxPoints - 1; }
                if (tEnd >= trailStart)
                {

                    // draw each point's position
                    for (int c = 0; c < maxUsed; c += polygonLength)
                    {

                        for (int p = trailStart; p < tEnd; p += trailSkip)
                        {
                            int prev = p + 1;
                            if (prev >= maxPoints) prev -= maxPoints;
                            for (int v = 0; v < polygonLength - 1; v++)
                            {
                                GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.Polygon);
                                GL.Color3(bColor[c, p].X, bColor[c, p].Y, bColor[c, p].Z);
                                GL.LineWidth(thicknessValue[c, p]);
                                GL.Vertex2(bPos[c + v, p].X, bPos[c + v, p].Y);
                                GL.Vertex2(bPos[c + v + 1, p].X, bPos[c + v + 1, p].Y);
                                GL.Vertex2(bPos[c + v + 1, prev].X, bPos[c + v + 1, prev].Y);
                                GL.Vertex2(bPos[c + v, prev].X, bPos[c + v, prev].Y);
                                GL.End();
                            }
                            GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.Polygon);
                            GL.Color3(bColor[c, p].X, bColor[c, p].Y, bColor[c, p].Z);
                            GL.LineWidth(thicknessValue[c, p]);
                            GL.Vertex2(bPos[c + polygonLength - 1, p].X, bPos[c + polygonLength - 1, p].Y);
                            GL.Vertex2(bPos[c, p].X, bPos[c, p].Y);
                            GL.Vertex2(bPos[c, prev].X, bPos[c, prev].Y);
                            GL.Vertex2(bPos[c + polygonLength - 1, prev].X, bPos[c + polygonLength - 1, prev].Y);
                            GL.End();
                        }
                    }
                }
                else//otherwise we need to wrap around
                {
                    for (int c = 0; c < maxUsed; c += polygonLength)
                    {

                        for (int p = trailStart; p < maxPoints; p += trailSkip)
                        {
                            int prev = p + 1;
                            if (prev >= maxPoints) prev -= maxPoints;
                            for (int v = 0; v < polygonLength - 1; v++)
                            {
                                GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.Polygon);
                                GL.Color3(bColor[c, p].X, bColor[c, p].Y, bColor[c, p].Z);
                                GL.LineWidth(thicknessValue[c, p]);
                                GL.Vertex2(bPos[c + v, p].X, bPos[c + v, p].Y);
                                GL.Vertex2(bPos[c + v + 1, p].X, bPos[c + v + 1, p].Y);
                                GL.Vertex2(bPos[c + v + 1, prev].X, bPos[c + v + 1, prev].Y);
                                GL.Vertex2(bPos[c + v, prev].X, bPos[c + v, prev].Y);
                                GL.End();
                            }
                            GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.Polygon);
                            GL.Color3(bColor[c, p].X, bColor[c, p].Y, bColor[c, p].Z);
                            GL.LineWidth(thicknessValue[c, p]);
                            GL.Vertex2(bPos[c + polygonLength - 1, p].X, bPos[c + polygonLength - 1, p].Y);
                            GL.Vertex2(bPos[c, p].X, bPos[c, p].Y);
                            GL.Vertex2(bPos[c, prev].X, bPos[c, prev].Y);
                            GL.Vertex2(bPos[c + polygonLength - 1, prev].X, bPos[c + polygonLength - 1, prev].Y);
                            GL.End();
                        }
                        //now that we've passed the threshold, we need to wrap around accounting for trailskip size

                        int wrap = trailSkip - ((maxPoints - trailStart) % trailSkip);

                        for (int p = wrap; p < tEnd; p += trailSkip)
                        {
                            int prev = p + 1;
                            if (prev >= maxPoints) prev -= maxPoints;
                            for (int v = 0; v < polygonLength - 1; v++)
                            {
                                GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.Polygon);
                                GL.Color3(bColor[c, p].X, bColor[c, p].Y, bColor[c, p].Z);
                                GL.LineWidth(thicknessValue[c, p]);
                                GL.Vertex2(bPos[c + v, p].X, bPos[c + v, p].Y);
                                GL.Vertex2(bPos[c + v + 1, p].X, bPos[c + v + 1, p].Y);
                                GL.Vertex2(bPos[c + v + 1, prev].X, bPos[c + v + 1, prev].Y);
                                GL.Vertex2(bPos[c + v, prev].X, bPos[c + v, prev].Y);
                                GL.End();
                            }
                            GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.Polygon);
                            GL.Color3(bColor[c, p].X, bColor[c, p].Y, bColor[c, p].Z);
                            GL.LineWidth(thicknessValue[c, p]);
                            GL.Vertex2(bPos[c + polygonLength - 1, p].X, bPos[c + polygonLength - 1, p].Y);
                            GL.Vertex2(bPos[c, p].X, bPos[c, p].Y);
                            GL.Vertex2(bPos[c, prev].X, bPos[c, prev].Y);
                            GL.Vertex2(bPos[c + polygonLength - 1, prev].X, bPos[c + polygonLength - 1, prev].Y);
                            GL.End();
                        }
                    }

                }
            }

            GL.Flush();
            SwapBuffers();
        }








        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            processInput(e);
        }

        bool wasPressed(Key key)
        {
            return (keyboard[key] && !lastkeyboard[key]);
        }

        //if a key is being held down, after 700 ms, we need to start repeating the keystroke
        void setKeyRepeat(int k, Key key, int t)
        {
            if (keyboard[key])
            {
                if (lastkeyboard[key])
                {
                    //a key needs to be repeated when:
                    // it is down now, it was down last time, timer is less than 0
                    keyTimers[k] -= t;
                    if (keyTimers[k] <= 0)
                    {
                        keysHeld[k] = true;
                        keyTimers[k] = 50;
                    }

                }
                else
                {
                    //first time through here since pressing this key, set the timer for longer
                    keyTimers[k] = 350;
                }

            }

        }

        void processInput(FrameEventArgs e)
        {
            keyboard = OpenTK.Input.Keyboard.GetState();
            int dT = (int)(e.Time * 1000);
            setKeyRepeat((int)keysUsed.Left, Key.Left, dT);
            setKeyRepeat((int)keysUsed.Right, Key.Right, dT);
            setKeyRepeat((int)keysUsed.PageUp, Key.PageUp, dT);
            setKeyRepeat((int)keysUsed.PageDown, Key.PageDown, dT);
            setKeyRepeat((int)keysUsed.Enter, Key.Enter, dT);

            if (!Focused) return; //only process if window has focus!

            if (keyboard.IsKeyDown(Key.BackSpace))
            {
                initCritters();
            }

            if (keyboard.IsKeyDown(Key.Space) && !lastkeyboard.IsKeyDown(Key.Space))
            {
                connectMode++;
                if (connectMode > 4) connectMode = 0;
            }

            if (keyboard.IsKeyDown(Key.M) && !lastkeyboard.IsKeyDown(Key.M))
            {
                MouseGrav++;
                if (MouseGrav > 1) MouseGrav = 0; //turned off the linear and squared grav modes for mouse.
            }

            if (keyboard.IsKeyDown(Key.R) && !lastkeyboard.IsKeyDown(Key.R))
            {
                rainbowMode++;
                if (rainbowMode > 3) rainbowMode = 0;
            }

            //if (keyboard.IsKeyDown(Key.Escape) && !lastkeyboard.IsKeyDown(Key.Escape))
            //{
            //    //settingsForm.Show();
            //    settingsForm.Focus();
            //    if (inputMode)
            //    {
            //        inputMode = false;
            //    }
            //    else
            //    {
            //        if (showHelp)
            //        {
            //            showHelp = false;
            //        }
            //        else
            //        {
            //            exitQuestion = !exitQuestion;
            //        }
            //    }

            //}
            //if (wasPressed(Key.Y) && exitQuestion) Exit();

            //if (wasPressed(Key.Period)) lineWidth *= 1.5f;
            //if (wasPressed(Key.Comma)) { lineWidth /= 1.5f; if (lineWidth < 1f) lineWidth = 1f; }

            //params to be changed:
            // gravmode
            // number of grav points up/down
            // edge bouncing (or let them go past edges)
            // brightness cycling (like rainbow mode) in several different speeds
            // size cycling in different speeds
            // speed up/down
            // trail length up/down
            // number of critters up/down
            // sound reaction mode
            // attractor Mode
            if (wasPressed(Key.A))
            {
                attractorMode = !attractorMode;
                if (attractorMode)
                {
                    Random r = new Random();
                    for (int c = 0; c < maxCritters; c++)
                    {
                        bPos[c, trailStart].X = (float)r.NextDouble() * 10f;
                        bPos[c, trailStart].Y = (float)r.NextDouble() * 10f;
                        bVel[c].X = 0;
                        bVel[c].Y = 0;
                        zPos[c, 0] = (float)r.NextDouble() * 10f;
                        for (int p = 0; p < maxPoints; p++)
                        {
                            bPos[c, p].X = bPos[c, trailStart].X;
                            bPos[c, p].Y = bPos[c, trailStart].Y;
                            //bVel[c].X = 0;
                            //bVel[c].Y = 0;
                            bColor[c, p] = new Vector3(1f, 1f, 1f);
                        }
                    }
                }
                else
                {
                    initCritters();
                }
            }

            if (wasPressed(Key.E)) edgesOn = !edgesOn;

            if (wasPressed(Key.Right) || keysHeld[(int)keysUsed.Right])
            {
                keysHeld[(int)keysUsed.Right] = false;
            }
            if (wasPressed(Key.Left) || keysHeld[(int)keysUsed.Left])
            {
                keysHeld[(int)keysUsed.Left] = false;
            }

            if (wasPressed(Key.PageUp) || keysHeld[(int)keysUsed.PageUp])
            {
                keysHeld[(int)keysUsed.PageUp] = false;
            }
            if (wasPressed(Key.PageDown) || keysHeld[(int)keysUsed.PageDown])
            {
                keysHeld[(int)keysUsed.PageDown] = false;
            }

            //if (wasPressed(Key.N))
            //{
            //    if (exitQuestion)
            //    {
            //        exitQuestion = false;
            //    }
            //    else
            //    {
            //        inputMode = true;
            //        inputVal = "";
            //    }
            //}

            //if (inputMode) //check for numeric characters being entered
            //{
            //    if (keyboard.IsAnyKeyDown)
            //    {
            //        for (int offset = 0; offset < 10; offset++)
            //        {
            //            if ((keyboard.IsKeyDown((Key)offset + 67) && !lastkeyboard.IsKeyDown((Key)offset + 67)) || (keyboard.IsKeyDown((Key)offset + 109) && !lastkeyboard.IsKeyDown((Key)offset + 109))) inputVal += offset.ToString();
            //        }
            //    }
            //}



            if (wasPressed(Key.PrintScreen))
            {
                //createMessage("WARNING: SCREENSHOT NOT SAVED! UNKNOWN REASON", -1000, 900, 3, 10000);

                System.Drawing.Bitmap bmp = takeScreenshot();
                var encoderParameters = new System.Drawing.Imaging.EncoderParameters(1);
                encoderParameters.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 1L);
                System.IO.Directory.CreateDirectory("screenshots");

                //find a filename that isn't taken
                string currentDirName = System.IO.Directory.GetCurrentDirectory();
                string[] files = System.IO.Directory.GetFiles(currentDirName + @"\screenshots", "*.png");
                string outName = "";
                for (int i = 0; i < 10000; i++)
                {
                    bool success = true;
                    outName = "Screenshot_" + i.ToString().PadLeft(4, '0') + ".png";
                    foreach (string s in files)
                    {
                        if (Path.GetFileName(s) == outName)
                        {
                            success = false;
                            break;
                        }
                    }
                    if (success) break;
                }
                if (outName != "Screenshot_9999.png")
                {
                    string fName = currentDirName + @"\screenshots\" + outName;
                    bmp.Save(fName, GetEncoder(System.Drawing.Imaging.ImageFormat.Png), encoderParameters);
                    //createMessage("SCREENSHOT \'" + outName + "\' SAVED!", -1000, 900, 3, 10000);
                }
                else
                {
                    //createMessage("WARNING: SCREENSHOT DIR IS FULL! FILE NOT SAVED!",-1000,900,3,10000);
                }

            }

            lastkeyboard = keyboard;
            lastMousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            if (Mouse.GetState().LeftButton == OpenTK.Input.ButtonState.Pressed)
            {
                // on the first tick of mouse being pressed, do not calculate the delta from the last time mousepos was stored
                // otherwise it jumps way too far and unpredictably for the user
                if (lastButtonState)
                {
                    Vector2 delta = lastMousePos - new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
                    //lock to either X or Y axis to make rotating less difficult:
                    if (Math.Abs(delta.X) > Math.Abs(delta.Y)) spinAngle -= delta.X / 100;
                    else spinAngleY -= delta.Y / 100;
                }

                //lastMousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
                //createMessage("Spin " + ((int)(spinAngle*180/Math.PI)).ToString() + "," + ((int)(spinAngleY*180/Math.PI)).ToString(), -1000f, 800f, 2, 1000);
            }

            int deltaWheel = lastWheel - Mouse.GetState().Wheel;
            if (deltaWheel > 0)
            {
                viewScale *= 1.05f;
                //createMessage("Zoom " + (1/viewScale).ToString(), -1000, 800, 3, 3000);
            }
            if (deltaWheel < 0)
            {
                viewScale /= 1.05f;
                //createMessage("Zoom " + (1/viewScale).ToString(), -1000, 800, 3, 3000);
            }
            lastWheel = Mouse.GetState().Wheel;
            lastButtonState = Mouse.GetState().LeftButton == OpenTK.Input.ButtonState.Pressed;
        }

        public System.Drawing.Bitmap takeScreenshot()
        {
            if (OpenTK.Graphics.GraphicsContext.CurrentContext == null)
                throw new OpenTK.Graphics.GraphicsContextMissingException();
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(Width, Height);
            System.Drawing.Imaging.BitmapData data =
                bmp.LockBits(ClientRectangle, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            GL.ReadPixels(0, 0, Width, Height, PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
            bmp.UnlockBits(data);

            bmp.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipY);
            return bmp;
        }

        private static System.Drawing.Imaging.ImageCodecInfo GetEncoder(System.Drawing.Imaging.ImageFormat format)
        {
            var codecs = System.Drawing.Imaging.ImageCodecInfo.GetImageDecoders();
            foreach (var codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }






        protected override void OnFocusedChanged(EventArgs e)
        {
            base.OnFocusedChanged(e);
            lastMousePos = new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);
        }


        void initCritters()
        {
            float targetAspectRatio = (float)this.Width / this.Height; // 1.777
            float screenLeft = -1000f * targetAspectRatio;
            float screenRight = 1000f * targetAspectRatio;
            float screenTop = -1000f;
            float screenBot = 1000f;
            float speedRange = maxVel - minVel;

            Random r = new Random();
            int thisHue = r.Next(0, 768);

            trailStart = 1;
            trailPrev = 0;
            trailEnd = trailStart + trailLength;

            if (r.NextDouble() > 0.8)
            {//totally random positions and motion:
                for (int c = 0; c < maxCritters; c++)
                {
                    bPos[c, 0].X = screenLeft + ((float)r.NextDouble() * (screenRight - screenLeft));
                    bPos[c, 0].Y = screenTop + ((float)r.NextDouble() * (screenBot - screenTop));
                    if (r.NextDouble() > 0.5) bVel[c].X = ((float)r.NextDouble() * speedRange) + minVel;
                    else bVel[c].X = ((float)r.NextDouble() * -speedRange) - minVel;
                    if (r.NextDouble() > 0.5) bVel[c].Y = ((float)r.NextDouble() * speedRange) + minVel;
                    else bVel[c].Y = (float)r.NextDouble() * -maxVel;
                }
            }
            else if (r.NextDouble() > 0.7)
            { //explode from center
                for (int c = 0; c < maxCritters; c++)
                {
                    bPos[c, 0].X = 0f;
                    bPos[c, 0].Y = 0f;
                    double a = r.NextDouble() * 6.28;
                    float v = ((float)r.NextDouble() * speedRange) + minVel;
                    bVel[c].X = (float)Math.Sin(a) * v;
                    bVel[c].Y = (float)Math.Cos(a) * v;
                }
            }
            else if (r.NextDouble() > 0.5)
            { //explode from a corner
                switch (r.Next(4))
                {
                    case 0: //upper left
                        for (int c = 0; c < maxCritters; c++)
                        {
                            bPos[c, 0].X = screenLeft;
                            bPos[c, 0].Y = screenTop;
                            double a = (r.NextDouble() * 1.57) + 1.57;
                            float v = ((float)r.NextDouble() * speedRange) + minVel;
                            bVel[c].X = (float)Math.Sin(a) * v;
                            bVel[c].Y = (float)Math.Cos(a) * -v;
                        }
                        break;
                    case 1: //upper right
                        for (int c = 0; c < maxCritters; c++)
                        {
                            bPos[c, 0].X = screenRight;
                            bPos[c, 0].Y = screenTop;
                            double a = (r.NextDouble() * 1.57) - 3.14159;
                            float v = ((float)r.NextDouble() * speedRange) + minVel;
                            bVel[c].X = (float)Math.Sin(a) * v;
                            bVel[c].Y = (float)Math.Cos(a) * -v;
                        }
                        break;
                    case 2: //From the right
                        for (int c = 0; c < maxCritters; c++)
                        {
                            bPos[c, 0].X = screenRight;
                            bPos[c, 0].Y = screenTop + ((float)r.NextDouble() * (screenBot - screenTop));
                            double a = (r.NextDouble() * 0.2) - 1.67;
                            float v = ((float)r.NextDouble() * speedRange) + minVel;
                            bVel[c].X = (float)Math.Sin(a) * v;
                            bVel[c].Y = (float)Math.Cos(a) * -v;
                        }
                        break;
                    case 3: //lower left
                        for (int c = 0; c < maxCritters; c++)
                        {
                            bPos[c, 0].X = screenLeft;
                            bPos[c, 0].Y = screenTop + ((float)r.NextDouble() * (screenBot - screenTop));
                            double a = (r.NextDouble() * 0.2) + 1.47;
                            float v = ((float)r.NextDouble() * speedRange) + minVel;
                            bVel[c].X = (float)Math.Sin(a) * v;
                            bVel[c].Y = (float)Math.Cos(a) * -v;
                        }
                        break;

                }


            }
            else //Rain mode: all start at one edge of the screen going nearly the same direction.
            {
                switch (r.Next(4))
                {
                    case 0: //Top
                        for (int c = 0; c < maxCritters; c++)
                        {
                            bPos[c, 0].X = screenLeft + ((float)r.NextDouble() * (screenRight - screenLeft));
                            bPos[c, 0].Y = screenTop;
                            double a = (r.NextDouble() * 0.3) - 0.15;
                            float v = ((float)r.NextDouble() * speedRange) + minVel;
                            bVel[c].X = (float)Math.Sin(a) * v;
                            bVel[c].Y = (float)Math.Cos(a) * v;
                        }
                        break;
                    case 1: //Bottom
                        for (int c = 0; c < maxCritters; c++)
                        {
                            bPos[c, 0].X = screenLeft + ((float)r.NextDouble() * (screenRight - screenLeft));
                            bPos[c, 0].Y = screenBot;
                            double a = (r.NextDouble() * 0.3) - 0.15;
                            float v = ((float)r.NextDouble() * speedRange) + minVel;
                            bVel[c].X = (float)Math.Sin(a) * v;
                            bVel[c].Y = (float)Math.Cos(a) * -v;
                        }
                        break;
                    case 2: //right
                        for (int c = 0; c < maxCritters; c++)
                        {
                            bPos[c, 0].X = screenRight;
                            bPos[c, 0].Y = screenBot;
                            double a = (r.NextDouble() * 1.57) - 1.57;
                            float v = ((float)r.NextDouble() * speedRange) + minVel;
                            bVel[c].X = (float)Math.Sin(a) * v;
                            bVel[c].Y = (float)Math.Cos(a) * -v;
                        }
                        break;
                    case 3: //lower left
                        for (int c = 0; c < maxCritters; c++)
                        {
                            bPos[c, 0].X = screenLeft;
                            bPos[c, 0].Y = screenBot;
                            double a = (r.NextDouble() * 1.57);
                            float v = ((float)r.NextDouble() * speedRange) + minVel;
                            bVel[c].X = (float)Math.Sin(a) * v;
                            bVel[c].Y = (float)Math.Cos(a) * -v;
                        }
                        break;

                }
            }

            for (int c = 0; c < maxCritters; c++)
            {

                zPos[c, 0] = ((float)r.NextDouble() * 10f) - 5f;
                for (int p = 0; p < maxPoints; p++)
                {
                    bPos[c, p].X = bPos[c, 0].X;
                    bPos[c, p].Y = bPos[c, 0].Y;
                    bColor[c, p] = new System.Numerics.Vector3(0f, 0f, 0f);
                }
                if (brightnessSynch)
                {
                    brightnessValue[c] = 0f;
                    brightnessDir[c] = brightnessCycleSpeed;
                }
                else
                {
                    brightnessValue[c] = (float)r.NextDouble();
                    brightnessDir[c] = brightnessCycleSpeed;
                }

                if (thicknessSynch)
                {
                    thicknessValue[c, 1] = 1f;
                    thicknessDir[c] = thicknessCycleSpeed;
                }
                else
                {
                    thicknessValue[c, 1] = r.Next(1, 10);
                    thicknessDir[c] = thicknessCycleSpeed;
                }

                if (rainbowMode == 0)
                {
                    currentHue[c] = r.Next(0, 768);
                    hueTarget[c] = currentHue[c];
                    hueSpeed[c] = 1;
                }
                else if (rainbowMode == 1)
                {
                    currentHue[c] = thisHue;
                    hueTarget[c] = currentHue[c];
                    hueSpeed[c] = 0;
                }

                //also initialize the grav points here
                for (int g = 0; g < maxGravPoints; g++)
                {
                    gPos[g].X = screenLeft + ((float)r.NextDouble() * (screenRight - screenLeft));
                    gPos[g].Y = screenTop + ((float)r.NextDouble() * (screenBot - screenTop));
                    gVel[g].X = minGravSpeed + (float)r.NextDouble() * maxGravSpeed;
                    gVel[g].Y = minGravSpeed + (float)r.NextDouble() * maxGravSpeed;
                }




            }

            tickCount = 0;
            curTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            lastTime = curTime;








        }
    }
}