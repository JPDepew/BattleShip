using System;

namespace BattleShip
{
    class Program
    {
        static void Main(string[] args)
        {
            // Creating boards
            Board Player1 = new Board();
            Board Player2 = new Board();

            HitStatus hitStatus;
            int turn = 0;
            bool gameIsOver = false;

            Console.Clear();

            Console.WriteLine("Player 1 ship set up");
            Player1.SetUpShips();
            Console.WriteLine("Player 2 ship set up");
            Player2.SetUpShips();

            while (!gameIsOver)
            {
                // Player 1
                if (turn % 2 == 0)
                {
                    PrintPlayerView(Player1);
                    do
                    {
                        Console.Clear();
                        Console.WriteLine("Player 1");
                        PrintPlayerView(Player1);
                        hitStatus = Player1.Move(Player2);
                    } while (hitStatus == HitStatus.RETRY);

                    Console.Clear();
                    if (hitStatus == HitStatus.HIT)
                    {
                        Console.WriteLine("Player 1");
                        PrintPlayerView(Player1);
                        Console.WriteLine("Hit!");
                        if (Player2.GetHits() >= Player2.GetMaxHits())
                        {
                            Console.WriteLine("Game Over, Player 1 wins!");
                            break;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Player 1");
                        PrintPlayerView(Player1);
                        Console.WriteLine("Miss");
                    }
                }
                // Player 2
                else
                {
                    PrintPlayerView(Player2);
                    do
                    {
                        Console.Clear();
                        Console.WriteLine("Player 2");
                        PrintPlayerView(Player2);
                        hitStatus = Player2.Move(Player1);
                    } while (hitStatus == HitStatus.RETRY);

                    Console.Clear();
                    if (hitStatus == HitStatus.HIT)
                    {
                        Console.WriteLine("Player 2");
                        PrintPlayerView(Player2);
                        Console.WriteLine("Hit!");
                        if (Player1.GetHits() >= Player1.GetMaxHits())
                        {
                            Console.WriteLine("Game Over, Player 2 wins!");
                            break;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Player 2");
                        PrintPlayerView(Player2);
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

        //static void SetUpShips(string[,] playerBoard, int[] shipLengths, int boardSize)
        //{
        //    int shipIndex = 0;

        //    while (shipIndex < shipLengths.GetLength(0))
        //    {
        //        PrintBoard(playerBoard, boardSize);

        //        // Getting coordinates
        //        Console.WriteLine("Enter coordinates for ship of length " + shipLengths[shipIndex]);
        //        Console.Write("Enter X coordinate: ");
        //        int x = Convert.ToInt32(Console.ReadLine());
        //        Console.Write("Enter Y coordinate: ");
        //        int y = Convert.ToInt32(Console.ReadLine());

        //        // Getting orientation
        //        Console.Write("Enter d to orient the ship downwards, or r to orient the ship to the right: ");
        //        string orientation;
        //        do
        //        {
        //            orientation = Console.ReadLine();
        //        } while (orientation != "d" && orientation != "r");
                
        //        // Checking to make sure ship doesn't go off side
        //        if (orientation == "d")
        //        {
        //            if (shipLengths[shipIndex] + y > playerBoard.GetLength(0))
        //            {
        //                Console.WriteLine("Ship goes off edge. Press enter to try again.");
        //                Console.ReadKey();
        //                Console.Clear();
        //                continue;
        //            }
        //        }
        //        else
        //        {
        //            if (shipLengths[shipIndex] + x > playerBoard.GetLength(1))
        //            {
        //                Console.WriteLine("Ship goes off edge. Press enter to try again.");
        //                Console.ReadKey();
        //                Console.Clear();
        //                continue;
        //            }
        //        }

        //        // If all is well, place the ship.
        //        PlaceShip(playerBoard, shipLengths[shipIndex], x, y, orientation);

        //        Console.Clear();
        //        shipIndex++;
        //    }

        //    Console.WriteLine("Setup complete. Press Enter to continue.");
        //    PrintBoard(playerBoard, boardSize);
        //    Console.ReadKey();
        //    Console.Clear();
        //}

        //static void PlaceShip(string[,] playerBoard, int shipLength, int x, int y, string orientation)
        //{
        //    for (int i = 0; i < shipLength; i++)
        //    {
        //        playerBoard[y, x] = "[S]";
        //        if (orientation == "d")
        //        {
        //            y++;
        //        }
        //        else
        //        {
        //            x++;
        //        }
        //    }
        //}

        static void PrintPlayerView(Board playerBoard)
        {
            Console.WriteLine("Enemy board:");
            playerBoard.PrintEnemyView();
            Console.WriteLine("Your board:");
            playerBoard.PrintBoard();
        }

        //static void InitializeArray(string[,] board, int boardLength)
        //{
        //    for (int i = 0; i < boardLength; i++)
        //    {
        //        for (int j = 0; j < boardLength; j++)
        //        {
        //            board[i, j] = "[ ]";
        //        }
        //    }
        //}

        //static void PrintBoard(string[,] board, int boardLength)
        //{
        //    Console.Write("  ");
        //    for (int i = 0; i < boardLength; i++)
        //    {
        //        Console.Write("  " + i + " ");
        //    }
        //    Console.WriteLine();
        //    for (int i = 0; i < boardLength; i++)
        //    {
        //        Console.Write(i + "  ");
        //        for (int j = 0; j < boardLength; j++)
        //        {
        //            Console.Write(board[i, j] + " ");
        //        }
        //        Console.WriteLine();
        //    }
        //}

        //static HitStatus Move(Board enemyBoard, string[,] enemyBoardView)
        //{
        //    HitStatus hitStatus;
        //    Console.Write("Enter X coordinate: ");
        //    int x = Convert.ToInt32(Console.ReadLine());
        //    Console.Write("Enter Y coordinate: ");
        //    int y = Convert.ToInt32(Console.ReadLine());

        //    if (enemyBoard[y, x] == "[S]")
        //    {
        //        enemyBoard[y, x] = "[X]";
        //        enemyBoardView[y, x] = "[X]";
        //        hitStatus = HitStatus.HIT;
        //    }
        //    else if (enemyBoard[y, x] == "[X]" || enemyBoard[y, x] == "[O]")
        //    {
        //        Console.WriteLine("You already went there. Press enter to try again.");
        //        Console.ReadKey();
        //        hitStatus = HitStatus.RETRY;
        //    }
        //    else
        //    {
        //        enemyBoard[y, x] = "[O]";
        //        enemyBoardView[y, x] = "[O]";
        //        hitStatus = HitStatus.MISS;
        //    }
        //    return hitStatus;
        //}
    }
}
