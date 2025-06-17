using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace PiskaBobraFormsApp1
{ 
    public partial class Form1 : Form
    {
        private string returnedData;


        public Form1()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        // Владислав - Контрольная работа (in Progress).
        private void button3_Click(object sender, EventArgs e)
        {
            Form2 controlwork = new Form2();
            this.Hide();
            if (controlwork.ShowDialog() == DialogResult.OK)
            {
                this.returnedData = controlwork.ReturnedData;
                this.Show();

                MessageBox.Show(returnedData, "Результат контрольная работа", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                this.Show();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
