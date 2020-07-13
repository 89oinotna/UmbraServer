using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.Win32;
using ZXing;
using ZXing.QrCode;
using ZXing.Common;
using System.Security;
using System.Security.Cryptography;
using System.IO;

namespace Server
{

    public class ServerC
    {
        private bool connected = false;
        private bool started = false;
        public bool Started
        {
            get
            {
                return this.started;
            }
            set
            {
                this.started = value;
            }
        }
        private UdpClient serverUdp = new UdpClient(4513);
        private string connectedIp;
        private string data = "";
        private TcpListener listener = null;
        private TcpClient clientTcp = null;
        private NetworkStream netStream = null;

        Mouse mouse = new Mouse();

        private static byte BROADCAST = 0x00;
        private static byte ALIVE = 0x01;

        //Connection status
        public static byte CONNECTED = 0x01;
        public static byte DISCONNECTED = 0x02;
        public static byte CONNECTED_PASSWORD = 0x03;

        //connection request
        public static byte CONNECTION = 0x04;
        public static byte CONNECTION_PASSWORD = 0x05;

        //response
        public static byte CONNECTION_ERROR = 0x04;
        public static byte REQUIRE_PASSWORD = 0x05;
        public static byte WRONG_PASSWORD = 0x06;

        private string base64Key;
        private bool usingPassword=false;
        System.Security.Cryptography.AesCryptoServiceProvider crypto = new System.Security.Cryptography.AesCryptoServiceProvider();
        
       
        private IPEndPoint RemoteIp = new IPEndPoint(IPAddress.Any, 0);

        public void close()
        {
            serverUdp.Close();
            listener.Stop();
            connected = false;
            started = false;
            connectedIp = null;
            if (netStream != null)
            {
                netStream.Close();
                clientTcp.Close();
            }
        }

        public void StartListeningUdp() {
            try {
                serverUdp.BeginReceive(new AsyncCallback(recv), null);
                started = true;
                
            }
            catch (Exception e) {
                MessageBox.Show(e.Message.ToString());
            }
        }
      

        void recv(IAsyncResult res) {
            byte[] received;
            try
            {
                received = serverUdp.EndReceive(res, ref RemoteIp);
            }
            catch {
                return;
            }
           
            //controllo che sia connesso e che provenga dall'ip corretto
            if (connected && RemoteIp.Address.ToString()!=connectedIp) {
                return;
            }

            data = Encoding.UTF8.GetString(received);
            Console.WriteLine(data);

            try {
                //broadcast
                if (received[0] == BROADCAST) {
                    //if connected check if client active
                    //if (!connected || (connected && connectedIp == RemoteIp.Address.ToString()))
                    //{
                    //connected = false;
                    Console.WriteLine(RemoteIp.Address.ToString());
                    sendAlive();
                    //}
                }

                /*else if ((received[0] == CONNECTION || received[0] == CONNECTION_PASSWORD) && connected) {
                    sendMessage(RemoteIp.Address.ToString(), CONNECTION_ERROR);
                }*/

                else if ((received[0] == CONNECTED || received[0] == CONNECTED_PASSWORD) && connected)
                {
                    if (usingPassword && received[0] == CONNECTED_PASSWORD)
                    {
                        ICryptoTransform decryptor = crypto.CreateDecryptor(crypto.Key, crypto.IV);
                        var origValue = decryptor.TransformFinalBlock(received, 1, received.Length - 1);
                        mouse.mouseControl(origValue);

                    }
                    else if (!usingPassword && received[0] == CONNECTED)
                    {
                        byte[] destfoo = new byte[received.Length - 1];
                        Array.Copy(received, 1, destfoo, 0, received.Length - 1);
                        mouse.mouseControl(destfoo);
                    }
                    else {
                        //TODO: codice errore
                        sendMessage(RemoteIp.Address.ToString(), CONNECTION_ERROR);
                    }
                }

                
            }
            catch (Exception e)
            { 
                Console.Write(e.StackTrace);
                
            }

            serverUdp.BeginReceive(new AsyncCallback(recv), null);
        }

        internal void usePassword(byte[] keyGenerated, string base64Key)
        {
            //key = keyGenerated;
            this.base64Key = base64Key;
            crypto.KeySize = 128;
            crypto.BlockSize = 128;
            crypto.Key = keyGenerated;
            crypto.Mode = System.Security.Cryptography.CipherMode.CBC;
            crypto.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
            crypto.IV = Encoding.ASCII.GetBytes("1111111111111111");
            usingPassword = true;
        }
             
