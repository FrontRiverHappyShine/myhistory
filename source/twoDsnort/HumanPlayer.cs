using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace twoDSnort
{
    public class HumanPlayer : Player
    {
        public HumanPlayer(int turn, int able_move, int board_size) : base(turn, able_move, board_size)
        {
            name = "人間P" + turn;
        }
        public override int reallyPlay()
        {
            // 手を打っているか
            if (button_pushed == true)
            {
                // 打っている
                return base.position;
            }
            else
            {
                // 打っていない
                return -1;
            }
        }    
        

        public override void button_Pushed(int position)
        {
            // 押した状態にする
            button_pushed = true;
            base.position = position;
        }

        public override void button_NonPushed()
        {
            // 押していない状態に戻す
            button_pushed = false;
        }
        public bool isButtonPushed()
        {
            // 押した状態になっていたら
            if (button_pushed == true)
            {
                // 押してない状態にして
                button_pushed = false;
                // 手を打ったと返す
                return true;
            }
            else
            {
                return false;
            }
        }
        
    }
    
}
