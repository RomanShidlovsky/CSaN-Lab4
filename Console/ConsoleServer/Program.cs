using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;


namespace ConsoleServer
{
    class Program
    {
        const int port = 25;
        static TcpListener listener;
        
        static void Main(string[] args)
        {
            try
            {
                listener = new TcpListener(IPAddress.Parse("172.20.10.2"), port);
                listener.Start();
                Console.WriteLine("Waiting for connection...");

                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    ClientObject clientObject = new ClientObject(client);

                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message); 
            }
            finally
            {
                if (listener != null)
                    listener.Stop();
            }
        }
    }
}
