using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace twoDSnort
{
    public partial class SettingDialog : Form
    {
        // デフォルトの行の大きさ
        private const int DEFAULT_ROW = 4;
        // デフォルトの列の大きさ
        private const int DEFAULT_COLUMN = 4;
        // デフォルトの先手プレイヤー
        private const int DEFAULT_PLAYER1 = 1;
        // デフォルトの後手プレイヤー
        private const int DEFAULT_PLAYER2 = 2;
        // デフォルトの持ち時間
        private const int DEFAULT_LIMITTIME = 100;

        public int int_row { get; private set; } = 0;
        public int int_column { get; private set; } = 0;
        public int[] int_players { get; private set; } = new int[2];
        public int int_limittime { get; private set; } = DEFAULT_LIMITTIME;

        private string[] player_list = {"人間プレイヤ", "ランダムプレイヤ", "MCプレイヤ", "MCTSプレイヤ"};
        
        Label[] lb_instraction = new Label[7];
        NumericUpDown nud_row, nud_column, nud_limittime;
        ComboBox cb_firstplayer, cb_secondplayer;
        Button btn_OK;

        public SettingDialog()
        {
            
            Text = "初期ゲーム設定";
            Size = new Size(300, 380);
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            lb_instraction[0] = new Label()
            {
                Text = "盤面サイズ",
                Location = new Point(10, 10),
                AutoSize = true,
                Parent = this,
            };

            lb_instraction[1] = new Label()
            {
                Text = "行",
                Location = new Point(40, 30),
                AutoSize = true,
                Parent = this,
            };

            nud_row = new NumericUpDown()
            {
                Minimum = 1,
                Maximum = 30,
                Value = DEFAULT_ROW,
                Location = new Point(70, 40),
                Size = new Size(60, 10),
                Parent = this,
            };

            lb_instraction[2] = new Label()
            {
                Text = "列",
                Location = new Point(150, 30),
                AutoSize = true,
                Parent = this,
            };

            nud_column = new NumericUpDown()
            {
                Minimum = 1,
                Maximum = 30,
                Value = DEFAULT_COLUMN,
                Location = new Point(180, 40),
                Size = new Size(60, 10),
                Parent = this,
            };

            lb_instraction[3] = new Label()
            {
                Text = "先手プレイヤー",
                Location = new Point(10, 80),
                AutoSize = true,
                Parent = this,
            };
            cb_firstplayer = new ComboBox()
            {
                Text = player_list[DEFAULT_PLAYER1],
                Location = new Point(70, 100),
                Size = new Size(170, 20),
                Parent = this,
            };
            cb_firstplayer.Items.AddRange(player_list);

            lb_instraction[4] = new Label()
            {
                Text = "後手プレイヤー",
                Location = new Point(10, 140),
                AutoSize = true,
                Parent = this,
            };
            cb_secondplayer = new ComboBox()
            {
                Text = player_list[DEFAULT_PLAYER2],
                Location = new Point(70, 160),
                Size = new Size(170, 20),
                Parent = this,
            };
            cb_secondplayer.Items.AddRange(player_list);

            lb_instraction[5] = new Label()
            {
                Text = "持ち時間(1～90000ms)",
                Location = new Point(10, 200),
                AutoSize = true,
                Parent = this,
            };

            btn_OK = new Button()
            {
                Text = "OK",
                Location = new Point(100, 280),
                Size = new Size(100, 20),
                TabIndex = 0,
                Parent = this,
            };

            nud_limittime = new NumericUpDown()
            {
                Minimum = 1,
                Maximum = 90000,
                Increment = 1000,
                // default
                Value = DEFAULT_LIMITTIME,
                Location = new Point(70, 220),
                Size = new Size(60, 10),
                Parent = this,
            };
            lb_instraction[6] = new Label()
            {
                Text = "(ms)",
                Location = new Point(140, 225),
                AutoSize = true,
                Parent = this,
            };

            btn_OK.Click += Btn_OK_Click;
        }

        private void Btn_OK_Click(object sender, EventArgs e)
        {
            // Clickボタンが押された際の処理をする
            ok_Click();
        }

        private void ok_Click()
        {
            // OKボタンとして処理する
            DialogResult = DialogResult.OK;

            // コンボボックスが何も選択されていないときのデフォルト
            if (cb_firstplayer.SelectedIndex == -1)
            {
                cb_firstplayer.SelectedIndex = DEFAULT_PLAYER1;
            }
            if (cb_secondplayer.SelectedIndex == -1)
            {
                cb_secondplayer.SelectedIndex = DEFAULT_PLAYER2;
            }

            // 各コントロールの値を代入する
            // 行の値
            int_row = (int)nud_row.Value;
            // 列の値
            int_column = (int)nud_column.Value;
            // 先手プレイヤの種類
            int_players[0] = cb_firstplayer.SelectedIndex;
            // 後手プレイヤの種類
            int_players[1] = cb_secondplayer.SelectedIndex;
            // 持ち時間の値
            int_limittime = (int)nud_limittime.Value;

            // このダイアログを閉じる
            this.Close();
        }
    }
}
