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
        [DllImport("user32.dll")]
        static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        // Flags for mouse_event api
        [Flags]
        public enum MouseEventFlagsAPI
        {
            LEFTDOWN = 0x00000002,
            LEFTUP = 0x00000004,
            MIDDLEDOWN = 0x00000020,
            MIDDLEUP = 0x00000040,
            MOVE = 0x00000001,
            ABSOLUTE = 0x00008000,
            WHEEL = 0x00000800,
            RIGHTDOWN = 0x00000008,
            RIGHTUP = 0x00000010
        }

        private bool connected=false;
        private UdpClient _server = new UdpClient(4511);
        private string connectedIp;
        private string data = "";
        private float speed = 0.4f;
        private bool holding = false;
        private IPEndPoint RemoteIp = new IPEndPoint(IPAddress.Any, 0);
        public void StartListening() {
            try {
                _server.BeginReceive(new AsyncCallback(recv), null);
            }
            catch (Exception e) {
                MessageBox.Show(e.Message.ToString());
            }
        }

      

        void recv(IAsyncResult res) {
            
            byte[] received = _server.EndReceive(res, ref RemoteIp);
            if (connected && RemoteIp.Address.ToString()!=connectedIp) {
                return;
            }
            data = Encoding.UTF8.GetString(received);
           
            //MessageBox.Show(data);

            try
            {

                //broadcast
                if (data.StartsWith("broad")) {
                    //if connected check if client active
                    if ((connected && connectedIp == RemoteIp.Address.ToString()) || !connected)
                    {
                        connected = false;
                        Console.WriteLine(RemoteIp.Address.ToString());
                        Byte[] sendBytes = Encoding.ASCII.GetBytes("server");
                        UdpClient _client = new UdpClient(RemoteIp.Address.ToString(), 4512);
                        _client.Send(sendBytes, sendBytes.Length);
                    }
                }
                //connection
                else if (data.StartsWith("conn") && !connected) {

                    connectedIp = RemoteIp.Address.ToString();
                    connected = true;
                }

                // click
                else if (data.StartsWith("click") && connected)
                    this.SendClick();

                // long click = double click
                else if (data.StartsWith("d.click") && connected)
                {
                    Console.WriteLine(data);
                    this.SendClick();
                    this.SendClick();
                }

                // richt click
                else if (data.StartsWith("r.click") && connected)
                {
                    this.SendRClick();

                }

                //wheel
                else if (data.StartsWith("wheel") && connected) {
                    Console.WriteLine(data);
                    String val = data.Split(',')[1];
                    this.SendWheelScroll(Int32.Parse(val));
                }

                //holding click
                else if (data.StartsWith("hold") && connected)
                {
                    Console.WriteLine(data);
                    holding = true;
                    mouse_event((int)MouseEventFlagsAPI.LEFTDOWN, 0, 0, 0, 0);
                }
                else if(data.StartsWith("endhold") && connected) {
                    holding = false;
                    Console.WriteLine(data);
                    mouse_event((int)MouseEventFlagsAPI.LEFTUP, 0, 0, 0, 0);

                }

                //disconnection
                else if (data.StartsWith("disconn") && connected) {
                    Console.WriteLine(data);
                    this.connected = false;
                }

                // Otherwise move
                else
                {
                    if (connected) {
                        // calculate delta
                        String[] delta = data.Split(',');

                        float deltaX = float.Parse(delta[0]) * this.speed;
                        float deltaY = float.Parse(delta[1]) * this.speed;
                        // set new point
                        Cursor.Position = new Point(Cursor.Position.X + (int)deltaX, Cursor.Position.Y + (int)deltaY);
                        
                    }


                }
                
            }
            catch (Exception)
            {
                Console.Write(data);
            }

            //MessageBox.Show("received" + data + RemoteIp.ToString());
            _server.BeginReceive(new AsyncCallback(recv), null);
            
        }

        private void SendClick()
        {
            // Send click to system
            mouse_event((int)MouseEventFlagsAPI.LEFTDOWN, 0, 0, 0, 0);
            mouse_event((int)MouseEventFlagsAPI.LEFTUP, 0, 0, 0, 0);
        }
        private void SendRClick()
        {
            // Send click to system
            mouse_event((int)MouseEventFlagsAPI.RIGHTDOWN, 0, 0, 0, 0);
            mouse_event((int)MouseEventFlagsAPI.RIGHTUP, 0, 0, 0, 0);
        }

        private void SendWheelScroll(int val)
        {
            // Send click to system
            mouse_event((int)MouseEventFlagsAPI.WHEEL, 0, 0, 120, 0);
        }

    }
}



