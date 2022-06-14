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
using System.Globalization;

namespace DIP
{
    
    public partial class monkey : Form
    {

        int[] cdf;

        internal Bitmap res_Bitmap = null;
        Bitmap input_bitmap = null;

         public monkey(Bitmap bitmap)
                {
                        InitializeComponent();
                        input_bitmap = bitmap;
                }

        public static Bitmap dyn_array2bmp(int[] ImgData, int ByteDepth, PixelFormat pixelFormat, ColorPalette palette, int width, int height)
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
        public monkey()
        {
            InitializeComponent();
        }

        public void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        public void monkey_Load(object sender, EventArgs e)
        {
            pictureBox1.Image = input_bitmap;
        }

        public void button1_Click(object sender, EventArgs e)
        {
            double Trans = Convert.ToDouble(textBox1.Text);
            Bitmap a = new Bitmap(pictureBox1.Image);
            Bitmap b = new Bitmap(pictureBox1.Image);
            res_Bitmap = Rotate(a, Convert.ToInt32(textBox1.Text)); 
        }

        public Bitmap Rotate(Bitmap b, int angle)
        {
            angle = angle % 360;

            //弧度轉換
            double radian = angle * Math.PI / 180.0;
            double cos = Math.Cos(radian);
            double sin = Math.Sin(radian);

            //原圖的寬和高
            int w = b.Width;
            int h = b.Height;
            int W = (int)(Math.Max(Math.Abs(w * cos - h * sin), Math.Abs(w * cos + h * sin)));
            int H = (int)(Math.Max(Math.Abs(w * sin - h * cos), Math.Abs(w * sin + h * cos)));

            //目標點陣圖
            Bitmap dsImage = new Bitmap(W, H);
            Graphics g = Graphics.FromImage(dsImage);

           

            //計算偏移量
            Point Offset = new Point((W - w) / 2, (H - h) / 2);

            //構造影象顯示區域：讓影象的中心與視窗的中心點一致
            Rectangle rect = new Rectangle(Offset.X, Offset.Y, w, h);
            Point center = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
            g.TranslateTransform(center.X, center.Y);
            g.RotateTransform(360 - angle);

            //恢復影象在水平和垂直方向的平移
            g.TranslateTransform(-center.X, -center.Y);
            g.DrawImage(b, rect);

            //重至繪圖的所有變換
            g.ResetTransform();

            g.Save();
            g.Dispose();
            return dsImage;
        }
    }

}
