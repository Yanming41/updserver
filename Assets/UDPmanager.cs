using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
public class UDPmanager : MonoBehaviour
{

    class UDPData
    {
        private readonly UdpClient udpClient;
        public UdpClient UDPClient
        {
            get { return udpClient; }
        }
        private readonly IPEndPoint endPoint;
        public IPEndPoint EndPoint
        {
            get { return endPoint; }
        }
        //构造函数
        public UDPData(IPEndPoint endPoint, UdpClient udpClient)
        {
            this.endPoint = endPoint;
            this.udpClient = udpClient;
        }
    }
    private void HandleMessage(string message)
    {
        Console.WriteLine("收到消息: " + message);
    }

    string receiveData = string.Empty;
    private Action<string> ReceiveCallBack = null;
    private Thread RecviveThread;
    private void Start()
    {
        //print("开启线程");
        //开启线程
        ThreadRecvive();
    }
  
    private void Update()
    {
        if (ReceiveCallBack != null && !string.IsNullOrEmpty(receiveData))
        {
            ReceiveCallBack = HandleMessage;
            //调用处理函数对数据进行处理,RecieveCallBack参数是一个方法,所以可以对recieveData数据进行处理
            //但是这个参数方法需要自己写，在NetController里面自定义

            print("收到了！:"+receiveData+"");
            ReceiveCallBack(receiveData);
            //使用之后清空接受的数据
            receiveData = string.Empty;
        }
        else if (string.IsNullOrEmpty(receiveData))
        {
            //print("目前没收到数据");
        }
    }
    private void OnDestroy()
    {
        if (RecviveThread != null)
        {
            RecviveThread.Abort();
        }
    }
    //Action<string> action==RecieveCallBack参数是一个方法
    public void SetReceiveCallBack(Action<string> action)
    {
        ReceiveCallBack = action;
    }
    /// <summary>
    /// 开始线程接收
    /// </summary>
    private void ThreadRecvive()
    {
        print("开启线程");
        //开一个新线程接收UDP发送的数据
        RecviveThread = new Thread(() =>
        {
            //实例化一个IPEndPoint，任意IP和对应端口 端口自行修改

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("192.168.0.199"), 8001);//数据从哪传来的的地址和端口号
            UdpClient udpReceive = new UdpClient(endPoint);
            //实例化一个UdpData对象
            UDPData data = new UDPData(endPoint, udpReceive);//udp数据
            //开启异步接收data里面的数据
            udpReceive.BeginReceive(CallBackRecvive, data);//接收数据
            Debug.Log("本地监听地址: " + endPoint.Address.ToString());
            Debug.Log("本地监听端口: " + endPoint.Port.ToString());
        })
        {
            //设置为后台线程
            IsBackground = true
        };

        //开启线程
        RecviveThread.Start();
        print("开启了！！！");
    }

    /// <summary>
    /// 异步接收回调
    /// </summary>
    /// <param name="ar"></param>
    private void CallBackRecvive(IAsyncResult ar)
    {
        print("接收数据");
        try
        {
            //将传过来的异步结果转为我们需要解析的类型
            //获取udpdata里面的UDPClient参数和IPEndPoint参数
            UDPData state = ar.AsyncState as UDPData;
            IPEndPoint ipEndPoint = state.EndPoint;
            //结束异步接受 不结束会导致重复挂起线程卡死
            byte[] data = state.UDPClient.EndReceive(ar, ref ipEndPoint);
            print("接收到数据，数据长度为:" + data.Length+"数据为："+ BitConverter.ToString(data));

            //解析数据（字节流），去掉源端口目的端口和长度等，只留下数据 编码自己调整暂定为默认 依客户端传过来的编码而定
            string receiveData = Encoding.Default.GetString(data);
            Debug.Log(receiveData);
            //数据的解析再Update里执行 Unity中Thread无法调用主线程的方法
            //再次开启异步接收数据
            state.UDPClient.BeginReceive(CallBackRecvive, state);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            throw;
        }
    }
    /// <summary>
    /// 发送UDP信息
    /// </summary>
    /// <param name="remoteIP">发送地址</param>
    /// <param name="remotePort">发送端口</param>
    /// <param name="message">需要发送的信息</param>
    public void UDPSendMessage(string remoteIP, int remotePort, string message)
    {
        //将需要发送的内容转为byte数组 编码以接收端为主，自行修改
        byte[] sendbytes = Encoding.Unicode.GetBytes(message);
        IPEndPoint remoteIPEndPoint = new IPEndPoint(IPAddress.Parse(remoteIP), remotePort);
        UdpClient udpSend = new UdpClient();
        //发送数据到对应目标
        udpSend.Send(sendbytes, sendbytes.Length, remoteIPEndPoint);
        //关闭
        udpSend.Close();
    }
}

