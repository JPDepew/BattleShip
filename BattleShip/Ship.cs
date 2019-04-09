using System;
using System.Collections.Generic;
using System.Text;

namespace BattleShip
{
	class Ship{
		public int length;
		public int hits;
        public List<Coordinate> coordinates;

        public Ship(int _length)
        {
            length = _length;
        }
    }
}