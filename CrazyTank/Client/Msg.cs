using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace CrazyTank
{
    public enum MsgType
    {
        None,TankNew,TankMove,BulletNew
    }

    public interface Msg
    {
        /// <summary>
        /// 消息发送
        /// </summary>
        /// <param name="uc">udp数据包发送对象</param>
        /// <param name="ip">服务器ip地址</param>
        /// <param name="udpPort">服务器的upd端口</param>
        void Send(UdpClient uc, string ip, int udpPort);

        /// <summary>
        /// 消息解析
        /// </summary>
        /// <param name="b">要解析的字节数组</param>
        void Parse(byte[] b);
    }
}
