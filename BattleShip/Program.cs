using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace BattleShip
{
    class Program
    {
        static void Main(string[] args)
        {
            // Creating boards
            Board Player1 = new Board("Player 1");
            Board Player2 = new Board("Player 2");

            Player1.freqFile = "one.freq";
            Player2.freqFile = "two.freq";

            int turn = 0;
            bool gameIsOver = false;

            Console.WriteLine("AI player? Enter '1' for player 1, '2' for player 2, 'b' for both, or any key for no AI.");
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
            else if (ans == "b")
            {
                Player1.AI = true;
                Player2.AI = true;
                Console.WriteLine("Both are AI. Press any key.");
                Console.ReadLine();
            }

            Console.Clear();

            BinaryFormatter bf = new BinaryFormatter();

            if (Player1.AI)
            {
                if (File.Exists(Player1.freqFile))
                {
                    using (FileStream fs = new FileStream(Player1.freqFile, FileMode.Open))
                        Player1.freqTable = (int[,])bf.Deserialize(fs);
                }

            }

            if (Player2.AI)
            {
                if (File.Exists(Player2.freqFile))
                {
                    using (FileStream fs = new FileStream(Player2.freqFile, FileMode.Open))
                        Player2.freqTable = (int[,])bf.Deserialize(fs);
                }

            }

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
                    if (gameIsOver)
                    {

                        break;
                    }
                }
                // Player 2's turn
                else
                {
                    gameIsOver = Player2.TakeTurn(Player1);
                    if (gameIsOver) { break; }
                }
                turn++;
            }

            for (int i = 0; i < Player1.board.GetLength(0); i++)
            {
                for (int j = 0; j < Player1.board.GetLength(1); j++)
                {
                    if (Player1.board[i, j] != "[ ]")
                    {
                        Player1.freqTable[i, j] += 1;
                    }
                }
            }

            for (int i = 0; i < Player2.board.GetLength(0); i++)
            {
                for (int j = 0; j < Player2.board.GetLength(1); j++)
                {
                    if (Player2.board[i, j] != "[ ]")
                    {
                        Player2.freqTable[i, j] += 1;
                    }
                }
            }

            // Serialize frequency table, this will allow us to learn strong positions for AI vs AI.  Will need to change this model to adapt against players

            using (FileStream fs = new FileStream(Player1.freqFile, FileMode.Create))
                bf.Serialize(fs, Player1.freqTable);

            using (FileStream fs = new FileStream(Player2.freqFile, FileMode.Create))
                bf.Serialize(fs, Player2.freqTable);

            Console.Read();
        }
    }
}
