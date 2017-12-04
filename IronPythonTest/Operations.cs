using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace DImgConv
{
    public class Operations
    {
        /// <summary>
        /// Returns the image vertically mirrored
        /// </summary>
        /// <param name="img">image</param>
        /// <returns></returns>
        public static Bitmap FlipVertical(Bitmap img)
        {
            int h = img.Height, w = img.Width;
            Bitmap img2 = new Bitmap(w, h);

            for (int j = 0; j < h; j++)
            {
                for (int i = 0; i < w; i++)
                {
                    img2.SetPixel(i, j, img.GetPixel(i, h - 1 - j));
                }
            }
            return img2;
        }

        /// <summary>
        /// Returns the image horizontally mirrored
        /// </summary>
        /// <param name="img">image</param>
        /// <returns></returns>
        public static Bitmap FlipHorizontal(Bitmap img)
        {
            int h = img.Height, w = img.Width;
            Bitmap img2 = new Bitmap(w, h);

            for (int j = 0; j < h; j++)
            {
                for (int i = 0; i < w; i++)
                {
                    img2.SetPixel(i, j, img.GetPixel(w - 1 - i, j));
                }
            }
            return img2;
        }

        /// <summary>
        /// Returns a negative of the image
        /// </summary>
        /// <param name="img">image</param>
        /// <returns></returns>
        public static Bitmap Negative(Bitmap img)
        {
            int h = img.Height, w = img.Width;
            Bitmap img2 = new Bitmap(w, h);

            for (int j = 0; j < h; j++)
            {
                for (int i = 0; i < w; i++)
                {
                    img2.SetPixel(i, j,
                        Color.FromArgb(
                        img.GetPixel(i, j).A,
                        255 - img.GetPixel(i, j).R, 
                        255 - img.GetPixel(i, j).G, 
                        255 - img.GetPixel(i, j).B));
                }
            }

            return img2;
        }

        /// <summary>
        /// Removes the alpha channel from the image
        /// </summary>
        /// <param name="img">image</param>
        /// <returns></returns>
        public static Bitmap RemoveAlpha(Bitmap img)
        {
            int h = img.Height, w = img.Width;
            Bitmap img2 = new Bitmap(w, h);

            for (int j = 0; j < h; j++)
            {
                for (int i = 0; i < w; i++)
                {
                    img2.SetPixel(i, j,
                        Color.FromArgb(
                        img.GetPixel(i, j).R,
                        img.GetPixel(i, j).G,
                        img.GetPixel(i, j).B));
                }
            }
            return img2;
        }

        private static void GetChannels(Bitmap img, byte[,] Red, byte[,] Green, byte[,] Blue, byte[,] Alpha)
        {
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    Red[i, j] = img.GetPixel(i, j).R;
                    Green[i, j] = img.GetPixel(i, j).G;
                    Blue[i, j] = img.GetPixel(i, j).B;
                    Alpha[i, j] = img.GetPixel(i, j).A;
                }
            }
        }

        /*private Bitmap BitmapFromArray(byte[,] arr, int W, int H)
        {
            Bitmap img = new Bitmap(W, H);
            for (int i = 0; i < W; i++)
            {
                for (int j = 0; j < H; j++)
                {
                    img.SetPixel(i, j, Color.FromArgb(arr[i, j], arr[i, j], arr[i, j]));
                }
            }
            return img;
        }*/

        private static void InterpolateChannel(byte[,] aOrg, byte[,] aNew, double[, ,] newCoord, int W, int H, int W2, int H2)
        {
            double x = 0, y = 0;
            int x1 = 0, x2 = 0, y1 = 0, y2 = 0;
            double q11 = 0, q12 = 0, q21 = 0, q22 = 0;
            for (int i = 0; i < W2; i++)
            {
                for (int j = 0; j < H2; j++)
                {
                    x = newCoord[i, j, 0];
                    y = newCoord[i, j, 1];
                    x1 = (int)Math.Ceiling(newCoord[i, j, 0]);
                    y1 = (int)Math.Floor(newCoord[i, j, 1]);
                    x2 = (int)Math.Floor(newCoord[i, j, 0]);
                    y2 = (int)Math.Ceiling(newCoord[i, j, 1]);
                    if (Math.Floor(newCoord[i, j, 0]) == Math.Ceiling(newCoord[i, j, 0]))
                    {
                        x1 = (int)Math.Ceiling(newCoord[i, j, 0]) - 1;
                        x2 = x1 + 1;
                    }
                    if (Math.Floor(newCoord[i, j, 1]) == Math.Ceiling(newCoord[i, j, 1]))
                    {
                        y1 = (int)Math.Floor(newCoord[i, j, 1]) - 1;
                        y2 = y1 + 1;
                    }

                    int x1t = x1, x2t = x2, y1t = y1, y2t = y2;
                    if (y1t < 0)
                        y1t = 0;
                    if (x1t < 0)
                        x1t = 0;
                    if (y2t < 0)
                        y2t = 0;
                    if (x2t < 0)
                        x2t = 0;

                    if (y1t >= H)
                        y1t = H - 1;
                    if (x1t >= W)
                        x1t = W - 1;
                    if (y2t >= H)
                        y2t = H - 1;
                    if (x2t >= W)
                        x2t = W - 1;

                    /*if (y1t == y2t)
                    {
                        if (y1t == 0)
                            y1t += 1;
                        else
                            y1t -= 1;
                    }*/
                    if (y1t == y2t)
                    {
                        if (y1t == 0)
                            y1t += 1;
                        else
                            y2t -= 1;
                    }

                    if (x1t == x2t)
                    {
                        if (x1t == 0)
                            x1t += 1;
                        else
                            x1t -= 1;
                    }

                    byte a1 = aOrg[x1t, y1t];
                    byte a2 = aOrg[x1t, y2t];
                    byte a3 = aOrg[x2t, y1t];
                    byte a4 = aOrg[x2t, y2t];


                    q11 = a1 * (x2 - x) * (y2 - y) / (x2 - x1) / (y2 - y1);
                    q12 = a2 * (x2 - x) * (y - y1) / (x2 - x1) / (y2 - y1);
                    q21 = a3 * (x - x1) * (y2 - y) / (x2 - x1) / (y2 - y1);
                    q22 = a4 * (x - x1) * (y - y1) / (x2 - x1) / (y2 - y1);
                    aNew[i, j] = (byte)(q11 + q21 + q12 + q22);
                }
            }
        }

        public static Bitmap ResizeImg(Bitmap img, double resizeRate)
        {
            //Ширина и высота
            int W = 0, H = 0, W2 = 0, H2 = 0;
            //Составляющие каналы
            byte[,] Red, Green, Blue, Alpha;
            byte[,] Red2, Green2, Blue2, Alpha2;
            //Множитель размера изображения
            //double resizeRate;

            //Bitmap img;

            double[, ,] newCoord;

            if (resizeRate == 1.0)
                return img;
            //this.resizeRate = resizeRate;
            //this.img = img;

            //PrepareToResize();
            W = img.Width;
            H = img.Height;

            Red = new byte[W, H];
            Green = new byte[W, H];
            Blue = new byte[W, H];
            Alpha = new byte[W, H];
            GetChannels(img, Red, Green, Blue, Alpha);

            W2 = (int)(W * resizeRate);
            H2 = (int)(H * resizeRate);
            Red2 = new byte[W2, H2];
            Green2 = new byte[W2, H2];
            Blue2 = new byte[W2, H2];
            Alpha2 = new byte[W2, H2];
            newCoord = new double[W2, H2, 2];

            double dw = (double)W / (double)W2;
            double dh = (double)H / (double)H2;
            for (int i = 0; i < W2; i++)
            {
                for (int j = 0; j < H2; j++)
                {
                    newCoord[i, j, 0] = i * dw;
                    newCoord[i, j, 1] = j * dh;
                }
            }

            InterpolateChannel(Red, Red2, newCoord, W, H, W2, H2);
            InterpolateChannel(Green, Green2, newCoord, W, H, W2, H2);
            InterpolateChannel(Blue, Blue2, newCoord, W, H, W2, H2);
            InterpolateChannel(Alpha, Alpha2, newCoord, W, H, W2, H2);

            Bitmap bmp = new Bitmap(W2, H2);
            //for (int j = 0; j < W2; j++)
            for (int i = 0; i < W2; i++)
            {
                //for (int i = 0; i < H2; i++)
                for (int j = 0; j < H2; j++)
                {
                    bmp.SetPixel(i, j, Color.FromArgb(Alpha2[i, j], Red2[i, j], Green2[i, j], Blue2[i, j]));
                }
            }
            return bmp;
        }
    }
}
