namespace Server
{
    partial class ServerForm
    {
        /// <summary>
        /// Variabile di progettazione necessaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Pulire le risorse in uso.
        /// </summary>
        /// <param name="disposing">ha valore true se le risorse gestite devono essere eliminate, false in caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Codice generato da Progettazione Windows Form

        /// <summary>
        /// Metodo necessario per il supporto della finestra di progettazione. Non modificare
        /// il contenuto del metodo con l'editor di codice.
        /// </summary>
        private void InitializeComponent()
        {
            this.btn_start = new System.Windows.Forms.Button();
            this.btn_new_password = new System.Windows.Forms.Button();
            this.ckb_auto_startup = new System.Windows.Forms.CheckBox();
            this.pbox_qrcode = new System.Windows.Forms.PictureBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rb_off = new System.Windows.Forms.RadioButton();
            this.rb_on = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.pbox_qrcode)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_start
            // 
            this.btn_start.Location = new System.Drawing.Point(12, 171);
            this.btn_start.Name = "btn_start";
            this.btn_start.Size = new System.Drawing.Size(75, 23);
            this.btn_start.TabIndex = 1;
            this.btn_start.Text = "Start";
            this.btn_start.UseVisualStyleBackColor = true;
            this.btn_start.Click += new System.EventHandler(this.btn_start_Click);
            // 
            // btn_new_password
            // 
            this.btn_new_password.Location = new System.Drawing.Point(16, 115);
            this.btn_new_password.Name = "btn_new_password";
            this.btn_new_password.Size = new System.Drawing.Size(83, 20);
            this.btn_new_password.TabIndex = 4;
            this.btn_new_password.Text = "NewPassword";
            this.btn_new_password.UseVisualStyleBackColor = true;
            this.btn_new_password.Click += new System.EventHandler(this.btn_new_password_Click);
            // 
            // ckb_auto_startup
            // 
            this.ckb_auto_startup.AutoSize = true;
            this.ckb_auto_startup.Location = new System.Drawing.Point(12, 200);
            this.ckb_auto_startup.Name = "ckb_auto_startup";
            this.ckb_auto_startup.Size = new System.Drawing.Size(65, 17);
            this.ckb_auto_startup.TabIndex = 5;
            this.ckb_auto_startup.Text = "Start Up";
            this.ckb_auto_startup.UseVisualStyleBackColor = true;
            this.ckb_auto_startup.CheckedChanged += new System.EventHandler(this.ckb_auto_startup_CheckedChanged);
            // 
            // pbox_qrcode
            // 
            this.pbox_qrcode.Location = new System.Drawing.Point(105, 21);
            this.pbox_qrcode.Name = "pbox_qrcode";
            this.pbox_qrcode.Size = new System.Drawing.Size(200, 200);
            this.pbox_qrcode.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pbox_qrcode.TabIndex = 6;
            this.pbox_qrcode.TabStop = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rb_off);
            this.groupBox1.Controls.Add(this.rb_on);
            this.groupBox1.Location = new System.Drawing.Point(16, 25);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(83, 76);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Password";
            // 
            // rb_off
            // 
            this.rb_off.AutoSize = true;
            this.rb_off.Location = new System.Drawing.Point(7, 53);
            this.rb_off.Name = "rb_off";
            this.rb_off.Size = new System.Drawing.Size(37, 17);
            this.rb_off.TabIndex = 1;
            this.rb_off.TabStop = true;
            this.rb_off.Text = "off";
            this.rb_off.UseVisualStyleBackColor = true;
            // 
            // rb_on
            // 
            this.rb_on.AutoSize = true;
            this.rb_on.Location = new System.Drawing.Point(7, 19);
            this.rb_on.Name = "rb_on";
            this.rb_on.Size = new System.Drawing.Size(37, 17);
            this.rb_on.TabIndex = 0;
            this.rb_on.TabStop = true;
            this.rb_on.Text = "on";
            this.rb_on.UseVisualStyleBackColor = true;
            this.rb_on.CheckedChanged += new System.EventHandler(this.rb_on_CheckedChanged);
            // 
            // ServerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(327, 229);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.pbox_qrcode);
            this.Controls.Add(this.ckb_auto_startup);
            this.Controls.Add(this.btn_new_password);
            this.Controls.Add(this.btn_start);
            this.Name = "ServerForm";
            this.Text = "Server";
            this.Load += new System.EventHandler(this.ServerForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pbox_qrcode)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        
        #endregion
        private System.Windows.Forms.Button btn_start;
        private System.Windows.Forms.Button btn_new_password;
        private System.Windows.Forms.CheckBox ckb_auto_startup;
        private System.Windows.Forms.PictureBox pbox_qrcode;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rb_off;
        private System.Windows.Forms.RadioButton rb_on;
    }
}

