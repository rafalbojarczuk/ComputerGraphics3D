using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ComputerGraphics3D.Utilities;
using System.Diagnostics;
using System.Media;
using System.Globalization;
using System.Xml.Serialization;
using System.IO;

namespace ComputerGraphics3D
{
    public partial class Form1 : Form
    {
        List<SceneObject> SceneObjects = new List<SceneObject>();
        Camera CurrentCamera = new Camera(new Vector(0, 0, -10));
        Cuboid CurrentCuboid;
        Sphere CurrentSphere;
        double aspect = 0.0;
        SerializableData Data = new SerializableData();
        bool BackfaceCulling = true;
        bool DrawingGrid = true;
        bool Filling = true;
        int timerTicks = 0;
        Drawer d;


        public Form1()
        {

            InitializeComponent();
            d = new Drawer(pictureBox1);
            KeyPreview = true;
            pictureBox1.Image = d.DirectBitmap.Bitmap;
            timer1.Start();
            SceneObjects.Add(CurrentCamera);
            Data.CamData.Add(CurrentCamera);
            SceneObjectsListBox.DataSource = SceneObjects;
        }






        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            Vector Forward = CurrentCamera.LookDir.VectorMul(1.1);
            switch (e.KeyCode)
            {
                case Keys.A:
                    CurrentCamera.FY += 0.05;
                    break;
                case Keys.D:
                    CurrentCamera.FY -= 0.05;
                    break;
                case Keys.W:
                    CurrentCamera.Position = CurrentCamera.Position.VectorAdd(Forward);
                    break;
                case Keys.S:
                    CurrentCamera.Position = CurrentCamera.Position.VectorSub(Forward);
                    break;
                case Keys.NumPad8:
                    CurrentCamera.Position.Y -= 0.2;
                    break;
                case Keys.NumPad2:
                    CurrentCamera.Position.Y += 0.2;
                    break;
                case Keys.NumPad4:
                    CurrentCamera.Position.X -= 0.2;
                    break;
                case Keys.NumPad6:
                    CurrentCamera.Position.X += 0.2;
                    break;
                case Keys.D8:
                    CurrentCamera.Position.Y -= 0.2;
                    break;
                case Keys.D2:
                    CurrentCamera.Position.Y += 0.2;
                    break;
                case Keys.D4:
                    CurrentCamera.Position.X -= 0.2;
                    break;
                case Keys.D6:
                    CurrentCamera.Position.X += 0.2;
                    break;
                default:
                    break;
            }
        }










