using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class CommandHandler
{
    public static void UpdatePos(MoveData data, Client client)
    {
        Console.WriteLine($"处理位置更新: {data.x}, {data.y}, {data.z}");
        // 业务逻辑...
    }
}
