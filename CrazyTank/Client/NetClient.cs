using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System.Threading;
using System.Diagnostics;


namespace CrazyTank
{
    public class NetClient
    {
        private int udpPort; //客户端upd端口
        private UdpClient uc; //发送数据的udp对象
        private UdpClient _uc; //接收数据的upd对象
        private Random r = new Random(); //生成随机数
        private string ip; //服务器ip
        private Controller ctrl;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ctrl"></param>
        public NetClient(Controller ctrl)
        {
            //生成一个随机数作为客户端的udp端口
            udpPort = r.Next(9000, 10000);
            _uc = new UdpClient(udpPort);
            uc = new UdpClient();
            this.ctrl = ctrl;
        }

        /// <summary>
        /// 连接服务器并开启udp接受线程
        /// </summary>
        /// <param name="ip">服务器ip地址</param>
        /// <param name="port">服务器端口号</param>
        public bool Connect(string ip, int port)
        {
            try
            {
                this.ip = ip;
                TcpClient client = new TcpClient();
                client.Connect(ip, port);
                Stream ns = client.GetStream();
                BinaryWriter bw = new BinaryWriter(ns);
                bw.Write(udpPort);
                ns.Close();
                client.Close();
                TankNewMsg msg = new TankNewMsg(ctrl.myTank);
                Send(msg);
                //开启接收线程
                Thread t = new Thread(UDPRecvThread);
                t.IsBackground = true;
                t.Start();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 给服务器发送udp数据包
        /// </summary>
        /// <param name="msg"></param>
        public void Send(Msg msg)
        {
            msg.Send(uc, ip, 7777); //接收端口7777
        }

        /// <summary>
        /// 客户端tcp接收函数
        /// </summary>
        private void TCPRecvThread()
        {
            byte[] buf = new byte[1024];
            while (true)
            {
                IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 0);
                buf = _uc.Receive(ref ipep);
                Parse(buf);
            }
        }

        /// <summary>
        /// 客户端udp接收函数
        /// </summary>
        private void UDPRecvThread()
        {
            byte[] buf = new byte[1024];
            while (true)
            {
                IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 0);
                buf = _uc.Receive(ref ipep);
                Parse(buf);
            }
        }

        /// <summary>
        /// udp数据包解析函数
        /// </summary>
        /// <param name="buf">字节数组</param>
        private void Parse(byte[] buf)
        {
            MsgType msgType = MsgType.None;
            Msg msg = null;
            string str = Encoding.UTF32.GetString(buf);
            string[] strs = str.Split('|');
            msgType = (MsgType)Convert.ToInt32(strs[0]);
            //通过不同的消息类型，对消息进行解析
            switch (msgType)
            {
                case MsgType.TankNew:
                    msg = new TankNewMsg(ctrl);
                    msg.Parse(buf);
                    break;
                case MsgType.TankMove:
                    msg = new TankMoveMsg(ctrl);
                    msg.Parse(buf);
                    break;
                case MsgType.BulletNew:
                    msg = new BulletNewMsg(ctrl);
                    msg.Parse(buf);
                    break;
            }
        }
    }
}
