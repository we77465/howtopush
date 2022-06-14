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
    public partial class OTSU : Form
    {
        [DllImport("dip_proc.dll", CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern void OTSU_process(int* f, int w_in, int h_in, int D);
        private void ChangeImage(PictureBox pictureBox, Bitmap bitmap)
        {
            pictureBox.Image = bitmap;
            pictureBox.Width = bitmap.Width;
            pictureBox.Height = bitmap.Height;
        }
        public OTSU(Bitmap bitmap)
        {
            input_bitmap = bitmap;
            InitializeComponent();
        }
        Bitmap input_bitmap = null;
        private PictureBox pictureBox1;
        Bitmap output_bitmap = null;
        private void OTSU_Load(object sender, EventArgs e)
        {
            int[] f;
            int[] g;
            int PB_Width = 0;
            int PB_Height = 0;
            int ByteDepth = 0;
            PixelFormat pixelFormat = new PixelFormat();
            ColorPalette palette = null;
            f = dyn_bmp2array((Bitmap)input_bitmap, ref ByteDepth, ref pixelFormat, ref palette, ref PB_Width, ref PB_Height);
            g = new int[PB_Width * PB_Height * ByteDepth];
            unsafe
            {
                fixed (int* f0 = f) fixed (int* g0 = g)
                {
                    OTSU_process(f0, PB_Width, PB_Height, ByteDepth);
                }
            }
            output_bitmap = dyn_array2bmp(f, ByteDepth, pixelFormat, palette, PB_Width, PB_Height);
            ChangeImage(pictureBox1, input_bitmap);
            ChangeImage(pictureBox2, output_bitmap);

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
                            ImgData[(x + byteArray.Width * y) * ByteDepth + c] = (int)*(imgPtr);
                            imgPtr += (int)(byteArray.Stride / myBitmap.Width) / ByteDepth;
                        }
                    }
                    imgPtr += ByteOfSkip;
                }
            }
            myBitmap.UnlockBits(byteArray);
            return ImgData;
        }

        private void InitializeComponent()
        {
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(113, 84);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(223, 214);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Location = new System.Drawing.Point(596, 84);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(200, 214);
            this.pictureBox2.TabIndex = 1;
            this.pictureBox2.TabStop = false;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(433, 288);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // OTSU
            // 
            this.ClientSize = new System.Drawing.Size(884, 424);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.pictureBox1);
            this.Name = "OTSU";
            this.Load += new System.EventHandler(this.OTSU_Load_1);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);

        }

        internal PictureBox pictureBox2;
        internal Button button1;

        private void OTSU_Load_1(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
