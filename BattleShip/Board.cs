using System;
using System.Collections.Generic;
using System.Text;

namespace BattleShip
{
    class Board
    {
        string[,] board;
        string[,] enemyView;
        int[] shipLengths = { 5, 4, 3, 3, 2 }; // add array of ships
        Ship[] ships = new Ship[5];
        int boardSize = 10;
        int hits;
        int maxHits = 0;

        public Board()
        {
            hits = 0;
            // initializing the number of max hits
            for (int i = 0; i < shipLengths.Length; i++)
            {
                maxHits += shipLengths[i];
            }
            // initializing ships to correct lengths
            for(int i = 0; i < ships.Length; i++)
            {
                ships[i] = new Ship(shipLengths[i]);
            }
            board = new string[boardSize, boardSize];
            enemyView = new string[boardSize, boardSize];
            InitializeBoard(board);
            InitializeBoard(enemyView);
            Console.WriteLine(maxHits);
        }

        string GetLocation(int x, int y)
        {
            return board[y, x];
        }

        public int GetHits()
        {
            return hits;
        }

        public int GetMaxHits()
        {
            return maxHits;
        }

        /// <summary>
        /// Sets a location to a certain string value. If the value is [X] then the total hits
        /// for the board increases.
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <param name="setTo">string value to set the cell to</param>
        void SetLocation(int x, int y, string setTo)
        {
            if(setTo == "[X]")
            {
                hits++;
            }
            board[y, x] = setTo;
        }

        void InitializeBoard(string[,] board)
        {
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    board[i, j] = "[ ]";
                }
            }
        }

        /// <summary>
        /// Sets up all ships in shipLengths, checking to make sure it doesn't go off edge.
        /// </summary>
        public void SetUpShips()
        {
            int shipIndex = 0;

            while (shipIndex < shipLengths.GetLength(0))
            {
                PrintBoard();

                // Getting coordinates
                Console.WriteLine("Enter coordinates for ship of length " + shipLengths[shipIndex]);
                Console.Write("Enter X coordinate: ");
                int x = Convert.ToInt32(Console.ReadLine());
                Console.Write("Enter Y coordinate: ");
                int y = Convert.ToInt32(Console.ReadLine());

                // Getting orientation
                Console.Write("Enter d to orient the ship downwards, or r to orient the ship to the right: ");
                string orientation;
                do
                {
                    orientation = Console.ReadLine();
                } while (orientation != "d" && orientation != "r");

                // Checking to make sure ship doesn't go off side
                if (orientation == "d")
                {
                    if (shipLengths[shipIndex] + y > board.GetLength(0))
                    {
                        Console.WriteLine("Ship goes off edge. Press enter to try again.");
                        Console.ReadKey();
                        Console.Clear();
                        continue;
                    }
                }
                else
                {
                    if (shipLengths[shipIndex] + x > board.GetLength(1))
                    {
                        Console.WriteLine("Ship goes off edge. Press enter to try again.");
                        Console.ReadKey();
                        Console.Clear();
                        continue;
                    }
                }

                // If all is well, place the ship.
                PlaceShip(board, shipIndex, x, y, orientation);

                Console.Clear();
                shipIndex++;
            }

            Console.WriteLine("Setup complete. Press Enter to continue.");
            PrintBoard();
            Console.ReadKey();
            Console.Clear();
        }

        /// <summary>
        /// Changes cells on table to [S] to represent a ship based on the orientation
        /// </summary>
        /// <param name="playerBoard"></param>
        /// <param name="shipLength"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="orientation">The orientation of the ship (down or right)</param>
        void PlaceShip(string[,] playerBoard, int shipIndex, int x, int y, string orientation)
        {
            string shipType = "[" + (5 - shipIndex).ToString() + "]";
            for (int i = 0; i < shipLengths[shipIndex]; i++)
            {
                playerBoard[y, x] = shipType;
                
                if (orientation == "d")
                {
                    y++;
                }
                else
                {
                    x++;
                }
            }
        }

        public void PrintBoard()
        {
            Console.Write("  ");
            for (int i = 0; i < board.GetLength(0); i++)
            {
                Console.Write("  " + i + " ");
            }
            Console.WriteLine();
            for (int i = 0; i < board.GetLength(1); i++)
            {
                Console.Write(i + "  ");
                for (int j = 0; j < board.GetLength(0); j++)
                {
                    Console.Write(board[i, j] + " ");
                }
                Console.WriteLine();
            }
        }

        public void PrintEnemyView()
        {
            Console.Write("  ");
            for (int i = 0; i < enemyView.GetLength(0); i++)
            {
                Console.Write("  " + i + " ");
            }
            Console.WriteLine();
            for (int i = 0; i < enemyView.GetLength(1); i++)
            {
                Console.Write(i + "  ");
                for (int j = 0; j < enemyView.GetLength(0); j++)
                {
                    Console.Write(enemyView[i, j] + " ");
                }
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Asks the player for x and y coordinates and checks if it is a hit, miss, or if they already went there.
        /// If a ship is sunk, then it returns SUNK
        /// </summary>
        /// <param name="enemyBoard"></param>
        /// <returns>hitStatus - HIT, SUNK, RETRY, MISS</returns>
        public HitStatus Move(Board enemyBoard)
        {
            HitStatus hitStatus;
            Console.Write("Enter X coordinate: ");
            int x = Convert.ToInt32(Console.ReadLine());
            Console.Write("Enter Y coordinate: ");
            int y = Convert.ToInt32(Console.ReadLine());

            // TODO: should add a check to make sure x and y are not out of bounds - if so return hitStatus.RETRY

            // This tests the location to see what number it is. It just used number 1 - 5
            // to make things easier. Because of the array setup, index 0 is the ship of length 5.
            switch(enemyBoard.GetLocation(x, y))
            {
                case "[5]":
                    ships[0].hits++;
                    hitStatus = ships[0].hits >= ships[0].length ? HitStatus.SUNK : HitStatus.HIT;

                    enemyBoard.SetLocation(x, y, "[X]");
                    enemyView[y, x] = "[X]";
                    break;
                case "[4]":
                    ships[1].hits++;
                    hitStatus = ships[1].hits >= ships[1].length ? HitStatus.SUNK : HitStatus.HIT;

                    enemyBoard.SetLocation(x, y, "[X]");
                    enemyView[y, x] = "[X]";
                    break;
                case "[3]":
                    ships[2].hits++;
                    hitStatus = ships[2].hits >= ships[2].length ? HitStatus.SUNK : HitStatus.HIT;

                    enemyBoard.SetLocation(x, y, "[X]");
                    enemyView[y, x] = "[X]";
                    break;
                case "[2]":
                    ships[3].hits++;
                    hitStatus = ships[3].hits >= ships[3].length ? HitStatus.SUNK : HitStatus.HIT;

                    enemyBoard.SetLocation(x, y, "[X]");
                    enemyView[y, x] = "[X]";
                    break;
                case "[1]":
                    ships[4].hits++;
                    hitStatus = ships[4].hits >= ships[4].length ? HitStatus.SUNK : HitStatus.HIT;

                    enemyBoard.SetLocation(x, y, "[X]");
                    enemyView[y, x] = "[X]";
                    break;
                case "[X]":
                case "[O]":
                    Console.WriteLine("You already went there. Press enter to try again.");
                    Console.ReadKey();
                    hitStatus = HitStatus.RETRY;
                    break;
                default:
                    enemyBoard.SetLocation(x, y, "[O]");
                    enemyView[y, x] = "[O]";
                    hitStatus = HitStatus.MISS;
                    break;
            }
            return hitStatus;
        }
    }
}
