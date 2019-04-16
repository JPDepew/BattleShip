using System;
using System.Collections.Generic;
using System.Text;

namespace BattleShip
{
    class Ship
    {
        public int length;
        public int hits;
        public List<Coordinate> coordinates;

        public Ship()
        {
            coordinates = new List<Coordinate>();
        }

        public Ship(int _length)
        {
            length = _length;
            coordinates = new List<Coordinate>();
        }

        public void AddSunkShips(List<Coordinate> _coordinates)
        {
            foreach(Coordinate c in _coordinates)
            {
                coordinates.Add(c);
            }
        }

        public bool AreCoordinatesAligned()
        {
            for (int i = 0; i < coordinates.Count; i++)
            {
                for (int j = 0; j < coordinates.Count; j++)
                {
                    if (coordinates[i].x != coordinates[j].x && coordinates[i].y != coordinates[j].y)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        //public Coordinate GetMisalignedCoordinates()
        //{
        //    List<Coordinate> tempCoordinates = new List<Coordinate>();
        //    int ys = 0;
        //    int xs = 0;

        //    for (int i = 0; i < coordinates.Count; i++)
        //    {
        //        for (int j = 0; j < coordinates.Count; j++)
        //        {
        //            if (coordinates[i].x == coordinates[j].x)
        //            {
        //                xs++;
        //            }
        //            else if(coordinates[i].y == coordinates[j].y)
        //            {
        //                ys++;
        //            }
        //        }
        //    }

        //    for (int i = 0; i < coordinates.Count; i++)
        //    {
        //        for (int j = 0; j < coordinates.Count; j++)
        //        {
        //            // it is a vertical ship
        //            if (xs > ys)
        //            {
                        
        //            }
        //            // it is a horizontal ship
        //            else
        //            {
                        
        //            }
        //        }
        //    }
        //}
    }
}