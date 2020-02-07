using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ComputerGraphics3D.Utilities;
namespace ComputerGraphics3D
{
    public class Matrix
    {
        public double[,] Values { get; set; }
        public Matrix()
        {
            Values = new double[4, 4];
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    Values[i, j] = 0.0;
        }

    }

    public class Vector
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double W { get; set; } = 1.0;

        public Vector()
        {
            X = 0.0;
            Y = 0.0;
            Z = 0.0;
        }
        public Vector(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
            W = 1.0;
        }
        public Vector(double x, double y, double z, double w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public Vector VectorAdd(Vector v2)
        {
            return new Vector(X + v2.X, Y + v2.Y, Z + v2.Z);
        }
        public Vector VectorSub(Vector v2)
        {
            return new Vector(X - v2.X, Y - v2.Y, Z - v2.Z);
        }
        public Vector VectorMul(double val)
        {
            return new Vector(X *val, Y *val, Z *val);
        }
        public Vector VectorDiv(double val)
        {
            return new Vector(X / val, Y / val, Z / val);
        }

        public double DotProduct(Vector v2)
        {
            return X*v2.X+Y*v2.Y+Z*v2.Z;
        }
        public double Len()
        {
            return Math.Sqrt(DotProduct(this));
        }

        public void Normalize()
        {
            double l = this.Len();
            X /= l;
            Y /= l;
            Z /= l;
        }

        public Vector CrossProduct(Vector v2)
        {
            return new Vector(
                Y*v2.Z - Z*v2.Y,
                Z*v2.X - X*v2.Z,
                X*v2.Y - Y*v2.X);
        }

        public static Vector Reflect(Vector incoming, Vector normal)
        {

            Vector result = normal.VectorMul(2.0 * incoming.DotProduct(normal));
            return incoming.VectorSub(result);
             
        }
    }

    public class ViewMatrix : Matrix
    {
        public ViewMatrix(Vector pos, Vector target, Vector up) : base()
        {
            Vector D = target.VectorSub(pos);
            D.Normalize();

            Vector U = D.VectorMul(up.DotProduct(D));
            U = up.VectorSub(U);
            U.Normalize();

            Vector R = U.CrossProduct(D);

            Values[0, 0] = R.X; Values[0, 1] = R.Y; Values[0, 2] = R.Z; Values[0, 3] = -R.DotProduct(target);
            Values[1, 0] = U.X; Values[1, 1] = U.Y; Values[1, 2] = U.Z; Values[1, 3] = -U.DotProduct(target);
            Values[2, 0] = D.X; Values[2, 1] = D.Y; Values[2, 2] = D.Z; Values[2, 3] = -D.DotProduct(target);
            Values[3, 3] = 1;

        }

        public void Inverse()
        {

        }
    }

    public class ProjectionMatrix : Matrix
    {
        public ProjectionMatrix(double aspect, double fieldOfView, double fFar, double fNear): base()
        {
            double ctgOfFov = 1 / Math.Tan(fieldOfView / 2);
            Values[0, 0] = ctgOfFov/aspect;
            Values[1, 1] = ctgOfFov;
            Values[2, 2] = (fFar+fNear) / (fFar - fNear);
            Values[3, 2] = 1.0;
            Values[2, 3] = (-2*fFar * fNear) / (fFar - fNear);
        }
    }

    public class TranslationMatrix : Matrix
    {
        public TranslationMatrix(double x,double y,double z): base()
        {
            Values[0, 3] = x;
            Values[1, 3] = y;
            Values[2, 3] = z;
            for (int i = 0; i < 4; i++)
                Values[i, i] = 1.0;

        }


    }

    public class ScalingMatrix : Matrix
    {
        public ScalingMatrix(double x, double y, double z) : base()
        {
            Values[0, 0] = x;
            Values[1, 1] = y;
            Values[2, 2] = z;
            Values[3, 3] = 1.0;

        }
    }

    public class XRotationMatrix : Matrix
    {
        public XRotationMatrix(double alpha) : base()
        {
            Values[0, 0] = 1.0;
            Values[1, 1] = Math.Cos(alpha*0.5);
            Values[1, 2] = -Math.Sin(alpha * 0.5);
            Values[2, 1] = Math.Sin(alpha * 0.5);
            Values[2, 2] = Math.Cos(alpha * 0.5);
            Values[3, 3] = 1.0;
        }
    }

    public class YRotationMatrix : Matrix
    {
        public YRotationMatrix(double alpha) : base()
        {
            Values[0, 0] = Math.Cos(alpha);
            Values[1, 1] = 1.0;
            Values[0, 2] = -Math.Sin(alpha);
            Values[2, 0] = Math.Sin(alpha);
            Values[2, 2] = Math.Cos(alpha);
            Values[3, 3] = 1.0;
        }
    }

    public class ZRotationMatrix : Matrix
    {
        public ZRotationMatrix(double alpha) : base()
        {
            Values[0, 0] = Math.Cos(alpha);
            Values[1, 1] = Math.Cos(alpha);
            Values[0, 1] = -Math.Sin(alpha);
            Values[1, 0] = Math.Sin(alpha);
            Values[2, 2] = 1.0;
            Values[3, 3] = 1.0;
        }
    }

    public class PerspectiveMatrix : Matrix
    {
        public PerspectiveMatrix(int H, int W) : base()
        {
            Values[0, 0] = (double)H/(double)W;
            Values[1, 1] = 1.0;
            Values[2, 3] = -3.0;
            Values[3, 2] = 1.0;
            Values[2, 2] = 2.0;
        }
    }
}
