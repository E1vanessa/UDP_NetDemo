using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class Client
{
    public IPEndPoint EndPoint { get; }
    private readonly Server _server;

    public Client(IPEndPoint endPoint, Server server)
    {
        EndPoint = endPoint;
        _server = server;
    }

    public void Send<T>(ProtocolPackage<T> pack) where T : class, INetworkData
    {
        if (pack.package == null) return;
        try
        {
            _server.socket.BeginSendTo(pack.package, 0, pack.package.Length, SocketFlags.None,
                EndPoint, SendCallback, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"发送到 {EndPoint} 失败: {ex.Message}");
            _server.clientMap.Remove(EndPoint);
        }
    }

    private void SendCallback(IAsyncResult ar)
    {
        try
        {
            _server.socket.EndSendTo(ar);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"发送回调异常: {ex.Message}");
        }
    }
}
