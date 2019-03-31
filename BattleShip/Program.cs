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
                    gameIsOver = Player1.TakeTurn(Player2);
                    if (gameIsOver) { break; }
                }
                // Player 2's turn
                else
                {
                    gameIsOver = Player2.TakeTurn(Player1);
                    if (gameIsOver) { break; }
                }
                turn++;
            }
            Console.Read();
        }
    }
}
