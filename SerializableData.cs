using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerGraphics3D
{
    [Serializable]
    public class SerializableData
    {
        public List<Camera> CamData = new List<Camera>();
        public List<Cuboid> CubData = new List<Cuboid>();
        public List<Sphere> SphData = new List<Sphere>();
        public List<Light> LightData = new List<Light>();
        public SerializableData() { }
    }
}
