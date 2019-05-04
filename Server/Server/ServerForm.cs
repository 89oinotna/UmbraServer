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
        public ServerForm()
        {
            InitializeComponent();
            
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            
            s.StartListening();
            button1.Text = "stop";
        }

        private void Button2_Click(object sender, EventArgs e)
        {
           
        }
    }
}
