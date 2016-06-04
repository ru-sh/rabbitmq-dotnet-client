﻿using System;

using System.Net.Sockets;
using System.Threading.Tasks;

namespace RabbitMQ.Client
{
    /// <summary>
    /// Wrapper interface for standard TCP-client. Provides socket for socket frame handler class. 
    /// </summary>
    /// <remarks>Contains all methods that are currenty in use in rabbitmq client.</remarks>
    public interface ITcpClient
    {
        bool Connected { get; }

        int ReceiveTimeout { get; set; }

#if !CORECLR
        Socket Client { get; set; }
#endif

        Task ConnectAsync(string host, int port);

        NetworkStream GetStream();

        void Close();
    }
}
