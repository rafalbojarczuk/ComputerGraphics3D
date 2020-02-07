using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using System.Numerics;
using static ComputerGraphics3D.Utilities;
namespace ComputerGraphics3D
{
    public class Drawer
    {
        public DirectBitmap DirectBitmap { get; set; }
        public double[,] DepthBuffer;

        public bool ZBufferOn { get; set; } = false;

        Random random = new Random();
        const int COL_MIN = 70;
        const int COL_MAX = 185;
        const int ERROR_POINT = 9999;

        public Drawer(PictureBox p)
        {
            DirectBitmap = new DirectBitmap(p.Width, p.Height);
            DepthBuffer = new double[p.Width, p.Height];
        }

        internal void FillTriangle(Triangle t, Color c)
        {

            List<Point> tri = new List<Point>() {
                new Point(Convert.ToInt32(t.Ver[0].X), Convert.ToInt32(t.Ver[0].Y)),
                new Point(Convert.ToInt32(t.Ver[1].X), Convert.ToInt32(t.Ver[1].Y)),
                new Point(Convert.ToInt32(t.Ver[2].X), Convert.ToInt32(t.Ver[2].Y))
            };
            tri.Sort((p1, p2) => (p1.Y - p2.Y));
            Vector planeCoefficients = CalculatePlaneCoefficients(t.Ver[0], t.Ver[1], t.Ver[2]);

            int dx1 = tri[1].X - tri[0].X;
            int dx2 = tri[2].X - tri[0].X;
            int dy1 = tri[1].Y - tri[0].Y;
            int dy2 = tri[2].Y - tri[0].Y;

            double daxStep = 0.0;
            double dbxStep = 0.0;


            if (dy1 != 0) daxStep = dx1 / (double)Math.Abs(dy1);
            if (dy2 != 0) dbxStep = dx2 / (double)Math.Abs(dy2);

            if(dy1 != 0)
            {
                for(int i = tri[0].Y; i<= tri[1].Y; i++)
                {
                    int ax = Convert.ToInt32(tri[0].X + (i - tri[0].Y) * daxStep);
                    int bx = Convert.ToInt32(tri[0].X + (i - tri[0].Y) * dbxStep);
                    if(ax > bx)
                    {
                        int tmp = ax;
                        ax = bx;
                        bx = tmp;
                    }

                    FillLine(ax, bx, i, c, planeCoefficients);
                }
            }

            dy1 = tri[2].Y - tri[1].Y;
            dx1 = tri[2].X - tri[1].X;

            if (dy1 != 0) daxStep = dx1 / (double)Math.Abs(dy1);
            if (dy2 != 0) dbxStep = dx2 / (double)Math.Abs(dy2);

            if (dy1 != 0)
            {
                for (int i = tri[1].Y; i <= tri[2].Y; i++)
                {
                    int ax = Convert.ToInt32(tri[1].X + (i - tri[1].Y) * daxStep);
                    int bx = Convert.ToInt32(tri[0].X + (i - tri[0].Y) * dbxStep);
                    if (ax > bx)
                    {
                        int tmp = ax;
                        ax = bx;
                        bx = tmp;
                    }

                    FillLine(ax, bx, i, c, planeCoefficients);
                }
            }


        }


        private void FillLine(int xStart, int xEnd, int y, Color c, Vector planeCoefficients)
        {
            for (int i = xStart; i <= xEnd; i++)
            {
                if (ZBufferOn == true)
                {
                    double Z = DepthValue(planeCoefficients, i, y);
                    if (Z < DepthBuffer[i, y])
                    {
                        DepthBuffer[i, y] = Z;
                    }
                }
                else
                {
                    DirectBitmap.SetPixel(i, y, c);
                }
            }
        }



 

        internal void DrawTriangle(Triangle t)
        {
            Point[] s = new Point[3];
            List<Vertex> vertices = new List<Vertex>();
            for (int i = 0; i < 3; i++)
            {
                s[i] = new Point(Convert.ToInt32(t.Ver[i].X), Convert.ToInt32(t.Ver[i].Y));
                vertices.Add(new Vertex(s[i], t.Col));
            }
            Polygon p = new Polygon(vertices);
            DrawPolygon(p);
        }





        public void DrawEdge(Edge e)
        {
            Point A = e.V1.Coords;
            Point B = e.V2.Coords;
            int w = B.X - A.X;
            int h = B.Y - A.Y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            int numerator = longest >> 1;
            int x = A.X;
            int y = A.Y;
            for (int i = 0; i <= longest; i++)
            {
                DirectBitmap.SetPixel(x, y, e.V1.Color);
                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    x += dx1;
                    y += dy1;
                }
                else
                {
                    x += dx2;
                    y += dy2;
                }
            }

        }



        public void DrawPolygon(Polygon p)
        {
            int n = p.Vertices.Count();
            for (int i = 0; i < n; i++)
            {
                DrawEdge(new Edge(p.Vertices[i], p.Vertices[(i + 1) % n]));
            }

        }



    }

 
}

