using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace WinAutoMessenger
{
    public class ConnectionHelper
    {
        public delegate void Connected(ConnectionHelper helper);


        private TcpListener m_tcp = null;       //our server
        private TcpClient m_client = null;      //our connected socket/client.... we need only one
        private int m_port = 0;        //this is our port

        

        public int Port { get { return m_port; } }
        public bool IsConnected { get { return m_client != null ? m_client.Connected : false; } }
        public int DataAvailable { get { return m_client != null? m_client.Available : 0; } }
        public bool IsListening { get; private set; }
        public Stream ConnectionStream { get; private set; }



        public ConnectionHelper()
        {
            
        }
        ~ConnectionHelper()
        {
            this.Close();
        }

        public override string ToString()
        {
            return $"[port:{Port}]";
        }
        public enum ConnectionType
        {
            TCP,
            UDP,
        };
        public void Start(int port, ConnectionType type = ConnectionType.TCP)
        {
            if (m_tcp != null && m_client != null && m_client.Connected)
                return;

            this.Close();

            if (type != ConnectionType.TCP)
                throw new NotSupportedException("only tcp connection is supported!");


            m_port = port;

            IsListening = true;
            //IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPAddress ipAddress = IPAddress.Any;
            m_tcp = new TcpListener(ipAddress, Port);
            m_tcp.Start();
            try
            {
                m_client = m_tcp.AcceptTcpClient();
                ConnectionStream = m_client.GetStream();
            }
            catch(System.Net.Sockets.SocketException)
            {

            }
            
            IsListening = false;
        }
        public void Close()
        {
            if(m_tcp != null)
            {
                if(m_client != null)
                    m_client.Close();

                m_tcp.Stop();
                m_tcp = null;
                m_client = null;
            }
        }

        public byte[] Receive()
        {
            if(this.IsConnected)
            {
                int count = m_client.Available;
                if(count > 0)
                {
                    byte[] buffer = new byte[count];
                    ConnectionStream.Read(buffer, 0, count);
                    return buffer;
                }
            }
            return null;
        }
        public bool Send(byte[] buffer, int off, int count)
        {
            if (this.IsConnected)
            {
                ConnectionStream.Write(buffer, off, count);
                return true;
            }
            return false;
        }
        public bool Send(byte[] buffer) => Send(buffer, 0, buffer.Length);

        public String ReceiveString(StringHelper.TextType type = StringHelper.TextType.Default)
        {
            byte[] buffer = Receive();
            if (buffer != null)
            {
                return StringHelper.BytesToString(buffer, type);
            }
            return null;
        }
        public bool SendString(String buffer, StringHelper.TextType type = StringHelper.TextType.Default)
        {
            if (String.IsNullOrEmpty(buffer) == false)
                return Send(StringHelper.StringToBytes(buffer, type));
            return false;
        }

    }
}
