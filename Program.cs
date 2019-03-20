﻿using System;

namespace BattleShip
{
    class Program
    {
        enum HitStatus { HIT, MISS, RETRY };
        static void Main(string[] args)
        {
            HitStatus hitStatus;

            int boardSize = 4;
            int turn = 0;

            Board Player1 = new Board();
            Board Player1EnemyV = new Board();
            Board Player2 = new Board();
            Board Player2EnemyV = new Board();

            string[,] Player1Board;
            string[,] Player1EnemyView;
            string[,] Player2Board;
            string[,] Player2EnemyView;
            int[] shipLengths = { 5, 4, 3, 3, 2 };

            bool gameIsOver = false;

            //Console.WriteLine("Enter the board length: ");
            //boardSize = Convert.ToInt32(Console.ReadLine());

            // Setting up boards
            Player1Board = new string[boardSize, boardSize];
            Player1EnemyView = new string[boardSize, boardSize];
            Player2Board = new string[boardSize, boardSize];
            Player2EnemyView = new string[boardSize, boardSize];
            InitializeArray(Player1Board, boardSize);
            InitializeArray(Player1EnemyView, boardSize);
            InitializeArray(Player2Board, boardSize);
            InitializeArray(Player2EnemyView, boardSize);

            Console.Clear();

            Console.WriteLine("Player 1 ship set up");
            Player1.SetUpShips();
            //SetUpShips(Player1Board, shipLengths, boardSize);
            Console.WriteLine("Player 2 ship set up");
            Player2.SetUpShips();
            //SetUpShips(Player2Board, shipLengths, boardSize);

            while (!gameIsOver)
            {
                if (turn % 2 == 0) // Player 1
                {
                    PrintPlayerView(Player1Board, Player1EnemyView, boardSize);
                    do
                    {
                        Console.Clear();
                        Console.WriteLine("Player 1");
                        PrintPlayerView(Player1Board, Player1EnemyView, boardSize);
                        hitStatus = Move(Player2Board, Player1EnemyView);
                    } while (hitStatus == HitStatus.RETRY);

                    Console.Clear();
                    if (hitStatus == HitStatus.HIT)
                    {
                        Console.WriteLine("Player 1");
                        PrintPlayerView(Player1Board, Player1EnemyView, boardSize);
                        Console.WriteLine("Hit!");
                    }
                    else
                    {
                        Console.WriteLine("Player 1");
                        PrintPlayerView(Player1Board, Player1EnemyView, boardSize);
                        Console.WriteLine("Miss");
                    }
                }
                else // Player 2
                {
                    PrintPlayerView(Player2Board, Player2EnemyView, boardSize);
                    do
                    {
                        Console.Clear();
                        Console.WriteLine("Player 2");
                        PrintPlayerView(Player2Board, Player2EnemyView, boardSize);
                        hitStatus = Move(Player1Board, Player2EnemyView);
                    } while (hitStatus == HitStatus.RETRY);

                    Console.Clear();
                    if (hitStatus == HitStatus.HIT)
                    {
                        Console.WriteLine("Player 2");
                        PrintPlayerView(Player2Board, Player2EnemyView, boardSize);
                        Console.WriteLine("Hit!");
                    }
                    else
                    {
                        Console.WriteLine("Player 2");
                        PrintPlayerView(Player2Board, Player2EnemyView, boardSize);
                        Console.WriteLine("Miss");
                    }
                }

                Console.WriteLine("Press Enter to end turn");
                Console.ReadKey();
                Console.Clear();
                Console.WriteLine("Press Enter to start turn");
                Console.WriteLine();
                Console.ReadKey();
                Console.Clear();
                turn++;
            }

            Console.Read();
        }

        static void SetUpShips(string[,] playerBoard, int[] shipLengths, int boardSize)
        {
            int shipIndex = 0;

            while (shipIndex < shipLengths.GetLength(0))
            {
                PrintBoard(playerBoard, boardSize);

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
                    if (shipLengths[shipIndex] + y > playerBoard.GetLength(0))
                    {
                        Console.WriteLine("Ship goes off edge. Press enter to try again.");
                        Console.ReadKey();
                        Console.Clear();
                        continue;
                    }
                }
                else
                {
                    if (shipLengths[shipIndex] + x > playerBoard.GetLength(1))
                    {
                        Console.WriteLine("Ship goes off edge. Press enter to try again.");
                        Console.ReadKey();
                        Console.Clear();
                        continue;
                    }
                }

                // If all is well, place the ship.
                PlaceShip(playerBoard, shipLengths[shipIndex], x, y, orientation);

                Console.Clear();
                shipIndex++;
            }

            Console.WriteLine("Setup complete. Press Enter to continue.");
            PrintBoard(playerBoard, boardSize);
            Console.ReadKey();
            Console.Clear();
        }

        static void PlaceShip(string[,] playerBoard, int shipLength, int x, int y, string orientation)
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

        static void PrintPlayerView(string[,] playerBoard, string[,] enemyBoard, int boardSize)
        {
            Console.WriteLine("Enemy board:");
            PrintBoard(enemyBoard, boardSize);
            Console.WriteLine("Your board:");
            PrintBoard(playerBoard, boardSize);
        }

        static void InitializeArray(string[,] board, int boardLength)
        {
            for (int i = 0; i < boardLength; i++)
            {
                for (int j = 0; j < boardLength; j++)
                {
                    board[i, j] = "[ ]";
                }
            }
        }

        static void PrintBoard(string[,] board, int boardLength)
        {
            Console.Write("  ");
            for (int i = 0; i < boardLength; i++)
            {
                Console.Write("  " + i + " ");
            }
            Console.WriteLine();
            for (int i = 0; i < boardLength; i++)
            {
                Console.Write(i + "  ");
                for (int j = 0; j < boardLength; j++)
                {
                    Console.Write(board[i, j] + " ");
                }
                Console.WriteLine();
            }
        }

        static HitStatus Move(string[,] enemyBoard, string[,] enemyBoardView)
        {
            HitStatus hitStatus;
            Console.Write("Enter X coordinate: ");
            int x = Convert.ToInt32(Console.ReadLine());
            Console.Write("Enter Y coordinate: ");
            int y = Convert.ToInt32(Console.ReadLine());

            if (enemyBoard[y, x] == "[S]")
            {
                enemyBoard[y, x] = "[X]";
                enemyBoardView[y, x] = "[X]";
                hitStatus = HitStatus.HIT;
            }
            else if (enemyBoard[y, x] == "[X]" || enemyBoard[y, x] == "[O]")
            {
                Console.WriteLine("You already went there. Press enter to try again.");
                Console.ReadKey();
                hitStatus = HitStatus.RETRY;
            }
            else
            {
                enemyBoard[y, x] = "[O]";
                enemyBoardView[y, x] = "[O]";
                hitStatus = HitStatus.MISS;
            }
            return hitStatus;
        }
    }
}
