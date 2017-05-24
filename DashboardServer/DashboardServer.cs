﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Net;
using System.Text;
using R3E;
using WebSocketSharp.Server;
using WebSocketSharp;

namespace DashboardServer
{
    public class Data : WebSocketBehavior
    {
        private Sample r3e = new Sample();

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
            } catch (Exception)
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
        public static Collector Listener
        {
            get;
            private set;
        }

        public static void Main(string[] args)
        {
            // Start listener as separate thread.
            Listener = new Collector();
            Thread newThread = new Thread(new ThreadStart(Listener.Run));
            newThread.Start();

            // Start web socket server.
            var wssv = new WebSocketServer("ws://localhost:8080");
            wssv.AddWebSocketService<Data>("/");
            wssv.Start();
            
            Console.ReadKey(true);
            wssv.Stop();
        }
    }
}