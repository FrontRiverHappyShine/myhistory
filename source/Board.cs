using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace twoDSnort
{
    public class Board
    {
        // ゲームの盤面の領地(-1:禁止領域, 0:先手領, 1:後手領, 2:公領)
        private int[] territory;
        // 盤面上に実際の配置(false:置いてない, true:置いた)
        private bool[] putting;

        public Board()
        {

        }

        // setter
        public void setTerritory(int index, int value)
        {
            this.territory[index] = value;
        }

        public void setPutting(int index, bool value)
        {
            this.putting[index] = value;
        }

        // getter
        public int[] getTerritory()
        {
            return territory;
        }

        public bool[] getPutting()
        {
            return putting;
        }
    }
}
