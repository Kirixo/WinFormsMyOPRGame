using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyOPRGame
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            this.DoubleBuffered = true;
            this.FormClosing += Form1_FormClosing;
            //KeyPreview = true;

        }

        //protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        //{
        //    // Check if the active control is a button
        //    if (ActiveControl is Button)
        //    {
        //        // Check for the keys you want to disable (e.g., arrow keys and Enter)
        //        if (keyData == Keys.Left || keyData == Keys.Right || keyData == Keys.Up || keyData == Keys.Down || keyData == Keys.Enter)
        //        {
        //            // Return true to indicate that the key has been handled and should not be processed further
        //            return true;
        //        }
        //    }

        //    // Call the base implementation for normal processing of other keys
        //    return base.ProcessCmdKey(ref msg, keyData);
        //}

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show("Ви дійсно хочете вийти?\nВаш ігровий процес не буде збережено.", "Підтвердження", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
            {
                e.Cancel = true;
            } else
            {
                Environment.Exit(0);
            }
        }

    }
}
