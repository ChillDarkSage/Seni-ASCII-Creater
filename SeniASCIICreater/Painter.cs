using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using Size = System.Drawing.Size;
using System.Windows.Data;

namespace SeniASCIICreater
{
    public class Painter : System.Windows.DependencyObject
    {
        //$@B%8&WM#*oahkbdpqwmZO0QLCJUYXzcvunxrjft/\|()1{}[]?-_+~<>i!lI;:,\"^`'. 
        //@W#$OEXC[(/?=^~_.` 
        // `._~^=?/([CXEO$#W@
        public const string ASCIIList = "@W#$OEXC[(/?=^~_.` ";
        public const string colorList = "ABCDEFGHIJKLMNOPQRSTOVWXYZ@#$%&";



        public string UserList
        {
            get { return (string)GetValue(UserListProperty); }
            set { SetValue(UserListProperty, value); }
        }
        public static readonly DependencyProperty UserListProperty =
            DependencyProperty.Register("UserList", typeof(string), typeof(Painter));



        public bool? IsUserList
        {
            get { return (bool?)GetValue(IsUserListProperty); }
            set { SetValue(IsUserListProperty, value); }
        }
        public static readonly DependencyProperty IsUserListProperty =
            DependencyProperty.Register("IsUserList", typeof(bool?), typeof(Painter));



        public bool? IsColorful
        {
            get { return (bool?)GetValue(IsColorfulProperty); }
            set { SetValue(IsColorfulProperty, value); }
        }
        public static readonly DependencyProperty IsColorfulProperty =
            DependencyProperty.Register("IsColorful", typeof(bool?), typeof(Painter));



        public bool? IsGaussionBlured
        {
            get { return (bool?)GetValue(IsGaussionBluredProperty); }
            set { SetValue(IsGaussionBluredProperty, value); }
        }
        public static readonly DependencyProperty IsGaussionBluredProperty =
            DependencyProperty.Register("IsGaussionBlured", typeof(bool?), typeof(Painter));



        public bool? IsSorted
        {
            get { return (bool?)GetValue(IsSortedProperty); }
            set { SetValue(IsSortedProperty, value); }
        }
        public static readonly DependencyProperty IsSortedProperty =
            DependencyProperty.Register("IsSorted", typeof(bool?), typeof(Painter));



        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }
        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register("Scale", typeof(double), typeof(Painter));



        private static Random rand = new Random();

        public double scale = 1;



        public void Draw(string inPath, string outPath)
        {
            string[] str = inPath.Split('\\');
            string fileName = str[str.Length - 1];
            outPath += @"\\" + fileName;

            Bitmap bitmap = new Bitmap(inPath);
            if (IsColorful == false)
                bitmap = RGB2Gray(bitmap);

            Size size = bitmap.Size;
            int width = size.Width;
            int height = size.Height;

            Bitmap imageData = new Bitmap(bitmap, (int)(width / 6 * scale), (int)(height / 9 * scale));
            Bitmap canvas = new Bitmap((int)(width * scale), (int)(height * scale));
            Graphics sourceGra = Graphics.FromImage(canvas);
            Color bg = Color.FromArgb(20, 150, 150, 150);
            SolidBrush brush = new SolidBrush(Color.White);

            if (IsColorful == false)
            {
                bg = Color.FromArgb(255, 255, 255, 255);
                brush.Color = Color.Black;
                if (IsSorted == true)
                {
                    Balance(imageData, out Bitmap newMap);
                    imageData = newMap;
                }
            }

            sourceGra.Clear(bg);
            if (IsGaussionBlured == true)
            {
                Bitmap blur = new Bitmap(bitmap);
                blur = ChangeDPi(blur, 10);
                blur = Resize(blur, canvas.Size);
                blur = BrightnessP(blur, -10);
                sourceGra.DrawImage(blur, 0, 0);
            }

            Font font = new Font("Courier New", 9f, System.Drawing.FontStyle.Regular);

            PointF dir = new PointF(-1.35f, -2.5f);

            for (int i = 0; i < imageData.Size.Height; i++)
            {
                for (int j = 0; j < imageData.Size.Width; j++)
                {
                    float brightness = imageData.GetPixel(j, i).GetBrightness();
                    Color color = imageData.GetPixel(j, i);
                    brush.Color = color;
                    sourceGra.DrawString(GetChar(brightness).ToString(), font, brush, dir.X, dir.Y);
                    dir.X += 6f;
                }
                dir = new PointF(-1.35f, -2.5f + (i + 1) * 9);
            }

            if (File.Exists(outPath))
                File.Delete(outPath);

            canvas.Save(outPath, ImageFormat.Png);
        }

        private char GetChar(float brightness)
        {
            if (IsColorful == false)
            {
                if (IsUserList == true)
                    return UserList[(int)(brightness * 0.99 * UserList.Count())];

                int index = (int)(brightness * 0.99 * ASCIIList.Count());
                return ASCIIList[index];
            }
            else
            {
                if (IsUserList == true)
                    return UserList[(int)(brightness * 0.99 * UserList.Count())];

                int index = rand.Next(0, colorList.Length);
                return colorList[index];
            }

        }

