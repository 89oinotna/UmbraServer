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

namespace Server
{

    public class ServerC
    {

       
        private bool connected = false;
        private bool open = false;
        private UdpClient _server = new UdpClient(4511);
        private string connectedIp;
        private string data = "";
        Mouse mouse = new Mouse();

       
        private IPEndPoint RemoteIp = new IPEndPoint(IPAddress.Any, 0);
        public void StartListening() {
            try {
                open = true;
                _server.BeginReceive(new AsyncCallback(recv), null);
            }
            catch (Exception e) {
                MessageBox.Show(e.Message.ToString());
            }
        }
        public void EndListening()
        {
                connected = false;
                open = false;
                //_server.Close();
            
        }

        public bool isConnected(){
            return connected;
        }
      

        void recv(IAsyncResult res) {


            byte[] received = _server.EndReceive(res, ref RemoteIp);
            if (!open)
            {
                _server.Close();
                return;
            }
            if (connected && RemoteIp.Address.ToString()!=connectedIp) {
                return;
            }
            data = Encoding.UTF8.GetString(received);
            Console.WriteLine(data);

            //MessageBox.Show(data);

            try
            {

                //broadcast
                if (data.StartsWith("broad"))
                {
                    //if connected check if client active
                    if (!connected || (connected && connectedIp == RemoteIp.Address.ToString()))
                    {
                        connected = false;
                        Console.WriteLine(RemoteIp.Address.ToString());

                        sendMessage(RemoteIp.Address.ToString(), "server:");
                    }
                }
                //connection
                else if (data.StartsWith("conn") && !connected)
                {
                    Console.WriteLine(data);
                    connectedIp = RemoteIp.Address.ToString();
                    connected = true;
                }

                //disconnection
                else if (data.StartsWith("disconn"))
                {
                    Console.WriteLine(data);
                    this.connected = false;
                }

                else if (data.StartsWith("mouse") && connected) {
                    String mEv = mouse.mouseControl(data.Split('/')[1]);
                    if (mEv!=null) {
                        Console.WriteLine(mEv);
                        sendMessage(RemoteIp.Address.ToString(), mEv);
                    }
                }
                
            }
            catch (Exception)
            {
                Console.Write(data);
            }

            
            _server.BeginReceive(new AsyncCallback(recv), null);
            
        }


        public void sendMessage(String ip, String message) {
            Byte[] sendBytes = Encoding.ASCII.GetBytes(message + Environment.MachineName);
            UdpClient _client = new UdpClient(ip, 4512);
            _client.Send(sendBytes, sendBytes.Length);
        }

    }
}



