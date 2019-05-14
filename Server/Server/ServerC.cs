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
       

        internal struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }


        internal struct GuiThreadInfo
        {
            public int cbSize;
            public uint flags;
            public IntPtr hwndActive;
            public IntPtr hwndFocus;
            public IntPtr hwndCapture;
            public IntPtr hwndMenuOwner;
            public IntPtr hwndMoveSize;
            public IntPtr hwndCaret;
            public Rect rcCaret;
        }

        [DllImport("user32.dll", EntryPoint = "GetGUIThreadInfo")]
        internal static extern bool GetGUIThreadInfo(uint idThread, ref GuiThreadInfo threadInfo);

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

        private bool connected = false;
        private bool open = false;
        private UdpClient _server = new UdpClient(4511);
        private string connectedIp;
        private string data = "";
        //private float speed = 0.4f;
        private bool holding = false;
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

                    connectedIp = RemoteIp.Address.ToString();
                    connected = true;
                }

                // click
                else if (data.StartsWith("click") && connected)
                {
                    this.SendClick();
                    if (GetFocusedHandle().ToInt32() != 0) {
                        sendMessage(RemoteIp.Address.ToString(), "tastiera");
                    }
                    
                    
                }

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
                else if (data.EndsWith("wheel") && connected)
                {
                    float val;

                    if (float.TryParse(data.Split(',')[0], out val))
                    {
                        int delta = (int)val;
                        this.SendWheelScroll(delta);
                    }

                }

                //holding click
                else if (data.StartsWith("hold") && connected)
                {
                    Console.WriteLine(data);
                    this.holding = true;
                    mouse_event((int)MouseEventFlagsAPI.LEFTDOWN, 0, 0, 0, 0);
                }
                else if (data.StartsWith("endhold") && connected)
                {
                    this.holding = false;
                    Console.WriteLine(data);
                    mouse_event((int)MouseEventFlagsAPI.LEFTUP, 0, 0, 0, 0);

                }

                //disconnection
                else if (data.StartsWith("disconn") && connected)
                {
                    Console.WriteLine(data);
                    this.connected = false;
                }

                // Otherwise move
                else if ((data.EndsWith("move") || data.EndsWith("holding")) && connected)
                {
                    // calculate delta
                    String[] delta = data.Split(',');
                    Console.WriteLine(data);
                    int deltaX = int.Parse(delta[0]);
                    Console.WriteLine(deltaX);
                    int deltaY = int.Parse(delta[1]);
                    // set new point

                    Cursor.Position = new Point(Cursor.Position.X + deltaX, Cursor.Position.Y + deltaY);




                }
                
            }
            catch (Exception)
            {
                Console.Write(data);
            }

            
            _server.BeginReceive(new AsyncCallback(recv), null);
            
        }

        static IntPtr GetFocusedHandle()
        {
            var info = new GuiThreadInfo();
            info.cbSize = Marshal.SizeOf(info);
            if (!GetGUIThreadInfo(0, ref info))
                throw new Exception();
            return info.hwndFocus;
        }

        private void SendClick()
        {
            Console.WriteLine(data);
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

            Console.WriteLine(data);
            mouse_event((int)MouseEventFlagsAPI.WHEEL, 0, 0, val, 0);
        }

        public void sendMessage(String ip, String message) {
            Byte[] sendBytes = Encoding.ASCII.GetBytes("message" + Environment.MachineName);
            UdpClient _client = new UdpClient(ip, 4512);
            _client.Send(sendBytes, sendBytes.Length);
        }

    }
}



