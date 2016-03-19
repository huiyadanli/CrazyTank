using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace CrazyTank
{
    class TankMoveMsg : Msg
    {
        private MsgType msgType = MsgType.TankMove;
        private string name;
        private int x, y;
        private Direction dir;
        private Controller ctrl;

        /// <summary>
        /// 构造函数 用于发包
        /// </summary>
        /// <param name="name"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="dir"></param>
        public TankMoveMsg(string name, int x, int y, Direction dir)
        {
            this.name = name;
            this.dir = dir;
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// 构造函数 用于解包
        /// </summary>
        /// <param name="ctrl"></param>
        public TankMoveMsg(Controller ctrl)
        {
            this.ctrl = ctrl;
        }

        /// <summary>
        /// 发包
        /// </summary>
        /// <param name="uc"></param>
        /// <param name="ip"></param>
        /// <param name="udpPort"></param>
        public void Send(UdpClient uc, string ip, int udpPort)
        {
            uc.Connect(ip, udpPort);
            //程序中用"|"来分割发送的内容
            string str = (int)msgType + "|" + name + "|" + x + "|" + y + "|" + (int)dir;
            uc.Send(Encoding.UTF32.GetBytes(str), Encoding.UTF32.GetBytes(str).Length);
        }

        /// <summary>
        /// 解包
        /// </summary>
        /// <param name="b">二进制数据</param>
        public void Parse(byte[] b)
        {
            string str = Encoding.UTF32.GetString(b);
            string[] strs = str.Split('|');
            string name = strs[1];
            //如果数据包里是自己的坦克不处理
            if (name == ctrl.myTank.Name)
            {
                return;
            }
            int x = Convert.ToInt32(strs[2]);
            int y = Convert.ToInt32(strs[3]);
            Direction dir = (Direction)Convert.ToInt32(strs[4]);
            for (int i = 0; i < ctrl.tanks.Count; i++)
            {
                Tank t = ctrl.tanks[i];
                if (t.Name == name)
                {
                    t.Dir = dir;
                    t.X = x;
                    t.Y = y;
                    break;
                }
            }
        }
    }
}
