using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System.Threading;

namespace TankServer
{
    class Program
    {
        public static int tcpPort = 7776; //TCP端口
        public static int udpPort = 7777; //UDP端口

        /// <summary>
        /// 客户端 列表
        /// </summary>
        List<Client> clients = new List<Client>();

        /// <summary>
        /// 开启tcp udp服务 函数
        /// </summary>
        public void Start()
        {
            try
            {
                //开启udp线程
                Thread t = new Thread(UDPThread);
                t.IsBackground = true;
                t.Start();
                //开启tcp服务
                Console.WriteLine("TCP port :" + tcpPort);
                TcpListener tcpListener = new TcpListener(IPAddress.Any, tcpPort);
                tcpListener.Start();
                while (true)
                {
                    TcpClient client = tcpListener.AcceptTcpClient();
                    Stream ns = client.GetStream();
                    BinaryReader br = new BinaryReader(ns);
                    int udpPort = br.ReadInt32();//br.Close();不能关闭br
                    //BinaryWriter bw = new BinaryWriter(ns);
                    //bw.Write(ID++);
                    IPEndPoint rep = (IPEndPoint)client.Client.RemoteEndPoint;
                    Client c = new Client(rep.Address.ToString(), udpPort);
                    clients.Add(c);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("A Client TCP Connect! Addr- " + rep.Address.ToString() + ":" + rep.Port);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error:" + ex.Message);
            }
        }

        static void Main(string[] args)
        {
            new Program().Start();
        }

        /// <summary>
        /// Client类
        /// </summary>
        private class Client
        {
            public string ip;
            public int udpPort;
            public Client(string ip, int udpPort)
            {
                this.ip = ip;
                this.udpPort = udpPort;
            }
        }

        /// <summary>
        /// Udp数据包接收函数
        /// </summary>
        private void UDPThread()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("UDP thread started at port :" + udpPort);
            byte[] buf = new byte[1024];
            UdpClient uc = new UdpClient(udpPort);
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                buf = uc.Receive(ref ipep);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("a udp packet received! from " + ipep.Address + ":" + ipep.Port);
                //把收到的数据转发给每一个客户端
                for (int i = 0; i < clients.Count; i++)
                {
                    Client c = clients[i];
                    UdpClient _uc = new UdpClient();
                    _uc.Connect(c.ip, c.udpPort);
                    _uc.Send(buf, buf.Length);
                }
            }
        }
    }
}
