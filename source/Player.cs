using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace twoDSnort
{
    public class Player
    {
        // 各プレイヤがもつ名前
        protected string name;
        // 各プレイヤがもつターン
        protected int turn;
        // 各プレイヤの元の盤面そのもの
        protected int[] board;
        // 各プレイヤの思考ルーチン
        protected SubGameMaster sub_gamemaster;
        // 各プレイヤの着手した位置
        protected int position;
        // 各プレイヤの置ける数
        protected int able_move;
        // ボタンを押したか(false:押してない, true:押した)
        protected bool button_pushed = false;
        // 各プレイヤの候補手一覧
        protected int[] candidate;

        // ボードの大きさ
        protected int board_size;

        public Player(int turn, int able_move, int board_size)
        {
            this.turn = turn;
            this.able_move = able_move;
            this.board_size = board_size;
            this.position = 0;
            this.candidate = new int[able_move];
            sub_gamemaster = new SubGameMaster(turn, able_move, board_size, this);
        }

        public virtual void initCandidate()
        {

        }

        public virtual int reallyPlay()
        {
            return -1;
        }

        public virtual void button_Pushed(int position)
        {

        }

        public virtual void button_NonPushed()
        {

        }

        // setter
        public void setAble_Move(int able_move)
        {
            this.able_move = able_move;
        }

        public void setButton_Pushed(bool button_pushed)
        {
            this.button_pushed = button_pushed;
        }

        public void setCandidate(int index, int value)
        {
            candidate[index] = value;
        }

        public void setPosition(int position)
        {
            this.position = position;
        }

        // getter
        public int getTurn()
        {
            return turn;
        }

        public string getName()
        {
            return name;
        }

        public int getAble_Hand()
        {
            return able_move;
        }

        public bool getButton_Pushed()
        {
            return button_pushed;
        }
        
        public SubGameMaster getSubGM()
        {
            return sub_gamemaster;
        }

        public virtual int[] getCandidate()
        {
            return candidate;
        }

        public int getBoardSize()
        {
            return board.Length;
        }
    }
}
