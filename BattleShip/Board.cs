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
        int boardSize = 10;
        int hits;
        int maxHits = 0;

        public Board()
        {
            hits = 0;
            for (int i = 0; i < shipLengths.Length; i++)
            {
                maxHits += shipLengths[i];
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
                PlaceShip(board, shipLengths[shipIndex], x, y, orientation);

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
        void PlaceShip(string[,] playerBoard, int shipLength, int x, int y, string orientation)
        {
            for (int i = 0; i < shipLength; i++)
            {
                playerBoard[y, x] = "[S]";
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
        /// </summary>
        /// <param name="enemyBoard"></param>
        /// <returns></returns>
        public HitStatus Move(Board enemyBoard)
        {
            HitStatus hitStatus;
            Console.Write("Enter X coordinate: ");
            int x = Convert.ToInt32(Console.ReadLine());
            Console.Write("Enter Y coordinate: ");
            int y = Convert.ToInt32(Console.ReadLine());

            // should add a check to make sure x and y are not out of bounds

            if (enemyBoard.GetLocation(x, y) == "[S]")
            {
                enemyBoard.SetLocation(x, y, "[X]");
                enemyView[y, x] = "[X]";
                hitStatus = HitStatus.HIT;
            }
            else if (enemyBoard.GetLocation(x, y) == "[X]" || enemyBoard.GetLocation(x, y) == "[O]")
            {
                Console.WriteLine("You already went there. Press enter to try again.");
                Console.ReadKey();
                hitStatus = HitStatus.RETRY;
            }
            else
            {
                enemyBoard.SetLocation(x, y, "[O]");
                enemyView[y, x] = "[O]";
                hitStatus = HitStatus.MISS;
            }
            return hitStatus;
        }
    }
}
