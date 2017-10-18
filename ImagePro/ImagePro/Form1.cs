using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ImagePro
{
    public partial class Form1 : Form
    {
        private static Bitmap img_ori;
        private static Bitmap img_des;

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
            img_ori = (Bitmap)pictureBox1.Image;

            img_des = (Bitmap)img_ori.Clone();
            pictureBox2.Image = img_des;
            pictureBox1.Update();
            pictureBox2.Update();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < img_ori.Height; ++i)
                for (int j = 0; j < img_ori.Width; ++j)
                    img_des.SetPixel(i, j, img_ori.GetPixel(j, i));
            pictureBox2.Image = img_des;
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
    }
}