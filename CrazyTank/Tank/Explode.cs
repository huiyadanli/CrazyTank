using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace CrazyTank
{
    public class Explode
    {
        public bool Live { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        private Bitmap bmpExplode = GameImages.explodePic;
        private static int span = 5; //设置爆炸存在时间长  时间为：总刷新时间间隔*span
        private int i = 0;  //用于计时

        public static Size size = new Size(28, 28); //实际显示大小
        public static Size bmpSize = new Size(28, 28); //素材图片大小

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Explode(int x, int y)
        {
            this.Live = true;
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// 画出爆炸效果
        /// </summary>
        /// <param name="g"></param>
        public void Draw(Graphics g)
        {
            if (i == span)
            {
                i = 0;
                Live = false;
                return;
            }
            g.DrawImage(bmpExplode, X, Y, size.Width, size.Width);
            i++;
        }
    }
}
