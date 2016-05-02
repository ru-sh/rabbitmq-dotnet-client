using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ.Client
{


    /// <summary>
    /// Simple wrapper around TcpClient. 
    /// </summary>
    public class TcpClientAdapter : ITcpClient
    {
        protected TcpClient _tcpClient;


        public TcpClientAdapter(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
        }

        public virtual Task ConnectAsync(string host, int port)
        {
            assertTcpClient();

            return _tcpClient.ConnectAsync(host, port);
        }
        
        private void assertTcpClient()
        {
            if (_tcpClient == null)
                throw new InvalidOperationException("Field tcpClient is null. Should have been passed to constructor.");
        }
        
        public virtual void Close()
        {
            assertTcpClient();

#if CORECLR
            _tcpClient.Dispose();
#else
            _tcpClient.Close();
#endif
        }

        public virtual NetworkStream GetStream()
        {
            assertTcpClient();

            return _tcpClient.GetStream();
        }

        public virtual Socket Client
        {
            get
            {
                assertTcpClient();

                return _tcpClient.Client;
            }
            set
            {
                _tcpClient.Client = value;
            }
        }

        public virtual bool Connected
        {
            get { return _tcpClient!=null && _tcpClient.Connected; }
        }

        public virtual int ReceiveTimeout
        {
            get
            {
                return _tcpClient.ReceiveTimeout;
            }
            set
            {
                _tcpClient.ReceiveTimeout = value;
            }
        }
    }
}
