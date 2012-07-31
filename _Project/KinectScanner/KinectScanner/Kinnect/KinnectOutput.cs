using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using OpenTK;


namespace KinectScanner.Kinnect
{
    class KinnectOutput
    {
        public MainWindow window;
        KinectSensor sensor;
         int[] depthArray;
         BitmapSource colorImage;
        public KinnectOutput(MainWindow w)
        {
            startSensor();
            window = w;
        }

        public void startSensor()
        {
            //check sensor
            if (KinectSensor.KinectSensors.Count > 0)
            {
                sensor = KinectSensor.KinectSensors[0];

                if (sensor.Status == KinectStatus.Connected)
                {
                    sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                    sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    sensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(sensor_AllFramesReady);
                    sensor.Start();
                }
            }
        }

        public void stopSensor()
        {
            if (sensor != null)
            {
                sensor.Stop();
            }
        }


        void sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame == null)
                    return;
                byte[] pixels = new byte[colorFrame.PixelDataLength];
                colorFrame.CopyPixelDataTo(pixels);
                int stride = colorFrame.Width * 4;
                colorImage = BitmapSource.Create(colorFrame.Width, colorFrame.Height, 96, 96, PixelFormats.Bgr32, null, pixels, stride);
                window.ColourImage.Source = colorImage;

            }
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame == null)
                    return;
                byte[] pixels = generateColouredBites(depthFrame);
                int stride = depthFrame.Width * 4;
               BitmapSource depthImage = BitmapSource.Create(depthFrame.Width, depthFrame.Height, 96, 96, PixelFormats.Bgr32, null, pixels, stride);
               window.DepthImage.Source = depthImage;
            }

            if (window.newPointCloud())
            {
                window.updatepointCloud(depthArray);
               window.updateColourPoints(alignBRGwithDepthData(e));
            }
        }

        byte[] generateColouredBites(DepthImageFrame d)
        {
            short[] rawData = new short[d.PixelDataLength];
            d.CopyPixelDataTo(rawData);
            byte[] pixels = new byte[d.Height*d.Width*4];
            depthArray = new int[d.Height*d.Width];
            const int bIndex = 0;
            const int gIndex = 1;
            const int rIndex = 2;

            for (int depthIndex = 0, colorIndex = 0; depthIndex < rawData.Length && colorIndex < pixels.Length; depthIndex++, colorIndex += 4)
            {
                int depth = rawData[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;
                depthArray[depthIndex] = depth;
                int inensity = 255 - 255*(depth - 300) / 8000;
                pixels[colorIndex + bIndex] = (byte)inensity;
                pixels[colorIndex + gIndex] = (byte)inensity;
                pixels[colorIndex + rIndex] = (byte)inensity;
            }
            return pixels;
        }

        byte[] alignBRGwithDepthData(AllFramesReadyEventArgs e)
        {
            DepthImageFrame depthFrame = e.OpenDepthImageFrame();
            ColorImageFrame imageFrame = e.OpenColorImageFrame();

            short[] depthPixel = new short[depthFrame.PixelDataLength];
            byte[] colorPixel = new byte[imageFrame.PixelDataLength];
            ColorImagePoint[] coloredDepth = new ColorImagePoint[depthFrame.PixelDataLength];
            imageFrame.CopyPixelDataTo(colorPixel);
            depthFrame.CopyPixelDataTo(depthPixel);
            
            //bitmapsource
            int stride = colorImage.PixelWidth * 4;
            int size = colorImage.PixelHeight * stride;
            byte[] ColorPixels = new byte[size];
            colorImage.CopyPixels(ColorPixels, stride, 0);
            KinectSensor.KinectSensors[0].MapDepthFrameToColorFrame(depthFrame.Format, depthPixel, imageFrame.Format, coloredDepth);
            
            byte[] pixels = new byte[depthFrame.Height * depthFrame.Width * 4];
            Console.WriteLine(pixels.Length);
            Console.WriteLine(ColorPixels.Length);

            for (int depthIndex = 0, colorIndex = 0; depthIndex < depthFrame.PixelDataLength && colorIndex < pixels.Length; depthIndex++, colorIndex += 4)
            {
                if (coloredDepth[depthIndex].Y < 0)
                    coloredDepth[depthIndex].Y = 0;
                else if (coloredDepth[depthIndex].Y > colorImage.PixelHeight)
                    coloredDepth[depthIndex].Y = colorImage.PixelHeight;

                if (coloredDepth[depthIndex].X < 0)
                    coloredDepth[depthIndex].X = 0;
                else if (coloredDepth[depthIndex].X > colorImage.PixelWidth)
                    coloredDepth[depthIndex].X = colorImage.PixelWidth;

                int index = coloredDepth[depthIndex].Y * stride + 4 * coloredDepth[depthIndex].X;

                if (index > colorPixel.Length)
                {
                    continue;
                }

               byte blue = ColorPixels[index];
               byte green = ColorPixels[index + 1];
               byte red = ColorPixels[index + 2];
               pixels[colorIndex + 0] = blue;
               pixels[colorIndex + 1] = green;
               pixels[colorIndex + 2] = red;
              // pixels[i + 3] = 0;
            }
            //BitmapSource depthImage = BitmapSource.Create(depthFrame.Width, depthFrame.Height, 96, 96, PixelFormats.Bgr32, null, pixels, stride);
            //window.DepthImage.Source = depthImage;

            return pixels;
        }
    }
}
