using System;
using System.Collections.Generic;
using System.Text;

namespace BattleShip
{
    class Board
    {
        public string name;

        // AI stuff
        public bool AI;
        public bool oddStartingParity = true;
        private List<Tuple<int, int>> hitCoordinates;
        Random rnd;

        string[,] board;
        string[,] enemyView;
        int[] shipLengths = { 5, 4, 3, 3, 2 };
        Ship[] ships = new Ship[5];
        int boardSize = 10;
        int hits;
        int maxHits = 0;

        public Board(string _name)
        {
            name = _name;
            hits = 0;
            // initializing the number of max hits
            for (int i = 0; i < shipLengths.Length; i++)
            {
                maxHits += shipLengths[i];
            }
            // initializing ships to correct lengths
            for (int i = 0; i < ships.Length; i++)
            {
                ships[i] = new Ship(shipLengths[i]);
            }
            board = new string[boardSize, boardSize];
            enemyView = new string[boardSize, boardSize];

            // AI stuff
            rnd = new Random();
            hitCoordinates = new List<Tuple<int, int>>();

            InitializeBoard(board);
            InitializeBoard(enemyView);
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
            if (setTo == "[X]")
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
            if (AI)
            {
                int ind = 0;
                while (ind < shipLengths.GetLength(0))
                {
                    int x;
                    int y;

                    Random rand = new Random();
                    y = rand.Next(0, board.GetLength(0));
                    x = rand.Next(0, board.GetLength(1));

                    int chance = rand.Next(0, 10);
                    string orientation;
                    if (chance >= 5)
                    {
                        orientation = "d";
                    }
                    else
                    {
                        orientation = "r";
                    }

                    if (!VerifyShipPlacement(x, y, orientation, ind))
                    {
                        // error in ship placement
                        continue;
                    }

                    //if (orientation == "d")
                    //{
                    //    if (shipLengths[ind] + y > board.GetLength(0))
                    //    {
                    //        continue;
                    //    }
                    //}
                    //else
                    //{
                    //    if (shipLengths[ind] + x > board.GetLength(1))
                    //    {
                    //        continue;
                    //    }
                    //}

                    // If all is well, place the ship.
                    PlaceShip(board, ind, x, y, orientation);

                    Console.Clear();
                    ind++;
                }
            }
            else
            {
                int shipIndex = 0;

                while (shipIndex < shipLengths.GetLength(0))
                {
                    PrintBoard();

                    string _x;
                    string _y;
                    int x;
                    int y;
                    // Getting coordinates
                    Console.WriteLine("Enter coordinates for ship of length " + shipLengths[shipIndex]);
                    Console.Write("Enter X coordinate: ");
                    do
                    {
                        _x = Console.ReadLine();
                    } while (!int.TryParse(_x, out x));

                    Console.Write("Enter Y coordinate: ");
                    do
                    {
                        _y = Console.ReadLine();
                    } while (!int.TryParse(_y, out y));

                    // Getting orientation
                    Console.Write("Enter d to orient the ship downwards, or r to orient the ship to the right: ");
                    string orientation;
                    do
                    {
                        orientation = Console.ReadLine();
                    } while (orientation != "d" && orientation != "r");

                    // Checking to make sure ship doesn't go off side
                    //if (orientation == "d")
                    //{
                    //    if (shipLengths[shipIndex] + y > board.GetLength(0))
                    //    {
                    //        Console.WriteLine("Ship goes off edge. Press enter to try again.");
                    //        Console.ReadKey();
                    //        Console.Clear();
                    //        continue;
                    //    }
                    //}
                    //else
                    //{
                    //    if (shipLengths[shipIndex] + x > board.GetLength(1))
                    //    {
                    //        Console.WriteLine("Ship goes off edge. Press enter to try again.");
                    //        Console.ReadKey();
                    //        Console.Clear();
                    //        continue;
                    //    }
                    //}

                    if (!VerifyShipPlacement(x,y,orientation,shipIndex))
                    {
                        Console.WriteLine("Error in ship placement. Press enter to try again.");
                        Console.ReadKey();
                        Console.Clear();
                        continue;
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
        }

        bool VerifyShipPlacement(int x, int y, string orientation, int shipIndex)
        {
            if (orientation == "d")
            {
                if (shipLengths[shipIndex] + y > board.GetLength(0))
                {
                    return false;
                }
                for (int i = y; i < shipLengths[shipIndex] + y; i++)
                {
                    if (board[i, x] != "[ ]")
                        return false;
                }
            }
            else
            {
                if (shipLengths[shipIndex] + x > board.GetLength(1))
                {
                    return false;
                }
                for (int i = x; i < shipLengths[shipIndex] + x; i++)
                {
                    if (board[y, i] != "[ ]")
                    {
                        return false;
                    }
                }
            }
            return true; // passed all tests
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
            string shipType = "[" + (ships.Length - shipIndex).ToString() + "]"; // could that 5 be changed to ships.Length?
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
        /// AI makes a move. Currently it is just random with parity.
        /// </summary>
        /// <param name="enemyBoard"></param>
        /// <returns>Whether the game is over or not</returns>
        bool AIMove(Board enemyBoard)
        {
            int yPos = 0;
            int xPos;
            HitStatus hitStatus;

            do
            {
                yPos = rnd.Next(0, 10);
                if (yPos % 2 == 0) // odd numbers
                {
                    xPos = rnd.Next(0, 5) * 2 + 1;
                }
                else // even numbers
                {
                    xPos = rnd.Next(0, 5) * 2;
                }

                hitStatus = MoveOnBoard(enemyBoard, xPos, yPos);
            } while (hitStatus == HitStatus.RETRY);

            if (hitStatus == HitStatus.HIT) // if it hit, add the coordinates to a tuple
            {
                hitCoordinates.Add(new Tuple<int, int>(xPos, yPos));
            }

            return false;
        }

        /// <summary>
        /// Asks the player for x and y coordinates and checks if it is a hit, miss, or if they already went there.
        /// If a ship is sunk, then it returns SUNK
        /// </summary>
        /// <param name="enemyBoard"></param>
        /// <returns>hitStatus - HIT, SUNK, RETRY, MISS</returns>
        HitStatus PlayerMove(Board enemyBoard)
        {
            HitStatus hitStatus;
            Console.Write("Enter X coordinate: ");
            string str_x = Console.ReadLine();
            Console.Write("Enter Y coordinate: ");
            string str_y = Console.ReadLine();

            int x;
            int y;

            if (!Int32.TryParse(str_x, out x))
                return HitStatus.RETRY;
            else if (!Int32.TryParse(str_y, out y))
                return HitStatus.RETRY;


            if (x > 9 || y > 9 || y < 0 || x < 0)
            {
                Console.WriteLine("Coordinates are outside of bounds. Enter coordinates between 0 and 9.");
                Console.WriteLine("Press enter to continue.");
                Console.ReadKey();
                return HitStatus.RETRY;
            }

            // This tests the location to see what number it is. It just used number 1 - 5
            // to make things easier. Because of the array setup, index 0 is the ship of length 5.
            hitStatus = MoveOnBoard(enemyBoard, x, y);

            //switch (enemyBoard.GetLocation(x, y))
            //{
            //    case "[5]":
            //        ships[0].hits++;
            //        hitStatus = ships[0].hits >= ships[0].length ? HitStatus.SUNK : HitStatus.HIT;

            //        enemyBoard.SetLocation(x, y, "[X]");
            //        enemyView[y, x] = "[X]";
            //        break;
            //    case "[4]":
            //        ships[1].hits++;
            //        hitStatus = ships[1].hits >= ships[1].length ? HitStatus.SUNK : HitStatus.HIT;

            //        enemyBoard.SetLocation(x, y, "[X]");
            //        enemyView[y, x] = "[X]";
            //        break;
            //    case "[3]":
            //        ships[2].hits++;
            //        hitStatus = ships[2].hits >= ships[2].length ? HitStatus.SUNK : HitStatus.HIT;

            //        enemyBoard.SetLocation(x, y, "[X]");
            //        enemyView[y, x] = "[X]";
            //        break;
            //    case "[2]":
            //        ships[3].hits++;
            //        hitStatus = ships[3].hits >= ships[3].length ? HitStatus.SUNK : HitStatus.HIT;

            //        enemyBoard.SetLocation(x, y, "[X]");
            //        enemyView[y, x] = "[X]";
            //        break;
            //    case "[1]":
            //        ships[4].hits++;
            //        hitStatus = ships[4].hits >= ships[4].length ? HitStatus.SUNK : HitStatus.HIT;

            //        enemyBoard.SetLocation(x, y, "[X]");
            //        enemyView[y, x] = "[X]";
            //        break;
            //    case "[X]":
            //    case "[O]":
            //        Console.WriteLine("You already went there. Press enter to try again.");
            //        Console.ReadKey();
            //        hitStatus = HitStatus.RETRY;
            //        break;
            //    default:
            //        enemyBoard.SetLocation(x, y, "[O]");
            //        enemyView[y, x] = "[O]";
            //        hitStatus = HitStatus.MISS;
            //        break;
            //}
            return hitStatus;
        }

        private HitStatus MoveOnBoard(Board enemyBoard, int x, int y)
        {
            HitStatus hitStatus;
            // This tests the location to see what number it is. It just used number 1 - 5
            // to make things easier. Because of the array setup, index 0 is the ship of length 5.
            switch (enemyBoard.GetLocation(x, y))
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
                    if (AI)
                        hitStatus = HitStatus.RETRY;
                    else
                    {
                        Console.WriteLine("You already went there. Press enter to try again.");
                        Console.ReadKey();
                        hitStatus = HitStatus.RETRY;
                    }
                    break;
                default:
                    enemyBoard.SetLocation(x, y, "[O]");
                    enemyView[y, x] = "[O]";
                    hitStatus = HitStatus.MISS;
                    break;
            }
            return hitStatus;
        }

        public bool TakeTurn(Board playerB)
        {
            // AI makes a move
            if (AI)
            {
                return AIMove(playerB);
            }
            // Player makes a move
            else
            {
                HitStatus hitStatus;
                PrintPlayerView();
                do
                {
                    Console.Clear();
                    Console.WriteLine(name);
                    Console.WriteLine();
                    PrintPlayerView();
                    hitStatus = PlayerMove(playerB);
                } while (hitStatus == HitStatus.RETRY);

                Console.Clear();
                if (hitStatus == HitStatus.HIT)
                {
                    Console.WriteLine(name);
                    Console.WriteLine();
                    PrintPlayerView();
                    Console.WriteLine("Hit!");
                    // Total hits means game is over
                    if (playerB.GetHits() >= playerB.GetMaxHits())
                    {
                        Console.WriteLine("Game Over, " + name + " wins!");
                        return true;
                    }
                }
                else if (hitStatus == HitStatus.SUNK)
                {
                    Console.WriteLine("Player 1");
                    Console.WriteLine();
                    PrintPlayerView();
                    Console.WriteLine("Hit and sink!");
                    // Total hits means game is over
                    if (playerB.GetHits() >= playerB.GetMaxHits())
                    {
                        Console.WriteLine("Game Over, " + name + " wins!");
                        return true;
                    }
                }
                else
                {
                    Console.WriteLine(name);
                    Console.WriteLine();
                    PrintPlayerView();
                    Console.WriteLine("Miss");
                }

                Console.WriteLine("Press Enter to end turn");
                Console.ReadKey();
                Console.Clear();
                Console.WriteLine("Press Enter to start turn");
                Console.WriteLine();
                Console.ReadKey();
                Console.Clear();

                return false;
            }
        }

        void PrintPlayerView()
        {
            Console.WriteLine("   ------------- Enemy board -------------");
            PrintEnemyView();
            Console.WriteLine();
            Console.WriteLine("   ------------- Your board -------------");
            PrintBoard();
        }
    }
}
