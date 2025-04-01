using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Threading;

public class TCP_Client
{

    static private Socket socketSend;                   //客户端套接字，用来链接远端服务器

    //建立链接
    static public void ConnectToServer()
    {
        try
        {
            socketSend = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            socketSend.Connect(new IPEndPoint(ip, 7878));
            Debug.Log("连接成功!");
        }
        catch { }
    }

    /// <summary>
    /// 接收服务端返回的消息
    /// </summary>
    //void Received()
    //{
    //    while (true)
    //    {
    //        try
    //        {
    //            byte[] buffer = new byte[1024 * 6];
    //            int len = socketSend.Receive(buffer);
    //            if (len == 0)
    //            {
    //                break;
    //            }

    //            recMes = Encoding.UTF8.GetString(buffer, 0, len);

    //            Debug.Log("客户端接收到的数据 ： " + recMes);

    //            recTimes++;
    //            staInfo = "接收到一次数据，接收次数为 ：" + recTimes;
    //            Debug.Log("接收次数为：" + recTimes);
    //        }
    //        catch { }
    //    }
    //}

    static public string SendMessage(string message)
    {

        // 发送数据
        {
            byte[] buffer = new byte[1024 * 8];
            buffer = Encoding.UTF8.GetBytes(message);
            socketSend.Send(buffer);
        }
        // 接受结果
        {
            byte[] buffer = new byte[1024 * 6];
            int len = socketSend.Receive(buffer);
            if (len == 0)
                return "";

            string recMes = Encoding.UTF8.GetString(buffer, 0, len);
            Debug.Log("收到数据:" + recMes);
            return recMes;
        }
    }
}