        private bool Balance(Bitmap srcBmp, out Bitmap dstBmp)
        {
            if (srcBmp == null)
            {
                dstBmp = null;
                return false;
            }
            int[] histogramArrayR = new int[256];//各个灰度级的像素数R
            int[] histogramArrayG = new int[256];//各个灰度级的像素数G
            int[] histogramArrayB = new int[256];//各个灰度级的像素数B
            int[] tempArrayR = new int[256];
            int[] tempArrayG = new int[256];
            int[] tempArrayB = new int[256];
            byte[] pixelMapR = new byte[256];
            byte[] pixelMapG = new byte[256];
            byte[] pixelMapB = new byte[256];
            dstBmp = new Bitmap(srcBmp);
            Rectangle rt = new Rectangle(0, 0, srcBmp.Width, srcBmp.Height);
            BitmapData bmpData = dstBmp.LockBits(rt, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            unsafe
            {
                //统计各个灰度级的像素个数
                for (int i = 0; i < bmpData.Height; i++)
                {
                    byte* ptr = (byte*)bmpData.Scan0 + i * bmpData.Stride;
                    for (int j = 0; j < bmpData.Width; j++)
                    {
                        histogramArrayB[*(ptr + j * 3)]++;
                        histogramArrayG[*(ptr + j * 3 + 1)]++;
                        histogramArrayR[*(ptr + j * 3 + 2)]++;
                    }
                }
                //计算各个灰度级的累计分布函数
                for (int i = 0; i < 256; i++)
                {
                    if (i != 0)
                    {
                        tempArrayB[i] = tempArrayB[i - 1] + histogramArrayB[i];
                        tempArrayG[i] = tempArrayG[i - 1] + histogramArrayG[i];
                        tempArrayR[i] = tempArrayR[i - 1] + histogramArrayR[i];
                    }
                    else
                    {
                        tempArrayB[0] = histogramArrayB[0];
                        tempArrayG[0] = histogramArrayG[0];
                        tempArrayR[0] = histogramArrayR[0];
                    }
                    //计算累计概率函数，并将值放缩至0~255范围内
                    pixelMapB[i] = (byte)(255.0 * tempArrayB[i] / (bmpData.Width * bmpData.Height) + 0.5);//加0.5为了四舍五入取整
                    pixelMapG[i] = (byte)(255.0 * tempArrayG[i] / (bmpData.Width * bmpData.Height) + 0.5);
                    pixelMapR[i] = (byte)(255.0 * tempArrayR[i] / (bmpData.Width * bmpData.Height) + 0.5);
                }
                //映射转换
                for (int i = 0; i < bmpData.Height; i++)
                {
                    byte* ptr = (byte*)bmpData.Scan0 + i * bmpData.Stride;
                    for (int j = 0; j < bmpData.Width; j++)
                    {
                        *(ptr + j * 3) = pixelMapB[*(ptr + j * 3)];
                        *(ptr + j * 3 + 1) = pixelMapG[*(ptr + j * 3 + 1)];
                        *(ptr + j * 3 + 2) = pixelMapR[*(ptr + j * 3 + 2)];
                    }
                }
            }
            dstBmp.UnlockBits(bmpData);
            return true;
        }

        private Bitmap RGB2Gray(Bitmap basemap)
        {
            Bitmap res = basemap.Clone() as Bitmap;
            int width = res.Size.Width;
            int height = res.Size.Height;
            for (int i = 0; i < width; ++i)
                for (int j = 0; j < height; ++j)
                {
                    Color pixel = basemap.GetPixel(i, j);
                    pixel = GetGray(Convert.ToByte(pixel.R), Convert.ToByte(pixel.G), Convert.ToByte(pixel.B));
                    res.SetPixel(i, j, pixel);
                }
            return res;
        }

        private Color GetGray(int r, int g, int b)
        {
            double gray = 0;
            gray = (r * 30 + g * 59 + b * 11 + 50) / 100.0;

            Color retColor = Color.FromArgb((int)gray, (int)gray, (int)gray);
            return retColor;
        }

        private Bitmap Resize(Bitmap bitmap, Size newSize)
        {
            Bitmap res = new Bitmap(bitmap, newSize);
            return res;
        }

        private Bitmap ChangeDPi(Bitmap bitmap, int scale)
        {
            Size size = bitmap.Size;
            Bitmap blur = new Bitmap(bitmap, size.Width / scale, size.Height / scale);
            Bitmap bmpDest = new Bitmap(size.Width / scale, size.Height / scale);

            Graphics g = Graphics.FromImage(bmpDest);
            g.DrawImage(blur, 0, 0, new Rectangle(0, 0, size.Width, size.Height), GraphicsUnit.Pixel);
            blur = bmpDest;
            blur = Resize(blur, size);

            return blur;
        }

        private Bitmap BrightnessP(Bitmap a, int v)
        {
            BitmapData bmpData = a.LockBits(new Rectangle(0, 0, a.Width, a.Height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            int bytes = a.Width * a.Height * 3;
            IntPtr ptr = bmpData.Scan0;
            int stride = bmpData.Stride;
            unsafe
            {
                byte* p = (byte*)ptr;
                int temp;
                for (int j = 0; j < a.Height; j++)
                {
                    for (int i = 0; i < a.Width * 3; i++, p++)
                    {
                        temp = (int)(p[0] + v);
                        temp = (temp > 255) ? 255 : temp < 0 ? 0 : temp;
                        p[0] = (byte)temp;
                    }
                    p += stride - a.Width * 3;
                }
            }
            a.UnlockBits(bmpData);
            return a;
        }

        public BindingExpressionBase SetBinding(DependencyProperty dp, BindingBase binding)
                                             => BindingOperations.SetBinding(this, dp, binding);
    }
}
