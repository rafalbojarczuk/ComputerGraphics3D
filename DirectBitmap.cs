using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;

namespace ComputerGraphics3D
{
    public class DirectBitmap : IDisposable
    {
        public Bitmap Bitmap { get; set; }
        public byte[] Bits { get; private set; }
        public bool Disposed { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }

        public GCHandle BitsHandle { get; private set; }

        public DirectBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            Bits = new byte[width * height * 4];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppArgb, BitsHandle.AddrOfPinnedObject());
        }

        public void SetPixel(int x, int y, Color c)
        {
            if (x >= Width * 4 || y >= Height) return;
            int index = x*4 + (y * Width * 4);

            if (index >= Bits.Length - 4 || index < 0) return;

            Bits[index++] = c.B;
            Bits[index++] = c.G;
            Bits[index++] = c.R;
            Bits[index] = 255;  
        }

        public Color GetPixel(int x, int y)
        {
            int index = (x*4 + (y * Width * 4))%Bits.Length;
            byte b = Bits[index++];
            byte g = Bits[index++];
            byte r = Bits[index];


            return Color.FromArgb(r,g,b);
        }

        public void ClearBitmap()
        {
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    SetPixel(i, j, Color.White);
        }

        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
            Bitmap.Dispose();
            BitsHandle.Free();
        }
    }

}