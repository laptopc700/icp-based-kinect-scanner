using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;
using KinectScanner.OpenGL;

namespace KinectScanner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private OpenGLViewPort viewPort;
        private bool getPointCloud;
        Kinnect.KinnectOutput kinectoutput;

        public MainWindow()
        {
            InitializeComponent();
        }
        public void updatepointCloud(int[] d)
        {
            viewPort.updatePointCloud(d);
            getPointCloud = false;
        }
        public void updateColourPoints(byte[] bgrMap)
        {
            viewPort.updateColor(bgrMap);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            

           
           
        }

        void glc_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            viewPort.keyDown(e);
        }

        void glc_Resize(object sender, EventArgs e)
        {
            viewPort.resize();
        }

        void glc_Paint(object sender, PaintEventArgs e)
        {
            viewPort.paint();
        }

        void glc_Load(object sender, EventArgs e)
        {
            viewPort.load();
        }

       

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            kinectoutput.stopSensor();
        }

        private void OpenGLViewPort_Loaded(object sender, RoutedEventArgs e)
        {
            // Create the GLControl.
            GLControl glc = new GLControl();
            // Assign the GLControl as the host control's child.
            OpenGLViewPort.Child = glc;
            viewPort = new OpenGL.OpenGLViewPort(glc);
            kinectoutput = new Kinnect.KinnectOutput(this);

            viewPort.glc.Load += new EventHandler(glc_Load);
            viewPort.glc.Paint += new PaintEventHandler(glc_Paint);
            viewPort.glc.Resize += new EventHandler(glc_Resize);
            viewPort.glc.KeyDown += new System.Windows.Forms.KeyEventHandler(glc_KeyDown);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            getPointCloud = true;
        }

        public bool newPointCloud()
        {
            
            return getPointCloud;
           
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void button2_Click_1(object sender, RoutedEventArgs e)
        {
            Tools.PLYExporter.convertVertexToPLY(viewPort.getPoints(), viewPort.getColors());
        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            Tools.PLYExporter.filename = textBox1.Text;
            Tools.PLYExporter.index = 0;
        }

    }
}
