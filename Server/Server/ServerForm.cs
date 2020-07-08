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

namespace Server
{
    public partial class ServerForm : Form
    {
        ServerC s;
        private byte[] k;
        private string base64k;
        private bool usePassword;
        public ServerForm()
        {
            InitializeComponent(); 
        }

        private void btn_start_Click(object sender, EventArgs e)
        {
            if (s != null && s.Started) {
                s.close();
                btn_start.Text = "Start";
            }
            else if(s!=null && !s.Started){
                s = new ServerC();
                if (k != null && usePassword) {
                    s.usePassword(k, base64k);
                }
                s.StartListeningUdp();
                s.StartListeningTcp();
                btn_start.Text = "Stop";
            }
            else if (s == null || !s.Started) {
                s = new ServerC();
                if (k != null && usePassword)
                {
                    s.usePassword(k, base64k);
                }
                s.StartListeningUdp();
                s.StartListeningTcp();
                btn_start.Text = "Stop";
            }
        }

        private void ckb_auto_startup_CheckedChanged(object sender, EventArgs e)
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

        private void btn_new_password_Click(object sender, EventArgs e)
        {
            System.Security.Cryptography.AesCryptoServiceProvider crypto = new System.Security.Cryptography.AesCryptoServiceProvider();
            crypto.KeySize = 128;
            crypto.BlockSize = 128;
            crypto.GenerateKey();
            byte[] keyGenerated = crypto.Key;
            QRCodeWriter qrWriter = new QRCodeWriter();
            int width=200;
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
            pbox_qrcode.Image = bmp;

            Properties.Settings.Default.password = EncryptString(ToSecureString(Convert.ToBase64String(keyGenerated)));
            Properties.Settings.Default.Save();
            k = keyGenerated;
            base64k = Convert.ToBase64String(keyGenerated);
            s.usePassword(keyGenerated, Convert.ToBase64String(keyGenerated));
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
                SecureString password = DecryptString(Properties.Settings.Default.password);
                string readable = ToInsecureString(password);
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
                k = Convert.FromBase64String(readable);
                base64k = readable;
            }
            if (Properties.Settings.Default.use_password == false)
            {
                rb_off.Checked = true;
                usePassword = false;
            }
            else {
                rb_on.Checked = true;
                usePassword = true;
            }
        }


        public static string EncryptString(SecureString input)
        {
            byte[] encryptedData = ProtectedData.Protect(Encoding.Unicode.GetBytes(ToInsecureString(input)), new byte[1], DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedData);
        }

        public static SecureString DecryptString(string encryptedData)
        {
            try
            {
                byte[] decryptedData = ProtectedData.Unprotect(Convert.FromBase64String(encryptedData), new byte[1], DataProtectionScope.CurrentUser);
                return ToSecureString(Encoding.Unicode.GetString(decryptedData));
            }
            catch
            {
                return new SecureString();
            }
        }

        public static SecureString ToSecureString(string input)
        {
            SecureString secure = new SecureString();
            foreach (char c in input)
            {
                secure.AppendChar(c);
            }
            secure.MakeReadOnly();
            return secure;
        }

        public static string ToInsecureString(SecureString input)
        {
            string returnValue = string.Empty;
            IntPtr ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(input);
            try
            {
                returnValue = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(ptr);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(ptr);
            }
            return returnValue;
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void rb_on_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_on.Checked)
            {
                usePassword = true;
                Properties.Settings.Default.use_password = true;
                Properties.Settings.Default.Save();
            }
            else {
                usePassword = false;
                Properties.Settings.Default.use_password = false;
                Properties.Settings.Default.Save();
            }
        }
    }
}
