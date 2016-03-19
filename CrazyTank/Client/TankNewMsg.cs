using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace CrazyTank
{
    class TankNewMsg : Msg
    {
        private MsgType msgType = MsgType.TankNew;
        private Tank tank;
        private Controller ctrl;

        /// <summary>
        /// 构造函数 用于发包
        /// </summary>
        /// <param name="tank"></param>
        public TankNewMsg(Tank tank)
        {
            this.tank = tank;
        }

        /// <summary>
        /// 构造函数 用于解包
        /// </summary>
        /// <param name="ctrl"></param>
        public TankNewMsg(Controller ctrl)
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
            //包中用"|"来分割发送的内容
            string str = (int)msgType + "|" + tank.Name + "|" + tank.X + "|" + tank.Y + "|" + (int)tank.Dir 
                + "|" + tank.Color[0] + "|" + tank.Color[1] + "|" + tank.Color[2];
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
            //如果数据包里是自己的坦克,则不处理
            if (name == ctrl.myTank.Name)
            {
                return;
            }
            int x = Convert.ToInt32(strs[2]);
            int y = Convert.ToInt32(strs[3]);
            Direction dir = (Direction)Convert.ToInt32(strs[4]);
            float[] color = new float[3];
            color[0] = Convert.ToSingle(strs[5]);
            color[1] = Convert.ToSingle(strs[6]);
            color[2] = Convert.ToSingle(strs[7]);

            bool exist = false;
            for (int i = 0; i < ctrl.tanks.Count; i++)
            {
                Tank t = ctrl.tanks[i];
                if (t.Name == name)
                {
                    exist = true;
                    break;
                }
            }
            //如果坦克不存在就创建出来
            if (!exist)
            {
                TankNewMsg msg = new TankNewMsg(ctrl.myTank);
                ctrl.nc.Send(msg);
                Tank t = new Tank(name, x, y, dir, color);
                t.Name = name;
                ctrl.tanks.Add(t);
            }
        }
    }
}
