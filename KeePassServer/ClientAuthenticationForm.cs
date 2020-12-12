using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

//======================================================================================
//
//        filename : ClientAuthenticationForm.cs
//        description : This WinForm is used for client authentication. 
//                      User has to enter PIN shown on KeePass to this window.
//        created by Erni Gao at  Nov 2020
//   
//======================================================================================

namespace KeePassServer
{
    public partial class ClientAuthenticationForm : Form
    {
        byte[] hashedCK = null;   //hashed client public key

        public ClientAuthenticationForm(byte[] hashedKey)
        {
            InitializeComponent();
            if (hashedCK == null)
            {
                hashedCK = hashedKey;
            }
        }

        /// <summary>
        /// get user input pin
        /// </summary>
        /// <returns>pin entered by user</returns>
        private string getPin()
        {
            return pinTxt.Text;
        }

        /// <summary>
        /// click "confirm" button to check whether pin entered by end-user is correct
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void confirm_btn_Click(object sender, EventArgs e)
        {
            string pin = getPin();
            string hashResult = Convert.ToBase64String(hashedCK).Substring(0, 6);
            if (pin.Trim() == hashResult)
            {
                hashedCK = null;
                this.Close();
            }
            else
            {
                label1.Text = "PIN is incorrect. Please retry";
                label1.ForeColor = Color.Red;
                pinTxt.Clear();
            }
        }
    }
}
