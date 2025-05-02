using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

// 所有可传输数据的基接口
public interface INetworkData { }

// 基础协议包泛型类
public class ProtocolPackage<T> where T : class, INetworkData
{
    public byte[]? package;
    public ushort messageId;
    public uint sequenceId;
    public ushort packetIndex;
    public ushort packetLength;
    public T? bodyData;

    public ProtocolPackage(byte[] bytes) => NonSerialized(bytes);

    public ProtocolPackage(ushort msgId, uint seqId, ushort pktIndex,ushort pktLength, T data)
    {
        messageId = msgId;
        sequenceId = seqId;
        packetIndex = pktIndex;
        packetLength = pktLength;
        bodyData = data;
        Serialized();
    }

    private void Serialized()
    {
        using (var ms = new MemoryStream())
        using (var writer = new BinaryWriter(ms))
        {
            // 协议头
            writer.Write((ushort)0x55AA);
            writer.Write(messageId);
            writer.Write(sequenceId);
            writer.Write(packetIndex);

            // 序列化数据体
            string json = JsonSerializer.Serialize(bodyData);
            byte[] bodyBytes = Encoding.UTF8.GetBytes(json);
            writer.Write((ushort)bodyBytes.Length); // 数据体长度
            writer.Write(bodyBytes);

            package = ms.ToArray();
        }
    }

    private void NonSerialized(byte[] bytes)
    {
        if (bytes.Length < 12) return;

        // 读取协议头
        ushort magic = BitConverter.ToUInt16(bytes, 0);
        if (magic != 0x55AA) return;

        messageId = BitConverter.ToUInt16(bytes, 2);
        sequenceId = BitConverter.ToUInt32(bytes, 4);
        packetIndex = BitConverter.ToUInt16(bytes, 8);
        ushort bodyLength = BitConverter.ToUInt16(bytes, 10);

        // 读取数据体
        string json = Encoding.UTF8.GetString(bytes, 12, bodyLength);
        bodyData = JsonSerializer.Deserialize<T>(json);
    }
}

public enum Message
{
    UpdatePos = 0,
}
