using Gma.System.MouseKeyHook;
using IronOcr;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QScanner
{
    public partial class Form1 : Form
    {
        private IKeyboardMouseEvents m_GlobalHook;
        private Point pStartHover;
        private Graphics g=null;
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_SHOWWINDOW = 0x0040;

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private enum  MouseState
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
              this.TopMost = true;// fm always on top
            this.FormBorderStyle = FormBorderStyle.None;

            Rectangle rect = new Rectangle(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue);

            
            

            int screenWidth = SystemInformation.VirtualScreen.Width;
            int screenHeight =SystemInformation.VirtualScreen.Height;

            this.Size = new Size(screenWidth, screenHeight);

         
            Button btn = new Button();
            btn.Location = new Point(this.Width-100,20);
            btn.Text = "Close";
            btn.BackColor = Color.Red;
            btn.Click += Btn_Click;
            btn.Size = new Size(50, 50);
            this.Controls.Add(btn);



            mouseState = MouseState.Defaul;
            g = this.CreateGraphics();
          
            Subscribe();
           this.FormClosing += Form1_FormClosing;


        
            this.BackColor = Color.White;
            this.TransparencyKey = Color.White;

            Form from2 = new Form();
          
            from2.Location = this.Location;
            from2.Size = this.Size;
            from2.Opacity = 0.01;
            from2.Show();
         
            this.ShowDialog();

        }

        private void Btn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Unsubscribe();
        }

        void CaptureImage()
        {
            this.Opacity = 0.5;
            if (selectedBox.Width<=0||selectedBox.Height<=0)
            {
                return;
            }
            string startupPath = Environment.CurrentDirectory;

            Console.WriteLine("Cap: \t{0} y {1}", selectedBox.Width, selectedBox.Height);
            Console.WriteLine("Left top: \t{0} y {1}", selectedBox.Left, selectedBox.Top);
            Bitmap captureBitmap = new Bitmap(selectedBox.Width, selectedBox.Height, PixelFormat.Format32bppArgb);
             g = Graphics.FromImage(captureBitmap);
            g.CopyFromScreen(selectedBox.Left, selectedBox.Top, 0,0, selectedBox.Size);
            captureBitmap.Save(@".\Img.png", System.Drawing.Imaging.ImageFormat.Png);


            var Ocr = new AutoOcr();
            var Result = Ocr.Read(@".\Img.png");
            Console.WriteLine(Result.Text);
            textBox1.BackColor = Color.Aqua;
            textBox1.Visible = true;
            textBox1.Text = Result.Text;
            



        }

        public void Subscribe()
        {
            // Note: for the application hook, use the Hook.AppEvents() instead
            m_GlobalHook = Hook.GlobalEvents();

            m_GlobalHook.MouseDownExt += GlobalHookMouseDownExt;
            m_GlobalHook.KeyPress += GlobalHookKeyPress;
            m_GlobalHook.MouseDragStarted += M_GlobalHook_MouseDragStarted;
            this.m_GlobalHook.MouseDragFinished += M_GlobalHook_MouseDragFinished;
            this.m_GlobalHook.MouseMove += M_GlobalHook_MouseMove;
        }
        private void M_GlobalHook_MouseMove(object sender, MouseEventArgs e)

        {
            if (mouseState==MouseState.StartDrag)
            {
                Console.WriteLine("Draw: \t{0}");
                selectedBox = new Rectangle(this.pStartHover.X, this.pStartHover.Y
                                   , MousePosition.X-this.pStartHover.X, MousePosition.Y- this.pStartHover.Y);
                DrawRectangle(selectedBox);
                Console.WriteLine("pos: \t{0} y {1}",MousePosition.X,MousePosition.Y );
            }
          //  this.mouseState = MouseState.Move;
           
        }

        private void M_GlobalHook_MouseDragFinished(object sender, MouseEventArgs e)
        {
            this.Opacity =1;
            this.mouseState = MouseState.FinishDrag;
            g.Clear(Color.White);
            // g.Clear(Color.White);
            CaptureImage();
            Console.WriteLine("Finish Start: \t{0}");
        }

        private void M_GlobalHook_MouseDragStarted(object sender, MouseEventArgs e)
        {
            this.Opacity = 0.2;
          //  this.textBox1.Visible = false;
            this.mouseState = MouseState.StartDrag;
            this.pStartHover = new Point(MousePosition.X, MousePosition.Y);
            Console.WriteLine("Draw Start: \t{0}");
        }

        private void GlobalHookKeyPress(object sender, KeyPressEventArgs e)
        {
            Console.WriteLine("KeyPress: \t{0}", e.KeyChar);
        }


        private void GlobalHookMouseDownExt(object sender, MouseEventExtArgs e)
        {
            Console.WriteLine("MouseDown: \t{0}; \t System Timestamp: \t{1}", e.Button, e.Timestamp);

            // uncommenting the following line will suppress the middle mouse button click
            // if (e.Buttons == MouseButtons.Middle) { e.Handled = true; }
        }

        public void Unsubscribe()
        {
            m_GlobalHook.MouseDownExt -= GlobalHookMouseDownExt;
            m_GlobalHook.KeyPress -= GlobalHookKeyPress;
            m_GlobalHook.MouseDragStarted -= M_GlobalHook_MouseDragStarted;
            m_GlobalHook.MouseDragFinished-= M_GlobalHook_MouseDragFinished;
            m_GlobalHook.MouseMove -= M_GlobalHook_MouseMove;
            //It is recommened to dispose it
            m_GlobalHook.Dispose();
        }

        public void DrawRectangle(System.Drawing.Rectangle rect)
        {

         
            g.Clear(Color.White);
            Pen selPen = new Pen(Color.Blue);
            Brush brush = new SolidBrush(Color.FromArgb(20, 0, 0, 255));
            g.DrawRectangle(selPen,rect);
            g.FillRectangle(brush, rect);
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            Console.WriteLine("Form clicked");
        }

     
 

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
          
        }
    }
}
