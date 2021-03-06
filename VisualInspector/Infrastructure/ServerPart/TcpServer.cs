﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using NLog;

namespace VisualInspector.Infrastructure.ServerPart
{
    class ClientMsgEventArgs : EventArgs
	{
        public string Message { get; set; }
        public ClientMsgEventArgs(string message)
        {
            Message = message;
        }
    }

    class TcpServer
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

        TcpListener server = null;

        private SynchronizationContext synchContext;
        public int Port { get; set; }
        public string ListenAddress { get; set; }

        private bool isRunning;

        public event EventHandler<ClientMsgEventArgs> MessageRecieved;

        private void OnMessageRecieved(string message)
        {
			var handler = MessageRecieved;
			if(handler != null)
				handler(this, new ClientMsgEventArgs(message));
        }

        public TcpServer(SynchronizationContext context)
        {
            isRunning = false;
            synchContext = context;

        }
        public void Stop()
        {
			logger.Info("TcpListener has been stopped");
            isRunning = false;
            server.Stop();
        }

        public void Start()
		{
            try
			{
                isRunning = true;
                var maxThreadsCount = Environment.ProcessorCount * 2;
                ThreadPool.SetMaxThreads(maxThreadsCount, maxThreadsCount);
                ThreadPool.SetMinThreads(2, 2);
                int port = 3010;
                var localAddr = IPAddress.Parse("0.0.0.0");
                server = new TcpListener(localAddr, port);
                server.Start();
				logger.Info("TcpListener has been started");
                int counter = 0;
                while (isRunning)
                {
                    ThreadPool.QueueUserWorkItem(ProcessingQuery, server.AcceptTcpClient());
                    counter++;
                }
            }
            catch (SocketException ex)
			{
				//TODO try to start server again in case of error
				logger.Error("Error in TcpListener has occured: {0}", ex.Message);
            }
        }

        private void ProcessingQuery(object clientObj)
        {
            var bytes = new byte[256];
            string data = null;
            Thread.Sleep(1000);
            var client = clientObj as TcpClient;
            var stream = client.GetStream();
            int i;
            while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                var msg = System.Text.Encoding.ASCII.GetBytes(data + " recieved at " + DateTime.Now.TimeOfDay);
                stream.Write(msg, 0, msg.Length);
                synchContext.Send(delegate
                {
                    OnMessageRecieved(data);
                }, null);
                /*string html = "<html><body><h1>It works!</h1></body></html>";
                // Необходимые заголовки: ответ сервера, тип и длина содержимого. После двух пустых строк - само содержимое
                string str = "HTTP/1.1 200 OK\nContent-type: text/html\nContent-Length:" + html.Length.ToString() + "\n\n" + html;
                byte[] htmlMsg = System.Text.Encoding.ASCII.GetBytes(str);
                stream.Write(htmlMsg, 0, htmlMsg.Length);*/
            }
            stream.Close();
            client.Close();
        }

        ~TcpServer()
        {
            Stop();
        }

    }
}
