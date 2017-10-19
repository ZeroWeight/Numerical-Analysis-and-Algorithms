using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ImagePro
{
    public partial class Form1 : Form
    {
        private static Bitmap _imgOri;
        private static Bitmap _imgDes;
        private static int row = 250;
        private static double theta = Math.PI;
        private static double amp = 1;
        private static double p = 20;
        private static double phi = 0;

        public Form1()
        {
            string filter = "|*.jpg;*.jpeg;*.bmp;*.png";
            InitializeComponent();
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.BorderStyle = BorderStyle.FixedSingle;
            pictureBox2.BorderStyle = BorderStyle.FixedSingle;
            openFileDialog1.SupportMultiDottedExtensions = false;
            openFileDialog1.Filter = filter;
            saveFileDialog1.Filter = filter;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            pictureBox1.Image = Image.FromFile(openFileDialog1.FileName);
            _imgOri = (Bitmap)pictureBox1.Image;
            _imgDes = (Bitmap)_imgOri.Clone();
            pictureBox2.Image = _imgDes;
            pictureBox1.Update();
            pictureBox2.Update();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < _imgOri.Height; ++i)
                for (int j = 0; j < _imgOri.Width; ++j)
                {
                    double cx = i - _imgOri.Height / 2.0;
                    double cy = j - _imgOri.Width / 2.0;
                    double r = Math.Sqrt(cx * cx + cy * cy);
                    if (r <= row)
                    {
                        double ox, oy;
                        if (r > 1)
                        {
                            double angle = Math.Acos(cx / r);
                            if (cy < 0)
                                angle = 2 * Math.PI - angle;
                            angle -= theta * (row - r) / row;
                            ox = r * Math.Cos(angle) + _imgOri.Height / 2.0;
                            oy = r * Math.Sin(angle) + _imgOri.Width / 2.0;
                        }
                        else
                        {
                            ox = _imgOri.Height / 2.0;
                            oy = _imgOri.Width / 2.0;
                        }
                        _imgDes.SetPixel(i, j, BiCubic(ox, oy));
                    }
                    else
                    {
                        _imgDes.SetPixel(i, j, _imgOri.GetPixel(i, j));
                    }
                }

            pictureBox2.Image = _imgDes;
            pictureBox2.Update();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            pictureBox2.Image.Save(saveFileDialog1.FileName);
        }

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

            #region cordef_x

            if (x < 0)
            {
                corX[0] = corX[1] = corX[2] = corX[3] = 0;
            }
            else if (x < 1)
            {
                corX[0] = corX[1] = 0;
                corX[2] = 1;
                corX[3] = 2;
            }
            else if (x > _imgOri.Height - 1)
            {
                corX[0] = corX[1] = corX[2] = corX[3] = _imgOri.Height - 1;
            }
            else if (x > _imgOri.Height - 2)
            {
                corX[0] = _imgOri.Height - 3;
                corX[1] = _imgOri.Height - 2;
                corX[2] = _imgOri.Height - 1;
                corX[3] = _imgOri.Height - 1;
            }
            else
            {
                corX[0] = (int)x - 1;
                corX[1] = (int)x;
                corX[2] = (int)x + 1;
                corX[3] = (int)x + 2;
            }

            #endregion cordef_x

            #region cordef_y

            if (y < 0)
            {
                corY[0] = corY[1] = corY[2] = corY[3] = 0;
            }
            else if (y < 1)
            {
                corY[0] = corY[1] = 0;
                corY[2] = 1;
                corY[3] = 2;
            }
            else if (y > _imgOri.Width - 1)
            {
                corY[0] = corY[1] = corY[2] = corY[3] = _imgOri.Width - 1;
            }
            else if (y > _imgOri.Width - 2)
            {
                corY[0] = _imgOri.Width - 3;
                corY[1] = _imgOri.Width - 2;
                corY[2] = _imgOri.Width - 1;
                corY[3] = _imgOri.Width - 1;
            }
            else
            {
                corY[0] = (int)y - 1;
                corY[1] = (int)y;
                corY[2] = (int)y + 1;
                corY[3] = (int)y + 2;
            }

            #endregion cordef_y

            #region color_def

            for (int j = 0; j < 4; ++j)
                for (int k = 0; k < 4; ++k)
                {
                    ndata[j, k, 0] = _imgOri.GetPixel(corX[j], corY[k]).B;
                    ndata[j, k, 1] = _imgOri.GetPixel(corX[j], corY[k]).G;
                    ndata[j, k, 2] = _imgOri.GetPixel(corX[j], corY[k]).R;
                    ndata[j, k, 3] = _imgOri.GetPixel(corX[j], corY[k]).A;
                }

            #endregion color_def

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

        private void button4_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < _imgOri.Height; ++i)
                for (int j = 0; j < _imgOri.Width; ++j)
                {
                    double cx = i - _imgOri.Height / 2.0;
                    double cy = j - _imgOri.Width / 2.0;
                    double r = Math.Sqrt(cx * cx + cy * cy);
                    if (r <= row)
                    {
                        double ox, oy;
                        if (r > 1)
                        {
                            double amount = amp *
                                Math.Sin(r / p * Math.PI - phi) *
                                Math.Exp(phi - 5 * r / row);

                            ox = cx * (1 + amount) + _imgOri.Height / 2.0;
                            oy = cy * (1 + amount) + _imgOri.Width / 2.0;
                        }
                        else
                        {
                            ox = _imgOri.Height / 2.0;
                            oy = _imgOri.Width / 2.0;
                        }
                        _imgDes.SetPixel(i, j, BiCubic(ox, oy));
                    }
                    else
                    {
                        _imgDes.SetPixel(i, j, _imgOri.GetPixel(i, j));
                    }
                }

            pictureBox2.Image = _imgDes;
            pictureBox2.Update();
        }
    }
}