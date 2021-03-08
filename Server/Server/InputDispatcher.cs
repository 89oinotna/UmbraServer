using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class InputDispatcher
    {
        private static byte MOUSE = 0x01;
        private static byte KEYBOARD = 0x02;
        public static void dispatch(byte[] command) {
            if (command[0] == MOUSE) {
                Mouse.mouseControl(command);
            }
            else if(command[0] == KEYBOARD) {
                Keyboard.keyboardControl(command);
            }
        }
    }
}
