using System.Net;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;

public class Server
{
    public Socket socket;
    public Dictionary<IPEndPoint, Client> clientMap = new Dictionary<IPEndPoint, Client>();
    private static readonly Dictionary<ushort, Type> _messageTypes = new()
    {
        { (ushort)Message.UpdatePos, typeof(MoveData) },
        // 其他消息类型在此注册
    };
    public ControllerManager controllerManager;

    private const int BufferSize = 1024;
    public Server(int port)
    {
        controllerManager = new ControllerManager();
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
        socket.Bind(ipPoint);
        Console.WriteLine("UDP服务端已开启");
        StartReceive();
    }

    void StartReceive()
    {
        var buffer = new byte[BufferSize];
        EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

        socket.BeginReceiveFrom(buffer, 0, BufferSize, SocketFlags.None, ref remoteEP, ReceiveCallback, buffer);
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            int bytesRead = socket.EndReceiveFrom(ar, ref remoteEP);
            var clientEP = (IPEndPoint)remoteEP;
            byte[] buffer = (byte[])ar.AsyncState;

            if (bytesRead > 0)
            {
                var packet = new byte[bytesRead];
                Buffer.BlockCopy(buffer, 0, packet, 0, bytesRead);

                if (!clientMap.TryGetValue(clientEP, out Client client))
                {
                    client = new Client(clientEP, this);
                    clientMap[clientEP] = client;
                    Console.WriteLine($"新客户端连接: {clientEP}");
                }

                controllerManager.HandleRequest(packet, client);
            }

            StartReceive();
        }
        catch (ObjectDisposedException)
        {
            // 正常关闭时的异常
        }
        catch (Exception ex)
        {
            Console.WriteLine($"接收异常: {ex.Message}");
            StartReceive(); // 继续接收即使出错
        }
    }

    public void Send<T>(IPEndPoint clientEP, ProtocolPackage<T> package) where T : class, INetworkData
    {
        if (clientMap.TryGetValue(clientEP, out Client client))
        {
            client.Send(package);
        }
    }

    // 动态发送方法（根据消息ID自动匹配类型）
    public void SendDynamic(IPEndPoint clientEP, ushort messageId, INetworkData data)
    {
        if (!_messageTypes.TryGetValue(messageId, out Type dataType))
            throw new ArgumentException("未注册的消息类型");

        // 反射创建 ProtocolPackage<T>
        var packageType = typeof(ProtocolPackage<>).MakeGenericType(dataType);
        dynamic package = Activator.CreateInstance(packageType, messageId, 0, 0, data);

        Send(clientEP, package);
    }

    public void Close()
    {
        socket.Shutdown(SocketShutdown.Both);
        socket?.Close();
        Console.WriteLine("UDP服务端已关闭");
    }
}
