using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CrazyTank
{
    public partial class FormMain : Form
    {
        public static int mapWidth = 40;
        public static int mapHeight = 30;

        private System.Timers.Timer paintTimer;
        private int timerSpan = 50;

        GameImages img = new GameImages();
        Controller ctrl;

        float[] color = new float[3];

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            this.GameStage.Width = mapWidth * Tile.size.Width;
            this.GameStage.Height = mapHeight * Tile.size.Height;
            this.MinimumSize = GameStage.Size;
            this.BackColor = Color.Black;

            picTank.Image = GameImages.tankPic[0][0];

            ctrl = new Controller(GameStage.Width, GameStage.Height);
        }

        private void bulletTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            GameStage.Invalidate();
        }

        private void FormMain_KeyPress(object sender, KeyPressEventArgs e)
        {
            ctrl.KeyPress(sender, e);
        }

        private void FormMain_KeyUp(object sender, KeyEventArgs e)
        {
            ctrl.KeyUp(sender, e);
        }

        private void GameStage_Paint(object sender, PaintEventArgs e)
        {
            ctrl.Paint(e.Graphics);
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            lblState.Text = "连接中...";
            if (ctrl.NetClientConnect(txtIP.Text, Convert.ToInt32(txtPort.Text),color))
            {
                pnlConnect.Enabled = false; //隐藏panel
                pnlConnect.Visible = false;
                this.Focus(); //窗体获取焦点

                paintTimer = new System.Timers.Timer(timerSpan);
                paintTimer.Elapsed += new System.Timers.ElapsedEventHandler(bulletTimer_Elapsed);
                paintTimer.AutoReset = true;
                paintTimer.Start();
            }
            else
            {
                lblState.Text = "连接失败";
            }
        }

        private void trbAll_Scroll(object sender, EventArgs e)
        {
            color[0] = (float)trbRed.Value / 100;
            color[1] = (float)trbGreen.Value / 100;
            color[2] = (float)trbBlue.Value / 100;
            //重新着色
            picTank.Image = GameImages.Recoloring(GameImages.tankPic[0][0], color);
        }
    }
}
