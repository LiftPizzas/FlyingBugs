using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Windows.Forms;
using System.IO;
using SharpDX.XAudio2;
using SharpDX.Multimedia;
using SharpDX.IO;

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
        const int maxCritters = 20;
        const int maxPoints = 16384;
        Vector2[,] bPos = new Vector2[maxCritters, maxPoints];
        float[,] zPos = new float[maxCritters, maxPoints];
        Vector2[,] bVel = new Vector2[maxCritters, maxPoints];
        Vector3[,] bHSL = new Vector3[maxCritters, maxPoints]; //expressed in HSL, needs to be looked up when newly calculated
        Vector3[,] bColor = new Vector3[maxCritters, maxPoints]; //this is the actual draw color, expressed in RGB

        float maxVel = 2f;
        float minVel = 1f;

        int connectMode = 0;
        //0 = individual bugs (lines)
        //1 = Individual squares
        //2 = Individual circles
        //3 = mystify style (each 4 critters form a loop)

        //brightness cycle mode, size cycle mode

        float lineWidth = 4f;

        int trailLength = 100; //number from 1 to maxpoints, how many trailing points are drawn and tracked

        Vector3[] lookupHSL = new Vector3[768]; //Hue is looked up, Saturation and Lightness (if <1) are calculated
        // gives full saturation of indexed hue 0-767

        bool gravOn = false;
        float gravStrength = 0.1f;
        int MouseGrav = 0;// Mouse cursor acts as an attractor.
        //0= off
        //1= directional (distance doesn't matter)
        //2= linear falloff
        //3= Squared Falloff

        bool rainbowMode = true;
        int rainbowPosition = 0; //current rainbow color to lookup in hue chart
        bool brightnessMode = true;
        float[] brightnessValue = new float [maxCritters];
        float[] brightnessDir = new float[maxCritters];
        float brightnessCycleSpeed = 0.1f;
        int brightnessCycleNotch = 0; //various preset speeds to select from

        bool attractorMode = false;

        bool SoundReaction = false;
        string soundFile = ""; //sound file to play and react to

        /// <summary>
        /// OLD VARS
        /// </summary>
        float viewScale = 1f;
        bool showVerts = true;
        float zWall;
        bool wireFrame = true;
        bool showBacks = true;
        bool showHelp = false;
        float lineThickness = 4f;
        float binocSplit = 500f;
        bool rotateNow = true; //rotates visualization if true
        float spinAngle = 0f; //updated each tick to rotate visualization
        float spinAngleY = 0f;
        bool fixedTop = false; //when true, point zero is fixed to the top of the sphere
        int lockedVerts = 0; // if > 0 then any verts up to this index get locked in position, this is for attempting to subdivide
        int findFaces = 1; //when 1 tries to find triangular faces, 2 shows flower view, 0 no faces rendered
        bool binocular = false; //when true, renders crosseyed 3d view
        bool haltGrav = false; //when true, stops trying to move points around until next reset
        bool inputMode = false;
        string inputVal;
        bool exitQuestion = false;
        bool hideMessage = false;



        // my vars
        Random random = new Random();

        long lastTime, curTime, deltaTime; //milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        int tempTicks; //number of ticks per loop
        int fps; //for counting how many frames are rendered each second
        int fpsCount; //to be displayed, updated once per second
        long fpsTime;

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
        XAudio2 xaudio;
        WaveFormat waveFormat;
        AudioBuffer buffer;
        SoundStream soundstream;
        SourceVoice[] sourceVoice;
        int soundOn = 0;
        int currentVoice = 0;
        int voiceLoop = 0;

        public Game()
            : base(1920, 1080, new OpenTK.Graphics.GraphicsMode(32, 24, 0, 4))
        {

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

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

            this.WindowBorder = WindowBorder.Hidden;
            Bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            Title = "Flying Bugs";
            GL.ClearColor(System.Drawing.Color.CornflowerBlue);
            GL.PointSize(5f);
        }


        Vector2[] circlePoints = new Vector2[64];
        void initcirclePoints()
        {
            int pNum = 0;
            double factor = Math.PI * 2d / 64d;
            for (int i = 0; i < 64; i++)
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

        void advanceTrailingPoints()
        {
            //advance each trailing point
            for (int c = 0; c < maxCritters; c++)
            {
                for (int p = trailLength; p > 0; p--)
                {
                    bPos[c, p] = bPos[c, p - 1];
                    bColor[c, p] = bColor[c, p - 1];
                }
            }
        }


        void LimitVelocity(int critter)
        {
            float vel = (float)Math.Sqrt((bVel[critter, 0].X * bVel[critter, 0].X) + (bVel[critter, 0].Y + bVel[critter, 0].Y));
            if (vel > maxVel)
            {
                bVel[critter, 0].X = (bVel[critter, 0].X / vel) * maxVel;
                bVel[critter, 0].Y = (bVel[critter, 0].Y / vel) * maxVel;
            }
            if (vel < minVel)
            {
                bVel[critter, 0].X = (bVel[critter, 0].X / vel) * minVel;
                bVel[critter, 0].Y = (bVel[critter, 0].Y / vel) * minVel;
            }
        }


        float largest = 1f;

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            //if (soundOn>0) playSounds();

            curTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            deltaTime = curTime - lastTime;
            fps++;
            fpsTime += deltaTime;
            if (fpsTime >= 1000)
            {
                fpsTime -= 1000;
                fpsCount = fps;
                fps = 0;
            }
            lastTime = curTime;

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


            //apply Mouse Gravity (if enabled)
            if (MouseGrav > 0)
            {
                //convert mouse screen coords to GL coordinates
                //Console.WriteLine(lastMousePos.X + "," + lastMousePos.Y);
                GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.Lines);
                GL.Color3(0.5f, 0.5f, 0.5f);
                GL.Vertex2(lastMousePos.X - 4, lastMousePos.Y);
                GL.Vertex2(lastMousePos.X + 4, lastMousePos.Y);
                GL.End();
                GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.Lines);
                GL.Vertex2(lastMousePos.X, lastMousePos.Y - 4);
                GL.Vertex2(lastMousePos.X, lastMousePos.Y + 4);
                GL.End();

                for (int c = 0; c < maxCritters; c++)
                {
                    //get vector pointing from point to mouse
                    Vector2 g = lastMousePos - bPos[c, 0];
                    float dist = g.Length;
                    Vector2 direction = g.Normalized();
                    if (dist > 0.1f)
                    {
                        switch (MouseGrav)
                        {
                            case 1: //directional only
                                bVel[c, 0] += (direction * gravStrength);
                                break;
                            case 2:
                                //dist = g.Length;
                                bVel[c, 0] += (direction * gravStrength / dist);
                                break;
                            case 3:
                                dist = dist * dist;
                                bVel[c, 0] += (direction * gravStrength / dist);
                                break;
                        }

                    }
                }
            }


            if (attractorMode) // do cool chaotic attractor mode with lorenz system
            {
                float sigma = 10f;
                float rho = 28f;
                float beta = 8f / 3f;
                float time = 0.025f;
                for (int c = 0; c < maxCritters; c++)
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

                    bPos[c, 1] = bPos[c, 0];


                    float xVel = sigma * (bPos[c, 0].Y - bPos[c, 0].X);
                    float yVel = (bPos[c, 0].X * (rho - zPos[c, 0])) - bPos[c, 0].Y;
                    float zVel = (bPos[c, 0].X * bPos[c, 0].Y) - (beta * zPos[c, 0]);

                    bPos[c, 0].X += xVel * time;
                    bPos[c, 0].Y += yVel * time;
                    zPos[c, 0] += zVel * time;

                    ////rescale to fit on screen
                    //if (bPos[c, 0].X > largest) largest = bPos[c, 0].X;
                    //if (bPos[c, 0].Y > largest) largest = bPos[c, 0].Y;

                    //float scale =  screenBot / largest;
                    //bPos[c, 0].X *= scale;
                    //bPos[c, 0].Y *= scale;
                    //zPos[c, 0] *= scale;

                    //Console.WriteLine(bPos[c, 0].X + ", " + bPos[c, 0].Y + ", " + zPos[c, 0]);


                    if (rainbowMode)
                    {
                        bColor[c, 0] = lookupHSL[rainbowPosition];
                    }
                    else
                    {
                        bColor[c, 0] = new Vector3(1f, 1f, 1f);
                    }
                }
            }
            else
            {
                //update point locations and velocities
                for (int c = 0; c < maxCritters; c++)
                {
                    bPos[c, 1] = bPos[c, 0];
                    bPos[c, 0] += bVel[c, 0] * deltaTime;

                    if (rainbowMode)
                    {
                        bColor[c, 0] = lookupHSL[rainbowPosition] * brightnessValue[c];
                    }
                    else
                    {
                        bColor[c, 0] = new Vector3(1f, 1f, 1f) * brightnessValue[c];
                    }

                    //Edge Detection
                    if (bPos[c, 0].X < screenLeft)
                    {
                        bPos[c, 0].X = screenLeft + (screenLeft - bPos[c, 0].X);
                        bVel[c, 0].X = (float)random.NextDouble() * maxVel;
                        LimitVelocity(c);
                    }
                    else if (bPos[c, 0].X > screenRight)
                    {
                        bPos[c, 0].X = screenRight - (bPos[c, 0].X - screenRight);
                        bVel[c, 0].X = -(float)random.NextDouble() * maxVel;
                        LimitVelocity(c);
                    }

                    if (bPos[c, 0].Y < screenTop)
                    {
                        bPos[c, 0].Y = screenTop + (screenTop - bPos[c, 0].Y);
                        bVel[c, 0].Y = (float)random.NextDouble() * maxVel;
                        LimitVelocity(c);
                    }
                    else if (bPos[c, 0].Y > screenBot)
                    {
                        bPos[c, 0].Y = screenBot - (bPos[c, 0].Y - screenBot);
                        bVel[c, 0].Y = -(float)random.NextDouble() * maxVel;
                        LimitVelocity(c);
                    }

                }
            }

            Vector3 rainbowColor;
            rainbowColor = lookupHSL[rainbowPosition];
            rainbowPosition += 5;
            if (rainbowPosition > 767) rainbowPosition = 0;

            // to cycle brightness values
            for (int c = 0; c < maxCritters; c++)
            {
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

            if (connectMode == 0)
            {
                advanceTrailingPoints();

                // draw each point's position
                for (int c = 0; c < maxCritters; c++)
                {
                    GL.LineWidth(lineWidth);
                    GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.LineStrip);
                    for (int p = 1; p < trailLength; p++)
                    { 
                        GL.Color3(bColor[c, p]);
                        GL.Vertex2(bPos[c, p]);
                    }
                    GL.End();
                }
            }
            else if (connectMode == 1) //Squares
            {
                advanceTrailingPoints();

                // draw each point's position
                for (int c = 0; c < maxCritters; c++)
                {
                    //float sSize = lineWidth * 3f;
                    GL.LineWidth(lineWidth);
                    for (int p = 1; p < trailLength; p++)
                    {
                        GL.Color3(bColor[c, p]);
                        GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.LineLoop);
                        GL.Vertex2(bPos[c, p].X, bPos[c, p].Y);
                        GL.Vertex2(bPos[c, p].X, bPos[c, p - 1].Y);
                        GL.Vertex2(bPos[c, p - 1].X, bPos[c, p - 1].Y);
                        GL.Vertex2(bPos[c, p - 1].X, bPos[c, p].Y);
                        GL.End();
                    }
                }
            }
            else if (connectMode == 2) //Circles
            {
                advanceTrailingPoints();

                // draw each point's position
                for (int c = 0; c < maxCritters; c++)
                {

                    GL.LineWidth(lineWidth);
                    
                    for (int p = 1; p < trailLength; p++)
                    {
                        float sSize = new Vector2(bPos[c, p].X - bPos[c, p - 1].X, bPos[c, p].Y - bPos[c, p - 1].Y).Length / 2f;
                        GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.LineLoop);
                        GL.Color3(bColor[c, p]);
                        for (int i = 0; i < circlePoints.Length; i++)
                        {
                            GL.Vertex2(bPos[c, p].X + (circlePoints[i].X * sSize), bPos[c, p].Y + (circlePoints[i].Y * sSize));
                        }
                        GL.End();
                    }
                }
            }
            else if (connectMode == 3) //Mystify
            {
                ////advance each trailing point
                //for (int c = 0; c < maxCritters; c++)
                //{
                //    for (int p = 8; p > 0; p--)
                //    {
                //        bPos[c, p] = bPos[c, p - 1];
                //    }
                //}

                advanceTrailingPoints();
                // draw each point's position
                for (int c = 0; c < maxCritters; c += 4)
                {
                    float sSize = lineWidth * 3f;
                    GL.LineWidth(lineWidth);
                    
                    for (int p = 0; p < 8; p++)
                    {
                        GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.LineLoop);
                        GL.Color3(bColor[c, p]);
                        GL.Vertex2(bPos[c, p]);
                        GL.Vertex2(bPos[c + 1, p]);
                        GL.Vertex2(bPos[c + 2, p]);
                        GL.Vertex2(bPos[c + 3, p]);
                        GL.End();
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
                if (connectMode > 3) connectMode = 0;
            }

            if (keyboard.IsKeyDown(Key.M) && !lastkeyboard.IsKeyDown(Key.M))
            {
                MouseGrav++;
                if (MouseGrav > 1) MouseGrav = 0; //turned off the linear and squared grav modes for mouse.
            }

            if (keyboard.IsKeyDown(Key.R) && !lastkeyboard.IsKeyDown(Key.R))
            {
                rainbowMode = !rainbowMode;
            }

            if (keyboard.IsKeyDown(Key.Escape) && !lastkeyboard.IsKeyDown(Key.Escape))
            {
                if (inputMode)
                {
                    inputMode = false;
                }
                else
                {
                    if (showHelp)
                    {
                        showHelp = false;
                    }
                    else
                    {
                        exitQuestion = !exitQuestion;
                    }
                }

                
            }
            if (wasPressed(Key.Y) && exitQuestion) Exit();

            if (wasPressed(Key.Period)) lineWidth *= 1.5f;
            if (wasPressed(Key.Comma)) { lineWidth /= 1.5f; if (lineWidth < 1f) lineWidth = 1f; }

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
                        bPos[c, 0].X = (float)r.NextDouble() * 10f;
                        bPos[c, 0].Y = (float)r.NextDouble() * 10f;
                        bVel[c, 0].X = 0;
                        bVel[c, 0].Y = 0;
                        zPos[c, 0] = (float)r.NextDouble() * 10f;
                        for (int p = 0; p < maxPoints; p++)
                        {
                            bPos[c, p].X = bPos[c, 0].X;
                            bPos[c, p].Y = bPos[c, 0].Y;
                            bVel[c, p].X = 0;
                            bVel[c, p].Y = 0;
                            bColor[c, p] = new Vector3(1f, 1f, 1f);
                        }
                    }
                }
                else
                {
                    initCritters();
                }
            }

            if (wasPressed(Key.U)) Console.WriteLine(largest);

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

            if (wasPressed(Key.N))
            {
                if (exitQuestion)
                {
                    exitQuestion = false;
                }
                else
                {
                    inputMode = true;
                    inputVal = "";
                }
            }

            if (inputMode) //check for numeric characters being entered
            {
                if (keyboard.IsAnyKeyDown)
                {
                    for (int offset = 0; offset < 10; offset++)
                    {
                        if ((keyboard.IsKeyDown((Key)offset + 67) && !lastkeyboard.IsKeyDown((Key)offset + 67)) || (keyboard.IsKeyDown((Key)offset + 109) && !lastkeyboard.IsKeyDown((Key)offset + 109))) inputVal += offset.ToString();
                    }
                }
            }



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

            Random r = new Random();
            for (int c = 0; c < maxCritters; c++)
            {
                bPos[c, 0].X = screenLeft + ((float)r.NextDouble() * (screenRight - screenLeft));
                bPos[c, 0].Y = screenTop + ((float)r.NextDouble() * (screenBot - screenTop));
                bVel[c, 0].X = (float)r.NextDouble() * maxVel;
                bVel[c, 0].Y = (float)r.NextDouble() * maxVel;
                zPos[c, 0] = ((float)r.NextDouble() * 10f) - 5f;
                for (int p = 0; p < maxPoints; p++)
                {
                    bPos[c, p].X = bPos[c, 0].X;
                    bPos[c, p].Y = bPos[c, 0].Y;
                    bVel[c, p].X = bVel[c, 0].X;
                    bVel[c, p].Y = bVel[c, 0].Y;
                    bColor[c, p] = new Vector3(0f, 0f, 0f);
                }
                brightnessValue[c] = (float)r.NextDouble();
                brightnessDir[c] = 0.1f;
            }


            tickCount = 0;
            curTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            lastTime = curTime;

        }










    }
}
