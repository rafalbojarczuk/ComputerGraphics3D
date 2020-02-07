using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerGraphics3D
{
    public static class Utilities
    {
        public static Vector MultMatrixVector(Matrix M, Vector V)
        {
            double W = V.X * M.Values[3, 0] + V.Y * M.Values[3, 1] + V.Z * M.Values[3, 2] + V.W * M.Values[3, 3];
            if (W != 0.0)
                return new Vector(
                    V.X * M.Values[0, 0] + V.Y * M.Values[0, 1] + V.Z * M.Values[0, 2] + V.W * M.Values[0, 3],
                    V.X * M.Values[1, 0] + V.Y * M.Values[1, 1] + V.Z * M.Values[1, 2] + V.W * M.Values[1, 3],
                    V.X * M.Values[2, 0] + V.Y * M.Values[2, 1] + V.Z * M.Values[2, 2] + V.W * M.Values[2, 3],
                    W
                    //V.X * M.Values[3, 0] + V.Y * M.Values[3, 1] + V.Z * M.Values[3, 2] + M.Values[3, 3]
                );
            else
                return new Vector(
                    V.X * M.Values[0, 0] + V.Y * M.Values[0, 1] + V.Z * M.Values[0, 2] + V.W * M.Values[0, 3],
                    V.X * M.Values[1, 0] + V.Y * M.Values[1, 1] + V.Z * M.Values[1, 2] + V.W * M.Values[1, 3],
                    V.X * M.Values[2, 0] + V.Y * M.Values[2, 1] + V.Z * M.Values[2, 2] + V.W * M.Values[2, 3],
                    1.0
                //V.X * M.Values[3, 0] + V.Y * M.Values[3, 1] + V.Z * M.Values[3, 2] + M.Values[3, 3]
                );

        }

        public static Matrix MultMatrices(Matrix A, Matrix B)
        {
            Matrix C = new Matrix();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        C.Values[i, j] += A.Values[i, k] * B.Values[k, j];
                    }
                }
            }

            return C;
        }

        public static void ConvertToVisibleSpace(Triangle t, int W, int H)
        {
            t.Ver[0] = t.Ver[0].VectorDiv(t.Ver[0].W);
            t.Ver[1] = t.Ver[1].VectorDiv(t.Ver[1].W);
            t.Ver[2] = t.Ver[2].VectorDiv(t.Ver[2].W);
            Vector offset = new Vector(1.0, 1.0, 0.0);
            t.Ver[0] = t.Ver[0].VectorAdd(offset);
            t.Ver[1] = t.Ver[1].VectorAdd(offset);
            t.Ver[2] = t.Ver[2].VectorAdd(offset);

            t.Ver[0].X *= 0.5 * W;
            t.Ver[0].Y *= 0.5 * H;
            t.Ver[1].X *= 0.5 * W;
            t.Ver[1].Y *= 0.5 * H;
            t.Ver[2].X *= 0.5 * W;
            t.Ver[2].Y *= 0.5 * H;
        }

        public static void ConvertToVisibleSpace(Vector v, int W, int H)
        {
            v = v.VectorDiv(v.W);

            Vector offset = new Vector(1.0, 1.0, 0.0);
            v = v.VectorAdd(offset);


            v.X *= 0.5 * W;
            v.Y *= 0.5 * H;

        }

        public static Vector VectorIntersectsPlane(Vector planePoint, Vector planeNormal, Vector lineStart, Vector lineEnd)
        {
            planeNormal.Normalize();
            double D = -planeNormal.DotProduct(planePoint);
            double AD = lineStart.DotProduct(planeNormal);
            double BD = lineEnd.DotProduct(planeNormal);
            double t = (-D - AD) / (BD - AD);
            Vector lineStartToEnd = lineEnd.VectorSub(lineStart);
            Vector lineToIntersect = lineStartToEnd.VectorMul(t);
            return lineStart.VectorAdd(lineToIntersect);
        }

        public static int ClipAgainstPlane(Vector planePoint, Vector planeNormal, Triangle tri, out Triangle triOut1, out Triangle triOut2)
        {
            planeNormal.Normalize();
            triOut1 = null;
            triOut2 = null;
            Vector[] insidePoints = new Vector[3];
            Vector[] outsidePoints = new Vector[3];
            int insidePointCount = 0;
            int outsidePointCount = 0;

            double d0 = SignedShortestDistance(planePoint, planeNormal, tri.Ver[0]);
            double d1 = SignedShortestDistance(planePoint, planeNormal, tri.Ver[1]);
            double d2 = SignedShortestDistance(planePoint, planeNormal, tri.Ver[2]);

            if(d0 >= 0.0)
                insidePoints[insidePointCount++] = tri.Ver[0];
            else
                outsidePoints[outsidePointCount++] = tri.Ver[0];
            if (d1 >= 0.0)
                insidePoints[insidePointCount++] = tri.Ver[1];
            else
                outsidePoints[outsidePointCount++] = tri.Ver[1]; 
            if (d2 >= 0.0)
                insidePoints[insidePointCount++] = tri.Ver[2];
            else
                outsidePoints[outsidePointCount++] = tri.Ver[2];

            if (insidePointCount == 0)
                return 0;
            else if(insidePointCount == 1)
            {
                triOut1 = new Triangle(insidePoints[0]
                    , VectorIntersectsPlane(planePoint, planeNormal, insidePoints[0], outsidePoints[0])
                    , VectorIntersectsPlane(planePoint, planeNormal, insidePoints[0], outsidePoints[1]));

                return 1;
            }
            else if(insidePointCount == 2)
            {
                triOut1 = new Triangle(insidePoints[0], insidePoints[1]
                    , VectorIntersectsPlane(planePoint, planeNormal, insidePoints[0], outsidePoints[0]));

                triOut2 = new Triangle(insidePoints[1], triOut1.Ver[2]
                    , VectorIntersectsPlane(planePoint, planeNormal, insidePoints[1], outsidePoints[0]));
                return 2;
            }
            else
            {
                triOut1 = tri;
                return 1;
            }
        }

        public static double SignedShortestDistance(Vector planePoint, Vector planeNormal, Vector point)
        {
            return planeNormal.DotProduct(point) - planeNormal.DotProduct(planePoint);
        }

        public static Vector CalculatePlaneCoefficients(Vector p1, Vector p2, Vector p3)
        {
            double A = (p2.Z - p3.Z) * (p1.Y - p2.Y) - (p1.Z - p2.Z) * (p2.Y - p3.Y);
            double B = (p2.X - p3.X) * (p1.Z - p2.Z) - (p1.X - p2.X) * (p2.Z - p3.Z);
            double C = (p2.Y - p3.Y) * (p1.X - p2.X) - (p1.Y - p2.Y) * (p2.X - p3.X);
            double D = -p1.X * (p2.Y*p3.Z-p2.Z*p3.Y) + p1.Y * (p2.X * p3.Z - p2.Z * p3.X) - p1.Z * (p2.X * p3.Y - p2.Y * p3.X);
            return new Vector(A, B, C, D);
        }

        public static double DepthValue(Vector planeCoefficients, int x, int y)
        {
            double z = 1000.0;
            if (Math.Abs(planeCoefficients.Y) < 0.001)
                z = (-planeCoefficients.X * x - planeCoefficients.Z * y - planeCoefficients.W) / planeCoefficients.Y;
            return z;
        }


    }
}
