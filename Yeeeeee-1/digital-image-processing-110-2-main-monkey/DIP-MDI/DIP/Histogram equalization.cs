using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace DIP
{
        public partial class Histogram_equalization : Form
        {
                int[] cdf;

                internal Bitmap res_Bitmap = null;
                Bitmap input_bitmap = null;
                [DllImport("dip_proc.dll", CallingConvention = CallingConvention.Cdecl)]
                unsafe public static extern void analyze_Histogram_process(int* f, int w, int h, int* count, int D);
                [DllImport("dip_proc.dll", CallingConvention = CallingConvention.Cdecl)]
                unsafe public static extern void analyze_Histogram_image_process(int* f, int w, int h, int* g,int* cdf, int D);
                public Histogram_equalization(Bitmap bitmap)
                {
                        InitializeComponent();
                        input_bitmap = bitmap;
                }
                private Bitmap analyze_Histogram(Bitmap analyze_Bitmap)
                {
                        Bitmap bitmap = new Bitmap(255, 255);
                        Graphics G = Graphics.FromImage(bitmap);
                        SolidBrush solid = new SolidBrush(Color.White);
                        Pen pen = new Pen(Color.Black);
                       G.FillRectangle(solid, 0, 0, 255, 255);


                        int[] f;
                        int[] g;
                        int PB_Width = 0;
                        int PB_Height = 0;
                        int ByteDepth = 0;
                        PixelFormat pixelFormat = new PixelFormat();
                        ColorPalette palette = null;
                        f = dyn_bmp2array(analyze_Bitmap, ref ByteDepth, ref pixelFormat, ref palette, ref PB_Width, ref PB_Height);
                        g = new int[256* ByteDepth];
                        cdf = new int[256 * ByteDepth];
                        unsafe
                        {
                                fixed (int* f0 = f) fixed (int* g0 = g)
                                {
                                        analyze_Histogram_process(f0, PB_Width, PB_Height, g0, ByteDepth);
                                }
                        }
                        int gmax = g.Max();
                        if (ByteDepth == 1)
                        {
                                for (int i = 0; i < 256; i++) G.DrawLine(pen, i, 255, i, 255 - (g[i]*256/gmax));
                                cdf[0] = g[0];
                                for (int i = 1; i < 256; i++) cdf[i] = g[i] + cdf[i - 1];
                        }
                        if (ByteDepth == 3)
                        {
                                for (int i = 0; i < 256; i++) G.DrawLine(new Pen(Color.Red), i, 255, i, 255 - (g[i] * 256 / gmax));
                                cdf[0] = g[0];
                                for (int i = 1; i < 256; i++) cdf[i] = g[i] + cdf[i - 1];
                                for (int i = 256; i < 512; i++) G.DrawLine(new Pen(Color.Green), i-256, 255, i - 256, 255 - (g[i] * 256 / gmax));
                                cdf[256] = g[256];
                                for (int i = 257; i < 512; i++) cdf[i] = g[i] + cdf[i - 1];
                                for (int i = 512; i < 768; i++) G.DrawLine(new Pen(Color.Blue), i- 512, 255, i- 512, 255 - (g[i] * 256 / gmax));
                                cdf[512] = g[512];
                                for (int i = 513; i < 768; i++) cdf[i] = g[i] + cdf[i - 1];
                        }

                        return bitmap;
                }


                private static Bitmap dyn_array2bmp(int[] ImgData, int ByteDepth, PixelFormat pixelFormat, ColorPalette palette, int width, int height)
                {
                        int Width = width;
                        int Height = height;
                        Bitmap myBitmap = new Bitmap(Width, Height, pixelFormat);
                        BitmapData byteArray = myBitmap.LockBits(new Rectangle(0, 0, Width, Height),
                                                       ImageLockMode.WriteOnly,
                                                       pixelFormat);
                        try
                        {
                                myBitmap.Palette = palette;
                        }
                        catch
                        {

                        }
                        //Padding bytes的長度
                        int ByteOfSkip = byteArray.Stride - myBitmap.Width * ByteDepth;
                        unsafe
                        {                                   // 指標取出影像資料
                                byte* imgPtr = (byte*)byteArray.Scan0;
                                for (int y = 0; y < Height; y++)
                                {
                                        for (int x = 0; x < Width; x++)
                                        {
                                                for (int c = 0; c < ByteDepth; c++)
                                                {
                                                        *imgPtr = (byte)ImgData[(x + Width * y) * ByteDepth + c];       //B
                                                        imgPtr += 1;
                                                }
                                        }
                                        imgPtr += ByteOfSkip; // 跳過Padding bytes
                                }
                        }
                        myBitmap.UnlockBits(byteArray);
                        return myBitmap;
                }
                private int[] dyn_bmp2array(Bitmap myBitmap, ref int ByteDepth, ref PixelFormat pixelFormat, ref ColorPalette palette, ref int Width, ref int Height)
                {
                        Width = myBitmap.Width;
                        Height = myBitmap.Height;
                        BitmapData byteArray = myBitmap.LockBits(new Rectangle(0, 0, myBitmap.Width, myBitmap.Height),
                                                      ImageLockMode.ReadWrite,
                                                      myBitmap.PixelFormat);
                        pixelFormat = myBitmap.PixelFormat;
                        palette = myBitmap.Palette;
                        ByteDepth = (int)(byteArray.Stride / myBitmap.Width);
                        int[] ImgData = new int[myBitmap.Width * myBitmap.Height * ByteDepth];
                        int ByteOfSkip = byteArray.Stride - byteArray.Width * ByteDepth;
                        unsafe
                        {
                                byte* imgPtr = (byte*)(byteArray.Scan0);
                                for (int y = 0; y < byteArray.Height; y++)
                                {
                                        for (int x = 0; x < byteArray.Width; x++)
                                        {
                                                for (int c = 0; c < ByteDepth; c++)
                                                {
                                                        ImgData[(x + byteArray.Height * y) * ByteDepth + c] = (int)*(imgPtr);
                                                        imgPtr += (int)(byteArray.Stride / myBitmap.Width) / ByteDepth;
                                                }
                                        }
                                        imgPtr += ByteOfSkip;
                                }
                        }
                        myBitmap.UnlockBits(byteArray);
                        return ImgData;
                }

                private void Histogram_equalization_Load(object sender, EventArgs e)
                {
                        pictureBox1.Image = input_bitmap;
                        pictureBox2.Image = analyze_Histogram(input_bitmap);
                        int[] f;
                        int[] g;
                        int PB_Width = 0;
                        int PB_Height = 0;
                                        int ByteDepth = 0;
                                        PixelFormat pixelFormat = new PixelFormat();
                                        ColorPalette palette = null;
                                        f = dyn_bmp2array(input_bitmap, ref ByteDepth, ref pixelFormat, ref palette, ref PB_Width, ref PB_Height);
                                        g = new int[PB_Width * PB_Height * ByteDepth];
                                        unsafe
                                        {
                                                fixed (int* f0 = f) fixed (int* g0 = g)fixed(int* cdf0=cdf)
                                                {
                                                        analyze_Histogram_image_process(f0, PB_Width, PB_Height, g0, cdf0, ByteDepth);
                                                }
                                        }
                        res_Bitmap = dyn_array2bmp(g, ByteDepth, pixelFormat, palette, PB_Height, PB_Width);
                        pictureBox4.Image = res_Bitmap;
                        pictureBox3.Image = analyze_Histogram(res_Bitmap);


                }

                private void button2_Click(object sender, EventArgs e)
                {
                        this.Close();
                }

                private void button1_Click(object sender, EventArgs e)
                {
                        this.Close();
                }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
