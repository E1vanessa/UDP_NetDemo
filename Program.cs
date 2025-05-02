using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
class Program
{
    static void Main(string[] args)
    {
        Server server = new Server(9999);
        
        Console.ReadKey();
    }
}