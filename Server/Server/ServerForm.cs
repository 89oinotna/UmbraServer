using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using System.Security.Cryptography;
using Microsoft.Win32;
using System.Runtime.InteropServices.ComTypes;

namespace Server
{
    public partial class ServerForm : Form
    {
        ServerC s = new ServerC();
        public ServerForm()
        {
            InitializeComponent();
        }

        private void Btn_start_Click(object sender, EventArgs e)
        {
            ChangeState();
        }

        private void ChangeState(){
            
            if (s.Running)
            {
                //close
                s.close();
                btn_start.Text = "Start";
            }
            else {
                //start
                s.start();
                btn_start.Text = "Stop";
            }
        }
        
        private void Ckb_auto_startup_CheckedChanged(object sender, EventArgs e)
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            string path = rk.GetValue("mouseserver", "null").ToString();
            if (path == "null" || path.Last() == '_')
            {
                path = System.Reflection.Assembly.GetEntryAssembly().Location;
                rk.SetValue("mouseserver", path);
            }
            else
            {
                path = System.Reflection.Assembly.GetEntryAssembly().Location;
                rk.SetValue("mouseserver", path + '_');
            }
        }

        private void Btn_new_password_Click(object sender, EventArgs e) {
            if (s.Running)
            {
                string message = "The server will shutdown, continue?";
                string caption = "Warning";
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult result;

                result = MessageBox.Show(message, caption, buttons);
                if (result == System.Windows.Forms.DialogResult.No)
                {
                    return;
                }
                else
                {
                    ChangeState();
                }
            }
            NewPassword();
            
        }

        private void NewPassword() {
            byte[] keyGenerated = s.generateKey();

            pbox_qrcode.Image = BuildQR(keyGenerated);

            Properties.Settings.Default.password = s.encryptedPassword();

            Properties.Settings.Default.Save();
        }

        private Bitmap BuildQR(byte[] keyGenerated) {
            QRCodeWriter qrWriter = new QRCodeWriter();
            int width = 200;
            int height = 200;
            BitMatrix matrix = qrWriter.encode(Convert.ToBase64String(keyGenerated), BarcodeFormat.QR_CODE, 200, 200);
            Bitmap bmp = new Bitmap(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color pixel = matrix[x, y] ? Color.Black : Color.White;
                    bmp.SetPixel(x, y, pixel);
                }
            }
            return bmp;
        }

        private void ServerForm_Load(object sender, EventArgs e)
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            string path = rk.GetValue("mouseserver", "null").ToString();
            if (path != "null" || path.Last() != '_')
            {
                ckb_auto_startup.Checked = true;
            }
            else {
                ckb_auto_startup.Checked = false;
            }
            if (Properties.Settings.Default.password != string.Empty)
            {
                //SecureString password = DecryptString(Properties.Settings.Default.password);
                //string readable = ToInsecureString(password);
                string readable = s.readKey(Properties.Settings.Default.password);
                QRCodeWriter qrWriter = new QRCodeWriter();
                int width = 200;
                int height = 200;
                BitMatrix matrix = qrWriter.encode(readable, BarcodeFormat.QR_CODE, 200, 200);
                Bitmap bmp = new Bitmap(width, height);
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        Color pixel = matrix[x, y] ? Color.Black : Color.White;
                        bmp.SetPixel(x, y, pixel);
                    }
                }
                pbox_qrcode.Image = bmp;
            }
            else {
                NewPassword();
            }
            if (Properties.Settings.Default.use_password == false)
            {
                rb_off.Checked = true;
                s.usePassword( false);
            }
            else {
                rb_on.Checked = true;
                s.usePassword(true);
            }
        }

        private void Rb_on_CheckedChanged(object sender, EventArgs e)
        {
            if (s.Running)
            {
                string message = "The server will shutdown, continue?";
                string caption = "Warning";
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult result;

                result = MessageBox.Show(message, caption, buttons);
                if (result == System.Windows.Forms.DialogResult.No)
                {
                    return;
                }
                else {
                    ChangeState();
                }
            }
            if (rb_on.Checked)
            {
                s.usePassword(true);
                Properties.Settings.Default.use_password = true;
                Properties.Settings.Default.Save();
            }
            else {
                s.usePassword(false);
                Properties.Settings.Default.use_password = false;
                Properties.Settings.Default.Save();
            }
            
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if(s.Running)s.close();
        }
    }
}
