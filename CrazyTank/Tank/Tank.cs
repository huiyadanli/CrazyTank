using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace CrazyTank
{
    public class Tank
    {
        public string Name { get; set; }
        public int Life { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int XOld { get; set; }
        public int YOld { get; set; }
        public Direction Dir { get; set; }
        public Direction DirOld { get; set; }
        public int Speed { get; set; }
        public int Level { get; set; }
        public float[] Color { get; private set; }

        public static Size size = new Size(28, 28); //实际显示大小
        public static Size bmpSize = new Size(28, 28); //素材图片大小
        private Bitmap bmpTankNow, bmpTankOld;
        private List<Bitmap>[] bmpTankAll=new List<Bitmap>[4];
        private int bulletId = 0;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">坦克名字</param>
        /// <param name="x">位置x</param>
        /// <param name="y">位置y</param>
        /// <param name="bmpTank">坦克使用的图片</param>
        public Tank(string name, int x, int y, Direction dir, float[] tankColor)
        {
            this.Name = name;
            this.X = x;
            this.Y = y;
            this.Life = 1;
            this.Dir = dir;
            this.DirOld = dir;
            this.Speed = 5;
            this.Level = 0;
            this.Color = tankColor;

            this.bmpTankAll[0] = GameImages.Recoloring(GameImages.tankPic[0], Color); //0级坦克图片
            this.bmpTankAll[1] = GameImages.Recoloring(GameImages.tankPic[2], Color); //1级坦克图片
            this.bmpTankAll[2] = GameImages.Recoloring(GameImages.tankPic[4], Color); //2级坦克图片
            this.bmpTankAll[3] = GameImages.Recoloring(GameImages.tankPic[6], Color); //3级坦克图片
            this.bmpTankOld = bmpTankAll[Level][0];
        }

        /// <summary>
        /// 移动
        /// </summary>
        public void Move()
        {
            this.XOld = this.X;
            this.YOld = this.Y;
            switch (Dir)
            {
                case Direction.Up:
                    Y -= Speed;
                    break;
                case Direction.Down:
                    Y += Speed;
                    break;
                case Direction.Left:
                    X -= Speed;
                    break;
                case Direction.Right:
                    X += Speed;
                    break;
                case Direction.Stop:
                    break;
            }
        }

        /// <summary>
        /// 开火
        /// </summary>
        public Bullet Fire()
        {
            //子弹没有Stop状态
            Direction bulletDir = this.Dir;
            if (this.Dir == Direction.Stop)
            {
                bulletDir = this.DirOld;
            }
            Bullet bullet = new Bullet(Name, bulletId++, X + size.Width / 2 - Bullet.size.Width / 2,
                Y + size.Height / 2 - Bullet.size.Height / 2, bulletDir);
            return bullet;
        }

        /// <summary>
        /// 画出坦克
        /// </summary>
        /// <param name="g"></param>
        public void Draw(Graphics g)
        {
            switch (Dir)
            {
                case Direction.Up:
                    bmpTankNow = bmpTankAll[Level][0];
                    break;
                case Direction.Down:
                    bmpTankNow = bmpTankAll[Level][2];
                    break;
                case Direction.Left:
                    bmpTankNow = bmpTankAll[Level][3];
                    break;
                case Direction.Right:
                    bmpTankNow = bmpTankAll[Level][1];
                    break;
                case Direction.Stop:
                    bmpTankNow = bmpTankOld;
                    break;
            }
            bmpTankOld = bmpTankNow;
            g.DrawString(Name, new Font(new FontFamily("宋体"), 8), Brushes.Silver,
                new PointF((float)(X + size.Width / 2 - Name.Length * 3), Y - 15));
            g.DrawImage(bmpTankNow, X, Y, size.Width, size.Height);
        }

        /// <summary>
        /// 得到碰撞面积
        /// </summary>
        /// <returns></returns>
        public Rectangle GetRectangle()
        {
            Rectangle rec = new Rectangle(new Point(X, Y), size);
            return rec;
        }
    }
}
