using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace CrazyTank
{
    public class Controller
    {
        public Tank myTank;
        public List<Tank> tanks = new List<Tank>();
        public List<Bullet> bullets = new List<Bullet>();
        public List<Explode> explodes = new List<Explode>();
        public Tile[,] map;

        private Size tis = Tile.size;

        private int gameWidth, gameHeight, mapWidth, mapHeight;
        private bool dirUp;
        private bool dirDown;
        private bool dirLeft;
        private bool dirRight;
        private Direction dirOld;
        public NetClient nc;

        public Controller(int gameWidth, int gameHeight)
        {
            this.gameWidth = gameWidth;
            this.gameHeight = gameHeight;
            this.mapWidth = gameWidth / tis.Width;
            this.mapHeight = gameHeight / tis.Height;
            map = new Tile[mapWidth, mapHeight];



            LoadMap(Application.StartupPath + "//Map//map_1.txt");
        }

        private void LoadMap(string path)
        {
            StreamReader sr = new StreamReader(path);
            int j = 0;
            TileType tmpTileType;
            while (!sr.EndOfStream)
            {
                string[] items = sr.ReadLine().Split(' ');
                for (int i = 0; i < items.Length; i++)
                {
                    tmpTileType = (TileType)Convert.ToInt32(items[i]);
                    if (tmpTileType != TileType.None)
                    {
                        map[i, j] = new Tile(i * tis.Width, j * tis.Width, tmpTileType);
                    }
                }
                j++;
            }
        }

        public bool NetClientConnect(string ip, int port, float[] tankColor)
        {
            //创建我的坦克
            Random r = new Random();
            int rNum = r.Next(100, 999);
            myTank = new Tank("hehe" + rNum.ToString(), r.Next(0, gameWidth), r.Next(0, gameHeight), Direction.Up, tankColor);
            //连接服务器
            nc = new NetClient(this);
            if (nc.Connect(ip, port))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Paint(Graphics g)
        {
            //信息
            if (myTank != null)
            {
                g.DrawString("MyTank      Life:  " + myTank.Life, new Font("宋体", 8), new SolidBrush(Color.White), 20, 20);
                g.DrawString("Bullets     Count: " + bullets.Count, new Font("宋体", 8), new SolidBrush(Color.White), 20, 35);
                g.DrawString("EnemyTanks  Count: " + tanks.Count, new Font("宋体", 8), new SolidBrush(Color.White), 20, 50);
            }
            //物块
            for (int i = 0; i < mapWidth; i++)
            {
                for (int j = 0; j < mapHeight; j++)
                {
                    if (map[i, j] != null && map[i, j].Type == TileType.Water)
                    {
                        map[i, j].Draw(g);
                    }
                }
            }
            //子弹
            for (int i = 0; i < bullets.Count; i++)
            {
                if (bullets[i].Life > 0)
                {
                    bullets[i].Draw(g);
                    BulletMove(bullets[i]);
                }
            }
            //其他人的坦克
            for (int i = 0; i < tanks.Count; i++)
            {
                if (tanks[i].Life > 0)
                {
                    tanks[i].Draw(g);
                    tanks[i].Move();
                }
            }
            //自己的坦克
            if (myTank != null && myTank.Life > 0)
            {
                myTank.Draw(g);
                TankMove(myTank);
            }
            //物块
            for (int i = 0; i < mapWidth; i++)
            {
                for (int j = 0; j < mapHeight; j++)
                {
                    if (map[i, j] != null && map[i, j].Type != TileType.Water)
                    {
                        map[i, j].Draw(g);
                    }
                }
            }
            //爆炸效果
            for (int i = 0; i < explodes.Count; i++)
            {
                if (explodes[i].Live == true)
                {
                    explodes[i].Draw(g);
                }
                else
                {
                    explodes.Remove(explodes[i]);
                    i--;
                }
            }
        }

        private void TankMove(Tank tank)
        {
            dirOld = tank.Dir;
            if (tank.Dir != Direction.Stop)
            {
                tank.DirOld = tank.Dir;
            }

            if (dirUp)
            {
                tank.Dir = Direction.Up;
                tank.Move();
            }
            if (dirDown)
            {
                tank.Dir = Direction.Down;
                tank.Move();
            }
            if (dirLeft)
            {
                tank.Dir = Direction.Left;
                tank.Move();
            }
            if (dirRight)
            {
                tank.Dir = Direction.Right;
                tank.Move();
            }
            if (!dirUp && !dirDown && !dirLeft && !dirRight)
            {
                tank.Dir = Direction.Stop;
            }
            //与边界的碰撞检测
            if (tank.X < 0 || tank.Y < 0 || tank.X > gameWidth - Tank.size.Width || tank.Y > gameHeight - Tank.size.Height)
            {
                myTank.X = myTank.XOld;
                myTank.Y = myTank.YOld;
                tank.Dir = Direction.Stop;
            }
            //与坦克的碰撞检测
            for (int i = 0; i < tanks.Count; i++)
            {
                if (myTank != null && CollisionDetection(tanks[i].GetRectangle(), myTank.GetRectangle()))
                {
                    myTank.X = myTank.XOld;
                    myTank.Y = myTank.YOld;
                    tank.Dir = Direction.Stop;
                }
            }
            //与地图物块的碰撞检测
            int tmpX = myTank.X / tis.Width;
            int tmpY = myTank.Y / tis.Height;
            for (int i = tmpX - 1; i < tmpX + 3; i++)
            {
                for (int j = tmpY - 1; j < tmpY + 3; j++)
                {
                    if (i < 0 || j < 0 || i >= mapWidth || j >= mapHeight)
                    {
                        continue;
                    }
                    if (map[i, j] != null)
                    {
                        if (CollisionDetection(map[i, j].GetRectangle(), myTank.GetRectangle()))
                        {
                            myTank.X = myTank.XOld;
                            myTank.Y = myTank.YOld;
                            tank.Dir = Direction.Stop;
                        }
                    }
                }
            }
            //改变方向时发包
            if (nc != null && tank.Dir != dirOld)
            {
                TankMoveMsg msg = new TankMoveMsg(tank.Name, tank.X, tank.Y, tank.Dir);
                nc.Send(msg);
            }
        }

        public void BulletMove(Bullet bullet)
        {
            #region 子弹与坦克相撞
            for (int i = 0; i < tanks.Count; i++)
            {
                if (CollisionDetection(tanks[i].GetRectangle(), bullet.GetRectangle())
                    && tanks[i].Name != bullet.FromName)
                {
                    Explode e = new Explode(tanks[i].X, tanks[i].Y);
                    explodes.Add(e);

                    tanks[i].Life -= 1;
                    tanks.Remove(tanks[i]);
                    bullets.Remove(bullet);
                    return;
                }
            }
            if (myTank != null && CollisionDetection(myTank.GetRectangle(), bullet.GetRectangle())
                && myTank.Name != bullet.FromName && myTank.Life > 0)
            {
                Explode e = new Explode(myTank.X, myTank.Y);
                explodes.Add(e);

                myTank.Life -= 1;
                bullets.Remove(bullet);
                return;
            }
            #endregion

            #region 子弹与子弹碰撞
            for (int i = 0; i < bullets.Count; i++)
            {
                if (CollisionDetection(bullets[i].GetRectangle(), bullet.GetRectangle()) && bullet.FromName != bullets[i].FromName)
                {
                    bullets[i].Life = 0;
                    bullets.Remove(bullets[i]);
                    bullet.Life = 0;
                    bullets.Remove(bullet);
                    return;
                }
            }
            #endregion

            #region 子弹与障碍物相撞
            int tmpX = bullet.X / tis.Width;
            int tmpY = bullet.Y / tis.Height;
            for (int i = tmpX - 1; i < tmpX + 2; i++)
            {
                for (int j = tmpY - 1; j < tmpY + 2; j++)
                {
                    if (i < 0 || j < 0 || i >= mapWidth || j >= mapHeight)
                    {
                        continue;
                    }
                    if (map[i, j] != null)
                    {
                        if (map[i, j].Type == TileType.Brick || map[i, j].Type == TileType.Iron)
                        {
                            if (CollisionDetection(map[i, j].GetRectangle(), bullet.GetRectangle()))
                            {
                                map[i, j] = null;

                                Explode e = new Explode(bullet.X - Explode.size.Width / 2 + Bullet.size.Width / 2,
                                    bullet.Y - Explode.size.Height / 2 + Bullet.size.Height / 2);
                                explodes.Add(e);

                                bullet.Life = 0;
                                bullets.Remove(bullet);
                                return;
                            }
                        }
                    }
                }
            }
            #endregion

            #region 子弹飞出边界
            if (bullet.X < 0 || bullet.Y < 0
                || bullet.X > gameWidth - Bullet.size.Width
                || bullet.Y > gameHeight - Bullet.size.Height)
            {
                bullet.Life = 0;
                bullets.Remove(bullet);
            }
            #endregion
        }

        #region 操作控制

        public void KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.W || e.KeyChar == (char)Keys.W + 32)
            {
                dirUp = true;
                dirDown = false;
                dirLeft = false;
                dirRight = false;
            }
            if (e.KeyChar == (char)Keys.A || e.KeyChar == (char)Keys.A + 32)
            {
                dirUp = false;
                dirDown = false;
                dirLeft = true;
                dirRight = false;
            }
            if (e.KeyChar == (char)Keys.S || e.KeyChar == (char)Keys.S + 32)
            {
                dirUp = false;
                dirDown = true;
                dirLeft = false;
                dirRight = false;
            }
            if (e.KeyChar == (char)Keys.D || e.KeyChar == (char)Keys.D + 32)
            {
                dirUp = false;
                dirDown = false;
                dirLeft = false;
                dirRight = true;
            }
        }

        public void KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A || e.KeyCode == (Keys)((char)Keys.A + 32))
            {
                dirLeft = false;
            }
            if (e.KeyCode == Keys.D || e.KeyCode == (Keys)((char)Keys.D + 32))
            {
                dirRight = false;
            }
            if (e.KeyCode == Keys.W || e.KeyCode == (Keys)((char)Keys.W + 32))
            {
                dirUp = false;
            }
            if (e.KeyCode == Keys.S || e.KeyCode == (Keys)((char)Keys.S + 32))
            {
                dirDown = false;
            }
            //发射子弹
            if (e.KeyCode == Keys.Space && nc != null)
            {
                if (myTank.Life > 0)
                {
                    Bullet b = myTank.Fire();
                    bullets.Add(b);
                    //发射子弹时发包
                    BulletNewMsg msg = new BulletNewMsg(b);
                    nc.Send(msg);
                }
            }
        }

        #endregion

        #region 碰撞检测

        private bool CollisionDetection(Rectangle rec1, Rectangle rec2)
        {
            if ((rec1.Width == 0 && rec1.Height == 0) || (rec2.Width == 0 && rec2.Height == 0))
            {
                return false;
            }
            if (rec1.X + rec1.Width > rec2.X && rec1.Y + rec1.Height > rec2.Y
                && rec1.X < rec2.X + rec2.Width && rec1.Y < rec2.Y + rec2.Height)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion
    }
}
