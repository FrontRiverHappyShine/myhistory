using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace twoDSnort
{
    public class GameMaster
    {
        // ゲームのプレイヤー定数
        public const int PLAYER = 2;

        // ゲーム中のターン
        protected int count = 0;

        // ゲームの盤面(-1:禁止領域, 0:先手領, 1:後手領, 2:公領)
        protected int[] board;
        // 盤面上に実際の配置(false:置いてない, true:置いた)
        protected bool[] board_put;
        // プレイヤの種類を数値化(0:人間, 1:ランダム, 2:MC, 3:MCTS)
        private int[] int_players = new int[PLAYER];
        // プレイヤー
        private Player[] players = new Player[PLAYER];
        // プレイヤー思考用SubGameMaster
        private SubGameMaster[] sub_gamemasters = new SubGameMaster[PLAYER];
        // メインフォーム
        protected Form1 mainform;

        // コンストラクタ
        public GameMaster()
        {

        }
        public GameMaster(Form1 mainform)
        {
            // フォームを受け取る
            this.mainform = mainform;

            // -- 盤面生成 --
            this.makeBoard();

            // -- プレイヤの作成 --
            // プレイヤーの種類を受け取る
            this.int_players = mainform.getInt_Players();
            // 先手を作成
            this.makePlayers(int_players[0], 0);
            sub_gamemasters[0] = players[0].getSubGM();
            // 先手へフォームを渡す
            sub_gamemasters[0].setForm(mainform);
            // 先手へ盤面をコピーする
            sub_gamemasters[0].copyBoard(board, board_put);
            // 先手へ合法手を渡す
            sub_gamemasters[0].initCandidate(int_players[0]);
            
            // 後手を作成
            this.makePlayers(int_players[1], 1);
            sub_gamemasters[1] = players[1].getSubGM();
            // 後手へフォームを渡す
            sub_gamemasters[1].setForm(mainform);
            // 後手へ盤面をコピーする
            sub_gamemasters[1].copyBoard(board, board_put);
            // 後手へ合法手を渡す
            sub_gamemasters[1].initCandidate(int_players[1]);

            // -- プレイヤ情報の表示 --
            this.outPlayerInfo();
        }

        /// <summary>
        ///  ゲームのプレイヤを新規生成する
        ///  {"人間プレイヤ", "ランダムプレイヤ", "MCプレイヤ", "MCTSプレイヤ"};
        /// </summary>
        /// <param name="int_players"></param>
        /// <param name="int_orderofplayers"></param>
        public void makePlayers(int int_players, int int_orderofplayers)
        {
            // デフォルトで渡すマスの数を定義
            int int_defaultMN = mainform.getRow() * mainform.getColumn();
            // 表現している盤面のマスの数を定義
            int int_MN = (mainform.getRow() + 2) * (mainform.getColumn() + 2);
            // プレイヤーの種類と一致するものを選ぶ
            switch (int_players)
            {
                case 0:
                    players[int_orderofplayers] = new HumanPlayer(int_orderofplayers, int_defaultMN, int_MN);
                    break;
                case 1:
                    players[int_orderofplayers] = new RandomPlayer(int_orderofplayers, int_defaultMN, int_MN);
                    break;
                case 2:
                    players[int_orderofplayers] = new MCPlayer(int_orderofplayers, int_defaultMN, int_MN);
                    break;
                    // 新しいプレイヤを作成した場合は，此処に追加する
            }
        }

        /// <summary>
        /// ゲームの盤面を新規生成する
        /// </summary>
        public void makeBoard()
        {
            // 見せかけの行数
            int fakeRow = mainform.getRow() + 2;
            // 見せかけの列数
            int fakeColumn = mainform.getColumn() + 2;

            // 盤面生成(設定した大きさよりも一回り大きい)
            board = new int[fakeRow * fakeColumn];
            board_put = new bool[fakeRow * fakeColumn];
            // --設定した範囲外の部分は-1にする--
            for(int i = 0; i < board.Length; i++)
            {
                // --壁の領域-- 
                // 左上から右上まで
                if (i < fakeColumn)
                {
                    board[i] = -1;
                    board_put[i] = true;
                }
                // 最も左側/右側
                else if (i % fakeColumn == 0 || i % fakeColumn == fakeColumn - 1)
                {
                    board[i] = -1;
                    board_put[i] = true;
                }
                // 左下から右下まで
                else if (board.Length - fakeColumn <= i && i < board.Length)
                {
                    board[i] = -1;
                    board_put[i] = true;
                }
                // --壁以外の領域--
                else
                {
                    // 公領とする
                    board[i] = 2;
                    // 壁以外の領域をまだ置かれていないものとする
                    board_put[i] = false;
                }
            }
        }

        // ゲーム開始
        public void gameStart()
        {
            // 開始出力
            mainform.setConsole("ゲーム開始！");
            mainform.setWrite("先手 : " + players[0].getName() + " vs " + "後手 : " + players[1].getName());

            // プレイヤ情報表示
            outPlayerInfo();

            // ターン管理開始
            this.controlTurn();
        }

        // ターン管理
        public void controlTurn()
        {
            mainform.setWrite("===========================================");

            // コンソール表示部
            mainform.setWrite(getFirstSecond(getTurn()) + "番です");
            // 人とコンピュータで異なるUIを定義する
            this.onOffUI();
        }

        // 一定間隔で呼び出される
        public void controlTime()
        {
            int move = -1;

            // どちらの手番か表示する
            controlTurn();
            // UIの状態を変更する
            onOffUI();
            // 盤面をコピー
            sub_gamemasters[getTurn()].copyBoard(board, board_put);

            // 手を決めた状態にする
            sub_gamemasters[getTurn()].button_Pushed();

            // それぞれのプレイヤに打って貰い，手を受け取る
            move = sub_gamemasters[getTurn()].reallyPlay(count);

            // 手が入力されていないとき
            if (move != -1)
            {
                // 着手が正常かどうか判定
                if (this.isCheckGameRule(move) == true)
                {
                    // -- 正常な手が打たれた時 --
                    // 手を打つ
                    playMove(move);
                    // 打った手の周囲も自分のものにする
                    killTheSurrounding(move);
                    // 打てる場所を数える
                    isFinish();
                    // 状況更新
                    outPlayerInfo();
                    // 盤面出力
                    outBoard();

                    // 手を決めてない状態にする
                    sub_gamemasters[getTurn()].button_NonPushed();

                    if (isFinish() == false)
                    {
                        gameEnd(getTurn());
                    }
                }
                else
                {
                    // --正常な手が打たれなかったとき
                    mainform.setWrite("そこには置けません！");
                    // ターンを進めずにもう一度打ってもらう
                    return;
                }
            }
            // ターンを進める
            setIncrease();
            // UIの状態を変更する
            onOffUI();
        }

        public void selectMove()
        {
            int move = -1;
            // それぞれのプレイヤに打って貰い，手を受け取る
            move = players[getTurn()].reallyPlay();

            // 手が入力されていないとき
            if (move != -1)
            {
                // 着手が正常かどうか判定
                if (this.isCheckGameRule(move) == true)
                {
                    // -- 正常な手が打たれた時 --
                    // 手を打つ
                    playMove(move);
                    // 打った手の周囲も自分のものにする
                    killTheSurrounding(move);
                    // 打てる場所を数える
                    isFinish();
                    // 状況更新
                    outPlayerInfo();
                    // 盤面出力
                    outBoard();

                    // 手を入力していない状態にする
                    players[getTurn()].button_NonPushed();
                    
                    if (isFinish() == false)
                    {
                        gameEnd(getTurn());
                    }
                }
                else
                {
                    // --正常な手が打たれなかったとき
                    mainform.setWrite("そこには置けません！");
                    // ターンを進めずにもう一度打ってもらう
                    return;
                }
            }
            // ターンを進める
            setIncrease();
            // UIの状態を変更する
            onOffUI();
        }

        // ゲームルールに準じた正常な着手か
        public bool isCheckGameRule(int judgemove)
        {
            // 敵の番号
            int enemy = getEnemy(getTurn());

            // まだおいていない全ての場所
            if (board_put[judgemove] == false)
            {
                // 自分からの相対位置を指定
                int moveUp = judgemove - (mainform.getColumn() + 2);
                int moveDown = judgemove + (mainform.getColumn() + 2);
                int moveLeft = judgemove - 1;
                int moveRight = judgemove + 1;

                // -- 自分の上の判定 --
                // 自分の真上が壁でないとき，敵がいるか
                if(board[moveUp] != -1)
                {
                    if (board[moveUp] == enemy && board_put[moveUp] == true)
                    {
                        return false;
                    }
                }
                // -- 自分の下の判定 --
                // 自分の真下が壁でないとき，敵がいるか
                if (board[moveDown] != -1)
                {
                    if (board[moveDown] == enemy && board_put[moveDown] == true)
                    {
                        return false;
                    }
                }
                // -- 自分の左の判定 --
                // 自分の真左が壁でないとき，敵がいるか
                if (board[moveLeft] != -1)
                {
                    if (board[moveLeft] == enemy && board_put[moveLeft] == true)
                    {
                        return false;
                    }
                }
                // -- 自分の右の判定 --
                // 自分の真右が壁でないとき，敵がいるか
                if (board[moveRight] != -1)
                {
                    if (board[moveRight] == enemy && board_put[moveRight] == true)
                    {
                        return false;
                    }
                }

                // どれにも該当しなければ，その場所は置けると返す
                return true;
                
            }

            // その場所は置けないと返す
            return false;
        }

        /// <summary>
        /// ゲームが終了していないか判定
        /// </summary>
        /// <returns>true 終了していない false 終了している</returns>
        public bool isFinish()
        {
            // playerが置けるマス数(0:先手, 1:後手)
            int[] playerable = new int[PLAYER];
            playerable[0] = 0;
            playerable[1] = 0;

            // 無効マスでない全てのマスに対して
            for (int index = 0; index < board.Length; index++)
            {
                if (board[index] != -1)
                {
                    
                    // 先手だけが置ける(まだ置いてないかつ先手のもの)
                    if(board_put[index] == false && board[index] == 0)
                    {
                        playerable[0]++;
                    }
                    // 後手だけが置ける(まだ置いてないかつ後手のもの)
                    else if (board_put[index] == false && board[index] == 1)
                    {
                        playerable[1]++;
                    }
                    // 全プレイヤが置ける場所
                    if (board[index] == 2 && board_put[index] == false)
                    {
                        // 全プレイヤが置ける
                        playerable[0]++;
                        playerable[1]++;
                    }
                }
            }
            // 置けるマスの数を更新する
            players[0].setAble_Move(playerable[0]);
            players[1].setAble_Move(playerable[1]);
            
            // 敵が置ける場所がない
            if(playerable[getEnemy(getTurn())] == 0)
            {
                // ゲーム終了
                return false;
            }
            // 先手と後手の両者が置ける場所がある状況
            else
            {
                // ゲームは続行
                return true;
            }
        }

        // 人間ならばUIを有効化，非人間ならばUIを無効化する
        public void onOffUI()
        {
            // 人間のとき
            if(int_players[getTurn()] == 0)
            {
                mainform.trueBoard();
            }
            // 非人間のとき
            else
            {
                mainform.falseBoard();
            }
        }

        // 盤面上がクリックされたとき
        public void boardClicked(int position)
        {
            // ターンを確認し，押したプレイヤーの手を打った状態にする
            players[getTurn()].button_Pushed(position);

            // 手を選ぶ判定へ
            selectMove();
            // controlTime(); 将来はこっちに統合したい
        }

        // 手を打った処理をする
        public void playMove(int move)
        {
            // 打った後のマスにする
            board_put[move] = true;
            // 自分のものにする
            board[move] = getTurn();
        }

        /// <summary>
        /// 置いたマスの周辺を自分の領地とする
        /// </summary>
        /// <param name="position">位置</param>
        public void killTheSurrounding(int position)
        {
            // 現在の自分のターン
            int myMove = getTurn();
            // 敵を取得
            int enemy = getEnemy(getTurn());

            // 自分からの相対位置を指定
            int moveUp = position - (mainform.getColumn() + 2);
            int moveDown = position + (mainform.getColumn() + 2);
            int moveLeft = position - 1;
            int moveRight = position + 1;

            
            // -- 自分の上の判定 --
            // 自分の真上が壁でないとき
            if (board[moveUp] != -1)
            {
                if (board[moveUp] == enemy)
                {
                    // 既に敵の領地のとき，壁にする
                    board[moveUp] = -1;
                }
                else
                {
                    board[moveUp] = myMove;
                }
            }
            // -- 自分の下の判定 --
            // 自分の真下が壁でないとき
            if (board[moveDown] != -1)
            {
                if (board[moveDown] == enemy)
                {
                    // 既に敵の領地のとき，壁にする
                    board[moveDown] = -1;
                }
                else
                {
                    board[moveDown] = myMove;
                }
            }
            // -- 自分の左の判定 --
            // 自分の真左が壁でないとき
            if (board[moveLeft] != -1)
            {
                if (board[moveLeft] == enemy)
                {
                    // 既に敵の領地のとき，壁にする
                    board[moveLeft] = -1;
                }
                else
                {
                    board[moveLeft] = myMove;
                }
            }
            // -- 自分の右の判定 --
            // 自分の真右が壁でないとき
            if (board[moveRight] != -1)
            {
                if (board[moveRight] == enemy)
                {
                    // 既に敵の領地のとき，壁にする
                    board[moveRight] = -1;
                }
                else
                {
                    board[moveRight] = myMove;
                }
            }
        }

        // 各プレイヤ情報の出力
        public void outPlayerInfo()
        {
            for (int index = 0; index < PLAYER; index++)
            {
                mainform.printPlayerInfo(index, players[index]);
            }
        }

        // 盤面の出力
        public void outBoard()
        {
            for(int index = 0; index < board.Length; index++)
            {
                // 置いてあるマスに対して
                if (board[index] != -1 && board_put[index] == true)
                {
                    mainform.printBoard(board[index], index);
                }
            }
        }

        // ゲームの終了
        public void gameEnd(int winner)
        {
            // タイマーを停止
            mainform.timerStop();
            // ゲーム終了のダイアログを表示
            mainform.endingMessage("(" +getFirstSecond(winner)+")" + players[winner].getName());
        }

        // ログ書き込み
        public void writeLog()
        {

        }

        // setter
        public void setBoard(int index, int value)
        {
            board[index] = value;
        }
        public void setBoard_Put(int index, bool value)
        {
            board_put[index] = value;
        }

        public void setIncrease()
        {
            // カウントを増やす
            this.count++;
        }

        public void setForm(Form1 mainform)
        {
            this.mainform = mainform;
        }

        // getter
        /// <summary>
        /// 盤面を返す
        /// </summary>
        /// <returns></returns>
        public int[] getBoard()
        {
            return board;
        }

        public bool[] getBoard_Put()
        {
            return board_put;
        }

        /// <summary>
        /// プレイヤを返す
        /// </summary>
        /// <returns></returns>
        public Player[] getPlayers()
        {
            return players;
        }

        /// <summary>
        /// 進行しているターン数を返す
        /// </summary>
        /// <returns></returns>
        public int getCount()
        {
            return count;
        }
        /// <summary>
        /// 今のプレイヤーのターンとして返す
        /// </summary>
        /// <returns>ターン(0:先手番，1:後手番)</returns>
        public int getTurn()
        {
            // 今打っているプレイヤーの順番を返す
            return count % 2;
        }

        /// <summary>
        /// 敵プレイヤーの手番を返す
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public int getEnemy(int num)
        {
            if(num == 0)
            {
                return 1;
            }
            else if(num == 1)
            {
                return 0;
            }
            return -1;
        }
        /// <summary>
        /// 数字に対応する「先手」または「後手」の文字列を返す
        /// </summary>
        /// <param name="num"></param>
        /// <returns>(0)先手，(1)後手</returns>
        public string getFirstSecond(int num)
        {
            if(num == 0)
            {
                return "先手";
            }
            else if(num == 1)
            {
                return "後手";
            }

            return null;
        }
    }
}
