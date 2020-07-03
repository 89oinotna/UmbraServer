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
        private UdpClient serverUdp = new UdpClient(4511);
        private TcpClient
        private string connectedIp;
        private string data = "";
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
        public static byte DISCONNECT = 0x06;
        public static byte DISCONNECT_PASSWORD = 0x07;

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
            connected = false;
            started = false;
        }

        public void StartListening() {
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
                if (received[0] == BROADCAST)
                {
                    //if connected check if client active
                    //if (!connected || (connected && connectedIp == RemoteIp.Address.ToString()))
                    //{
                    //connected = false;
                    Console.WriteLine(RemoteIp.Address.ToString());
                    sendAlive();
                    //}
                }
                //connection
                else if ((received[0] == CONNECTION || received[0] == CONNECTION_PASSWORD) /*&& !connected*/)
                {

                    if (received[0] == CONNECTION_PASSWORD)
                    {
                        if (!usingPassword)
                        {
                            sendMessage(RemoteIp.Address.ToString(), CONNECTION_ERROR);
                        }
                        else
                        {
                            ICryptoTransform decryptor = crypto.CreateDecryptor(crypto.Key, crypto.IV);
                            var origValue = decryptor.TransformFinalBlock(received, 1, received.Length - 1);
                            if (base64Key.Equals(Encoding.ASCII.GetString(origValue).Split(':')[1]))
                            {
                                connectedIp = RemoteIp.Address.ToString();
                                connected = true;
                                sendMessage(RemoteIp.Address.ToString(), CONNECTED_PASSWORD);
                            }
                            else
                            {
                                sendMessage(RemoteIp.Address.ToString(), WRONG_PASSWORD);
                            }
                        }
                    }
                    else
                    {
                        if (usingPassword)
                        {
                            sendMessage(RemoteIp.Address.ToString(), REQUIRE_PASSWORD);
                        }
                        else
                        {
                            connectedIp = RemoteIp.Address.ToString();
                            connected = true;
                            sendMessage(RemoteIp.Address.ToString(), CONNECTED);
                        }
                    }
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

                else if ((received[0] == DISCONNECT || received[0] == DISCONNECT_PASSWORD) && connected) {
                    //TODO: separare i casi
                    connected = false;
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

         public void sendMessage(String ip, String message, int port)
        {
            Byte[] sendBytes = Encoding.ASCII.GetBytes(message + Environment.MachineName);
            UdpClient client = new UdpClient(ip, port);
            client.Send(sendBytes, sendBytes.Length);
        }

        public void sendMessage(String ip, byte b)
        {
            Byte[] sendBytes = new Byte[] { b };
            sendBytes.Concat(Encoding.ASCII.GetBytes(Environment.MachineName));
            UdpClient client = new UdpClient(ip, 4512);
            client.Send(sendBytes, sendBytes.Length);
        }


    }
}



