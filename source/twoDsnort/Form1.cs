using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace twoDSnort
{
    public partial class Form1 : Form
    {
        private int row;
        private int column;
        private int[] int_players;
        private int limit_time;
        private int timer_cnt;

        SettingDialog settingdialog = new SettingDialog();
        Panel pn_main;
        TableLayoutPanel pn_grid;

        Label[] printwhathand = new Label[2], printwhatname = new Label[2], printwhatscore = new Label[2], printwhatlimit = new Label[2];
        Button gamestart = new Button();
        Button[] board;
        TextBox console;
        Font mainfont = new Font("游ゴシック", 14);
        Timer maintimer;

        // 唯一のゲームマスター
        GameMaster gamemaster;

        // Formを継承したForm1のコンストラクタ
        public Form1()
        {
            InitializeComponent();

            // call dialog
            settingdialog.ShowDialog();
            /* Form Style Setting */
            // Title
            Text = "2DSnort";
            // Client Size
            ClientSize = new Size(1200, 800);
            // SetMaxWindow
            MaximizeBox = false;
            // SetMinWindow
            MinimizeBox = true;
            // Border Line
            FormBorderStyle = FormBorderStyle.Fixed3D;
            // StartPosition = Center
            StartPosition = FormStartPosition.CenterScreen;

            /* Value Setting */
            // サブフォームから値を受け取る
            // 行
            this.row = settingdialog.int_row;
            // 列
            this.column = settingdialog.int_column;
            // 選ばれたプレイヤ
            this.int_players = settingdialog.int_players;
            // 持ち時間
            this.limit_time = settingdialog.int_limittime / 10;
            // 自作のタイマー
            this.timer_cnt = 0;

            // ボードの大きさを定義
            board = new Button[row * column];

            /*フォーム設定ここまで*/

            /*ここから画面の部品*/

            /* パネル設置　画面の縮小拡大に対応 */
            pn_main = new Panel()
            {
                Location = new Point(0, 0),
                Size = new Size(1200, 800),
                Parent = this,
            };

            pn_grid = new TableLayoutPanel()
            {
                Location = new Point(120, 10),
                /* SetRow */
                RowCount = row,
                /* SetColumn */
                ColumnCount = column,
                AutoSize = true,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                Parent = pn_main,
            };

            /* SetButtuon */
            for (int k = 0; k < row * column; k++)
            {
                board[k] = new Button();
                board[k].Size = new Size(550 / column - 5, 550 / row - 5);
                board[k].Font = new Font("游ゴシック", 12);
                board[k].BackColor = Color.FromArgb(192, 192, 192);
                board[k].Enabled = false;

                /* Tag */
                board[k].Tag = k.ToString();
                pn_grid.Controls.Add(board[k]);

                /* Event */
                //boardをクリックしたら
                board[k].Click += new EventHandler(boardButtuon_Click);

            }

            gamestart = new Button()
            {
                Text = "対局開始",
                Location = new Point(345, 580),
                Font = mainfont,
                AutoSize = true,
                Parent = pn_main,
            };

            console = new TextBox()
            {
                Text = "ゲーム待機中...\r\n",
                Location = new Point(10, 650),
                Size = new Size(1180, 110),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Parent = pn_main,
            };

            maintimer = new Timer()
            {
                Enabled = false,
            };

            /* 先手番用 */
            printwhathand[0] = new Label()
            {
                Text = "先手",
                Location = new Point(690, 300),
                AutoSize = true,
                Font = mainfont,
                ForeColor = Color.Red,
                Parent = pn_main,
            };
            printwhatname[0] = new Label()
            {
                Location = new Point(690, 330),
                AutoSize = true,
                Font = new Font("游ゴシック", 12),
                ForeColor = Color.Red,
                Parent = pn_main,
            };
            printwhatscore[0] = new Label()
            {
                Text = (row * column).ToString(),
                Location = new Point(740, 290),
                Size = new Size(60, 35),
                Font = new Font("游ゴシック", 20),
                BackColor = Color.Red,
                ForeColor = Color.Black,
                Parent = pn_main,
            };
            printwhatlimit[0] = new Label()
            {
                Text = 10 * limit_time + "ms",
                Location = new Point(690, 360),
                Size = new Size(110, 35),
                Font = new Font("游ゴシック", 16),
                ForeColor = Color.Red,
                Parent = pn_main,
            };

            /* 後手番用 */
            printwhathand[1] = new Label()
            {
                Text = "後手",
                Location = new Point(5, 15),
                AutoSize = true,
                Font = mainfont,
                ForeColor = Color.Blue,
                Parent = pn_main,
            };
            printwhatname[1] = new Label()
            {
                Location = new Point(5, 45),
                AutoSize = true,
                Font = new Font("游ゴシック", 12),
                ForeColor = Color.Blue,
                Parent = pn_main,
            };
            printwhatscore[1] = new Label()
            {
                Text = (row * column).ToString(),
                Location = new Point(55, 5),
                Size = new Size(60, 35),
                Font = new Font("游ゴシック", 20),
                BackColor = Color.Blue,
                ForeColor = Color.White,
                Parent = pn_main,
            };
            printwhatlimit[1] = new Label()
            {
                Text = 10 * limit_time + "ms",
                Location = new Point(5, 75),
                Size = new Size(110, 35),
                Font = new Font("游ゴシック", 16),
                ForeColor = Color.Blue,
                Parent = pn_main,
            };

            // ゲームマスターを生成する
            gamemaster = new GameMaster(this);

            gamestart.Click += new EventHandler(gamestart_Click);
            maintimer.Tick += new EventHandler(Maintimer_Tick);
        }

        /// <summary>
        /// 押したときゲーム開始を告げる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gamestart_Click(object sender, EventArgs e)
        {
            // 二度とゲーム開始を押せなくする
            gamestart.Enabled = false;
            // ゲームを開始する
            gamemaster.gameStart();

            // 人間対人間以外のとき
            if (!(int_players[0] == 0 && int_players[1] == 0))
            {
                // タイマーを有効化する
                maintimer.Enabled = true;
                // タイマーの時間間隔を設定する
                maintimer.Interval = 1;
                // タイマーの計測を開始する
                maintimer.Start();
            }

        }   

        private void Maintimer_Tick(object sender, EventArgs e)
        {
            timerCall();
        }

        public void timerCall()
        {
            printwhatlimit[gamemaster.getTurn()].Text = 10 * timer_cnt + "ms";

            // interval = 1にして，カウントを増やしていき，そのカウントがgetLimit_Timeになった段階で呼び出すように変更
            if (timer_cnt == getLimit_Time())
            {
                // this.timer_Clock();
            }
            if (timer_cnt <= 0)
            {
                // 自作タイマーの値が0になる度に呼び出す
                this.timer_Clock();
            }
            // 自作タイマーを1つカウントダウン
            timer_cnt--;
        }

        public void timer_Clock()
        {
            // ゲームマスターのターンを進行する
            gamemaster.controlTime();

            // 自作タイマーを持ち時間最大に初期化する
            timer_cnt = getLimit_Time();
        }

        /// <summary>
        /// 各プレイヤ情報を画面上に描画する
        /// </summary>
        /// <param name="turn">先手(0)か後手(1)かを受け取る</param>
        /// <param name="player">どっちのプレイヤか聞く</param>
        public void printPlayerInfo(int turn, Player player)
        {
            // 各プレイヤの名前を表示する
            printwhatname[turn].Text = player.getName();
            // 各プレイヤの打てる数を表示する
            printwhatscore[turn].Text = player.getAble_Hand().ToString();
        }

        /// <summary>
        /// 全盤面のボタンを押せなくする
        /// </summary>
        public void falseBoard()
        {
            foreach(Button index in board)
            {
                index.Enabled = false;
            }
        }
        /// <summary>
        /// 盤面の押せる場所のボタンは押せるようにする
        /// </summary>
        public void trueBoard()
        {
            for (int index = 0; index < row * column; index++)
            {
                // 着手可能な場所であれば
                if (gamemaster.isCheckGameRule(translateFakeBoardXY(index)) == true)
                {
                    // その場所のボタンを押せるようにする
                    board[index].Enabled = true;
                }
            }
        }
        /// <summary>
        /// 盤面に反映する
        /// </summary>
        /// <param name="player">どちらの手か</param>
        /// <param name="position">どの位置か</param>
        public void printBoard(int player, int position)
        {
            // 位置を変換する
            int x2 = translateBoardXY(position);

            // -- 盤面に反映する --
            switch (player)
            {
                case 0:
                    // 先手領
                    changeColor(board[x2], 0);
                    break;
                case 1:
                    // 後手領
                    changeColor(board[x2], 1);
                    break;
                case 2:
                    // 公領
                    changeColor(board[x2], 99);
                    break;
            }      
        }

        public void printNumber(int owner, int position)
        {
            int x2 = translateBoardXY(position);

            board[x2].Text = owner.ToString();
        }

        public int translateBoardXY(int x1)
        {
            // 盤面が表現された座標(X1)を，表現している見た目の座標(X2)に変換する
            int x2 = x1 - 2 * (x1 / (column + 2)) - column - 1;

            return x2;
        }

        public int translateFakeBoardXY(int x2)
        {
            // 表現している見た目の座標(X2)を，盤面の表現された座標(X1)に変換する
            int x1 = (column + 2) - 1 + (2 * (x2 / column) + 1) + x2 + 1;
            return x1;
        }

        public void boardButtuon_Click(object sender, EventArgs e)
        {
            // --押されたボタンの位置を取得
            Button click = (Button)sender;
            int position = int.Parse(click.Tag.ToString());

            // 着手をますたーに知らせる
            gamemaster.boardClicked(translateFakeBoardXY(position));
        }

        public void changeColor(Button bn, int num)
        {
            switch (num)
            {
                case 0:
                    bn.BackColor = Color.FromArgb(255, 0, 0);
                    break;
                case 1:
                    bn.BackColor = Color.FromArgb(0, 0, 255);
                    break;
                case 2:
                    bn.BackColor = Color.FromArgb(192, 0, 0);
                    break;
                case 3:
                    bn.BackColor = Color.FromArgb(0, 0, 192);
                    break;
                default:
                    bn.BackColor = Color.FromArgb(64, 64, 64);
                    break;
            }
        }

        /// <summary>
        /// メインタイマーストップ
        /// </summary>
        public void timerStop()
        {
            // タイマーを停止する
            maintimer.Stop();
        }

        /// <summary>
        /// ゲーム終了のダイアログを表示する
        /// </summary>
        /// <param name="winner">勝者プレイヤー</param>
        public void endingMessage(string winner)
        {
            DialogResult result = MessageBox.Show(winner + "の勝利！\r\nリトライしますか？", "ゲーム勝敗確定", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                Application.Restart();
            }
            else if (result == DialogResult.No)
            {
                Application.Exit();
            }
        }

        /* getter */
        public int getRow()
        {
            return row;
        }

        public int getColumn()
        {
            return column;
        }

        public int[] getInt_Players()
        {
            return int_players;
        }

        public int getLimit_Time()
        {
            return limit_time;
        }

        /* setter */
        /// <summary>
        /// コンソールに追記して書くが，改行無し
        /// </summary>
        /// <param name="str"></param>
        public void setPrint(string str)
        {
            console.AppendText(str);
        }
        /// <summary>
        /// コンソールに追記して書き，改行有り
        /// </summary>
        /// <param name="str"></param>
        public void setWrite(string str)
        {
            console.AppendText(str + "\r\n");
        }

        /// <summary>
        /// コンソールに新規で書く
        /// </summary>
        /// <param name="str"></param>
        public void setConsole(string str)
        {
            console.Text = str + "\r\n";
        }
    }
}
