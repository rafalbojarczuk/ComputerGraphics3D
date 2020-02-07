using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerGraphics3D
{
    public class Polygon
    {
        public List<Vertex> Vertices { get; set; }
        public Polygon(List<Vertex> vertices)
        {
            Vertices = vertices;
        }
        public Polygon(Triangle t)
        {
            Vertices = new List<Vertex>()
            {
                new Vertex(new Point(Convert.ToInt32(t.Ver[0].X), Convert.ToInt32(t.Ver[0].Y)),Color.Black),
                new Vertex(new Point(Convert.ToInt32(t.Ver[1].X), Convert.ToInt32(t.Ver[1].Y)),Color.Black),
                new Vertex(new Point(Convert.ToInt32(t.Ver[2].X), Convert.ToInt32(t.Ver[2].Y)),Color.Black),
            };
        }
    }

    public class Edge
    {
        public Vertex V1 { get; set; }
        public Vertex V2 { get; set; }

        public Edge(Vertex p1, Vertex p2)
        {
            V1 = p1;
            V2 = p2;
        }
    }

    public class Vertex
    {
        public Point Coords { get; set; }
        public Color Color { get; set; }

        public Vertex(Point point, Color col)
        {
            Coords = point;
            Color = col;
        }
    }

    public class Triangle
    {
        public Vector[] Ver { get; set; } = new Vector[3];

        public Color Col = Color.Black;

        public Triangle()
        {
            Ver[0]=new Vector(0,0,0);
            Ver[1]=new Vector(0,0,0);
            Ver[2]=new Vector(0,0,0);
        }
        public Triangle(Vector v1, Vector v2, Vector v3)
        {
            Ver[0] = v1;
            Ver[1] = v2;
            Ver[2] = v3;
        }
        public Triangle(Triangle t)
        {
            Ver[0] = new Vector(t.Ver[0].X, t.Ver[0].Y, t.Ver[0].Z);
            Ver[1] = new Vector(t.Ver[1].X, t.Ver[1].Y, t.Ver[1].Z);
            Ver[2] = new Vector(t.Ver[2].X, t.Ver[2].Y, t.Ver[2].Z);
            Col = t.Col;
        }

        public Vector GetNormal()
        {
            Vector line1 = Ver[1].VectorSub(Ver[0]);
            Vector line2 = Ver[2].VectorSub(Ver[0]);
            Vector normal = line1.CrossProduct(line2);
            normal.Normalize();
            return normal;
        }
    }


}
