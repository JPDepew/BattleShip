using System;
using System.Collections.Generic;
using System.Text;

namespace BattleShip
{
    class Board
    {
        string[,] board;
        int hits;
        int[] shipLengths = { 5, 4, 3, 3, 2 };
        int maxHits = 0;

        public Board()
        {
            hits = 0;
            for(int i = 0; i < shipLengths.Length; i++)
            {
                maxHits += shipLengths[i];
            }
        }
    }
}
