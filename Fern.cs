using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;


// Three types of randomness. Direction. Color.

namespace FernNamespace
{
    /*
     * this class draws a fractal fern when the constructor is called.
     * Written as sample C# code for a CS 212 assignment -- October 2011.
     * 
     * Bugs: WPF and shape objects are the wrong tool for the task 
     */   
    class Fern
    {
        private static double DELTATHETA = 0.05;
        private static double SEGLENGTH = 5.0;
        Random Random { get; } = new Random();
        double Redux { get; }
        double Turnbias { get; }
        Graphics Graphics { get; } // Good old Graphics
        int canvas_width;
        int canvas_height;

        /* 
         * Fern constructor erases screen and draws a fern
         * 
         * Size: number of 3-pixel segments of tendrils
         * Redux: how much smaller children clusters are compared to parents
         * Turnbias: how likely to turn right vs. left (0=always left, 0.5 = 50/50, 1.0 = always right)
         * canvas: the canvas that the fern will be drawn on
         */
        public Fern(double size, double redux, double turnbias, Canvas canvas)
        {
            // http://stackoverflow.com/a/797519/2948122
            // Set up bitmap. Make this faster
            canvas_width = (int)(canvas.ActualWidth * 2);
            canvas_height = (int)(canvas.ActualHeight * 2);
            var wb = new System.Windows.Media.Imaging.WriteableBitmap((int)canvas_width, (int)canvas_height, 96, 96,
                              System.Windows.Media.PixelFormats.Pbgra32, null);
            wb.Lock();
            var bmp = new System.Drawing.Bitmap(wb.PixelWidth, wb.PixelHeight,
                                                 wb.BackBufferStride,
                                                 PixelFormat.Format32bppPArgb,
                                                 wb.BackBuffer);
            Graphics = System.Drawing.Graphics.FromImage(bmp); // Good old Graphics
            
            Redux = redux;
            Turnbias = turnbias;


            // Draw pot
            using (var pot = System.Drawing.Image.FromFile("cool.png"))
            {
                int pot_width = (int)canvas_width / 5;
                Graphics.DrawImage(pot, (int)(canvas_width / 2) - pot_width / 2, (int)(canvas_height / 1.4) - pot_width/9, pot_width, pot_width);
            }

            // draw a new fern at the center of the Canvas with given parameters
            //cluster((int) (canvas_width / 2), (int)(canvas_height / 2), size, Redux, Turnbias, Canvas);
            tendril((int)(canvas_width / 2), (int)(canvas_height/1.4), 140, 130, 90, size, -Math.PI);

            // ...and finally:
            Graphics.Dispose();
            bmp.Dispose();
            wb.AddDirtyRect(new Int32Rect(0, 0, (int) wb.Width, (int) wb.Height)); //https://msdn.microsoft.com/en-us/library/system.windows.media.imaging.writeablebitmap(v=vs.110).aspx
            wb.Unlock();
            // http://stackoverflow.com/a/2168498/2948122
            canvas.Background = new System.Windows.Media.ImageBrush(wb);
        }
    
        /// <summary>
        /// Draw a tendril recursively
        /// </summary>
        private void tendril(int x1, int y1, byte red, byte green, byte blue, double size, double direction)
        {
            int x2=x1, y2=y1;

            if (size < 1) // Because redux really small size doesn't end at the right time.
                return;

            for (int i = 0; i < size; i++)
            {
                direction += (Random.NextDouble() > Turnbias) ? -1 * DELTATHETA : DELTATHETA;
                x1 = x2; y1 = y2;
                x2 = x1 + (int)(SEGLENGTH * Math.Sin(direction));
                y2 = y1 + (int)(SEGLENGTH * Math.Cos(direction));
                line(x1, y1, x2, y2, red, green, blue, (int)(((1 + size - i) / 4)));

                // Make tendrils go out of branch

                int tendrils_per_tendril = 30;

                // Make tendril amount less on really small ones for looks.
                while((int)size / tendrils_per_tendril <= 0 && tendrils_per_tendril > 1) 
                {
                tendrils_per_tendril--;
                }
                //Math.Max(1, Math.Min((int)size, tendrils_per_tendril));

                if ((int)size / tendrils_per_tendril <= 0)
                    tendrils_per_tendril = 3;
                if ((int)size/tendrils_per_tendril > 0 &&
                    i % ((int) (size/tendrils_per_tendril)) == 0 && i > 0)
                {
                    // Randomness in the colors
                    tendril(x1, y1, (byte)(red-Random.Next(10, 20)), (byte)(green+ Random.Next(0, 50)), (byte)(blue- Random.Next(5, 20)), (((size - i ) <= 8 )? (size - i) - 1 : ((size -i) / Redux)), 
                        direction + (Random.NextDouble() * Math.PI/3 + Math.PI/6) * (i % (2*((int)size/ tendrils_per_tendril)) == 0 ? -1 : 1));
                }
                if (i + 1 == (int)size)
                {
                    if (Random.Next(2) == 0)
                        berry(x1, y1, Random.Next(1, (int)size));
                }
            }
        }

        /*
         * draw a circle centered at (x,y), radius radius, with a black edge, onto Canvas
         */
        private void berry(int x, int y, double radius)
        {
            // http://stackoverflow.com/questions/1819096/is-it-important-to-dispose-solidbrush-and-pen
            using (var pen = new SolidBrush(Color.FromArgb(Random.Next(10,255), Random.Next(0, 255), Random.Next(0, 255), Random.Next(0, 255))))
            {
                Graphics.FillEllipse(pen, (int)(x - radius), (int)(y - radius), (int)(radius * 2), (int)(radius * 2));
            }
        }

        /*
         * draw a line segment (x1,y1) to (x2,y2) with given color, thickness on Canvas
         */
        private void line(int x1, int y1, int x2, int y2, byte r, byte g, byte b, double thickness)
        {
            // http://stackoverflow.com/questions/1819096/is-it-important-to-dispose-solidbrush-and-pen
            using (var pen = new Pen(Color.FromArgb(255, r, g, b), (float) thickness))
            {
                Graphics.DrawLine(pen, x1, y1, x2, y2);
            }
        }
    }
}
