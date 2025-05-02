using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

public class ControllerManager
{
    private readonly Dictionary<ushort, Type> _messageHandlers = new()
    {
        { (ushort)Message.UpdatePos, typeof(MoveData) },
    };

    public void HandleRequest(byte[] rawData, Client client)
    {
        // 1. 动态解析消息类型
        ushort msgId = BitConverter.ToUInt16(rawData, 2);
        if (!_messageHandlers.TryGetValue(msgId, out Type? dataType))
        {
            Console.WriteLine($"未知消息类型: {msgId}");
            return;
        }

        // 2. 动态创建协议包实例
        var packageType = typeof(ProtocolPackage<>).MakeGenericType(dataType);
        dynamic package = Activator.CreateInstance(packageType, rawData);

        // 3. 调用对应处理方法
        string methodName = Enum.GetName(typeof(Message), msgId);
        MethodInfo? method = typeof(CommandHandler).GetMethod(methodName);
        method?.Invoke(null, new object[] { package.bodyData, client });
    }
}
