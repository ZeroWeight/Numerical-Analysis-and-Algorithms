using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ImagePro

{
    public partial class Form1 : Form
    {
        #region element

        private static Bitmap _imgOri;
        private static Bitmap _imgDes;
        private static int _centerX;
        private static int _centerY;
        private static int _row; // radius

        private static double _theta;

        private static double _amp;
        private static double _p;
        private static double _phi;

        private static short _status = 0;
        private static short _cosStatus = 0;
        private static bool _pressdown = false;
        private static Graphics _g;

        private static int[,,] _point;
        private static int[,,] _basepoint;
        private static int _selectedX = -1, _selectedY = -1;

        #endregion element

        public Form1()
        {
            string filter = "|*.jpg;*.jpeg;*.bmp;*.png";
            InitializeComponent();
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            openFileDialog1.SupportMultiDottedExtensions = false;
            openFileDialog1.Filter = filter;
            saveFileDialog1.Filter = filter;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;
            button7.Enabled = false;
            label1.Text = "Rotation angle: " + trackBar1.Value;
            groupBox6.Text = "Ready";
            label3.Text = "distortion amplitude " + (double)trackBar2.Value / 100.0;
            label4.Text = "wave length " + (double)trackBar3.Value;
            label5.Text = "initial distortion " + (double)trackBar4.Value / 100.0;
            _g = pictureBox2.CreateGraphics();
            radioButton1.Checked = true;
            radioButton4.Checked = true;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.LightBlue;
            MinimumSize = Size;
            MaximumSize = Size;
        }

        #region button

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _status = 1;
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;
            button7.Enabled = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            _status = 2;
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;
            button7.Enabled = false;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            _imgOri = (Bitmap)_imgDes.Clone();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            _imgDes = (Bitmap)pictureBox1.Image.Clone();
            pictureBox2.Image = _imgDes;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            _status = 3;
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;
            for (int i = _imgOri.Width / 2; i < _imgOri.Width; i += trackBar6.Value)
                for (int j = _imgOri.Height / 2; j < _imgOri.Height; j += trackBar5.Value)
                    _g.FillEllipse(new SolidBrush(Color.Black), i * pictureBox2.Width / _imgOri.Width - 5,
                        j * pictureBox2.Height / _imgOri.Height - 5, 10, 10);

            for (int i = _imgOri.Width / 2; i >= 0; i -= trackBar6.Value)
                for (int j = _imgOri.Height / 2; j < _imgOri.Height; j += trackBar5.Value)
                    _g.FillEllipse(new SolidBrush(Color.Black), i * pictureBox2.Width / _imgOri.Width - 5,
                        j * pictureBox2.Height / _imgOri.Height - 5, 10, 10);

            for (int i = _imgOri.Width / 2; i < _imgOri.Width; i += trackBar6.Value)
                for (int j = _imgOri.Height / 2; j >= 0; j -= trackBar5.Value)
                    _g.FillEllipse(new SolidBrush(Color.Black), i * pictureBox2.Width / _imgOri.Width - 5,
                        j * pictureBox2.Height / _imgOri.Height - 5, 10, 10);

            for (int i = _imgOri.Width / 2; i >= 0; i -= trackBar6.Value)
                for (int j = _imgOri.Height / 2; j >= 0; j -= trackBar5.Value)
                    _g.FillEllipse(new SolidBrush(Color.Black), i * pictureBox2.Width / _imgOri.Width - 5,
                        j * pictureBox2.Height / _imgOri.Height - 5, 10, 10);
        }

        #endregion button

        #region method

        private void rotation()
        {
            _theta = trackBar1.Value * Math.PI / 180;
            trackBar1.Enabled = false;
            progressBar1.Value = 0;
            groupBox6.Text = "Processing: " + progressBar1.Value + "%";
            groupBox6.Update();
            progressBar1.Update();
            for (int i = 0; i < _imgOri.Height; ++i)
            {
                for (int j = 0; j < _imgOri.Width; ++j)
                {
                    double cx = i - _centerX;
                    double cy = j - _centerY;
                    double r = Math.Sqrt(cx * cx + cy * cy);
                    if (r <= _row)
                    {
                        double ox, oy;
                        if (r > 1)
                        {
                            double angle = Math.Acos(cx / r);
                            if (cy < 0)
                                angle = 2 * Math.PI - angle;
                            angle -= _theta * (_row - r) / _row;
                            ox = r * Math.Cos(angle) + _centerX;
                            oy = r * Math.Sin(angle) + _centerY;
                        }
                        else
                        {
                            ox = _centerX;
                            oy = _centerY;
                        }
                        if (radioButton1.Checked) _imgDes.SetPixel(i, j, Nearest(ox, oy));
                        else if (radioButton2.Checked) _imgDes.SetPixel(i, j, BiLinear(ox, oy));
                        else if (radioButton3.Checked) _imgDes.SetPixel(i, j, BiCubic(ox, oy));
                    }
                    else
                    {
                        _imgDes.SetPixel(i, j, _imgOri.GetPixel(i, j));
                    }
                    progressBar1.Value = 100 * (i * _imgOri.Width + j)
                                         / _imgOri.Width / _imgOri.Height;
                }
                if (i % 100 == 0)
                {
                    groupBox6.Text = "Processing: " + progressBar1.Value + "%";
                    groupBox6.Update();
                }
            }

            pictureBox2.Image = _imgDes;
            pictureBox2.Update();
            trackBar1.Enabled = true;
        }

        private void wave()
        {
            _amp = trackBar2.Value / 100.0;
            _p = trackBar3.Value;
            _phi = trackBar4.Value / 100.0;
            progressBar1.Value = 0;
            groupBox6.Text = "Processing: " + progressBar1.Value + "%";
            groupBox6.Update();
            progressBar1.Update();
            for (int i = 0; i < _imgOri.Height; ++i)
            {
                for (int j = 0; j < _imgOri.Width; ++j)
                {
                    double cx = i - _centerX;
                    double cy = j - _centerY;
                    double r = Math.Sqrt(cx * cx + cy * cy);
                    if (r <= _row)
                    {
                        double ox, oy;
                        if (r > 1)
                        {
                            double amount = _amp *
                                            Math.Sin(r / _p * Math.PI - _phi) *
                                            Math.Exp(_phi - 5 * r / _row);

                            ox = cx * (1 + amount) + _centerX;
                            oy = cy * (1 + amount) + _centerY;
                        }
                        else
                        {
                            ox = _centerX;
                            oy = _centerY;
                        }
                        if (radioButton1.Checked) _imgDes.SetPixel(i, j, Nearest(ox, oy));
                        else if (radioButton2.Checked) _imgDes.SetPixel(i, j, BiLinear(ox, oy));
                        else if (radioButton3.Checked) _imgDes.SetPixel(i, j, BiCubic(ox, oy));
                    }
                    else
                    {
                        _imgDes.SetPixel(i, j, _imgOri.GetPixel(i, j));
                    }
                    progressBar1.Value = 100 * (i * _imgOri.Width + j)
                                         / _imgOri.Width / _imgOri.Height;
                    progressBar1.Update();
                }
                if (i % 100 == 0)
                {
                    groupBox6.Text = "Processing: " + progressBar1.Value + "%";
                    groupBox6.Update();
                }
            }

            pictureBox2.Image = _imgDes;
            pictureBox2.Update();
        }

        #endregion method

        #region file

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            pictureBox2.Image.Save(saveFileDialog1.FileName);
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            pictureBox1.Image = Image.FromFile(openFileDialog1.FileName);
            _imgOri = (Bitmap)pictureBox1.Image;
            _imgDes = (Bitmap)_imgOri.Clone();
            pictureBox2.Image = _imgDes;
            pictureBox1.Update();
            pictureBox2.Update();
            button2.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;
            button7.Enabled = true;
        }

        #endregion file

        #region mouse

        private void _MouseDown(object sender, MouseEventArgs e)
        {
            _pressdown = true;
            if (_status != 0 && e.Button == MouseButtons.Right)
            {
                _status = 0;
                button1.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = true;
                button4.Enabled = true;
                button5.Enabled = true;
                button6.Enabled = true;
                button7.Enabled = true;
                trackBar5.Enabled = true;
                trackBar6.Enabled = true;
                pictureBox2.Image = _imgDes;
                pictureBox2.Update();
                _cosStatus = 0;
            }
            if ((_status == 1 || _status == 2) && _cosStatus == 0)
            {
                _g.FillEllipse(new SolidBrush(Color.Black), e.X - 10, e.Y - 10, 20, 20);
                _centerX = e.X * _imgOri.Width / pictureBox2.Width;
                _centerY = e.Y * _imgOri.Height / pictureBox2.Height;
                _cosStatus = 1;
            }
            else if (_status == 1 && _cosStatus == 1)
            {
                rotation();
                _status = 0;
                _cosStatus = 0;
                button1.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = true;
                button4.Enabled = true;
                button5.Enabled = true;
                button6.Enabled = true;
                button7.Enabled = true;
                progressBar1.Value = 100;
                groupBox6.Text = "Ready";
                groupBox6.Update();
                progressBar1.Update();
            }
            else if (_status == 2 && _cosStatus == 1)
            {
                wave();
                _status = 0;
                _cosStatus = 0;
                button1.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = true;
                button4.Enabled = true;
                button5.Enabled = true;
                button6.Enabled = true;
                button7.Enabled = true;
                progressBar1.Value = 100;
                groupBox6.Text = "Ready";
                groupBox6.Update();
                progressBar1.Update();
            }
            else if (_status == 3)
            {
                int lx = _imgOri.Width;
                int ax = lx / 2;
                int ly = _imgOri.Height;
                int ay = ly / 2;
                int bx = trackBar6.Value;
                int by = trackBar5.Value;
                if (trackBar5.Enabled)
                {
                    trackBar5.Enabled = false;
                    trackBar6.Enabled = false;
                    _point = new int[ax / bx + (lx - ax) / bx + 3, ay / by + (ly - ay) / by + 3, 2];
                    int xdl = ax - (ax / bx + 1) * bx;
                    for (int i = 0; i < ax / bx + (lx - ax) / bx + 3; ++i)
                    {
                        int ydl = ay - (ay / by + 1) * by;
                        for (int j = 0; j < ay / by + (ly - ay) / by + 3; ++j)
                        {
                            _point[i, j, 0] = xdl;
                            _point[i, j, 1] = ydl;
                            ydl += by;
                        }
                        xdl += bx;
                    }
                    _basepoint = (int[,,])_point.Clone();
                }
                int ttyx = e.X * _imgOri.Width / pictureBox2.Width;
                int ttyy = e.Y * _imgOri.Height / pictureBox2.Height;
                int min = Int32.MaxValue;
                for (int i = 0; i < ax / bx + (lx - ax) / bx + 3; ++i)
                {
                    for (int j = 0; j < ay / by + (ly - ay) / by + 3; ++j)
                    {
                        if ((ttyx - _point[i, j, 0]) * (ttyx - _point[i, j, 0]) +
                            (ttyy - _point[i, j, 1]) * (ttyy - _point[i, j, 1]) < min)
                        {
                            min = (ttyx - _point[i, j, 0]) * (ttyx - _point[i, j, 0]) +
                                  (ttyy - _point[i, j, 1]) * (ttyy - _point[i, j, 1]);
                            _selectedX = i;
                            _selectedY = j;
                        }
                    }
                }
                _g.FillEllipse(new SolidBrush(Color.Red),
                    _point[_selectedX, _selectedY, 0] * pictureBox2.Width / _imgOri.Width - 5,
                    _point[_selectedX, _selectedY, 1] * pictureBox2.Height / _imgOri.Height - 5, 10, 10);
                Point dl = Cursor.Position;
                Cursor.Position = new Point(
                    _point[_selectedX, _selectedY, 0] * pictureBox2.Width / _imgOri.Width - e.X + dl.X,
                    _point[_selectedX, _selectedY, 1] * pictureBox2.Height / _imgOri.Height - e.Y + dl.Y
                    );
                progressBar1.Value = 100;
                groupBox6.Text = "Ready";
                groupBox6.Update();
                progressBar1.Update();
                _basepoint = (int[,,])_point.Clone();
            }
        }

        private void _MouseMove(object sender, MouseEventArgs e)
        {
            if ((_status == 1 || _status == 2) && _cosStatus == 1)
            {
                int dx = e.X * _imgOri.Width / pictureBox2.Width - _centerX;
                int dy = e.Y * _imgOri.Height / pictureBox2.Height - _centerY;
                _row = (int)Math.Sqrt(dx * dx + dy * dy);
                pictureBox2.Image = _imgOri;
                pictureBox2.Update();
                _g.FillEllipse(new SolidBrush(Color.Black),
                    _centerX * pictureBox2.Width / _imgOri.Width - 10,
                    _centerY * pictureBox2.Height / _imgOri.Height - 10,
                    20, 20);
                _g.DrawEllipse(new Pen(Color.Black, 3),
                    (_centerX - _row) * pictureBox2.Width / _imgOri.Width,
                    (_centerY - _row) * pictureBox2.Height / _imgOri.Height,
                    2 * _row * pictureBox2.Width / _imgOri.Width,
                    2 * _row * pictureBox2.Height / _imgOri.Height);
            }
            if (_status == 3 && _pressdown)
            {
                pictureBox2.Image = _imgDes;
                pictureBox2.Update();
                _point[_selectedX, _selectedY, 0] = e.X * _imgOri.Width / pictureBox2.Width;
                _point[_selectedX, _selectedY, 1] = e.Y * _imgOri.Height / pictureBox2.Height;
                for (int i = 0; i < _point.GetLength(0); ++i)
                    for (int j = 0; j < _point.GetLength(1); ++j)
                    {
                        if (i == _selectedX && j == _selectedY)
                        {
                            _g.FillEllipse(new SolidBrush(Color.Red),
                                _point[i, j, 0] * pictureBox2.Width / _imgOri.Width - 5,
                                _point[i, j, 1] * pictureBox2.Height / _imgOri.Height - 5, 10, 10);
                        }
                        else
                        {
                            _g.FillEllipse(new SolidBrush(Color.Black),
                                _point[i, j, 0] * pictureBox2.Width / _imgOri.Width - 5,
                                _point[i, j, 1] * pictureBox2.Height / _imgOri.Height - 5, 10, 10);
                        }
                    }
            }
        }

        private void _MouseUp(object sender, MouseEventArgs e)
        {
            _pressdown = false;
            if (_status == 3)
            {
                //do something
                progressBar1.Value = 0;
                groupBox6.Text = "Processing: " + progressBar1.Value + "%";
                int order = 1;
                if (radioButton5.Checked)
                    order = 3;
                for (int i = 0; i < _imgOri.Width; ++i)
                {
                    for (int j = 0; j < _imgOri.Height; ++j)
                    {
                        double currX = i - _imgOri.Width / 2;
                        double currY = j - _imgOri.Height / 2;
                        for (int count = 0; count < 10; ++count)
                        {
                            double u = currX / trackBar6.Value - Math.Floor(currX / trackBar6.Value);
                            double v = currY / trackBar5.Value - Math.Floor(currY / trackBar5.Value);
                            int ip = (int)Math.Floor(currX / trackBar6.Value) - (order / 2);
                            int jp = (int)Math.Floor(currY / trackBar5.Value) - (order / 2);

                            double disX = 0;
                            double disY = 0;
                            for (int k = 0; k <= order; ++k)
                                for (int l = 0; l <= order; ++l)
                                {
                                    double T;
                                    if (radioButton4.Checked)
                                        T = CalcBaseLinear(k, u) * CalcBaseLinear(l, v);
                                    else
                                        T = CalcBaseCubic(k, u) * CalcBaseCubic(l, v);
                                    double tx = 0;
                                    double ty = 0;
                                    int ai = ip + k + _imgOri.Width / 2 / trackBar6.Value + 1;
                                    int aj = jp + l + _imgOri.Height / 2 / trackBar5.Value + 1;
                                    if (ai >= 0 && ai < _point.GetLength(0) && aj >= 0 && aj < _point.GetLength(1))
                                    {
                                        tx = _point[ai, aj, 0] - _basepoint[ai, aj, 0];
                                        ty = _point[ai, aj, 1] - _basepoint[ai, aj, 1];
                                    }
                                    disX += T * tx;
                                    disY += T * ty;
                                }
                            double diffX = currX + disX - i + _imgOri.Width / 2;
                            double diffY = currY + disY - j + _imgOri.Height / 2;
                            if (Math.Max(Math.Abs(diffX), Math.Abs(diffY)) < 2) break;

                            currX -= diffX;
                            currY -= diffY;
                        }
                        if (radioButton1.Checked)
                            _imgDes.SetPixel(i, j, Nearest(currX + _imgOri.Width / 2, currY + _imgOri.Height / 2));
                        else if (radioButton2.Checked)
                            _imgDes.SetPixel(i, j, BiLinear(currX + _imgOri.Width / 2, currY + _imgOri.Height / 2));
                        else if (radioButton3.Checked)
                            _imgDes.SetPixel(i, j, BiCubic(currX + _imgOri.Width / 2, currY + _imgOri.Height / 2));
                        progressBar1.Value = 100 * (i * _imgOri.Width + j)
                                             / _imgOri.Width / _imgOri.Height;
                    }
                    if (i % 100 == 0)
                    {
                        groupBox6.Text = "Processing: " + progressBar1.Value + "%";
                        groupBox6.Update();
                    }
                }
                _imgOri = (Bitmap)_imgDes.Clone();
                pictureBox2.Image = _imgDes;
                pictureBox2.Update();
                progressBar1.Value = 100;
                groupBox6.Text = "Ready";
                groupBox6.Update();
                for (int i = 0; i < _point.GetLength(0); ++i)
                    for (int j = 0; j < _point.GetLength(1); ++j)
                    {
                        _g.FillEllipse(new SolidBrush(Color.Black),
                            _point[i, j, 0] * pictureBox2.Width / _imgOri.Width - 5,
                            _point[i, j, 1] * pictureBox2.Height / _imgOri.Height - 5, 10, 10);
                    }
            }
        }

        #endregion mouse

        #region trackBar

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label1.Text = "Rotation angle: " + trackBar1.Value;
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            label3.Text = "distortion amplitude " + (double)trackBar2.Value / 100.0;
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            label4.Text = "wave length " + (double)trackBar3.Value;
        }

        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            label5.Text = "initial distortion " + (double)trackBar4.Value / 100.0;
        }

        private void trackBar5_Scroll(object sender, EventArgs e)
        {
            label7.Text = "Column Interval: " + trackBar5.Value;
            if (_status == 3)
            {
                pictureBox2.Image = _imgOri;
                pictureBox2.Update();
                for (int i = _imgOri.Width / 2; i < _imgOri.Width; i += trackBar6.Value)
                    for (int j = _imgOri.Height / 2; j < _imgOri.Height; j += trackBar5.Value)
                        _g.FillEllipse(new SolidBrush(Color.Black), i * pictureBox2.Width / _imgOri.Width - 5,
                            j * pictureBox2.Height / _imgOri.Height - 5, 10, 10);

                for (int i = _imgOri.Width / 2; i >= 0; i -= trackBar6.Value)
                    for (int j = _imgOri.Height / 2; j < _imgOri.Height; j += trackBar5.Value)
                        _g.FillEllipse(new SolidBrush(Color.Black), i * pictureBox2.Width / _imgOri.Width - 5,
                            j * pictureBox2.Height / _imgOri.Height - 5, 10, 10);

                for (int i = _imgOri.Width / 2; i < _imgOri.Width; i += trackBar6.Value)
                    for (int j = _imgOri.Height / 2; j >= 0; j -= trackBar5.Value)
                        _g.FillEllipse(new SolidBrush(Color.Black), i * pictureBox2.Width / _imgOri.Width - 5,
                            j * pictureBox2.Height / _imgOri.Height - 5, 10, 10);

                for (int i = _imgOri.Width / 2; i >= 0; i -= trackBar6.Value)
                    for (int j = _imgOri.Height / 2; j >= 0; j -= trackBar5.Value)
                        _g.FillEllipse(new SolidBrush(Color.Black), i * pictureBox2.Width / _imgOri.Width - 5,
                            j * pictureBox2.Height / _imgOri.Height - 5, 10, 10);
            }
        }

        private void trackBar6_Scroll(object sender, EventArgs e)
        {
            label6.Text = "Row Interval: " + trackBar6.Value;
            if (_status == 3)
            {
                pictureBox2.Image = _imgOri;
                pictureBox2.Update();
                for (int i = _imgOri.Width / 2; i < _imgOri.Width; i += trackBar6.Value)
                    for (int j = _imgOri.Height / 2; j < _imgOri.Height; j += trackBar5.Value)
                        _g.FillEllipse(new SolidBrush(Color.Black), i * pictureBox2.Width / _imgOri.Width - 5,
                            j * pictureBox2.Height / _imgOri.Height - 5, 10, 10);

                for (int i = _imgOri.Width / 2; i >= 0; i -= trackBar6.Value)
                    for (int j = _imgOri.Height / 2; j < _imgOri.Height; j += trackBar5.Value)
                        _g.FillEllipse(new SolidBrush(Color.Black), i * pictureBox2.Width / _imgOri.Width - 5,
                            j * pictureBox2.Height / _imgOri.Height - 5, 10, 10);

                for (int i = _imgOri.Width / 2; i < _imgOri.Width; i += trackBar6.Value)
                    for (int j = _imgOri.Height / 2; j >= 0; j -= trackBar5.Value)
                        _g.FillEllipse(new SolidBrush(Color.Black), i * pictureBox2.Width / _imgOri.Width - 5,
                            j * pictureBox2.Height / _imgOri.Height - 5, 10, 10);

                for (int i = _imgOri.Width / 2; i >= 0; i -= trackBar6.Value)
                    for (int j = _imgOri.Height / 2; j >= 0; j -= trackBar5.Value)
                        _g.FillEllipse(new SolidBrush(Color.Black), i * pictureBox2.Width / _imgOri.Width - 5,
                            j * pictureBox2.Height / _imgOri.Height - 5, 10, 10);
            }
        }

        #endregion trackBar

        #region kernel

        private double CubicPolate(double v0, double v1, double v2, double v3, double fracy)
        {
            double a = (v3 - v2) - (v0 - v1);
            double b = (v0 - v1) - a;
            double c = v2 - v0;
            double d = v1;
            return d + fracy * (c + fracy * (b + fracy * a));
        }

        private Color BiCubic(double x, double y)
        {
            double[,,] ndata = new double[4, 4, 4];
            double[] x1 = new double[4];
            double[] x2 = new double[4];
            double[] x3 = new double[4];
            double[] x4 = new double[4];
            double[] y1 = new double[4];
            int[] corX = new int[4];
            int[] corY = new int[4];

            #region cordef

            corX[0] = (int)x - 1;
            corX[1] = (int)x;
            corX[2] = (int)x + 1;
            corX[3] = (int)x + 2;
            corY[0] = (int)y - 1;
            corY[1] = (int)y;
            corY[2] = (int)y + 1;
            corY[3] = (int)y + 2;
            for (int j = 0; j < 4; ++j)
                for (int k = 0; k < 4; ++k)
                {
                    if (corX[j] >= 0 && corX[j] < _imgOri.Width &&
                        corY[k] >= 0 && corY[k] < _imgOri.Height)
                    {
                        ndata[j, k, 0] = _imgOri.GetPixel(corX[j], corY[k]).B;
                        ndata[j, k, 1] = _imgOri.GetPixel(corX[j], corY[k]).G;
                        ndata[j, k, 2] = _imgOri.GetPixel(corX[j], corY[k]).R;
                        ndata[j, k, 3] = _imgOri.GetPixel(corX[j], corY[k]).A;
                    }
                    else
                    {
                        ndata[j, k, 0] = ndata[j, k, 1] = ndata[j, k, 2] = 0;
                        ndata[j, k, 3] = 255;
                    }
                }

            #endregion cordef

            for (int i = 0; i < 4; ++i)
            {
                x1[i] = CubicPolate(ndata[0, 0, i], ndata[1, 0, i], ndata[2, 0, i], ndata[3, 0, i], x - (int)x);
                x2[i] = CubicPolate(ndata[0, 1, i], ndata[1, 1, i], ndata[2, 1, i], ndata[3, 1, i], x - (int)x);
                x3[i] = CubicPolate(ndata[0, 2, i], ndata[1, 2, i], ndata[2, 2, i], ndata[3, 2, i], x - (int)x);
                x4[i] = CubicPolate(ndata[0, 3, i], ndata[1, 3, i], ndata[2, 3, i], ndata[3, 3, i], x - (int)x);
                y1[i] = CubicPolate(x1[i], x2[i], x3[i], x4[i], y - (int)y);
            }
            for (int i = 0; i < 4; ++i)
            {
                if (y1[i] > 255.0) y1[i] = 255.0;
                if (y1[i] < 0) y1[i] = 0.0;
            }
            return Color.FromArgb((int)y1[3], (int)y1[2], (int)y1[1], (int)y1[0]);
        }

        private Color BiLinear(double x, double y)
        {
            int[] arr = new int[4];
            int[] cor_x = new int[2];
            int[] cor_y = new int[2];
            if (x < -1) cor_x[0] = cor_x[1] = -1;
            else if (x < 0)
            {
                cor_x[0] = -1; cor_x[1] = 0;
            }
            else if (x >= _imgOri.Width) cor_x[0] = cor_x[1] = -1;
            else if (x >= _imgOri.Width - 1)
            {
                cor_x[0] = _imgOri.Width - 1;
                cor_x[1] = -1;
            }
            else
            {
                cor_x[0] = (int)x;
                cor_x[1] = cor_x[0] + 1;
            }
            if (y < -1) cor_y[0] = cor_y[1] = -1;
            else if (y < 0)
            {
                cor_y[0] = -1; cor_y[1] = 0;
            }
            else if (y >= _imgOri.Height) cor_y[0] = cor_y[1] = -1;
            else if (y >= _imgOri.Height - 1)
            {
                cor_y[0] = _imgOri.Height - 1;
                cor_y[1] = -1;
            }
            else
            {
                cor_y[0] = (int)y;
                cor_y[1] = cor_y[0] + 1;
            }
            int[,,] pixel = new int[2, 2, 4];
            for (int i = 0; i < 2; ++i)
                for (int j = 0; j < 2; ++j)
                {
                    if (cor_x[i] == -1 || cor_y[j] == -1) pixel[i, j, 0] = pixel[i, j, 1] =
                            pixel[i, j, 2] = pixel[i, j, 3] = 0;
                    else
                    {
                        pixel[i, j, 0] = _imgOri.GetPixel(cor_x[i], cor_y[j]).A;
                        pixel[i, j, 1] = _imgOri.GetPixel(cor_x[i], cor_y[j]).R;
                        pixel[i, j, 2] = _imgOri.GetPixel(cor_x[i], cor_y[j]).G;
                        pixel[i, j, 3] = _imgOri.GetPixel(cor_x[i], cor_y[j]).B;
                    }
                }
            double[,] xArr = new double[2, 4];
            double xfrac = x - (int)x;
            double yfrac = y - (int)y;
            double temp;
            for (int i = 0; i < 2; ++i)
                for (int j = 0; j < 4; ++j)
                    xArr[i, j] = (1 - xfrac) * pixel[0, i, j] + xfrac * pixel[1, i, j];
            for (int i = 0; i < 4; ++i)
            {
                temp = (1 - yfrac) * xArr[0, i] + yfrac * xArr[1, i];
                if (temp < 0) arr[i] = 0;
                else if (temp > 255) arr[i] = 255;
                else arr[i] = (int)temp;
            }

            return Color.FromArgb(arr[0], arr[1], arr[2], arr[3]);
        }

        private Color Nearest(double x, double y)
        {
            int nearX = (int)(x + 0.5);
            int nearY = (int)(y + 0.5);
            if (nearX < 0 || nearY < 0 || nearX >= _imgOri.Width || nearY >= _imgOri.Height)
                return Color.Black;
            else return _imgOri.GetPixel(nearX, nearY);
        }

        private double CalcBaseLinear(int i, double t)
        {
            switch (i)
            {
                case 0:
                    return 1 - t;

                case 1:
                    return t;

                default:
                    throw new IndexOutOfRangeException();
            }
        }

        private double CalcBaseCubic(int i, double t)
        {
            switch (i)
            {
                case 0:
                    return (1 + t * (-3 + t * (3 + t * (-1)))) / 6;

                case 1:
                    return (4 + t * t * (-6 + t * 3)) / 6;

                case 2:
                    return (1 + t * (3 + t * (3 + t * (-3)))) / 6;

                case 3:
                    return t * t * t / 6;

                default:
                    throw new IndexOutOfRangeException();
            }
        }

        #endregion kernel

        #region UI

        [DllImportAttribute("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImportAttribute("user32.dll")]
        private static extern bool ReleaseCapture();

        private const int WmNclbuttondown = 0xA1;
        private const int HtCaption = 0x2;

        private void button8_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WmNclbuttondown, HtCaption, 0);
            }
        }

        #endregion UI
    }
}