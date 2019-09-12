using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using System.Runtime.InteropServices;


using System.Windows.Forms;
using System.Drawing;


namespace Server
{
    class Mouse
    {
        private bool holding = false;

        

        //Per vedere  se una finestra ha il focus
        internal struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }


        [DllImport("user32.dll", EntryPoint = "GetGUIThreadInfo")]
        internal static extern bool GetGUIThreadInfo(uint idThread, ref GuiThreadInfo threadInfo);

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




        public String mouseControl(String data) {
            
            if (data.StartsWith("click"))
            {
                this.SendClick();
                if (GetFocusedHandle().ToInt32() != 0)
                {
                    
                    return "tastiera";
                }


            }

            // long click = double click
            else if (data.StartsWith("d.click"))
            {
                Console.WriteLine(data);
                this.SendClick();
                this.SendClick();
            }

            // richt click
            else if (data.StartsWith("r.click"))
            {
                this.SendRClick();

            }

            //wheel
            else if (data.EndsWith("wheel"))
            {
                float val;

                if (float.TryParse(data.Split(',')[0], out val))
                {
                    int delta = (int)val;
                    this.SendWheelScroll(delta);
                }

            }

            //holding click
            else if (data.StartsWith("hold"))
            {
                Console.WriteLine(data);
                this.holding = true;
                mouse_event((int)MouseEventFlagsAPI.LEFTDOWN, 0, 0, 0, 0);
            }
            else if (data.StartsWith("endhold"))
            {
                this.holding = false;
                Console.WriteLine(data);
                mouse_event((int)MouseEventFlagsAPI.LEFTUP, 0, 0, 0, 0);

            }

            

            // Otherwise move
            else if ((data.EndsWith("move") || data.EndsWith("holding")))
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
            return null;


        }

        static IntPtr GetFocusedHandle()
        {
            var info = new GuiThreadInfo();
            info.cbSize = Marshal.SizeOf(info);
            if (!GetGUIThreadInfo(0, ref info))
                throw new Exception();
            return info.hwndFocus;
        }

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
            mouse_event((int)MouseEventFlagsAPI.WHEEL, 0, 0, val, 0);
        }
    }
}
