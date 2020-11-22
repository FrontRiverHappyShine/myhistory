using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace twoDSnort
{
    public class SubGameMaster:GameMaster
    {
        private Player player;
        private int able_move;
        private int board_size;
        // プレイヤの種類
        private int player_num;
        // ランダム試行回数(将来は時間切れにする)
        private int PLAY_NUM;

        public SubGameMaster(int turn, int able_move, int board_size, Player player) : base()
        {
            this.able_move = able_move;
            this.board_size = board_size;
            // 継承した親のボードを思考用のボードとして用いる
            base.count = 0;
            base.board = new int[board_size];
            base.board_put = new bool[board_size];
            this.player = player;

            // 盤面の大きさをランダムシミュレーションの回数とする
            // PLAY_NUM = board_size;
            PLAY_NUM = 2;
        }

        public void copyBoard(int[] board, bool[] board_put)
        {
            // 思考用の盤面をコピー
            Array.Copy(board, base.board, board.Length);
            Array.Copy(board_put, base.board_put, board_put.Length);
        }

        /// <summary>
        /// プレイヤー別に，打てる手の初期化を行う
        /// </summary>
        /// <param name="player_num">プレイヤの種類</param>
        public void initCandidate(int player_num)
        {
            this.player_num = player_num;
            switch(player_num)
            {
                case 0:
                    // 人間の初期化
                case 1:
                    // ランダムの初期化
                    setRandom();
                    break;
                case 2:
                    // MCの初期化
                    setRandom();
                    break;
            }
        }

        /// <summary>
        /// 打つ手の窓口
        /// </summary>
        /// <returns></returns>
        public int reallyPlay(int count)
        {
            // 現在の手番を渡す
            base.count = count;
            // 手を打っているか
            if (player.getButton_Pushed() == true)
            {
                int move = thinking();
                // 打っているときは決まった手を返す
                return move;
            }
            else
            {
                // 打っていないときは-1を固定で返す
                return -1;
            }
        }

        public int thinking()
        {
            int move = -1;
            switch(player_num)
            {
                case 1:
                    // ランダムプレイヤの思考
                    move = thinking_Random();
                    break;
                case 2:
                    // MCプレイヤの思考
                    move = thinking_MC();
                    break;
            }

            return move;
        }

        /// <summary>
        /// ランダムプレイヤの手の初期化
        /// </summary>
        public void setRandom()
        {
            // 合法手の初期化をする
            for (int inx = 0, iny = mainform.getColumn() + 3; (inx < able_move) && iny < board_size - mainform.getColumn() - 2; iny++)
            {
                if (isCheckGameRule(iny) == true)
                {
                    // 合法手であれば，それを合法手の配列に追加する
                    player.setCandidate(inx, iny);
                    // 合法手が見つかったときのみ数える
                    inx++;
                }
            }

            int[] haveCandidate = player.getCandidate();
            // 打てる手をシャッフルする
            haveCandidate = haveCandidate.OrderBy(x => Guid.NewGuid()).ToArray();
            for (int index = 0; index < haveCandidate.Length; index++)
            {
                player.setCandidate(index, haveCandidate[index]);
            }
        }

        /// <summary>
        /// 手の更新を行う
        /// </summary>
        public void updateRandom()
        {
            // 現在の合法手を取得する
            int[] current_candidate = player.getCandidate();
            // 更新後の合法手を定義する
            int[] update_candidate = new int[able_move];

            // index：探索する添え字，find_index：合法手が見つかったときの添え字
            int find_index = 0;
            for (int index = 0; index < able_move; index++)
            {
                if(current_candidate[index] == 0)
                {
                    break;
                }
                if (isCheckGameRule(current_candidate[index]) == true)
                {
                    // 合法手であれば，それを合法手の配列に追加する
                    update_candidate[find_index] = current_candidate[index];
                    // 合法手が見つかったときだけ，先頭から詰めて進める
                    find_index++;
                }
            }

            // 更新後の合法手を追加する
            for (int index = 0; index < able_move; index++)
            {
                // 更新後の合法手を，実際の合法手に追加する
                player.setCandidate(index, update_candidate[index]);
            }

        }
        /// <summary>
        /// ランダムプレイヤの思考
        /// </summary>
        /// <returns></returns>
        public int thinking_Random()
        {
            // 手を更新する
            updateRandom();
            // 合法手の配列の先頭を返す
            int move = player.getCandidate()[0];

            // 打つ手をセットしておく
            player.setPosition(move);
            return move;
        }
        /// <summary>
        /// モンテカルロ法プレイヤの思考
        /// </summary>
        /// <returns></returns>
        public int thinking_MC()
        {
            int move = -1;

            // 合法手の更新
            updateRandom();

            // 現在の合法手を取得する
            int[] current_candidate = player.getCandidate();
            // 現在の合法手それぞれの勝率を定義する
            float[] current_winrate = new float[current_candidate.Length];

            // 合法手を順に巡る
            for (int index = 0; index < able_move; index++)
            {
                // 合法手でない部分について
                if (current_candidate[index] == 0)
                {
                    // 評価せずにスキップ
                    break;
                }
                // 勝った回数を定義
                int sum = 0;
                // PLAY_NUM回数だけランダムシミュレーション
                for (int num = 0; num < PLAY_NUM; num++)
                {
                    // 勝った回数を計測
                    sum += random_Simulation(current_candidate[index]);
                }
                // それぞれの合法手について勝率を代入していく
                current_winrate[index] = sum / PLAY_NUM;
            }

            // 勝率の最大値を代入
            float max_value = current_winrate.Max();
            // 最大値があった要素の添え字を検索し取得
            int max_index = Array.IndexOf(current_winrate, max_value);
            // 返却する合法手を決定し，返却
            move = current_candidate[max_index];
            return move;
        }

        /// <summary>
        /// ランダムシミュレーションの実行
        /// </summary>
        /// <param name="first_move">現在の指し手の希望手</param>
        /// <returns>(int)勝利数(1:自分が勝利, 0:相手が勝利)</returns>
        public int random_Simulation(int first_move)
        {
            // 仮想空間のゲームマスタを定義
            GameMaster vir_gamemaster = new GameMaster(mainform);

            // プレイヤをランダムプレイヤへ入れ替える
            vir_gamemaster.makePlayers(1, 0);
            vir_gamemaster.makePlayers(1, 1);

            // ランダムプレイヤとして初期化する
            vir_gamemaster.getPlayers()[0].getSubGM().setForm(mainform);
            vir_gamemaster.getPlayers()[1].getSubGM().setForm(mainform);
            vir_gamemaster.getPlayers()[0].getSubGM().initCandidate(1);
            vir_gamemaster.getPlayers()[1].getSubGM().initCandidate(1);

            for (int index = 0; index < board.Length; index++)
            {
                // 現在の盤面を初期盤面として与える
                vir_gamemaster.setBoard(index, board[index]);
                vir_gamemaster.setBoard_Put(index, board_put[index]);
            }

            // 現在の手番をもつプレイヤが希望手を打ったことにする
            vir_gamemaster.playMove(first_move);
            // 希望手とその周囲も打ったことにする
            vir_gamemaster.killTheSurrounding(first_move);
            // 状況を更新し，ゲーム終了か調査する
            if(vir_gamemaster.isFinish() == false)
            {
                // 打って勝利したならば，それは必勝手より1を返却
                return 1;
            }
            // ターンを進める
            vir_gamemaster.setIncrease();

            // 続く限りゲームを進行する
            while(vir_gamemaster.isFinish() == true)
            {
                // vir_gamemaster.controlTime();相当を表示無しで行う
                // 盤面のコピー
                vir_gamemaster.getPlayers()[vir_gamemaster.getTurn()].getSubGM().
                    copyBoard(vir_gamemaster.getBoard(), vir_gamemaster.getBoard_Put());
                // 手を決めた状態へ
                vir_gamemaster.getPlayers()[vir_gamemaster.getTurn()].getSubGM().
                    button_Pushed();

                // 手を受け取る
                int virtual_move = vir_gamemaster.getPlayers()[vir_gamemaster.getTurn()].getSubGM().
                    reallyPlay(vir_gamemaster.getCount());
                // 手を打つ
                vir_gamemaster.playMove(virtual_move);
                // 周囲に反映させる
                vir_gamemaster.killTheSurrounding(virtual_move);
                // 手を決めていない状態へ
                vir_gamemaster.getPlayers()[vir_gamemaster.getTurn()].getSubGM().
                    button_NonPushed();
                // ゲームが終了しているとき
                if(vir_gamemaster.isFinish() == false)
                {
                    break;
                }

                // ターンを進める
                vir_gamemaster.setIncrease();
            }

            // ゲーム終了時に，その勝者と現在の指し手が一致したら
            if(vir_gamemaster.getTurn() == this.getTurn())
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        public void button_Pushed()
        {
            // 押した状態にする
            player.setButton_Pushed(true);
        }

        public void button_NonPushed()
        {
            // 押していない状態に戻す
            player.setButton_Pushed(false);
        }
    }
}
