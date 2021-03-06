﻿//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.VirtualVacation
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Toolkit;
    using Microsoft.Kinect.Toolkit.BackgroundRemoval;
    using Microsoft.Speech.Recognition;
    using System.Xml;
    using System.Text;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        /// <summary>
        /// Format we will use for the depth stream
        /// </summary>
        private const DepthImageFormat DepthFormat = DepthImageFormat.Resolution320x240Fps30;

        /// <summary>
        /// Format we will use for the color stream
        /// </summary>
        private const ColorImageFormat ColorFormat = ColorImageFormat.RgbResolution640x480Fps30;

        /// <summary>
        /// Bitmap that will hold color information
        /// </summary>
        private WriteableBitmap maskedColorBitmap;

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensorChooser sensorChooser;

        private SpeechRecognitionEngine recognizer;

        /// <summary>
        /// Our core library which does background 
        /// </summary>
        private BackgroundRemovedColorStream backgroundRemovedColorStream;

        private ColorTransfer.Point3D[] DecorrelatedValues;
        private ColorTransfer.Point3D DecorrelatedMean;
        private ColorTransfer.Point3D DecorrelatedStdDev;

        private byte[] DecorrelatedRGBA;

        /// <summary>
        /// Bitmap that will hold color information
        /// </summary>
        private WriteableBitmap depthColorBitmap;

        /// <summary>
        /// Intermediate storage for the depth data received from the camera
        /// </summary>
        private DepthImagePixel[] depthPixels;

        /// <summary>
        /// Intermediate storage for the depth data converted to color
        /// </summary>
        private byte[] depthColorPixels;

        /// <summary>
        /// Intermediate storage for the skeleton data received from the sensor
        /// </summary>
        private Skeleton[] skeletons;

        /// <summary>
        /// the skeleton that is currently tracked by the app
        /// </summary>
        private int currentlyTrackedSkeletonId;

        private float currentSkeletonHeight;

        private Tuple<float, float, float, float> lastFloorPlane = new Tuple<float, float, float, float>(0.0f, 0.0f, 0.0f, 0.0f);

        /// <summary>
        /// Track whether Dispose has been called
        /// </summary>
        private bool disposed;

        struct VacationImage
        {
            public string BackgroundFilename;
            public string DepthMaskFilename;
            public bool ColorCorrect;
            public int TargetDepthFloorPixelX;
            public int TargetDepthFloorPixelY;
            public int UserHeightAtTargetDepth;

            public ColorTransfer.Point3D StdDev;
            public ColorTransfer.Point3D Mean;
            public ColorTransfer.Point3D[] DecorrelatedValues;
        }

        List<VacationImage> VacationImages = new List<VacationImage>();
        int VacationIndex = 0;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {

            //ColorTransfer.UnitTest();

            this.InitializeComponent();

            // initialize the sensor chooser and UI
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.KinectChanged += this.SensorChooserOnKinectChanged;
            this.sensorChooser.Start();

            /*
             * <Stages>  
    <VirtualVacationSettingsBackgroundNode>
      <BackgroundImage>Images/VirtualVacation/ManOnMoonBackground.png</BackgroundImage>
      <ForegroundImage>Images/VirtualVacation/ManOnMoonForeground.png</ForegroundImage> 
      <ForegroundDepth>2.4384</ForegroundDepth>
      <BackgroundDepth>3.6576</BackgroundDepth>
      <ColorCorrect>false</ColorCorrect>
      <TargetDepthFloorPixel>
        <x>642</x>
        <y>533</y>
      </TargetDepthFloorPixel>
      <UserHeightAtTargetDepth>400</UserHeightAtTargetDepth>
      <UserCalibrationTargetDepth>2.4384</UserCalibrationTargetDepth>
    </VirtualVacationSettingsBackgroundNode>
             */
            using (XmlReader reader = XmlReader.Create("../../VacationSettings.xml"))
            {
                // Moves the reader to the root element.
                reader.MoveToContent();
                reader.ReadToFollowing("Stages");
                while (reader.ReadToFollowing("VirtualVacationSettingsBackgroundNode"))
                {
                    VacationImage vacationImage = new VacationImage();
                    if (reader.ReadToFollowing("BackgroundImage"))
                    {
                        vacationImage.BackgroundFilename = "../../" + reader.ReadInnerXml();
                    }

                    if (reader.ReadToFollowing("DepthImage"))
                    {
                        vacationImage.DepthMaskFilename = "../../" + reader.ReadInnerXml();
                    }

                    if (reader.ReadToFollowing("ColorCorrect"))
                    {
                        vacationImage.ColorCorrect = reader.ReadElementContentAsBoolean();
                    }

                    if (reader.ReadToFollowing("TargetDepthFloorPixel"))
                    {
                        if (reader.ReadToFollowing("x"))
                        {
                            vacationImage.TargetDepthFloorPixelX = reader.ReadElementContentAsInt();
                        }
                        if (reader.ReadToFollowing("y"))
                        {
                            vacationImage.TargetDepthFloorPixelY = reader.ReadElementContentAsInt();
                        }
                    }

                    if (reader.ReadToFollowing("UserHeightAtTargetDepth"))
                    {
                        vacationImage.UserHeightAtTargetDepth = reader.ReadElementContentAsInt();
                    }
                    VacationImages.Add(vacationImage);
                }
            }
        }

        /// <summary>
        /// Finalizes an instance of the MainWindow class.
        /// This destructor will run only if the Dispose method does not get called.
        /// </summary>
        ~MainWindow()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Dispose the allocated frame buffers and reconstruction.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);

            // This object will be cleaned up by the Dispose method.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Frees all memory associated with the FusionImageFrame.
        /// </summary>
        /// <param name="disposing">Whether the function was called from Dispose.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (null != this.backgroundRemovedColorStream)
                {
                    this.backgroundRemovedColorStream.Dispose();
                    this.backgroundRemovedColorStream = null;
                }

                this.disposed = true;
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.sensorChooser.Stop();
            this.sensorChooser = null;
        }

        private bool firstValueFloorPlane = true;

        /// <summary>
        /// Event handler for Kinect sensor's DepthFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            // in the middle of shutting down, or lingering events from previous sensor, do nothing here.
            if (null == this.sensorChooser || null == this.sensorChooser.Kinect || this.sensorChooser.Kinect != sender)
            {
                return;
            }

            try
            {
                using (var depthFrame = e.OpenDepthImageFrame())
                {
                    if (null != depthFrame)
                    {
                        this.backgroundRemovedColorStream.ProcessDepth(depthFrame.GetRawPixelData(), depthFrame.Timestamp);
                    }

                    if (depthFrame != null)
                    {
                        // Copy the pixel data from the image to a temporary array
                        depthFrame.CopyDepthImagePixelDataTo(this.depthPixels);

                        // Get the min and max reliable depth for the current frame
                        int minDepth = depthFrame.MinDepth;
                        int maxDepth = depthFrame.MaxDepth;
                        float span = (float)(maxDepth - minDepth);

                        // Convert the depth to RGB
                        int colorPixelIndex = 0;
                        for (int i = 0; i < this.depthPixels.Length; ++i)
                        {
                            // Get the depth for this pixel
                            short depth = depthPixels[i].Depth;

                            // To convert to a byte, we're discarding the most-significant
                            // rather than least-significant bits.
                            // We're preserving detail, although the intensity will "wrap."
                            // Values outside the reliable depth range are mapped to 0 (black).

                            // Note: Using conditionals in this loop could degrade performance.
                            // Consider using a lookup table instead when writing production code.
                            // See the KinectDepthViewer class used by the KinectExplorer sample
                            // for a lookup table example.
                            //byte intensity = (byte)(depth >= minDepth && depth <= maxDepth ? depth : 0);

                            //byte upper = (byte) (depth >> 8);
                            //byte lower = (byte)(depth & 0xff);

                            float norm = ((float)(depth - minDepth)) / span;
                            if (norm >= 1.0f)
                                norm = 1.0f;
                            else if (norm < 0.0f)
                                norm = 0.0f;
                            byte intensity = (byte)(255.0f * norm);
                            //byte upper = byte(depth >> 8);
                            //byte lower = byte(depth & 8);

                            // Write out blue byte
                            this.depthColorPixels[colorPixelIndex++] = intensity;

                            // Write out green byte
                            this.depthColorPixels[colorPixelIndex++] = intensity;

                            // Write out red byte                        
                            this.depthColorPixels[colorPixelIndex++] = intensity;

                            // We're outputting BGR, the last byte in the 32 bits is unused so skip it
                            // If we were outputting BGRA, we would write alpha here.
                            ++colorPixelIndex;
                        }

                        // Write the pixel data into our bitmap
                        this.depthColorBitmap.WritePixels(
                            new Int32Rect(0, 0, this.depthColorBitmap.PixelWidth, this.depthColorBitmap.PixelHeight),
                            this.depthColorPixels,
                            this.depthColorBitmap.PixelWidth * sizeof(int),
                            0);

                    }
                }

                using (var colorFrame = e.OpenColorImageFrame())
                {
                    if (null != colorFrame)
                    {
                        this.backgroundRemovedColorStream.ProcessColor(colorFrame.GetRawPixelData(), colorFrame.Timestamp);
                        
                    }
                }

                using (var skeletonFrame = e.OpenSkeletonFrame())
                {
                    if (null != skeletonFrame)
                    {
                        skeletonFrame.CopySkeletonDataTo(this.skeletons);
                        this.backgroundRemovedColorStream.ProcessSkeleton(this.skeletons, skeletonFrame.Timestamp);

                        if (firstValueFloorPlane == true && 
                            skeletonFrame.FloorClipPlane.Item1 != 0.0f &&
                            skeletonFrame.FloorClipPlane.Item2 != 0.0f && 
                            skeletonFrame.FloorClipPlane.Item3 != 0.0f && 
                            skeletonFrame.FloorClipPlane.Item4 != 0.0f)
                        {
                            lastFloorPlane = skeletonFrame.FloorClipPlane;
                            firstValueFloorPlane = false;
                            ComputeUserUVTranslation();
                        }


                        foreach (Skeleton skel in skeletons)
                        {
                            if (skel.TrackingState == SkeletonTrackingState.NotTracked)
                                continue;

                            SkeletonPoint pt = skel.Joints[JointType.Head].Position;
                            ColorImagePoint headColorPos = sensorChooser.Kinect.CoordinateMapper.MapSkeletonPointToColorPoint(pt, sensorChooser.Kinect.ColorStream.Format);
                            DepthImagePoint headDepthPos = sensorChooser.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(pt, sensorChooser.Kinect.DepthStream.Format);

                            pt = skel.Joints[JointType.FootLeft].Position;
                            ColorImagePoint lFootColorPos = sensorChooser.Kinect.CoordinateMapper.MapSkeletonPointToColorPoint(pt, sensorChooser.Kinect.ColorStream.Format);

                            pt = skel.Joints[JointType.FootRight].Position;
                            ColorImagePoint rFootColorPos = sensorChooser.Kinect.CoordinateMapper.MapSkeletonPointToColorPoint(pt, sensorChooser.Kinect.ColorStream.Format);

                            if (lFootColorPos.Y < -50 || rFootColorPos.Y < -50)
                                continue;

                            float charHeightColor = Math.Abs((float) headColorPos.Y - 0.5f * (float)(lFootColorPos.Y + rFootColorPos.Y));
                            //Console.WriteLine("Char height: " + charHeightColor + " Char depth: " + headDepthPos.Depth);

                        }
                    }
                }

                this.ChooseSkeleton();
            }
            catch (InvalidOperationException)
            {
                // Ignore the exception. 
            }
        }

        /// <summary>
        /// Handle the background removed color frame ready event. The frame obtained from the background removed
        /// color stream is in RGBA format.
        /// </summary>
        /// <param name="sender">object that sends the event</param>
        /// <param name="e">argument of the event</param>
        private void BackgroundRemovedFrameReadyHandler(object sender, BackgroundRemovedColorFrameReadyEventArgs e)
        {
            using (var backgroundRemovedFrame = e.OpenBackgroundRemovedColorFrame())
            {
                if (backgroundRemovedFrame != null)
                {
                    if (null == this.maskedColorBitmap || this.maskedColorBitmap.PixelWidth != backgroundRemovedFrame.Width
                        || this.maskedColorBitmap.PixelHeight != backgroundRemovedFrame.Height)
                    {
                        this.maskedColorBitmap = new WriteableBitmap(backgroundRemovedFrame.Width, backgroundRemovedFrame.Height, 96.0, 96.0, PixelFormats.Bgra32, null);
                        int numPoints = backgroundRemovedFrame.Width * backgroundRemovedFrame.Height;
                        DecorrelatedValues = new ColorTransfer.Point3D[numPoints];
                        for (int i = 0; i < numPoints; i++)
                            DecorrelatedValues[i] = new ColorTransfer.Point3D();

                        DecorrelatedRGBA = new byte[numPoints * 4];

                        ImageBrush liveColorMap = (ImageBrush)this.Resources["liveColorMap"];
                        liveColorMap.ImageSource = maskedColorBitmap;
                    }

                    backgroundRemovedFrame.CopyPixelDataTo(DecorrelatedRGBA);

                    bool colorCorrectLive = true;
                    if (VacationImages[VacationIndex].ColorCorrect && colorCorrectLive)
                    {
                        //if (DecorrelatedStdDev == null)
                        {
                            DecorrelatedStdDev = new ColorTransfer.Point3D();
                            DecorrelatedMean = new ColorTransfer.Point3D();
                            ColorTransfer.ComputeDecorrelation(DecorrelatedRGBA, 4, ref DecorrelatedValues, out DecorrelatedMean, out DecorrelatedStdDev);
                        }
                        ColorTransfer.TransferColor(DecorrelatedMean, DecorrelatedStdDev, VacationImages[VacationIndex].Mean, VacationImages[VacationIndex].StdDev, ref DecorrelatedRGBA, 4,
                            ref DecorrelatedValues);
                    }

                    // Write the pixel data into our bitmap
                    this.maskedColorBitmap.WritePixels(
                        new Int32Rect(0, 0, this.maskedColorBitmap.PixelWidth, this.maskedColorBitmap.PixelHeight),
                        DecorrelatedRGBA, //backgroundRemovedFrame.GetRawPixelData(),
                        this.maskedColorBitmap.PixelWidth * sizeof(int),
                        0);
                }
            }
        }

        /// <summary>
        /// Use the sticky skeleton logic to choose a player that we want to set as foreground. This means if the app
        /// is tracking a player already, we keep tracking the player until it leaves the sight of the camera, 
        /// and then pick the closest player to be tracked as foreground.
        /// </summary>
        private void ChooseSkeleton()
        {
            var isTrackedSkeltonVisible = false;
            var nearestDistance = float.MaxValue;
            var nearestSkeleton = 0;

            foreach (var skel in this.skeletons)
            {
                if (null == skel)
                {
                    continue;
                }

                if (skel.TrackingState != SkeletonTrackingState.Tracked)
                {
                    continue;
                }

                if (skel.TrackingId == this.currentlyTrackedSkeletonId)
                {
                    isTrackedSkeltonVisible = true;
                    break;
                }

                if (skel.Position.Z < nearestDistance)
                {
                    nearestDistance = skel.Position.Z;
                    nearestSkeleton = skel.TrackingId;
                }
            }

            if (!isTrackedSkeltonVisible && nearestSkeleton != 0)
            {
                this.backgroundRemovedColorStream.SetTrackedPlayer(nearestSkeleton);
                this.currentlyTrackedSkeletonId = nearestSkeleton;
            }
        }

        /// <summary>
        /// Called when the KinectSensorChooser gets a new sensor
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="args">event arguments</param>
        private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs args)
        {
            if (args.OldSensor != null)
            {
                try
                {
                    args.OldSensor.AllFramesReady -= this.SensorAllFramesReady;
                    args.OldSensor.DepthStream.Disable();
                    args.OldSensor.ColorStream.Disable();
                    args.OldSensor.SkeletonStream.Disable();

                    // Create the background removal stream to process the data and remove background, and initialize it.
                    if (null != this.backgroundRemovedColorStream)
                    {
                        this.backgroundRemovedColorStream.BackgroundRemovedFrameReady -= this.BackgroundRemovedFrameReadyHandler;
                        this.backgroundRemovedColorStream.Dispose();
                        this.backgroundRemovedColorStream = null;
                    }
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }

            if (args.NewSensor != null)
            {
                try
                {
                    args.NewSensor.DepthStream.Enable(DepthFormat);
                    args.NewSensor.ColorStream.Enable(ColorFormat);
                    args.NewSensor.SkeletonStream.Enable();

                    this.backgroundRemovedColorStream = new BackgroundRemovedColorStream(args.NewSensor);
                    this.backgroundRemovedColorStream.Enable(ColorFormat, DepthFormat);

                    // Allocate space to put the depth pixels we'll receive
                    depthPixels = new DepthImagePixel[args.NewSensor.DepthStream.FramePixelDataLength];

                    // Allocate space to put the color pixels we'll create
                    depthColorPixels = new byte[args.NewSensor.DepthStream.FramePixelDataLength * sizeof(int)];

                    // This is the bitmap we'll display on-screen
                    depthColorBitmap = new WriteableBitmap(args.NewSensor.DepthStream.FrameWidth, args.NewSensor.DepthStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);


                    // Allocate space to put the depth, color, and skeleton data we'll receive
                    if (null == this.skeletons)
                    {
                        this.skeletons = new Skeleton[args.NewSensor.SkeletonStream.FrameSkeletonArrayLength];
                    }

                    // Add an event handler to be called when the background removed color frame is ready, so that we can
                    // composite the image and output to the app
                    this.backgroundRemovedColorStream.BackgroundRemovedFrameReady += this.BackgroundRemovedFrameReadyHandler;

                    // Add an event handler to be called whenever there is new depth frame data
                    args.NewSensor.AllFramesReady += this.SensorAllFramesReady;

                    try
                    {
                        args.NewSensor.DepthStream.Range = this.checkBoxNearMode.IsChecked.GetValueOrDefault()
                                                    ? DepthRange.Near
                                                    : DepthRange.Default;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
                    }
                    catch (InvalidOperationException)
                    {
                        // Non Kinect for Windows devices do not support Near mode, so reset back to default mode.
                        args.NewSensor.DepthStream.Range = DepthRange.Default;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }

                    ComputeUniformScale();
                    this.statusBarText.Text = Properties.Resources.ReadyForScreenshot;
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }
        }


        /// <summary>
        /// Handles the user clicking on the screenshot button
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void ButtonScreenshotClick(object sender, RoutedEventArgs e)
        {
            TakeScreenShot();
        }

        private void TakeScreenShot()
        {
            if (null == this.sensorChooser || null == this.sensorChooser.Kinect)
            {
                this.statusBarText.Text = Properties.Resources.ConnectDeviceFirst;
                return;
            }

            int colorWidth = (int)this.Backdrop.Width;
            int colorHeight = (int)this.Backdrop.Height;


            if (VacationImages[VacationIndex].Mean != null && VacationImages[VacationIndex].ColorCorrect == true)
            {
                ColorTransfer.Point3D mean = new ColorTransfer.Point3D();
                ColorTransfer.Point3D stdDev = new ColorTransfer.Point3D();
                ColorTransfer.ComputeDecorrelation(DecorrelatedRGBA, 4, ref DecorrelatedValues, out mean, out stdDev);
                ColorTransfer.TransferColor(mean, stdDev, VacationImages[VacationIndex].Mean, VacationImages[VacationIndex].StdDev, ref DecorrelatedRGBA, 4,
                    ref DecorrelatedValues);

                // Write the pixel data into our bitmap
                this.maskedColorBitmap.WritePixels(
                    new Int32Rect(0, 0, this.maskedColorBitmap.PixelWidth, this.maskedColorBitmap.PixelHeight),
                    DecorrelatedRGBA, //backgroundRemovedFrame.GetRawPixelData(),
                    this.maskedColorBitmap.PixelWidth * sizeof(int),
                    0);
            }

            // create a render target that we'll render our controls to
            var renderBitmap = new RenderTargetBitmap(colorWidth, colorHeight, 96.0, 96.0, PixelFormats.Pbgra32);

            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                // render the backdrop
                var backdropBrush = new VisualBrush(Backdrop);
                dc.DrawRectangle(backdropBrush, null, new Rect(new Point(), new Size(colorWidth, colorHeight)));
            }

            renderBitmap.Render(dv);

            // create a png bitmap encoder which knows how to save a .png file
            BitmapEncoder encoder = new PngBitmapEncoder();

            // create frame from the writable bitmap and add to encoder
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            var time = DateTime.Now.ToString("MM-dd-yy_hhmmss", CultureInfo.CurrentUICulture.DateTimeFormat);

            var myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            var path = Path.Combine(myPhotos, "KinectSnapshot-" + time + ".png");

            // write the new file to disk
            try
            {
                using (var fs = new FileStream(path, FileMode.Create))
                {
                    encoder.Save(fs);
                }

                this.statusBarText.Text = string.Format(CultureInfo.InvariantCulture, Properties.Resources.ScreenshotWriteSuccess, path);
            }
            catch (IOException)
            {
                this.statusBarText.Text = string.Format(CultureInfo.InvariantCulture, Properties.Resources.ScreenshotWriteFailed, path);
            }
        }

        /// <summary>
        /// Handles the checking or unchecking of the near mode combo box
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void CheckBoxNearModeChanged(object sender, RoutedEventArgs e)
        {
            if (null == this.sensorChooser || null == this.sensorChooser.Kinect)
            {
                return;
            }

            // will not function on non-Kinect for Windows devices
            try
            {
                this.sensorChooser.Kinect.DepthStream.Range = this.checkBoxNearMode.IsChecked.GetValueOrDefault()
                                                    ? DepthRange.Near
                                                    : DepthRange.Default;
            }
            catch (InvalidOperationException)
            {
            }
        }

        float UserCalibrationTargetDepth = 2.5f;
        float TargetUserHeight = 1.8288f; // 6 feet
     
        private float ComputeUniformScale()
        {
            if (sensorChooser.Kinect == null)
                return 1.0f;

            float workingZ = UserCalibrationTargetDepth;
            SkeletonPoint pt = new SkeletonPoint();
            pt.X = 0.0f;
            pt.Y = TargetUserHeight * 0.5f;
            pt.Z = workingZ;
            DepthImagePoint headDepthPos = sensorChooser.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(pt, sensorChooser.Kinect.DepthStream.Format);

            pt.Y = -TargetUserHeight * 0.5f;
            DepthImagePoint feetDepthPos = sensorChooser.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(pt, sensorChooser.Kinect.DepthStream.Format);

            float screenHeightAtTargetDepth = Math.Abs(headDepthPos.Y - feetDepthPos.Y);
            Console.WriteLine("screenHeightAtTargetDepthSrc: " + screenHeightAtTargetDepth);

            float percentageOfImageSize = VacationImages[VacationIndex].UserHeightAtTargetDepth /
                (float)Backdrop.Source.Height;
            Console.WriteLine("screenHeightAtTargetDepthDest: " + percentageOfImageSize * (float)sensorChooser.Kinect.DepthStream.FrameHeight);
            float userUniformScale = screenHeightAtTargetDepth / (percentageOfImageSize * (float)sensorChooser.Kinect.DepthStream.FrameHeight);
            Console.WriteLine("userUniformScale: " + userUniformScale);
            return userUniformScale;
         
        }

        public class Vector2
        {
            public Vector2(float X, float Y)
            {
                x = X;
                y = Y;
            }

            public float x;
            public float y;
        };

        public Vector2 ComputeResizedTargetDepthFloorPixel(Vector2 targetDims, Vector2 sourceDims, bool invert)
        {
            if (invert)
            {
                return new Vector2(( VacationImages[VacationIndex].TargetDepthFloorPixelX / sourceDims.x) * targetDims.x,
                     ((sourceDims.y - VacationImages[VacationIndex].TargetDepthFloorPixelY) / sourceDims.y) * targetDims.y);
            }
            else
            {
                return new Vector2((VacationImages[VacationIndex].TargetDepthFloorPixelX / sourceDims.x) * targetDims.x,
                     ((VacationImages[VacationIndex].TargetDepthFloorPixelY) / sourceDims.y) * targetDims.y);
            }
        }

        private SkeletonPoint ClosestPointOnFloorPlane(SkeletonPoint pt)
        {
            SkeletonPoint normal = new SkeletonPoint();
            normal.X = lastFloorPlane.Item1;
            normal.Y = lastFloorPlane.Item2;
            normal.Z = lastFloorPlane.Item3;
            float p = lastFloorPlane.Item4;
            float distance = pt.X * normal.X + pt.Y * normal.Y + pt.Z * normal.Z + p;

            SkeletonPoint result = new SkeletonPoint();
            result.X = pt.X - normal.X * distance;
            result.Y = pt.Y - normal.Y * distance;
            result.Z = pt.Z - normal.Z * distance;

            return result;
        }

        private void ComputeUserUVTranslation()
        {
            if (sensorChooser.Kinect == null)
            {
                return;
            }

            int depthMapHeight = sensorChooser.Kinect.DepthStream.FrameHeight;

            float workingZ = UserCalibrationTargetDepth;
            SkeletonPoint pt = new SkeletonPoint();
            pt.X = 0.0f;
            pt.Y = TargetUserHeight * 0.5f;
            pt.Z = workingZ;
            DepthImagePoint headDepthPos = sensorChooser.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(pt, sensorChooser.Kinect.DepthStream.Format);
            int workingDepth = headDepthPos.Depth;

            Vector2 sourceDims = new Vector2((float)Backdrop.Source.Width,
                (float)Backdrop.Source.Height);
            Vector2 targetDims = new Vector2((float)sensorChooser.Kinect.DepthStream.FrameWidth,
                (float)sensorChooser.Kinect.DepthStream.FrameHeight);

            Vector2 resizedTargetFloorPixel = ComputeResizedTargetDepthFloorPixel(
                targetDims, sourceDims, false);
            
            SkeletonPoint targetUserPos = new SkeletonPoint();
            targetUserPos.X = 0.0f;
            targetUserPos.Y = 0.0f;
            targetUserPos.Z = UserCalibrationTargetDepth;

            SkeletonPoint closestPtTargetUserDepth = ClosestPointOnFloorPlane(targetUserPos);
            DepthImagePoint screenPtTargetUserDepth = sensorChooser.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(closestPtTargetUserDepth, sensorChooser.Kinect.DepthStream.Format);
            if (screenPtTargetUserDepth.Y > depthMapHeight)
                screenPtTargetUserDepth.Y = depthMapHeight;
            Console.WriteLine("screenPtTargetUserDepth: " + screenPtTargetUserDepth.X + ", " + screenPtTargetUserDepth.Y);
            Console.WriteLine("resizedTargetFloorPixel: " + resizedTargetFloorPixel.y);

            Vector2 userUVTranslate = new Vector2(0.0f,
                                          -(resizedTargetFloorPixel.y - screenPtTargetUserDepth.Y) / ((float)depthMapHeight));
            Console.WriteLine("******* userUVTranslate: " + userUVTranslate.y);

            this.actorXOffsetSlider.Value = 0.0f;
            this.actorYOffsetSlider.Value = userUVTranslate.y;
        }

        private bool SetBackground(int index)
        {
            VacationIndex = index;

            BitmapImage src = new BitmapImage();
            src.BeginInit();
            src.UriSource = new Uri(VacationImages[VacationIndex].BackgroundFilename, UriKind.Relative);
            src.CacheOption = BitmapCacheOption.OnLoad;
            src.EndInit();
            Backdrop.Source = src;
            Backdrop.Width = src.Width;
            Backdrop.Height = src.Height;
            Backdrop.MinWidth = src.Width;
            Backdrop.MinHeight = src.Height;
            Backdrop.InvalidateMeasure();

            if (VacationImages[VacationIndex].DecorrelatedValues == null)
            {
                int numPoints = src.PixelHeight * src.PixelWidth;
                int stride = src.PixelWidth * 4;
                int size = src.PixelHeight * stride;
                byte[] pixels = new byte[size];
                src.CopyPixels(pixels, stride, 0);

                VacationImage image = VacationImages[VacationIndex];
                image.DecorrelatedValues = new ColorTransfer.Point3D[numPoints];
                for (int i = 0; i < numPoints; i++)
                    image.DecorrelatedValues[i] = new ColorTransfer.Point3D();
                ColorTransfer.ComputeDecorrelation(pixels, 4, ref image.DecorrelatedValues, out image.Mean, out image.StdDev);

                VacationImages[VacationIndex] = image;
            }

            SetBackgroundDepthImage(VacationImages[VacationIndex].DepthMaskFilename);

            float userScale = ComputeUniformScale();
            ComputeUserUVTranslation();
            return true;
        }


        private bool SetBackgroundDepthImage(string filename)
        {
            BitmapImage src = new BitmapImage();
            src.BeginInit();
            src.UriSource = new Uri(filename, UriKind.Relative);
            src.CacheOption = BitmapCacheOption.OnLoad;
            src.EndInit();

            ImageBrush backgroundDepthMap = (ImageBrush)this.Resources["backgroundDepthMap"];
            backgroundDepthMap.ImageSource = src;
            return true;
        }


        private void PreviousImage()
        {
            --VacationIndex;
            if (VacationIndex < 0)
                VacationIndex = VacationImages.Count - 1;

            SetBackground(VacationIndex);
        }

        private void NextImage()
        {
            ++VacationIndex;
            if (VacationIndex >= VacationImages.Count)
                VacationIndex = 0;
            SetBackground(VacationIndex);
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                PreviousImage();
            }
            else if (e.Key == Key.Right)
            {
                NextImage();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetBackground(0);
            ImageBrush liveDepthMap = (ImageBrush)this.Resources["liveDepthMap"];
            liveDepthMap.ImageSource = depthColorBitmap;

            this.KeyUp += new KeyEventHandler(OnKeyUp);

            // Create a SpeechRecognitionEngine object for the default recognizer in the en-US locale.
            recognizer = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("en-US"));
            // Create a grammar for finding services in different cities.
            Choices services = new Choices(new string[] { "next", "previous", "click" });

            GrammarBuilder findServices = new GrammarBuilder();
            findServices.Append(services);

            // Create a Grammar object from the GrammarBuilder and load it to the recognizer.
            Grammar servicesGrammar = new Grammar(findServices);
            recognizer.LoadGrammarAsync(servicesGrammar);

            // Add a handler for the speech recognized event.
            recognizer.SpeechRecognized +=
              new EventHandler<SpeechRecognizedEventArgs>(recognizer_SpeechRecognized);

            // Configure the input to the speech recognizer.
            recognizer.SetInputToDefaultAudioDevice();

            // Start asynchronous, continuous speech recognition.
            recognizer.RecognizeAsync(RecognizeMode.Multiple);
        }

        // Handle the SpeechRecognized event.
        void recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            Console.WriteLine("Recognized text: " + e.Result.Text + " Confidence: " + e.Result.Confidence);

            if (e.Result.Text == "next" && e.Result.Confidence >= 0.7f)
                NextImage();
            else if (e.Result.Text == "previous" && e.Result.Confidence >= 0.7f)
                PreviousImage();
            else if (e.Result.Text == "click" && e.Result.Confidence >= 0.7f)
                TakeScreenShot();
        }
    }
}