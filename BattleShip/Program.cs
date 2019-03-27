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
            bool gameIsOver = false; // not actually used now...

            Console.Clear();

            Console.WriteLine("Player 1 ship set up");
            Player1.SetUpShips();
            Console.WriteLine("Player 2 ship set up");
            Player2.SetUpShips();

            while (!gameIsOver)
            {
                // Player 1's turn
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
                        // Total hits means game is over
                        if (Player2.GetHits() >= Player2.GetMaxHits())
                        {
                            Console.WriteLine("Game Over, Player 1 wins!");
                            break;
                        }
                    }
                    else if (hitStatus == HitStatus.SUNK)
                    {
                        Console.WriteLine("Player 1");
                        PrintPlayerView(Player1);
                        Console.WriteLine("Hit and sink!");
                        // Total hits means game is over
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
                // Player 2's turn
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
                        // Total hits means game is over
                        if (Player1.GetHits() >= Player1.GetMaxHits())
                        {
                            Console.WriteLine("Game Over, Player 2 wins!");
                            break;
                        }
                    }
                    else if (hitStatus == HitStatus.SUNK)
                    {
                        Console.WriteLine("Player 2");
                        PrintPlayerView(Player2);
                        Console.WriteLine("Hit and sink!");
                        // Total hits means game is over
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

        static void PrintPlayerView(Board playerBoard)
        {
            Console.WriteLine("Enemy board:");
            playerBoard.PrintEnemyView();
            Console.WriteLine("Your board:");
            playerBoard.PrintBoard();
        }
    }
}
