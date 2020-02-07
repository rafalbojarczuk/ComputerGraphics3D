using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ComputerGraphics3D
{
    public class SceneObject
    {
        public string Name = "";

    }
    public class Camera : SceneObject
    {
        public Vector Position { get; set; }
        //public Vector Up { get; set; } = new Vector(0, 1, 0);
        public Vector LookDir { get; set; }
        //public Vector Forward { get; set; } = new Vector(0, 0, 0);

        public double FNear { get; set; } = 0.1;
        public double FFar { get; set; } = 1000.0;
        public double FoV { get; set; } = Math.PI / 2;
        public double FY { get; set; } = 0.0;

        public Camera()
        {
            Position = new Vector(0, 0, 0);
        }
        public Camera(Vector pos)
        {
            Name = "Camera";
            Position = pos;
        }
        public Camera(Vector pos, double fNear, double fFar, double fov)
        {
            Name = "Camera";
            Position = pos;
            FNear = fNear;
            FFar = fFar;
            FoV = fov;
        }

    }

    public class Light : SceneObject
    {
        public Vector Position { get; set; } = new Vector(0, 0, 0);
        public Vector Intensity { get; set; } = new Vector(1,1,1);

        public Light()
        {

        }
        public Light(Vector pos, Vector intensity)
        {
            Position = pos;
            Intensity = intensity;
        }

    }

    public class Shape3D : SceneObject
    {
        public List<Triangle> Mesh = new List<Triangle>();
        public Color Color;
        public Vector RotAngle { get; set; } = new Vector(0.0, 0.0, 0.0);
        public Vector Translation { get; set; } = new Vector(0.0, 0.0, 0.0);
        public Vector Scaling { get; set; } = new Vector(1.0, 1.0, 1.0);
        public double Ambient { get; set; } = 0.0;
        public double Diffuse { get; set; } = 1.0;
        public double Specular { get; set; } = 1.0;
        public double Shininess { get; set; } = 100.0;

        public Vector Lighting(Vector position, Light light, Vector eye, Vector normal)
        {
            Vector effectiveColor = new Vector(this.Color.R/255.0 * light.Intensity.X
                , this.Color.G / 255.0 * light.Intensity.Y
                , this.Color.B / 255.0 * light.Intensity.Z);
            Vector lightVec = light.Position.VectorSub(Translation);
            lightVec.Normalize();
            Vector ambientColor = effectiveColor.VectorMul(Ambient);
            Vector diffuseColor;
            Vector specularColor;
            double lDotN = lightVec.DotProduct(normal);
            if(lDotN < 0.0)
            {
                diffuseColor = new Vector(0, 0, 0);
                specularColor = new Vector(0, 0, 0);

            }
            else
            {
                diffuseColor = effectiveColor.VectorMul(Diffuse * lDotN);
                Vector reflect = Vector.Reflect(lightVec.VectorMul(-1.0), normal);
                double rDotE = reflect.DotProduct(eye);
                if(rDotE <= 0.0)
                {
                    specularColor = new Vector(0, 0, 0);
                }
                else
                {
                    double factor = Math.Pow(rDotE, Shininess);
                    specularColor = light.Intensity.VectorMul(Specular * factor);
                }

            }
            return ambientColor.VectorAdd(diffuseColor).VectorAdd(specularColor);

        }
    }

    public class Cuboid : Shape3D
    {
        public Cuboid()
        {
            Color = Color.LightBlue;
            Name = "Cuboid";
        }
        public Cuboid(double a = 1.0, double b = 1.0, double c = 1.0)
        {
            Name = "Cuboid";
            double starta = -a / 2;
            double enda = a / 2;
            double startb = -b / 2;
            double endb = b / 2; 
            double startc = -c / 2;
            double endc = c / 2;
            Mesh.Add(new Triangle(new Vector(starta, startb, startc), new Vector(starta, endb, startc), new Vector(enda, endb, startc)));
            Mesh.Add(new Triangle(new Vector(starta, startb, startc), new Vector(enda, endb, startc), new Vector(enda, startb, startc)));
            Mesh.Add(new Triangle(new Vector(enda, startb, startc), new Vector(enda, endb, startc), new Vector(enda, endb, endc)));
            Mesh.Add(new Triangle(new Vector(enda, startb, startc), new Vector(enda, endb, endc), new Vector(enda, startb, endc)));
            Mesh.Add(new Triangle(new Vector(enda, startb, endc), new Vector(enda, endb, endc), new Vector(starta, endb, endc)));
            Mesh.Add(new Triangle(new Vector(enda, startb, endc), new Vector(starta, endb, endc), new Vector(starta, startb, endc)));
            Mesh.Add(new Triangle(new Vector(starta, startb, endc), new Vector(starta, endb, endc), new Vector(starta, endb, startc)));
            Mesh.Add(new Triangle(new Vector(starta, startb, endc), new Vector(starta, endb, startc), new Vector(starta, startb, startc)));
            Mesh.Add(new Triangle(new Vector(starta, endb, startc), new Vector(starta, endb, endc), new Vector(enda, endb, endc)));
            Mesh.Add(new Triangle(new Vector(starta, endb, startc), new Vector(enda, endb, endc), new Vector(enda, endb, startc)));
            Mesh.Add(new Triangle(new Vector(enda, startb, endc), new Vector(starta, startb, endc), new Vector(starta, startb, startc)));
            Mesh.Add(new Triangle(new Vector(enda, startb, endc), new Vector(starta, startb, startc), new Vector(enda, startb, startc)));
            Color = Color.LightBlue;
        }
    }

    public class Sphere : Shape3D
    {
        public Sphere()
        {
            Name = "Sphere";
            Color = Color.Orange;
        }
        public Sphere(double r, int d1, int d2)
        {
            Name = "Sphere";
            Color = Color.Orange;
            d1 += 1;
            Vector[,] sphere = new Vector[d1, d2];
            double step1 = Math.PI / (d1-1);
            double step2 = 2 * Math.PI / d2;
            double lat = 0.0;
            double lon = 0.0;

            for (int i = 0; i < d1; i++)
            {
                for (int j = 0; j < d2; j++)
                {
                    double x = r * Math.Sin(lon) * Math.Cos(lat);
                    double y = r * Math.Sin(lon) * Math.Sin(lat);
                    double z = r * Math.Cos(lon);
                    Vector v = new Vector(x, y, z);
                    sphere[i, j] = v;
                    lat += step2;

                }
                lon += step1;

            }

            for (int i = 0; i < d1-1; i++)
            {
                for (int j = 0; j < d2; j++)
                {
                    Triangle t1 = new Triangle(sphere[i, j], sphere[(i + 1) % d1, j], sphere[i, (j + 1) % d2]);
                    Triangle t2 = new Triangle(sphere[i, (j + 1) % d2], sphere[(i + 1) % d1, j], sphere[(i + 1) % d1, (j + 1) % d2]);
                    Mesh.Add(t1);
                    Mesh.Add(t2);
                }
            }
        }
    }


}
