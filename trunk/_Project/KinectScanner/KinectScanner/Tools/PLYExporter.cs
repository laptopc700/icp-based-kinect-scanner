using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using System.IO;

namespace KinectScanner.Tools
{
    class PLYExporter
    {
        public static string filename;
        public static int index;
         public PLYExporter()
        {

        }
         public static void convertVertexToPLY(Vector3[] points, Vector3[] colors)
         {
             StreamWriter writer = new StreamWriter(filename+index+".ply");
             Console.WriteLine(filename + index + ".ply");
             writer.WriteLine("ply");
             writer.WriteLine("format ascii 1.0");

             writer.WriteLine("element vertex " + points.Length);
             writer.WriteLine("property float x");
             writer.WriteLine("property float y");
             writer.WriteLine("property float z");
             writer.WriteLine("property uchar red");
             writer.WriteLine("property uchar green");
             writer.WriteLine("property uchar blue");
             writer.WriteLine("end_header");
             for (int i = 0; i < points.Length; i++)
             {
                uint x = Clamp((uint)(colors[i].X * 256), 0, 256);
                uint y = Clamp((uint)(colors[i].Y * 256), 0, 256);
                uint z = Clamp((uint)(colors[i].Z * 256), 0, 256);
                writer.WriteLine(points[i].X + " " + points[i].Y + " " + points[i].Z + " " + x + " " + y + " " + z);
             }
             writer.Close();
             index++;
         }
         public static uint Clamp(uint val, uint min, uint max)
         {
             if (val < min) return min;
             else if (val > max) return max;
             else return val;
         }
    }
}
