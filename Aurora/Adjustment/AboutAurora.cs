#define _BUFFERED_RENDERING
#define _JAGGED_ARRAYS

using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace Adjustment
{
    public partial class AboutAurora : Form
    {
        //鼠标拖动相关变量
        Point oldPoint = new Point(0, 0);
        bool mouseDown = false;

        //#region 窗体边框阴影效果变量申明
        //const int CS_DropSHADOW = 0x20000;
        //const int GCL_STYLE = (-26);
        ////声明Win32 API
        //[DllImport("user32.dll", CharSet = CharSet.Auto)]
        //public static extern int SetClassLong(IntPtr hwnd, int nIndex, int dwNewLong);
        //[DllImport("user32.dll", CharSet = CharSet.Auto)]
        //public static extern int GetClassLong(IntPtr hwnd, int nIndex);
        //#endregion

        private struct DropData
        {
            public int x;
            public int y;
            public int radius;
            public int height;
        }

        private static int _BITMAP_WIDTH = 0;
        private static int _BITMAP_HEIGHT = 0;
        private static int _BITS = 4; /* Dont change this, it 24 bit bitmaps are not supported*/
#if _JAGGED_ARRAYS
        private static int[][][] _waveHeight;
#endif
#if _RECTANGULAR_ARRAYS
        private static int[,,] _waveHeight;
#endif
#if _LINEAR_ARRAYS
        private static int[] _waveHeight;
#endif

        private static DropData[] _drops;
        private WaveEffects _image = null;
        private WaveEffects _originalImage = null;
        public int _currentHeightBuffer = 0;
        public int _newHeightBuffer = 0;
        private byte[] _bitmapOriginalBytes;
        private Random _r = new Random();

        public AboutAurora()
        {
            InitializeComponent();
            
            MouseDown += new MouseEventHandler(About_MouseDown);
            MouseUp += new MouseEventHandler(About_MouseUp);
            MouseMove += new MouseEventHandler(About_MouseMove);
        }

        void About_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                this.Left += (e.X - oldPoint.X);
                this.Top += (e.Y - oldPoint.Y);
            }
        }

        void About_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        void About_MouseDown(object sender, MouseEventArgs e)
        {
            oldPoint = e.Location;
            mouseDown = true;
        }

        //动画窗体调用
        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern bool AnimateWindow(IntPtr hwnd, int dwTime, int dwFlags);
        const int AW_HOR_POSITIVE = 0x0001;
        const int AW_HOR_NEGATIVE = 0x0002;
        const int AW_VER_POSITIVE = 0x0004;
        const int AW_VER_NEGATIVE = 0x0008;
        const int AW_CENTER = 0x0010;
        const int AW_HIDE = 0x10000;
        const int AW_ACTIVATE = 0x20000;
        const int AW_SLIDE = 0x40000;
        const int AW_BLEND = 0x80000;

        private void AboutAurora_Load(object sender, EventArgs e)
        {
            //SetClassLong(this.Handle, GCL_STYLE, GetClassLong(this.Handle, GCL_STYLE) | CS_DropSHADOW); //API函数加载，实现窗体边框阴影效果

            //首先加载语言
            if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-CN")
            {
                label1.Text = "右击试试？！                                                                                  © 2013 Aurora。保留所有权利。[部分图标依据ArcGIS 10.0制作，版权归ESRI公司所有，最终解释权归作者所有]";
            }
            else if (Thread.CurrentThread.CurrentUICulture.ToString() == "zh-Hant")
            {
                label1.Text = "右擊試試？！                                                                                  © 2013 Aurora。保留所有權利。[部分圖示依據ArcGIS 10.0製作，版權歸ESRI公司所有，最終解釋權歸作者所有]";
            }
            else //if (Thread.CurrentThread.CurrentUICulture.ToString() == "en")
            {
                label1.Text = "Try right click?!                                                                          © 2013 Aurora. All Rights Reserved.[Partof the icons are based on ArcGIS 10.0, copyrights to ESRI. This statement final right to interpret turns over to the author to possess]";
            }

            //System.ComponentModel.ComponentResourceManager res = new ComponentResourceManager(typeof(AboutAurora));
            //foreach (ToolStripItem item in this.contextMenuStrip1.Items)       //右键菜单
            //{
            //    res.ApplyResources(item, item.Name);
            //}

            this.pictureBox1.Top = 0;
            this.pictureBox1.Left = 0;
            this.pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
            this.Width = this.pictureBox1.Width;
            this.Height = this.pictureBox1.Height;
            _BITMAP_WIDTH = this.pictureBox1.Width;
            _BITMAP_HEIGHT = this.pictureBox1.Height;

#if _JAGGED_ARRAYS
            _waveHeight = new int[_BITMAP_WIDTH][][];
            for (int i = 0; i < _BITMAP_WIDTH; i++)
            {
                _waveHeight[i] = new int[_BITMAP_HEIGHT][];
                for (int j = 0; j < _BITMAP_HEIGHT; j++)
                {
                    _waveHeight[i][j] = new int[2];
                }
            }
#endif
#if _RECTANGULAR_ARRAYS
            _waveHeight = new int[_BITMAP_WIDTH, _BITMAP_HEIGHT, 2];
#endif

#if _LINEAR_ARRAYS
            _waveHeight = new int[_BITMAP_WIDTH * _BITMAP_HEIGHT * 2];
#endif
            //
            //
            //            
            CreateBitmap();
            CreateWaterDrops();

            this.waterTime.Enabled = true;
            this.dropsTime.Interval = 50;
            this.dropsTime.Enabled = true;

            AnimateWindow(this.Handle, 2500, AW_CENTER | AW_ACTIVATE);     //动画由小渐大
        }

        private void AboutAurora_FormClosing(object sender, FormClosingEventArgs e)             //关闭动画
        {
            AnimateWindow(this.Handle, 2000, AW_SLIDE | AW_HIDE | AW_VER_NEGATIVE);
        }

        private void CreateBitmap()
        {
            _originalImage = new WaveEffects((Bitmap)(this.pictureBox1.Image).Clone(), _BITS);
            _originalImage.LockBits();
            _image = new WaveEffects((Bitmap)(this.pictureBox1.Image).Clone(), _BITS);
            _bitmapOriginalBytes = new byte[_BITS * _image.Width() * _image.Height()];
            _image.LockBits();
            Marshal.Copy(_image.Data().Scan0, _bitmapOriginalBytes, 0, _bitmapOriginalBytes.Length);
            _image.Release();
        }

        private void DropWater(int x, int y, int radius, int height)
        {
            long _distance;
            int _x;
            int _y;
            Single _ratio;

            _ratio = (Single)((Math.PI / (Single)radius));

            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    _x = x + i;
                    _y = y + j;
                    if ((_x >= 0) && (_x <= _BITMAP_WIDTH - 1) && (_y >= 0) && (_y <= _BITMAP_HEIGHT - 1))
                    {
                        _distance = (long)Math.Sqrt(i * i + j * j);
                        if (_distance <= radius)
                        {
#if _JAGGED_ARRAYS
                            _waveHeight[_x][_y][_currentHeightBuffer] = (int)(height * Math.Cos((Single)_distance * _ratio));
#endif
#if _RECTANGULAR_ARRAYS
                            _waveHeight[_x,_y,_currentHeightBuffer] = (int)(height * Math.Cos((Single)_distance * _ratio));
#endif
#if _LINEAR_ARRAYS
                            _waveHeight[INDEX3D(_x, _y, _currentHeightBuffer)] = (int)(height * Math.Cos((Single)_distance * _ratio));
#endif
                        }
                    }
                }
            }
        }

        private void PaintWater()
        {
            _newHeightBuffer = (_currentHeightBuffer + 1) % 2;
            _image.LockBits();
#if _BUFFERED_RENDERING
            byte[] _bufferBits = new byte[_BITS * _image.Width() * _image.Height()];
            Marshal.Copy(_image.Data().Scan0, _bufferBits, 0, _bufferBits.Length);
#endif
            //
            // 
            //
            int _offX;
            int _offY;

            for (int _x = 1; _x < _BITMAP_WIDTH - 1; _x++)
            {
                for (int _y = 1; _y < _BITMAP_HEIGHT - 1; _y++)
                {
#if _JAGGED_ARRAYS
                    //
                    //  Simulate movement.
                    //
                    unchecked
                    {
                        _waveHeight[_x][_y][_newHeightBuffer] = ((
                            _waveHeight[_x - 1][_y][_currentHeightBuffer] +
                            _waveHeight[_x - 1][_y - 1][_currentHeightBuffer] +
                            _waveHeight[_x][_y - 1][_currentHeightBuffer] +
                            _waveHeight[_x + 1][_y - 1][_currentHeightBuffer] +
                            _waveHeight[_x + 1][_y][_currentHeightBuffer] +
                            _waveHeight[_x + 1][_y + 1][_currentHeightBuffer] +
                            _waveHeight[_x][_y + 1][_currentHeightBuffer] +
                            _waveHeight[_x - 1][_y + 1][_currentHeightBuffer]) >> 2)
                        - _waveHeight[_x][_y][_newHeightBuffer];
                    }
                    //
                    //  Dampenning.
                    //
                    _waveHeight[_x][_y][_newHeightBuffer] -= (_waveHeight[_x][_y][_newHeightBuffer] >> 5);
                    //
                    //
                    //
                    _offX = ((_waveHeight[_x - 1][_y][_newHeightBuffer] - _waveHeight[_x + 1][_y][_newHeightBuffer])) >> 3;
                    _offY = ((_waveHeight[_x][_y - 1][_newHeightBuffer] - _waveHeight[_x][_y + 1][_newHeightBuffer])) >> 3;
#endif
#if _RECTANGULAR_ARRAYS
                    unchecked
                    {
                        _waveHeight[_x,_y,_newHeightBuffer] = ((
                            _waveHeight[_x - 1,_y,_currentHeightBuffer] +
                            _waveHeight[_x - 1,_y - 1,_currentHeightBuffer] +
                            _waveHeight[_x,_y - 1,_currentHeightBuffer] +
                            _waveHeight[_x + 1,_y - 1,_currentHeightBuffer] +
                            _waveHeight[_x + 1,_y,_currentHeightBuffer] +
                            _waveHeight[_x + 1,_y + 1,_currentHeightBuffer] +
                            _waveHeight[_x,_y + 1,_currentHeightBuffer] +
                            _waveHeight[_x - 1,_y + 1,_currentHeightBuffer]) >> 2)
                        - _waveHeight[_x,_y,_newHeightBuffer];
                    }
                    //
                    //  Dampenning.
                    //
                    _waveHeight[_x,_y,_newHeightBuffer] -= (_waveHeight[_x,_y,_newHeightBuffer] >> 5);
                    //
                    //
                    //
                    _offX = ((_waveHeight[_x - 1,_y,_newHeightBuffer] - _waveHeight[_x + 1,_y,_newHeightBuffer])) >> 4;
                    _offY = ((_waveHeight[_x,_y - 1,_newHeightBuffer] - _waveHeight[_x,_y + 1,_newHeightBuffer])) >> 4;
#endif
#if _LINEAR_ARRAYS
                    unchecked
                    {
                        _waveHeight[INDEX3D(_x,_y, _newHeightBuffer)] = ((
                            _waveHeight[INDEX3D(_x - 1, _y + 0, _currentHeightBuffer)] +
                            _waveHeight[INDEX3D(_x - 1, _y - 1, _currentHeightBuffer)] +
                            _waveHeight[INDEX3D(_x - 0, _y - 1, _currentHeightBuffer)] +
                            _waveHeight[INDEX3D(_x + 1, _y - 1, _currentHeightBuffer)] +
                            _waveHeight[INDEX3D(_x + 1, _y + 0, _currentHeightBuffer)] +
                            _waveHeight[INDEX3D(_x + 1, _y + 1, _currentHeightBuffer)] +
                            _waveHeight[INDEX3D(_x + 0, _y + 1, _currentHeightBuffer)] +
                            _waveHeight[INDEX3D(_x - 1, _y + 1, _currentHeightBuffer)]) >> 2)
                        - _waveHeight[INDEX3D(_x, _y, _newHeightBuffer)];
                    }
                    //
                    //  Dampenning.
                    //
                    _waveHeight[INDEX3D(_x, _y, _newHeightBuffer)] -= (_waveHeight[INDEX3D(_x, _y, _newHeightBuffer)] >> 5);
                    //
                    //
                    //
                    _offX = ((_waveHeight[INDEX3D(_x - 1, _y - 0, _newHeightBuffer)] - _waveHeight[INDEX3D(_x + 1, _y + 0, _newHeightBuffer)])) >> 4;
                    _offY = ((_waveHeight[INDEX3D(_x + 0, _y - 1, _newHeightBuffer)] - _waveHeight[INDEX3D(_x + 0, _y + 1, _newHeightBuffer)])) >> 4;
#endif
                    //
                    //  Nothing to do
                    //
                    if ((_offX == 0) && (_offY == 0)) continue;
                    //
                    //  Fix boundaries
                    //
                    if (_x + _offX <= 0) _offX = -_x;
                    if (_x + _offX >= _BITMAP_WIDTH - 1) _offX = _BITMAP_WIDTH - _x - 1;
                    if (_y + _offY <= 0) _offY = -_y;
                    if (_y + _offY >= _BITMAP_HEIGHT - 1) _offY = _BITMAP_HEIGHT - _y - 1;
                    //
                    //  
                    //
#if _BUFFERED_RENDERING
                    _bufferBits[_BITS * (_x + _y * _BITMAP_WIDTH) + 0] = _bitmapOriginalBytes[_BITS * (_x + _offX + (_y + _offY) * _BITMAP_WIDTH) + 0];
                    _bufferBits[_BITS * (_x + _y * _BITMAP_WIDTH) + 1] = _bitmapOriginalBytes[_BITS * (_x + _offX + (_y + _offY) * _BITMAP_WIDTH) + 1];
                    _bufferBits[_BITS * (_x + _y * _BITMAP_WIDTH) + 2] = _bitmapOriginalBytes[_BITS * (_x + _offX + (_y + _offY) * _BITMAP_WIDTH) + 2];
                    // I dont not implement the ALPHA as previous version did. you can if you want.
                    //_bufferBits[_BITS * (_x + _y * _BITMAP_WIDTH) + 3] = alpha                    
#else
                    _image.SetPixel(_x, _y, _originalImage.GetPixel(_x + _offX, _y + _offY));
#endif
                }
            }
#if _BUFFERED_RENDERING
            Marshal.Copy(_bufferBits, 0, _image.Data().Scan0, _bufferBits.Length);
#endif
            _currentHeightBuffer = _newHeightBuffer;
            this.Invalidate();
        }

        private void waterTime_Tick(object sender, EventArgs e)
        {
            if (_image.IsLocked()) return;
            waterTime.Stop();
            PaintWater();
            waterTime.Start();
        }

        private void dropsTime_Tick(object sender, EventArgs e)
        {
            this.dropsTime.Enabled = false;

            int _percent = (int)(0.005 * (this.Width + this.Height));
            int _dropsNumber = _r.Next(_percent);
            int _drop = 0;

            for (int i = 0; i < _dropsNumber; i++)
            {
                _drop = _r.Next(_drops.Length);
                DropWater(_drops[_drop].x, _drops[_drop].y, _drops[_drop].radius, _drops[_drop].height);
            }

            this.dropsTime.Interval = _r.Next(15 * _percent) + 1;
            this.dropsTime.Enabled = true;
        }

        private void CreateWaterDrops()
        {
            int _dropX;
            int _dropY;
            int _dropRadius;
            int _height;

            int _percent = (int)(0.0015 * (this.Width + this.Height));
            _drops = new DropData[100];

            for (int i = 0; i < _drops.Length; i++)
            {
                _dropX = _r.Next(_BITMAP_WIDTH);
                _dropY = _r.Next(_BITMAP_HEIGHT);
                _height = _r.Next(400);
                _dropRadius = _r.Next(4 * _percent);

                if (_dropRadius < 4) _dropRadius = 4;

                _drops[i].x = _dropX;
                _drops[i].y = _dropY;
                _drops[i].radius = _dropRadius;
                _drops[i].height = _height;
            }

        }

        private void AboutAurora_Paint(object sender, PaintEventArgs e)
        {
            _image.Release();
            e.Graphics.DrawImage(_image.Bitmap, 0, 0, _image.Width(), _image.Height());
        }

        //右键菜单

        private void toolStripMenuItem_Exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void toolStripMenuItem_Dynamic_Click(object sender, EventArgs e)
        {
            if (this.dropsTime.Enabled == true)
            {
                toolStripMenuItem_Dynamic.Checked = false;
                this.dropsTime.Enabled = false;
                this.waterTime.Enabled = false;
            }
            else
            {
                toolStripMenuItem_Dynamic.Checked = true;
                this.dropsTime.Enabled = true;
                this.waterTime.Enabled = true;
                this.Refresh();
            }

            CreateBitmap();
            CreateBitmap();     //一次有可能停止不了，尼玛！
        }

        private void toolStripMenuItem_ReadMind_Click(object sender, EventArgs e)               //读心术
        {
            ReadMind FrmReadMind = new ReadMind();
            FrmReadMind.StartPosition = FormStartPosition.CenterScreen;
            FrmReadMind.Show();      //this 必须有，传递子窗体参数       //创建模态对话框
        }

        private void toolStripMenuItem_Weibo_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://weibo.com/aurorapro"); 
        }

        private void toolStripMenuItem_Website_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://aurora.isitestar.cn/"); 
        }

        public string strSysPath = System.Environment.GetFolderPath(Environment.SpecialFolder.System);

        private void toolStripMenuItem_DxDiag_Click(object sender, EventArgs e)
        {
            if (File.Exists(strSysPath + "\\dxdiag.exe"))
            {
                System.Diagnostics.Process.Start(strSysPath + "\\dxdiag.exe");
            }
            else MessageBox.Show("貌似你系统里面的DirectX诊断工具木有了。", "Aurora智能提示");
        }

        private void toolStripMenuItem_Perfmon_Click(object sender, EventArgs e)
        {
            if (File.Exists(strSysPath + "\\perfmon.exe"))
            {
                System.Diagnostics.Process.Start(strSysPath + "\\perfmon.exe");
            }
            else MessageBox.Show("貌似你系统里面的性能监视工具木有了。", "Aurora智能提示");
        }

        private void toolStripMenuItem_Resmon_Click(object sender, EventArgs e)
        {
            if (File.Exists(strSysPath + "\\resmon.exe"))
            {
                System.Diagnostics.Process.Start(strSysPath + "\\resmon.exe");
            }
            else MessageBox.Show("貌似你系统里面的资源监视工具木有了。", "Aurora智能提示");
        }

        private void toolStripMenuItem_Msinfo_Click(object sender, EventArgs e)
        {
            if (File.Exists(strSysPath + "\\msinfo32.exe"))
            {
                System.Diagnostics.Process.Start(strSysPath + "\\msinfo32.exe");
            }
            else MessageBox.Show("貌似你系统里面的系统信息工具木有了。", "Aurora智能提示");
        }

        private void toolStripMenuItem_Winver_Click(object sender, EventArgs e)
        {
            if (File.Exists(strSysPath + "\\winver.exe"))
            {
                System.Diagnostics.Process.Start(strSysPath + "\\winver.exe");
            }
            else MessageBox.Show("貌似你系统里面的系统版本工具木有了。", "Aurora智能提示");
        }

        private void AboutAurora_KeyDown(object sender, KeyEventArgs e)             //ESC == exit
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();               //设置按Esc键退出.
            }
        }

        private void ScrollText_Timer1_Tick(object sender, EventArgs e)             //左右滚动的字幕
        {
            label1.Left = label1.Left - 3;
            if (label1.Right < 0)
                label1.Left = this.pictureBox1.Width;
        }


#if _LINEAR_ARRAYS
        private int INDEX3D(int x, int y, int z) { unchecked { return x * _BITMAP_HEIGHT * 2 + y * 2 + z; } }
#endif

    }
}
