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

    static private Socket socketSend;                   //�ͻ����׽��֣���������Զ�˷�����

    //��������
    static public void ConnectToServer()
    {
        try
        {
            socketSend = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            socketSend.Connect(new IPEndPoint(ip, 7878));
            Debug.Log("���ӳɹ�!");
        }
        catch { }
    }

    /// <summary>
    /// ���շ���˷��ص���Ϣ
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

    //            Debug.Log("�ͻ��˽��յ������� �� " + recMes);

    //            recTimes++;
    //            staInfo = "���յ�һ�����ݣ����մ���Ϊ ��" + recTimes;
    //            Debug.Log("���մ���Ϊ��" + recTimes);
    //        }
    //        catch { }
    //    }
    //}

    static public string SendMessage(string message)
    {

        // ��������
        {
            byte[] buffer = new byte[1024 * 8];
            buffer = Encoding.UTF8.GetBytes(message);
            socketSend.Send(buffer);
        }
        // ���ܽ��
        {
            byte[] buffer = new byte[1024 * 6];
            int len = socketSend.Receive(buffer);
            if (len == 0)
                return "";

            string recMes = Encoding.UTF8.GetString(buffer, 0, len);
            Debug.Log("�յ�����:" + recMes);
            return recMes;
        }
    }
}
