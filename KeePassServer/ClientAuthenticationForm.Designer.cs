namespace KeePassServer
{
    partial class ClientAuthenticationForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ClientAuthenticationForm));
            this.confirm_btn = new System.Windows.Forms.Button();
            this.pinTxt = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // confirm_btn
            // 
            resources.ApplyResources(this.confirm_btn, "confirm_btn");
            this.confirm_btn.Name = "confirm_btn";
            this.confirm_btn.UseVisualStyleBackColor = true;
            this.confirm_btn.Click += new System.EventHandler(this.confirm_btn_Click);
            // 
            // pinTxt
            // 
            resources.ApplyResources(this.pinTxt, "pinTxt");
            this.pinTxt.Name = "pinTxt";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // ClientAuthenticationForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ControlBox = false;
            this.Controls.Add(this.confirm_btn);
            this.Controls.Add(this.pinTxt);
            this.Controls.Add(this.label1);
            this.Name = "ClientAuthenticationForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button confirm_btn;
        private System.Windows.Forms.TextBox pinTxt;
        private System.Windows.Forms.Label label1;
    }
}