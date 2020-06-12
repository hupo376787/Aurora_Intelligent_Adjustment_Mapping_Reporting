using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Adjustment
{
    class ListViewEx :ListView 

    {
        public ListViewEx()
        {
            this.initcomponent();
            // 开启双缓冲,防止闪烁
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

            // Enable the OnNotifyMessage event so we get a chance to filter out 
            // Windows messages before they get to the form's WndProc
            this.SetStyle(ControlStyles.EnableNotifyMessage, true);
        }

        protected override void OnNotifyMessage(Message m)// 开启双缓冲,防止闪烁
        {
            //Filter out the WM_ERASEBKGND message
            if (m.Msg != 0x14)
            {
                base.OnNotifyMessage(m);
            }

        }

        internal struct rect 
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        internal class Win32
        {
            public const int LVM_GETSUBITEMRECT = (0x1000) + 56;
  
            public const int LVIR_BOUNDS = 0;

            [DllImport("user32.dll", SetLastError = true)]
            public static extern int SendMessage(IntPtr hWnd, int messageID, int wParam, ref rect lParam);
        }

        private int col = -1;
        private int row = -1;

        private TextBox text = new TextBox();
        private bool mouseDown = false;

        public void initcomponent() 
        {
            this.text.Visible = false;
            text.BorderStyle = BorderStyle.FixedSingle;
            this.text.Leave += new EventHandler(textBox_Leave);
            this.text.KeyDown += new KeyEventHandler(textBox_KeyDown);
            this.Controls.Add(this.text);
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)             //编辑后按键对应操作
        {
            if (e.KeyCode == Keys.Enter)                //回车
            {
                textBox_Leave(sender, e);
            }
        }

        private void textBox_Leave(object sender, EventArgs e)                  //编辑完成
        {
            try
            {
                if (this.row != -1 && this.col != -1)
                {
                    this.Items[row].SubItems[col].Text = this.text.Text;
                    this.text.Hide();
                }
            }
            catch (Exception ex) { return; }
        }

        private rect getrect(Point po)
        {
            rect rect1 = new rect();
            this.row = this.col = -1;
            ListViewItem lvi = this.GetItemAt(po.X ,po.Y );
            if (lvi !=null){
                for (int i = 0; i <= this.Columns.Count ;i++ )
                {
                    rect1.top = i + 1;
                    rect1.left = Win32.LVIR_BOUNDS;
                    try {

                        int result = Win32.SendMessage(this.Handle ,Win32 .LVM_GETSUBITEMRECT ,lvi.Index ,ref rect1 );
                        if (result !=0){
                        if (po.X < rect1 .left ){
                            this.row = lvi.Index;
                            this.col = 0;
                            break;
                        }
                            if (po.X >=rect1 .left && po.X <=rect1.right  ){
                            this.row = lvi .Index ;
                                this.col = i+1;
                                break;
                            }
                        }
                        else
                        {
                            // This call will create a new Win32Exception with the last Win32 Error.
                            throw new Win32Exception();
                        }
                    }catch (Win32Exception ex){
                    }
                }
            }
            return rect1;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            try {

                this.text.Visible = false;
                if (!mouseDown)
                    return;
                if (!this.FullRowSelect || this.View != View.Details)
                {
                    return;
                }

                mouseDown = false;
                rect rect2 = this.getrect(new Point (e.X,e.Y ));
                if (this.row !=-1 && this.col != -1){
                    Size sz = new Size(this.Columns[col].Width ,Items[row].Bounds .Height );
                    Point po1 = col==0?new Point(0,rect2 .top ):new Point (rect2 .left ,rect2 .top );
                  
                    this.showtext(po1,sz );
                }
            }
            catch (Exception ex){
            }
        }

        private void showtext(Point location,Size sz)
        {
            text.Size = sz;
            text.Location = location;
            text.ForeColor = Color.Magenta;
            text.TextAlign = HorizontalAlignment.Center;
            text.Text = this.Items[row].SubItems[col].Text;
            text.Show();
            text.Focus();
            //text.AcceptsReturn = false;

        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)    //  双击单元格编辑     //单击编辑用OnMouseDown，不过单击的情况下无法选中行
        {
            try
            {
                mouseDown = true;   // Mouse down happened inside listview
                this.text.Hide();   // Hide the controls
            }
            catch (Exception ex){ }
        }


    }
}