        private void timer1_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < pictureBox1.Width; i++)
                for (int j = 0; j < pictureBox1.Height; j++)
                    d.DepthBuffer[i, j] = double.MaxValue;
            d.DirectBitmap.Dispose();
            d.DirectBitmap = new DirectBitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Image = d.DirectBitmap.Bitmap;
            RenderScene(CurrentCamera);


        }

        private void RenderScene(Camera camera)
        {
            aspect = pictureBox1.Width / (double)pictureBox1.Height;

            ProjectionMatrix P = new ProjectionMatrix(aspect, camera.FoV, camera.FFar, camera.FNear);
            YRotationMatrix CameraRot = new YRotationMatrix(camera.FY);
            Vector up = new Vector(0, 1, 0);
            Vector target = new Vector(0, 0, 1);
            camera.LookDir = MultMatrixVector(CameraRot, target);
            target = camera.Position.VectorAdd(camera.LookDir);

            ViewMatrix V = new ViewMatrix(camera.Position, target, up);
            List<(Triangle, Color)> trianglesToRaster = new List<(Triangle, Color)>();
            int n = SceneObjects.Count();
            for (int k = 0; k < n; k++)
            {
                Shape3D shape;
                if ((shape = (SceneObjects[k] as Shape3D)) != null)
                {
                    XRotationMatrix xRot = new XRotationMatrix(shape.RotAngle.X * 0.01 * timerTicks);
                    YRotationMatrix yRot = new YRotationMatrix(shape.RotAngle.Y * 0.01 * timerTicks);
                    ZRotationMatrix zRot = new ZRotationMatrix(shape.RotAngle.Z * 0.01 * timerTicks);
                    ScalingMatrix scale = new ScalingMatrix(shape.Scaling.X, shape.Scaling.Y, shape.Scaling.Z);
                    TranslationMatrix trans = new TranslationMatrix(shape.Translation.X, shape.Translation.Y, shape.Translation.Z);

                    Matrix worldMatrix = MultMatrices(trans, MultMatrices(zRot, MultMatrices(yRot, MultMatrices(xRot, scale))));

                    foreach (Triangle t in shape.Mesh)
                    {
                        Triangle triTransformed = new Triangle(t);
                        Triangle triViewed = new Triangle();
                        Triangle triProjected = new Triangle();

                        triTransformed.Ver[0] = MultMatrixVector(worldMatrix, t.Ver[0]);
                        triTransformed.Ver[1] = MultMatrixVector(worldMatrix, t.Ver[1]);
                        triTransformed.Ver[2] = MultMatrixVector(worldMatrix, t.Ver[2]);

                        Vector normal = triTransformed.GetNormal();

                        Vector cameraRay = triTransformed.Ver[0].VectorSub(camera.Position);

                        if (BackfaceCulling == false || normal.DotProduct(cameraRay) < 0.0)
                        {
                            triViewed.Ver[0] = MultMatrixVector(V, triTransformed.Ver[0]);
                            triViewed.Ver[1] = MultMatrixVector(V, triTransformed.Ver[1]);
                            triViewed.Ver[2] = MultMatrixVector(V, triTransformed.Ver[2]);

                            Triangle[] clipped = new Triangle[2];
                            int nClippedTriangles = ClipAgainstPlane(new Vector(0, 0, camera.FNear), new Vector(0, 0, 1.0), triViewed, out clipped[0], out clipped[1]);

                            for (int i = 0; i < nClippedTriangles; i++)
                            {
                                triProjected.Ver[0] = MultMatrixVector(P, clipped[i].Ver[0]);
                                triProjected.Ver[1] = MultMatrixVector(P, clipped[i].Ver[1]);
                                triProjected.Ver[2] = MultMatrixVector(P, clipped[i].Ver[2]);
                                ConvertToVisibleSpace(triProjected, pictureBox1.Width, pictureBox1.Height);

                                trianglesToRaster.Add((triProjected, shape.Color));
                            }
                        }
                    }
                }
            }


            //Clipping
            foreach ((Triangle, Color) triToRaster in trianglesToRaster)
            {
                Triangle[] clipped = new Triangle[2];
                Queue<(Triangle, Color)> triangles = new Queue<(Triangle, Color)>();

                triangles.Enqueue(triToRaster);
                int nNewTriangles = 1;

                for (int i = 0; i < 4; i++)
                {
                    int nTrisToAdd = 0;
                    while (nNewTriangles > 0)
                    {
                        (Triangle, Color) triToClip = triangles.Dequeue();
                        nNewTriangles--;

                        switch (i)
                        {
                            case 0: nTrisToAdd = ClipAgainstPlane(new Vector(0, 0, 0), new Vector(0, 1, 0), triToClip.Item1, out clipped[0], out clipped[1]); break;
                            case 1: nTrisToAdd = ClipAgainstPlane(new Vector(0, pictureBox1.Height - 1, 0), new Vector(0, -1, 0), triToClip.Item1, out clipped[0], out clipped[1]); break;
                            case 2: nTrisToAdd = ClipAgainstPlane(new Vector(0, 0, 0), new Vector(1, 0, 0), triToClip.Item1, out clipped[0], out clipped[1]); break;
                            case 3: nTrisToAdd = ClipAgainstPlane(new Vector(pictureBox1.Width - 1, 0, 0), new Vector(-1, 0, 0), triToClip.Item1, out clipped[0], out clipped[1]); break;
                        }

                        for (int j = 0; j < nTrisToAdd; j++)
                        {
                            triangles.Enqueue((clipped[j], triToClip.Item2));
                        }

                    }
                    nNewTriangles = triangles.Count();
                }

                foreach ((Triangle, Color) t in triangles)
                {
                    if (Filling) d.FillTriangle(t.Item1, t.Item2);
                    if (DrawingGrid) d.DrawTriangle(t.Item1);
                }
            }
            timerTicks++;
        }



        private void AddCuboid_Click(object sender, EventArgs e)
        {
            CurrentCuboid = new Cuboid(Convert.ToDouble(WidthCub_TextBox.Text, CultureInfo.InvariantCulture), Convert.ToDouble(HeightCub_TextBox.Text, CultureInfo.InvariantCulture), Convert.ToDouble(DepthCub_TextBox.Text, CultureInfo.InvariantCulture));
            SceneObjects.Add(CurrentCuboid);
            Data.CubData.Add(CurrentCuboid);
            SceneObjectsListBox.DataSource = null;
            SceneObjectsListBox.DataSource = SceneObjects;

        }

        private void AddSphere_Button_Click(object sender, EventArgs e)
        {
            CurrentSphere = new Sphere(Convert.ToDouble(RadiusSphere_TextBox.Text, CultureInfo.InvariantCulture), Convert.ToInt32(LatitudesSphere_TextBox.Text), Convert.ToInt32(LongitudesSphere_TextBox.Text));
            SceneObjects.Add(CurrentSphere);
            Data.SphData.Add(CurrentSphere);
            SceneObjectsListBox.DataSource = null;
            SceneObjectsListBox.DataSource = SceneObjects;

        }

        private void AddCamera_Button_Click(object sender, EventArgs e)
        {
            Vector pos = new Vector(Convert.ToDouble(PositionXCam_TextBox.Text, CultureInfo.InvariantCulture)
                , Convert.ToDouble(PositionYCam_TextBox.Text, CultureInfo.InvariantCulture)
                , Convert.ToDouble(PositionZCam_TextBox.Text, CultureInfo.InvariantCulture));
            CurrentCamera = new Camera(pos);
            SceneObjects.Add(CurrentCamera);
            Data.CamData.Add(CurrentCamera);
            SceneObjectsListBox.DataSource = null;
            SceneObjectsListBox.DataSource = SceneObjects;
        }

        private void ScaleXCub_TextBox_TextChanged(object sender, EventArgs e)
        {
            double val;
            bool successful = double.TryParse(ScaleXCub_TextBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out val);
            if (successful && CurrentCuboid != null)
                CurrentCuboid.Scaling.X = val;
        }

        private void ScaleYCub_TextBox_TextChanged(object sender, EventArgs e)
        {
            double val;
            bool successful = double.TryParse(ScaleYCub_TextBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out val);
            if (successful && CurrentCuboid != null)
                CurrentCuboid.Scaling.Y = val;
        }

        private void ScaleZCub_TextBox_TextChanged(object sender, EventArgs e)
        {
            double val;
            bool successful = double.TryParse(ScaleZCub_TextBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out val);
            if (successful && CurrentCuboid != null)
                CurrentCuboid.Scaling.Z = val;
        }

        private void ScaleXSph_TextBox_TextChanged(object sender, EventArgs e)
        {
            double val;
            bool successful = double.TryParse(ScaleXSph_TextBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out val);
            if (successful && CurrentSphere != null)
                CurrentSphere.Scaling.X = val;
        }

        private void ScaleYSph_TextBox_TextChanged(object sender, EventArgs e)
        {
            double val;
            bool successful = double.TryParse(ScaleYSph_TextBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out val);
            if (successful && CurrentSphere != null)
                CurrentSphere.Scaling.Y = val;
        }

        private void ScaleZSph_TextBox_TextChanged(object sender, EventArgs e)
        {
            double val;
            bool successful = double.TryParse(ScaleZSph_TextBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out val);
            if (successful && CurrentSphere != null)
                CurrentSphere.Scaling.Z = val;
        }

        private void TranslateXCub_TextBox_TextChanged(object sender, EventArgs e)
        {
            double val;
            bool successful = double.TryParse(TranslateXCub_TextBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out val);
            if (successful && CurrentCuboid != null)
                CurrentCuboid.Translation.X = val;
        }

        private void TranslateYCub_TextBox_TextChanged(object sender, EventArgs e)
        {
            double val;
            bool successful = double.TryParse(TranslateYCub_TextBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out val);
            if (successful && CurrentCuboid != null)
                CurrentCuboid.Translation.Y = -val;
        }

        private void TranslateZCub_TextBox_TextChanged(object sender, EventArgs e)
        {
            double val;
            bool successful = double.TryParse(TranslateZCub_TextBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out val);
            if (successful && CurrentCuboid != null)
                CurrentCuboid.Translation.Z = val;
        }

        private void TranslateXSph_TextBox_TextChanged(object sender, EventArgs e)
        {
            double val;
            bool successful = double.TryParse(TranslateXSph_TextBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out val);
            if (successful && CurrentSphere != null)
                CurrentSphere.Translation.X = val;
        }

        private void TranslateYSph_TextBox_TextChanged(object sender, EventArgs e)
        {
            double val;
            bool successful = double.TryParse(TranslateYSph_TextBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out val);
            if (successful && CurrentSphere != null)
                CurrentSphere.Translation.Y = -val;
        }

        private void TranslateZSph_TextBox_TextChanged(object sender, EventArgs e)
        {
            double val;
            bool successful = double.TryParse(TranslateZSph_TextBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out val);
            if (successful && CurrentSphere != null)
                CurrentSphere.Translation.Z = val;
        }

        private void RotateXCub_TextBox_TextChanged(object sender, EventArgs e)
        {
            double val;
            bool successful = double.TryParse(RotateXCub_TextBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out val);
            if (successful && CurrentCuboid != null)
                CurrentCuboid.RotAngle.X = val;
        }

        private void RotateYCub_TextBox_TextChanged(object sender, EventArgs e)
        {
            double val;
            bool successful = double.TryParse(RotateYCub_TextBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out val);
            if (successful && CurrentCuboid != null)
                CurrentCuboid.RotAngle.Y = val;
        }

        private void RotateZCub_TextBox_TextChanged(object sender, EventArgs e)
        {
            double val;
            bool successful = double.TryParse(RotateZCub_TextBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out val);
            if (successful && CurrentCuboid != null)
                CurrentCuboid.RotAngle.Z = val;
        }

        private void RotateXSph_TextBox_TextChanged(object sender, EventArgs e)
        {
            double val;
            bool successful = double.TryParse(RotateXSph_TextBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out val);
            if (successful && CurrentSphere != null)
                CurrentSphere.RotAngle.X = val;
        }

        private void RotateYSph_TextBox_TextChanged(object sender, EventArgs e)
        {
            double val;
            bool successful = double.TryParse(RotateYSph_TextBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out val);
            if (successful && CurrentSphere != null)
                CurrentSphere.RotAngle.Y = val;
        }

        private void RotateZSph_TextBox_TextChanged(object sender, EventArgs e)
        {
            double val;
            bool successful = double.TryParse(RotateZSph_TextBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out val);
            if (successful && CurrentSphere != null)
                CurrentSphere.RotAngle.Z = val;
        }
        private void Fov_TextBox_TextChanged(object sender, EventArgs e)
        {
            double val;
            bool successful = double.TryParse(Fov_TextBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out val);
            if (successful)
                CurrentCamera.FoV = val;
        }

        private void fNear_TextBox_TextChanged(object sender, EventArgs e)
        {
            double val;
            bool successful = double.TryParse(fNear_TextBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out val);
            if (successful)
                CurrentCamera.FNear = val;
        }

        private void fFar_TextBox_TextChanged(object sender, EventArgs e)
        {
            double val;
            bool successful = double.TryParse(fFar_TextBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out val);
            if (successful)
                CurrentCamera.FFar = val;
        }

        private void Delete_Button_Click(object sender, EventArgs e)
        {
            SceneObject obj = (SceneObject)SceneObjectsListBox.SelectedItem;
            if (obj is Cuboid)
            {
                Cuboid cub = obj as Cuboid;
                Data.CubData.Remove(cub);
                SceneObjects.Remove(obj);

            }
            else if (obj is Sphere)
            {
                Sphere sph = obj as Sphere;
                Data.SphData.Remove(sph);
                SceneObjects.Remove(obj);

            }
            else if (obj is Camera && Data.CamData.Count() > 1)
            {
                Camera cam = obj as Camera;
                Data.CamData.Remove(cam);
                SceneObjects.Remove(obj);
                CurrentCamera = Data.CamData[0];
            }
            SceneObjectsListBox.DataSource = null;
            SceneObjectsListBox.DataSource = SceneObjects;
            SceneObjectsListBox.DisplayMember = "Name";
        }

        private void SceneObjectsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SceneObject obj = (SceneObject)SceneObjectsListBox.SelectedItem;
            if (obj is Cuboid)
            {
                Cuboid cub = obj as Cuboid;
                CurrentCuboid = cub;
                ScaleXCub_TextBox.Text = Convert.ToString(cub.Scaling.X, CultureInfo.InvariantCulture);
                ScaleYCub_TextBox.Text = Convert.ToString(cub.Scaling.Y, CultureInfo.InvariantCulture);
                ScaleZCub_TextBox.Text = Convert.ToString(cub.Scaling.Z, CultureInfo.InvariantCulture);
                TranslateXCub_TextBox.Text = Convert.ToString(cub.Translation.X, CultureInfo.InvariantCulture);
                TranslateYCub_TextBox.Text = Convert.ToString(-cub.Translation.Y, CultureInfo.InvariantCulture);
                TranslateZCub_TextBox.Text = Convert.ToString(cub.Translation.Z, CultureInfo.InvariantCulture);
                RotateXCub_TextBox.Text = Convert.ToString(cub.RotAngle.X, CultureInfo.InvariantCulture);
                RotateYCub_TextBox.Text = Convert.ToString(cub.RotAngle.Y, CultureInfo.InvariantCulture);
                RotateZCub_TextBox.Text = Convert.ToString(cub.RotAngle.Z, CultureInfo.InvariantCulture);
            }
            else if (obj is Sphere)
            {
                Sphere sph = obj as Sphere;
                CurrentSphere = sph;
                ScaleXSph_TextBox.Text = Convert.ToString(sph.Scaling.X, CultureInfo.InvariantCulture);
                ScaleYSph_TextBox.Text = Convert.ToString(sph.Scaling.Y, CultureInfo.InvariantCulture);
                ScaleZSph_TextBox.Text = Convert.ToString(sph.Scaling.Z, CultureInfo.InvariantCulture);
                TranslateXSph_TextBox.Text = Convert.ToString(sph.Translation.X, CultureInfo.InvariantCulture);
                TranslateYSph_TextBox.Text = Convert.ToString(-sph.Translation.Y, CultureInfo.InvariantCulture);
                TranslateZSph_TextBox.Text = Convert.ToString(sph.Translation.Z, CultureInfo.InvariantCulture);
                RotateXSph_TextBox.Text = Convert.ToString(sph.RotAngle.X, CultureInfo.InvariantCulture);
                RotateYSph_TextBox.Text = Convert.ToString(sph.RotAngle.Y, CultureInfo.InvariantCulture);
                RotateZSph_TextBox.Text = Convert.ToString(sph.RotAngle.Z, CultureInfo.InvariantCulture);
            }
            else if (obj is Camera)
            {
                Camera cam = obj as Camera;
                CurrentCamera = cam;
                Fov_TextBox.Text = Convert.ToString(cam.FoV, CultureInfo.InvariantCulture);
                fNear_TextBox.Text = Convert.ToString(cam.FNear, CultureInfo.InvariantCulture);
                fFar_TextBox.Text = Convert.ToString(cam.FFar, CultureInfo.InvariantCulture);

            }
        }

        private void Save_Button_Click(object sender, EventArgs e)
        {
            XmlSerializer x = new XmlSerializer(typeof(SerializableData));
            TextWriter writer = new StreamWriter("Scene.xml");
            x.Serialize(writer, Data);
            writer.Close();

            MessageBox.Show("Saved in bin/Debug/Scene.xml");
        }

        private void Load_Button_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Load scene";
                dlg.Filter = "Xml files (*.xml)|*.xml;";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    XmlSerializer x = new XmlSerializer(typeof(SerializableData));
                    TextReader reader = new StreamReader(dlg.FileName);
                    Data = (SerializableData)x.Deserialize(reader);
                    SceneObjects = new List<SceneObject>();
                    foreach (Camera c in Data.CamData)
                    {
                        SceneObjects.Add(c);
                    }
                    foreach (Cuboid c in Data.CubData)
                    {
                        c.Color = Color.LightBlue;
                        SceneObjects.Add(c);
                    }
                    foreach (Sphere c in Data.SphData)
                    {
                        c.Color = Color.Orange;
                        SceneObjects.Add(c);
                    }
                    foreach (Light c in Data.LightData)
                    {
                        SceneObjects.Add(c);
                    }
                    SceneObjectsListBox.DataSource = null;
                    SceneObjectsListBox.DataSource = SceneObjects;
                    SceneObjectsListBox.DisplayMember = "Name";
                }
            }
        }



        private void Grid_CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            DrawingGrid = Grid_CheckBox.Checked;
        }

        private void Backface_Checkbox_CheckedChanged(object sender, EventArgs e)
        {
            BackfaceCulling = Backface_Checkbox.Checked;
        }

        private void Filling_CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Filling = Filling_CheckBox.Checked;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
