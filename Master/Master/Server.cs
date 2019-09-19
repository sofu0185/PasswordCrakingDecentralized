﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Master
{
    using System.IO;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    public static class Server
    {
        private static Dictionary<int, Client> _clients;
        private static List<Task> _monitorTasks;
        private static Commander _commander;
        private static TcpListener _server;

        static Server()
        {
            _clients = new Dictionary<int, Client>();
            _monitorTasks = new List<Task>();            
            _commander = new Commander();
            _server = TcpListener.Create(Constants.TCP_SERVER_PORT);

            _server.Start();
        }

        public static void StartServer()
        {
            // static constructor runs before this line
            while (true)
            {
                TcpClient client = _server.AcceptTcpClient();
                Task.Run(() => HandShake(client));
            }
        }

        private static void HandShake(TcpClient client)
        {
            client.NoDelay = true;
            NetworkStream networkStream = client.GetStream();
            StreamReader streamReader = new StreamReader(networkStream);
            StreamWriter streamWriter = new StreamWriter(networkStream);
            streamWriter.AutoFlush = true;

            int clientId = _clients.Count;

            _clients.Add(clientId, new Client(client, networkStream, streamWriter, streamReader));

            _commander.Stopwatch.Start();
            _monitorTasks.Add(_commander.MonitorTaskMultiplePasswords(_clients[clientId], clientId));
        }
    }
}

