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
        private bool running = false;
        public bool Running
        {
            get
            {
                return this.running;
            }
            set
            {
                this.running = value;
            }
        }

        
        private UdpClient serverUdp;
        private string connectedIp;
        private string data = "";
        private TcpListener listener = null;
        private TcpClient clientTcp = null; //used to manage connection
        private NetworkStream netStream = null;
        private IPEndPoint RemoteIp = new IPEndPoint(IPAddress.Any, 0);

        

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

        private byte[] key;
        private string base64Key;
        private bool usingPassword=false;
        System.Security.Cryptography.AesCryptoServiceProvider crypto = new System.Security.Cryptography.AesCryptoServiceProvider();
        
       
        public ServerC()
        {
            crypto.KeySize = 128;
            crypto.BlockSize = 128;
            crypto.Mode = System.Security.Cryptography.CipherMode.CBC;
            crypto.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
            crypto.IV = Encoding.ASCII.GetBytes("1111111111111111");
        }

        public void close()
        {
            serverUdp.Close();
            listener.Stop();
            connected = false;
            running = false;
            connectedIp = null;
            if (netStream != null)
            {
                netStream.Close();
                clientTcp.Close();
            }
        }    

        public void start() {
            
            running = true;
            serverUdp = new UdpClient(4513);
            StartListeningUdp();
            StartListeningTcp();
        }

        public void StartListeningUdp() {
            try {
                serverUdp.BeginReceive(new AsyncCallback(recv), null);
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
           
            //check if connected and check that the ip corresponds
            if (connected && RemoteIp.Address.ToString()!=connectedIp) {
                //if not just ignore
                //this avoids that a broadcast can pass
                return;
            }

            data = Encoding.UTF8.GetString(received);
            Console.WriteLine(data);

            try {
                //broadcast
                if (received[0] == BROADCAST) {
                    //Keyboard.SendKeyUp(0);
                    //TODO: if connected check if client is active
                    Console.WriteLine(RemoteIp.Address.ToString());
                    sendAlive();
                }
                else if (connected && (received[0] == CONNECTED || received[0] == CONNECTED_PASSWORD) )
                {
                    if (usingPassword && received[0] == CONNECTED_PASSWORD)
                    {
                        ICryptoTransform decryptor = crypto.CreateDecryptor(crypto.Key, crypto.IV);
                        var origValue = decryptor.TransformFinalBlock(received, 1, received.Length - 1);
                        InputDispatcher.dispatch(origValue);

                    }
                    else if (!usingPassword && received[0] == CONNECTED)
                    {
                        byte[] destfoo = new byte[received.Length - 1];
                        Array.Copy(received, 1, destfoo, 0, received.Length - 1);
                        InputDispatcher.dispatch(destfoo);
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
                    while (client.Connected) {
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

        public void usePassword(bool b)
        {
            //key = keyGenerated;
            //this.base64Key = base64Key;
            if (running)
                close();
            usingPassword = b;
        }

        public byte[] generateKey()
        {
            key = Encryption.generateKey();
            crypto.Key = key;
            base64Key = Convert.ToBase64String(key);
            return key;
        }

        public string readKey(string encryptedData)
        {
            base64Key = Encryption.readKey(encryptedData);
            key = Convert.FromBase64String(base64Key);
            crypto.Key = key;
            return base64Key;
        }

        public string encryptedPassword() {
            return Encryption.EncryptString(Encryption.ToSecureString(base64Key));
        }
    }


    
}



