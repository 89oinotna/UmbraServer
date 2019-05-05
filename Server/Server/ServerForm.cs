using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    public partial class ServerForm : Form
    {
        ServerC s = new ServerC();
        bool started = false;
        public ServerForm()
        {
            InitializeComponent();
            
        }

        private void Button1_Click(object sender, EventArgs e)
        {

            if (s.isConnected()) {
                s.EndListening();
            }
            else {
                s.StartListening();
            }
            if (started){
                started = false;
                button1.Text = "Start";
            }
            else {
                started = true;
                button1.Text = "Stop";
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
           
        }
    }
}
