using System;
using System.Threading;
using WebSocketSharp.Server;
using WebSocketSharp;
using DashboardCore;

namespace DashboardServer
{
    public class Data : WebSocketBehavior
    {
        protected override void OnOpen()
        {
            Console.WriteLine("Welcoming new client with IP " + Context.UserEndPoint + ".");
            DashboardServer.Listener.Update += SendDataToClient;
        }

        private void SendDataToClient(ExchangeData data)
        {
            try
            {
                Send(data.ToJSON());
            }
            catch (Exception)
            {
                // Remove listener if connection was lost.
                if (State != WebSocketSharp.WebSocketState.Open)
                {
                    DashboardServer.Listener.Update -= SendDataToClient;
                }
            }
        }

        protected override void OnClose(CloseEventArgs e)
        {
            DashboardServer.Listener.Update -= SendDataToClient;
            base.OnClose(e);
        }

        protected override void OnError(ErrorEventArgs e)
        {
            DashboardServer.Listener.Update -= SendDataToClient;
            base.OnError(e);
        }
    }

    class DashboardServer
    {
        public static GameDataCollector Listener = new DashboardCore.GameDataCollector();

        public static void Main(string[] args)
        {
            // Start listener as separate thread.
            Listener = new GameDataCollector();
            Thread listenerThread = new Thread(new ThreadStart(Listener.Run));
            listenerThread.Start();

            // Start web socket server.
            var wssv = new WebSocketServer("ws://localhost:8080");
            wssv.AddWebSocketService<Data>("/");
            wssv.Start();
            
            Console.ReadKey(true);
            wssv.Stop();
        }
    }
}