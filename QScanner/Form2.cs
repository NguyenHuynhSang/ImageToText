using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QScanner
{
    public partial class QScanner : Form
    {
        Form1 mainForm;
        public QScanner()
        {
            InitializeComponent();
            
          
            mainForm = new Form1();
            mainForm.Hide();
            this.Opacity = 0.5;
            this.TopMost = true;
            this.mainForm.Unsubscribe();

            int screenWidth = SystemInformation.VirtualScreen.Width;
            int screenHeight = SystemInformation.VirtualScreen.Height;

            this.Location = new Point(screenWidth-this.Size.Width-100, (screenHeight-this.Size.Height)/2);
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            this.Hide();
            this.mainForm.Reset();
            this.mainForm.Subscribe();
            this.mainForm.ShowDialog();
            this.Show();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.mainForm.Close();
            this.Close();
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

      

        private bool _mousePress = false;
        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            _mousePress = false;
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (_mousePress)
            {

                this.Location = new Point(MousePosition.X, MousePosition.Y);

            }
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            _mousePress = true;
        }
    }
}
