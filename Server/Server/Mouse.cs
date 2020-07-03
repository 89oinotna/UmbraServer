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

        public static byte NULL = 0x00;
        public static byte LEFT_DOWN = 0x01;
        public static byte LEFT_UP = 0x02;
        public static byte RIGHT_DOWN = 0x03;
        public static byte RIGHT_UP = 0x04;
        public static byte WHEEL_MOVE = 0x05;
        public static byte PAD_MOVE = 0x06;
        public static byte SENSOR_MOVE = 0x07;

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

        
        public String mouseControl(byte[] received)
        {
            if (received[0] == LEFT_DOWN) {
                mouse_event((int)MouseEventFlagsAPI.LEFTDOWN, 0, 0, 0, 0);
            }
            else if (received[0] == LEFT_UP) {
                mouse_event((int)MouseEventFlagsAPI.LEFTUP, 0, 0, 0, 0);
            }
            else if (received[0] == RIGHT_DOWN){
                mouse_event((int)MouseEventFlagsAPI.RIGHTDOWN, 0, 0, 0, 0);
            }
            else if (received[0] == RIGHT_UP){
                mouse_event((int)MouseEventFlagsAPI.RIGHTUP, 0, 0, 0, 0);
            }
            else if (received[0] == WHEEL_MOVE){
            }
            else if (received[0] == PAD_MOVE){
                // calculate delta
                String data = Encoding.UTF8.GetString(received);
                String[] delta = data.Split(':', ',' );
                //Console.WriteLine(data);
                int deltaX = int.Parse(delta[1]);
                //Console.WriteLine(deltaX);
                int deltaY = int.Parse(delta[2]);
                // set new point

                Cursor.Position = new Point(Cursor.Position.X + deltaX, Cursor.Position.Y + deltaY);

            }
            else if (received[0] == SENSOR_MOVE){
                String data = Encoding.UTF8.GetString(received);
                String[] delta = data.Split(':', ',');
                //Console.WriteLine(data);
                int deltaX = int.Parse(delta[1]);
                //Console.WriteLine(deltaX);
                int deltaY = int.Parse(delta[2]);
                // set new point

                Cursor.Position = new Point(Cursor.Position.X + deltaX, Cursor.Position.Y + deltaY);
            }
            return null;
        }

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

        /*    public String mouseControl(String data) {

            if (data.StartsWith("click"))
            {
                this.SendClick();
                if (GetFocusedHandle().ToInt32() != 0)
                {

                    return "tastiera";
                }


            }

            return null;


        }*/

        static IntPtr GetFocusedHandle()
        {
            var info = new GuiThreadInfo();
            info.cbSize = Marshal.SizeOf(info);
            if (!GetGUIThreadInfo(0, ref info))
                throw new Exception();
            return info.hwndFocus;
        }

    }
}
