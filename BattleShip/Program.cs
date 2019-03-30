using System;

namespace BattleShip
{
    class Program
    {
        static void Main(string[] args)
        {
            // Creating boards
            Board Player1 = new Board("Player 1");
            Board Player2 = new Board("Player 2");

            int turn = 0;
            bool gameIsOver = false;

            Console.WriteLine("AI player? Enter '1' for player 1, '2' for player 2, or any key for no AI.");
            string ans = Console.ReadLine();
            if (ans == "1")
            {
                Player1.AI = true;
                Console.WriteLine("Player 1 is AI. Press any key.");
                Console.ReadLine();
            }
            else if (ans == "2")
            {
                Player2.AI = true;
                Console.WriteLine("Player 2 is AI. Press any key.");
                Console.ReadLine();
            }

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
                    gameIsOver = Turn(Player1, Player2);
                    if (gameIsOver)
                    {
                        break;
                    }
                    //PrintPlayerView(Player1);
                    //do
                    //{
                    //    Console.Clear();
                    //    Console.WriteLine("Player 1");
                    //    PrintPlayerView(Player1);
                    //    hitStatus = Player1.Move(Player2);
                    //} while (hitStatus == HitStatus.RETRY);

                    //Console.Clear();
                    //if (hitStatus == HitStatus.HIT)
                    //{
                    //    Console.WriteLine("Player 1");
                    //    PrintPlayerView(Player1);
                    //    Console.WriteLine("Hit!");
                    //    // Total hits means game is over
                    //    if (Player2.GetHits() >= Player2.GetMaxHits())
                    //    {
                    //        Console.WriteLine("Game Over, Player 1 wins!");
                    //        break;
                    //    }
                    //}
                    //else if (hitStatus == HitStatus.SUNK)
                    //{
                    //    Console.WriteLine("Player 1");
                    //    PrintPlayerView(Player1);
                    //    Console.WriteLine("Hit and sink!");
                    //    // Total hits means game is over
                    //    if (Player2.GetHits() >= Player2.GetMaxHits())
                    //    {
                    //        Console.WriteLine("Game Over, Player 1 wins!");
                    //        break;
                    //    }
                    //}
                    //else
                    //{
                    //    Console.WriteLine("Player 1");
                    //    PrintPlayerView(Player1);
                    //    Console.WriteLine("Miss");
                    //}
                }
                // Player 2's turn
                else
                {
                    gameIsOver = Turn(Player2, Player1);
                    if (gameIsOver)
                    {
                        break;
                    }
                    //PrintPlayerView(Player2);
                    //do
                    //{
                    //    Console.Clear();
                    //    Console.WriteLine("Player 2");
                    //    PrintPlayerView(Player2);
                    //    hitStatus = Player2.Move(Player1);
                    //} while (hitStatus == HitStatus.RETRY);

                    //Console.Clear();
                    //if (hitStatus == HitStatus.HIT)
                    //{
                    //    Console.WriteLine("Player 2");
                    //    PrintPlayerView(Player2);
                    //    Console.WriteLine("Hit!");
                    //    // Total hits means game is over
                    //    if (Player1.GetHits() >= Player1.GetMaxHits())
                    //    {
                    //        Console.WriteLine("Game Over, Player 2 wins!");
                    //        break;
                    //    }
                    //}
                    //else if (hitStatus == HitStatus.SUNK)
                    //{
                    //    Console.WriteLine("Player 2");
                    //    PrintPlayerView(Player2);
                    //    Console.WriteLine("Hit and sink!");
                    //    // Total hits means game is over
                    //    if (Player1.GetHits() >= Player1.GetMaxHits())
                    //    {
                    //        Console.WriteLine("Game Over, Player 2 wins!");
                    //        break;
                    //    }
                    //}
                    //else
                    //{
                    //    Console.WriteLine("Player 2");
                    //    PrintPlayerView(Player2);
                    //    Console.WriteLine("Miss");
                    //}
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

        static bool Turn(Board playerA, Board playerB)
        {
            HitStatus hitStatus;
            PrintPlayerView(playerA);
            do
            {
                Console.Clear();
                Console.WriteLine(playerA.name);
                Console.WriteLine();
                PrintPlayerView(playerA);
                hitStatus = playerA.Move(playerB);
            } while (hitStatus == HitStatus.RETRY);

            Console.Clear();
            if (hitStatus == HitStatus.HIT)
            {
                Console.WriteLine(playerA.name);
                Console.WriteLine();
                PrintPlayerView(playerA);
                Console.WriteLine("Hit!");
                // Total hits means game is over
                if (playerB.GetHits() >= playerB.GetMaxHits())
                {
                    Console.WriteLine("Game Over, " + playerA.name + " wins!");
                    return true;
                }
            }
            else if (hitStatus == HitStatus.SUNK)
            {
                Console.WriteLine("Player 1");
                Console.WriteLine();
                PrintPlayerView(playerA);
                Console.WriteLine("Hit and sink!");
                // Total hits means game is over
                if (playerB.GetHits() >= playerB.GetMaxHits())
                {
                    Console.WriteLine("Game Over, " + playerA.name + " wins!");
                    return true;
                }
            }
            else
            {
                Console.WriteLine(playerA.name);
                Console.WriteLine();
                PrintPlayerView(playerA);
                Console.WriteLine("Miss");
            }

            return false;
        }

        static void PrintPlayerView(Board playerBoard)
        {
            Console.WriteLine("   ------------- Enemy board -------------");
            playerBoard.PrintEnemyView();
            Console.WriteLine();
            Console.WriteLine("   ------------- Your board -------------");
            playerBoard.PrintBoard();
        }
    }
}