        public void sendAlive() {
            Byte[] sendBytes = new Byte[] { ALIVE } ; 
            sendBytes=sendBytes.Concat(Encoding.ASCII.GetBytes(":")).Concat(Encoding.ASCII.GetBytes(Environment.MachineName)).ToArray();
            UdpClient client = new UdpClient(RemoteIp.Address.ToString(), 4513);
            client.Send(sendBytes, sendBytes.Length);
            

        }

        public void sendMessage(String ip, byte b)
        {
            Byte[] sendBytes = new Byte[] { b };
            sendBytes.Concat(Encoding.ASCII.GetBytes(Environment.MachineName));
            UdpClient client = new UdpClient(ip, 4513);
            client.Send(sendBytes, sendBytes.Length);
        }

        public void StartListeningTcp() {
            Thread tcp = new Thread(
            new ThreadStart(serverTcp));
            tcp.Start();
        }

       

        public void serverTcp() {
            try
            {
                listener = new TcpListener(IPAddress.Any, 4512);
                listener.Start();
            }
            catch (SocketException e) {
                Console.WriteLine(e.ErrorCode + " " + e.Message);
                Thread.CurrentThread.Abort();
            }


            try
            {
                while (true)
                {
                    Console.Write("Waiting for connection...");
                    TcpClient client = listener.AcceptTcpClient();

                    Console.WriteLine("Connection accepted.");
                    NetworkStream ns = client.GetStream();
                    ThreadWithState tws = new ThreadWithState(client, ns, this);
                    Thread t = new Thread(new ThreadStart(tws.manageConnection));
                    t.Start();


                }
            }
            catch (ThreadAbortException e)
            {
                listener.Stop();
            }
            catch (SocketException ignore) {
            }
        }

        public class ThreadWithState {
            private TcpClient client = null;
            private NetworkStream netStream = null;
            private ServerC server;

            public ThreadWithState(TcpClient client, NetworkStream netStream, ServerC server)
            {
                this.client = client;
                this.netStream = netStream;
                this.server = server;
            }

            public void manageConnection()
            {
                byte[] rcvBuffer = new byte[1024];
                int bytesRcvd;
                //se 0 allora ha chiuso la connessione
                try
                {
                    while (Thread.CurrentThread.IsAlive) {
                        bytesRcvd=netStream.Read(rcvBuffer, 0, rcvBuffer.Length);
                        if (bytesRcvd > 0)
                        {
                            if ((rcvBuffer[0] == CONNECTION || rcvBuffer[0] == CONNECTION_PASSWORD) && !server.connected)
                            {

                                if (rcvBuffer[0] == CONNECTION_PASSWORD)
                                {
                                    if (!server.usingPassword)
                                    {
                                        netStream.Write(new byte[] { CONNECTION_ERROR }, 0, 1);
                                    }
                                    else
                                    {
                                        try
                                        {
                                            ICryptoTransform decryptor = server.crypto.CreateDecryptor(server.crypto.Key, server.crypto.IV);
                                            var origValue = decryptor.TransformFinalBlock(rcvBuffer, 1, bytesRcvd - 1);
                                            if (server.base64Key.Equals(Encoding.ASCII.GetString(origValue).Split(':')[1]))
                                            {
                                                var remoteEndPoint = client.Client.RemoteEndPoint as IPEndPoint;

                                                server.connectedIp = remoteEndPoint.Address.ToString();
                                                server.connected = true;

                                                server.clientTcp = client;

                                                server.netStream = netStream;
                                                netStream.Write(new byte[] { CONNECTED_PASSWORD }, 0, 1);

                                            }
                                            else
                                            {
                                                netStream.Write(new byte[] { WRONG_PASSWORD }, 0, 1);
                                            }
                                        } catch (Exception e) {
                                            netStream.Write(new byte[] { WRONG_PASSWORD }, 0, 1);
                                        }
                                    }
                                }
                                else
                                {
                                    if (server.usingPassword)
                                    {
                                        netStream.Write(new byte[] { REQUIRE_PASSWORD }, 0, 1);
                                    }
                                    else
                                    {
                                        var remoteEndPoint = client.Client.RemoteEndPoint as IPEndPoint;

                                        server.connectedIp = remoteEndPoint.Address.ToString();//RemoteIp.Address.ToString();

                                        server.clientTcp = client;
                                        server.netStream = netStream;
                                        server.connected = true;
                                        netStream.Write(new byte[] { CONNECTED }, 0, 1);
                                    }
                                }
                            }
                        }
                        else {
                            netStream.Close();
                            client.Close();
                            server.connectedIp = null;
                            server.connected = false;
                            Thread.CurrentThread.Abort();
                        }
                    }
                }
                catch (IOException e) {
                    Console.WriteLine(e.Message);
                    netStream.Close();
                    client.Close();
                    server.connectedIp = null;
                    server.connected = false;
                    Thread.CurrentThread.Abort();
                }
            }
        }
    }


    
}



