using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;
using System.Diagnostics;

namespace KinectScanner.OpenGL
{
    class OpenGLViewPort
    {
        public GLControl glc;
        float angle;
        float verticalAngle;
        Vector3[] points;
        Vector3[] colors;
        Vector3 averageDistance;
        float distance = 2;
        //Random
        Random rnd = new Random();

        public OpenGLViewPort(GLControl glc)
        {
            this.glc = glc;
            points = new Vector3[1];
        }

        public Vector3[] getPoints()
        {
            return points;
        }

        public Vector3[] getColors()
        {
            return colors;
        }

        public void load()
        {
            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            SetupViewport();
        }

        public void updatePointCloud(int[] d)
        {
            points = new Vector3[d.Length];
            averageDistance = new Vector3(0,0,0);
            int k = 0;
            float cx = 320.0f;
            float cy = 240.0f;
            float fx = 600.0f;
            float fy = 600.0f;
            for (int y = 0; y < 480; y++)
            {
                for (int x = 0; x < 640; x++)
                {
                    points[k] = new Vector3(0.001f * (-x + cx) * d[k] / fx, 0.001f * (-y + cy) * d[k] / fy, 0.001f * d[k] + distance);
                    averageDistance.X += points[k].X;
                    averageDistance.Y += points[k].Y;
                    averageDistance.Z += points[k].Z;
                    k++;
                }    
            }
            averageDistance.X = averageDistance.X / d.Length;
            averageDistance.Y = averageDistance.Y / d.Length;
            averageDistance.Z = averageDistance.Z / d.Length;
            glc.Invalidate();
        }

        public void updateColor(byte[] bgra)
        {
            int max = bgra.Length/4;
            colors = new Vector3[max];
            for (int i = 0, j=0; i < max && j < bgra.Length; i++, j+=4)
            {
                float x = (float)Convert.ToDouble(bgra[j].ToString())/256;
                float y = (float)Convert.ToDouble(bgra[j + 1].ToString()) / 256;
                float z = (float)Convert.ToDouble(bgra[j + 2].ToString()) / 256;
                //Console.WriteLine(bgra[i]);

                colors[i] = new Vector3(z, y, x);
            }
        }

        private void SetupViewport()
        {
            int w = glc.Width;
            int h = glc.Height;
            GL.Viewport(0, 0, w, h);

            OpenTK.Matrix4 projection = OpenTK.Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, glc.AspectRatio, 1.0f, 64.0f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
        }

        

        public void paint()
        {

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            OpenTK.Matrix4 modelview = OpenTK.Matrix4.LookAt(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);
            GL.Translate(averageDistance.X, averageDistance.Y, averageDistance.Z);
            GL.Rotate(angle, Vector3.UnitY);
            GL.Rotate(verticalAngle, Vector3.UnitX);
            GL.Translate(-averageDistance.X, -averageDistance.Y, -averageDistance.Z);

            
            float[] quadratic = { 1.0f, 0.0f, 0.01f };
            GL.PointSize(points.Length);
            GL.PointParameter(PointParameterName.PointFadeThresholdSize, 60.0f);
            GL.PointParameter(PointParameterName.PointSizeMin, 1.0f);
            GL.PointParameter(PointParameterName.PointSizeMax, 3.0f);
            GL.PointParameter(PointParameterName.PointDistanceAttenuation, quadratic);
            GL.Begin(BeginMode.Points);
            {
                int d = 10;
                for (int i = 0; i < points.Length/d; i ++)
                {
                    if (points[2 * i] != null)
                    {
                        GL.Color3(colors[d * i].X, colors[d * i].Y, colors[d * i].Z);
                        GL.Vertex3(points[d * i].X, points[d * i].Y, points[d * i].Z);
                    }
                }
            }
            GL.End();
            //GL.Begin(BeginMode.Triangles);

           // GL.Color3(1.0f, 1.0f, 0.0f); GL.Vertex3(-1.0f, -1.0f, 0);
           // GL.Color3(1.0f, 0.0f, 0.0f); GL.Vertex3(1.0f, -1.0f, 0);
           // GL.Color3(0.2f, 0.9f, 1.0f); GL.Vertex3(0.0f, 1.0f, 0);

           // GL.End();

            glc.SwapBuffers();
        }


        public void resize()
        {
            SetupViewport();
            glc.Invalidate();
        }

        public void keyDown(System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.D)
            {
                angle += 5;
                glc.Invalidate();
            }
            else if (e.KeyCode == Keys.A)
            {
                angle -= 5;
                glc.Invalidate();
            }
            else if (e.KeyCode == Keys.W)
            {
                verticalAngle -= 5;
                glc.Invalidate();
            }
            else if (e.KeyCode == Keys.S)
            {
                verticalAngle += 5;
                glc.Invalidate();
            }
        }

       
    }
}
