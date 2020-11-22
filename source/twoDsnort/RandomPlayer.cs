using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace twoDSnort
{
    public class RandomPlayer : Player
    {
        public RandomPlayer(int turn, int able_move, int board_size) : base(turn, able_move, board_size)
        {
            base.name = "ランダムP" + turn;
            base.board = new int[able_move];
            initCandidate();
        }       
        // setter
        
        // getter

    }
}