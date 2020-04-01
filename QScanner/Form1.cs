using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tesseract;

namespace QScanner
{
    public partial class Form1 : Form
    {
        private IKeyboardMouseEvents m_GlobalHook;
        private Point pStartHover;
        private Graphics g = null;
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_SHOWWINDOW = 0x0040;
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly String rootPath = @".\Img.png";
        private TesseractEngine Orc;
        private enum MouseState
        {
            Defaul,
            StartDrag,
            FinishDrag,
            Move,
        }

        private Rectangle selectedBox;

        [DllImport("User32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("User32.dll")]
        public static extern void ReleaseDC(IntPtr hwnd, IntPtr dc);

        private MouseState mouseState;
        public Form1()
        {

            InitializeComponent();
            //   this.TopMost = true;// fm always on top
            this.FormBorderStyle = FormBorderStyle.None;

            // get window size
            int screenWidth = SystemInformation.VirtualScreen.Width;
            int screenHeight = SystemInformation.VirtualScreen.Height;

            this.Size = new Size(screenWidth, screenHeight);

            mouseState = MouseState.Defaul;


            Subscribe();
            this.FormClosing += Form1_FormClosing;

            this.BackColor = Color.Red;
            this.TransparencyKey = Color.Red;

            this.tbxresult.Location = new Point(0, 0);
            this.tbxresult.Size = new Size(screenWidth / 2 - this.btnClose.Size.Width, screenHeight);



            this.btnClose.Location = new Point(this.tbxresult.Size.Width, this.btnClose.Location.Y);

            this.Orc = new TesseractEngine("../../Tranning/tessdata", "eng", EngineMode.TesseractAndLstm);
        }

        public void Reset()
        {
            this.tbxresult.Text = "";
            this.tbxresult.Hide();
        }

        /// <summary>
        /// Subscribe and Unsubscribe mouse event
        /// </summary>
        public void Subscribe()
        {
            // Note: for the application hook, use the Hook.AppEvents() instead
            m_GlobalHook = Hook.GlobalEvents();


            m_GlobalHook.MouseDragStarted += M_GlobalHook_MouseDragStarted;
            this.m_GlobalHook.MouseDragFinished += M_GlobalHook_MouseDragFinished;
            this.m_GlobalHook.MouseMove += M_GlobalHook_MouseMove;
        }
        public void Unsubscribe()
        {

            m_GlobalHook.MouseDragStarted -= M_GlobalHook_MouseDragStarted;
            m_GlobalHook.MouseDragFinished -= M_GlobalHook_MouseDragFinished;
            m_GlobalHook.MouseMove -= M_GlobalHook_MouseMove;
            //It is recommened to dispose it
            m_GlobalHook.Dispose();
        }


        /// <summary>
        /// Capture image on scene and OCR here
        /// </summary>
        void CaptureImage()
        {
           
            if (selectedBox.Width <= 0 || selectedBox.Height <= 0)
            {
                return;
            }
            Console.WriteLine("CaptureImage: \t");

            Bitmap captureBitmap = new Bitmap(selectedBox.Width, selectedBox.Height, PixelFormat.Format32bppArgb);
            Graphics j = Graphics.FromImage(captureBitmap);
            j.CopyFromScreen(selectedBox.Left, selectedBox.Top, 0, 0, selectedBox.Size);
          
      
            var Page = Orc.Process(captureBitmap);

            tbxresult.BackColor = Color.FromArgb(255, 0, 0, 0);
            string result = Page.GetText();
            result = result.Replace("\n", "\r\n");

            tbxresult.Visible = true;
            tbxresult.Text = result;

            captureBitmap.Dispose();

            j.Dispose();
            Page.Dispose();
        }
        public void DrawRectangle(System.Drawing.Rectangle rect)
        {
          
            g.Clear(Color.Red);
            Pen selPen = new Pen(Color.Blue);
            Brush brush = new SolidBrush(Color.FromArgb(20, 0, 0, 255));
            g.DrawRectangle(selPen, rect);
            g.FillRectangle(brush, rect);
        }

        private void M_GlobalHook_MouseMove(object sender, MouseEventArgs e)

        {
            if (mouseState == MouseState.StartDrag)
            {

                selectedBox = new Rectangle(this.pStartHover.X, this.pStartHover.Y
                                   , MousePosition.X - this.pStartHover.X, MousePosition.Y - this.pStartHover.Y);
                DrawRectangle(selectedBox);
            }

        }

        private void M_GlobalHook_MouseDragFinished(object sender, MouseEventArgs e)
        {
            this.Opacity = 0.9;
            this.mouseState = MouseState.FinishDrag;
            g.Clear(Color.Red);

            CaptureImage();
            Console.WriteLine("Finish Start: \t{0}");
        }

        private void M_GlobalHook_MouseDragStarted(object sender, MouseEventArgs e)
        {
            this.Opacity = 0.2;
         
           
            this.mouseState = MouseState.StartDrag;
            this.pStartHover = new Point(MousePosition.X, MousePosition.Y);
            Console.WriteLine("Draw Start: \t{0}");
        }









        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Unsubscribe();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void btnScan_Click(object sender, EventArgs e)
        {

            this.Show();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            g = this.CreateGraphics();
        }
    }
}
