using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace BattleShip
{
    class Coordinate
    {
        public int x;
        public int y;

        public Coordinate(int _x, int _y)
        {
            x = _x;
            y = _y;
        }

        public bool CoordinatesAreEqual(Coordinate c1)
        {
            return c1.x == x && c1.y == y;
        }
    }

    class Board
    {
        public string name;

        // AI stuff
        public bool AI;
        public bool oddStartingParity = true;
        private Coordinate currentHitCoordinate;
        private List<Coordinate> possibleHitCoordinates;
        private List<Coordinate> currentShipCoordinates;
        private List<Coordinate> startingCoordinates;
        private List<Coordinate> cleanupCoordinates;
        private List<int> remainingShipLengths;
        private List<Ship> sunkShips;
        private Ship currentShip;
        float[,] heatMap;
        Random rnd;

        string[,] board;
        string[,] enemyView;
        int[] shipLengths = { 5, 4, 3, 3, 2 };
        Ship[] ships = new Ship[5];

        int[,] freqTable = new int[10, 10];

        SearchMode searchMode;
        int boardSize = 10;
        int hits;
        int maxHits = 0;

        public Board(string _name)
        {
            // AI stuff
            rnd = new Random();
            possibleHitCoordinates = new List<Coordinate>();
            currentShipCoordinates = new List<Coordinate>();
            startingCoordinates = new List<Coordinate>();
            cleanupCoordinates = new List<Coordinate>();
            searchMode = SearchMode.SEARCH;
            remainingShipLengths = new List<int>();
            sunkShips = new List<Ship>();
            currentShip = new Ship();

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
                remainingShipLengths.Add(shipLengths[i]);
            }
            board = new string[boardSize, boardSize];
            enemyView = new string[boardSize, boardSize];
            heatMap = new float[boardSize, boardSize];

            InitializeStartingCoordinates();
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

        void GenerateHeatMap()
        {
            for (int i = 0; i < heatMap.GetLength(1); i++) // x
            {
                for (int j = 0; j < heatMap.GetLength(0); j++) // y
                {
                    heatMap[j, i] = CreateValueForCell(i, j);
                }
            }
        }

        private int GetLargestRemainingShipLength()
        {
            int min = 0;
            foreach (int l in remainingShipLengths)
            {
                if (l > min)
                {
                    min = l;
                }
            }
            return min;
        }

        private int GetSmallestRemainingShipLength()
        {
            int min = 100;
            foreach (int l in remainingShipLengths)
            {
                if (l < min)
                {
                    min = l;
                }
            }
            return min;
        }

        /// <summary>
        /// Very basic heat map. There are a few problems that we'll have to fix in the future. For example, once a ship has sunk, we 
        /// will have to mark those spots as sunk, or it will want to keep fireing around them.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>Value for cell</returns>
        float CreateValueForCell(int x, int y)
        {
            // There is still an edgecase not accounted for here. If ships like this: 1 1 2 2 2, and hit like this 1 X X X X.
            // If that '1' is surrounded by O's, then there is no chance that it will be hit. I think the way to solve this would be
            // to change sunk ships to [S], then nextToHit would be true next to [S].
            float sum = 0;
            float addValue = 0.0015f;
            float subtractValue = 0.0005f;
            float decrement = 0.0003f;
            float bonus = 0.01f;
            int totalVerticalSpaces = -1;
            int totalHorizontalSpaces = -1;
            int maxShipLength = GetLargestRemainingShipLength();
            int minShipLength = GetSmallestRemainingShipLength();

            bool nextToHit = false;

            if (enemyView[y, x] == "[O]")
            {
                return sum;
            }
            if (enemyView[y, x] == "[X]")
            {
                return -1;
            }
            if (enemyView[y, x] == "[S]")
            {
                return 0;
            }

            for (
                int _x = x, counter = 0; _x < enemyView.GetLength(1) && counter < maxShipLength; _x++, counter++)
            {
                if (enemyView[y, _x] == "[X]")
                {
                    nextToHit = true;
                    sum += (addValue + bonus);
                    break;
                }
                else if (enemyView[y, _x] == "[O]")
                {
                    sum -= subtractValue;
                    break;
                }
                else if (enemyView[y, _x] == "[S]")
                {
                    nextToHit = true;
                    //sum -= subtractValue;
                    break;
                }
                else
                {
                    sum += addValue;
                }
                totalHorizontalSpaces++;
                subtractValue -= decrement;
                addValue -= decrement;
                bonus -= decrement;
            }

            // reinitialize
            addValue = 0.0025f;
            subtractValue = 0.0005f;
            for (int _x = x, counter = 0; _x >= 0 && counter < maxShipLength; _x--, counter++)
            {
                if (enemyView[y, _x] == "[X]")
                {
                    nextToHit = true;
                    sum += (addValue + bonus);
                }
                else if (enemyView[y, _x] == "[O]")
                {
                    sum -= subtractValue;
                    break;
                }
                else if (enemyView[y, _x] == "[S]")
                {
                    nextToHit = true;
                    //sum -= subtractValue;
                    break;
                }
                else
                {
                    sum += addValue;
                }
                totalHorizontalSpaces++;
                subtractValue -= decrement;
                addValue -= decrement;
                bonus -= decrement;
            }

            addValue = 0.0025f;
            subtractValue = 0.0005f;
            for (int _y = y, counter = 0; _y < enemyView.GetLength(0) && counter < maxShipLength; _y++, counter++)
            {
                if (enemyView[_y, x] == "[X]")
                {
                    nextToHit = true;
                    sum += (addValue + bonus);
                }
                else if (enemyView[_y, x] == "[O]")
                {
                    sum -= subtractValue;
                    break;
                }
                else if (enemyView[_y, x] == "[S]")
                {
                    nextToHit = true;
                    //sum -= subtractValue;
                    break;
                }
                else
                {
                    sum += addValue;
                }
                totalVerticalSpaces++;
                subtractValue -= decrement;
                addValue -= decrement;
                bonus -= decrement;
            }

            addValue = 0.0025f;
            subtractValue = 0.0005f;
            for (int _y = y, counter = 0; _y >= 0 && counter < maxShipLength; _y--, counter++)
            {
                if (enemyView[_y, x] == "[X]")
                {
                    nextToHit = true;
                    sum += (addValue + bonus);
                }
                else if (enemyView[_y, x] == "[O]")
                {
                    sum -= subtractValue;
                    break;
                }
                else if (enemyView[_y, x] == "[S]")
                {
                    nextToHit = true;
                    //sum -= subtractValue;
                    break;
                }
                else
                {
                    sum += addValue;
                }
                totalVerticalSpaces++;
                subtractValue -= decrement;
                addValue -= decrement;
                bonus -= decrement;
            }

            // This line should discard any spaces that do not have the possiblilty of containing the ship.
            // - nextToHit is necessary because, even if there is only 1 enclosed spot, if it is next to a hit,
            //   it could still be a ship location
            if (totalHorizontalSpaces < minShipLength && totalVerticalSpaces < minShipLength && !nextToHit)
            {
                sum = 0;
            }

            return sum;
        }

        void PrintHeatMap()
        {
            for (int i = 0; i < heatMap.GetLength(1); i++) // y
            {
                for (int j = 0; j < heatMap.GetLength(0); j++) // x
                {
                    Console.Write(String.Format("{0:0.000}", heatMap[i, j]) + " ");
                }
                Console.WriteLine();
            }
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

        void InitializeStartingCoordinates()
        {
            for (int i = 0; i < boardSize - 1; i++)
            {
                // diagonal pattern across whole board;
                startingCoordinates.Add(new Coordinate(i + 1, i));
            }
            for (int i = 0; i < boardSize - 4; i++)
            {
                // starting at (4,0)
                startingCoordinates.Add(new Coordinate(i + 4, i));
            }
            for (int i = 0; i < boardSize - 7; i++)
            {
                // starting at (7,0)
                startingCoordinates.Add(new Coordinate(i + 7, i));
            }
            for (int i = 0; i < boardSize - 2; i++)
            {
                // starting at (0,2)
                startingCoordinates.Add(new Coordinate(i, i + 2));
            }
            for (int i = 0; i < boardSize - 5; i++)
            {
                // starting at (0,5)
                startingCoordinates.Add(new Coordinate(i, i + 5));
            }
            for (int i = 0; i < boardSize - 8; i++)
            {
                // starting at (0,7)
                startingCoordinates.Add(new Coordinate(i, i + 8));
            }
        }

        void InitializeCleanupCoordinates()
        {
            for (int i = 0; i < boardSize - 3; i++)
            {
                // diagonal pattern across whole board;
                startingCoordinates.Add(new Coordinate(i + 3, i));
            }
            for (int i = 0; i < boardSize - 6; i++)
            {
                // starting at (6,0)
                startingCoordinates.Add(new Coordinate(i + 6, i));
            }
            startingCoordinates.Add(new Coordinate(9, 0));
            for (int i = 0; i < boardSize - 1; i++)
            {
                // starting at (0,1)
                startingCoordinates.Add(new Coordinate(i, i + 1));
            }
            for (int i = 0; i < boardSize - 4; i++)
            {
                // starting at (0,4)
                startingCoordinates.Add(new Coordinate(i, i + 4));
            }
            for (int i = 0; i < boardSize - 7; i++)
            {
                // starting at (0,7)
                startingCoordinates.Add(new Coordinate(i, i + 7));
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

                // read in previous board values, this will be changing very soon (need to implement storing of frequencies also)
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();

                    using (FileStream fs = new FileStream("freq.tbl", FileMode.Open))
                        freqTable = (int[,])bf.Deserialize(fs);

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
                            Console.Write(freqTable[i, j] + " ");
                        }
                        Console.WriteLine();
                    }

                    Console.Read();
                }
                catch
                {
                    Console.Write("No previous board available");
                }


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

                    if (!VerifyShipPlacement(x, y, orientation, shipIndex))
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
            int xPos = 0;
            HitStatus hitStatus;

            GenerateHeatMap();
            PrintHeatMap();
            // makes sure that these lists don't contain any values that are equal to 0. No need to search there in that case.
            CleanUpList(startingCoordinates);
            CleanUpList(possibleHitCoordinates);
            CleanUpList(cleanupCoordinates);
            Console.ReadLine();

            if (searchMode == SearchMode.SEARCH)
            {
                do
                {
                    // This is the strafing pattern from the article
                    if (startingCoordinates.Count > 0)
                    {
                        Coordinate coordinate = ChooseCoordinateFromFromList(startingCoordinates);
                        yPos = coordinate.y;
                        xPos = coordinate.x;
                    }
                    // This is the cleanup pattern
                    else if (cleanupCoordinates.Count > 0)
                    {
                        Coordinate coordinate = ChooseCoordinateFromFromList(cleanupCoordinates);
                        xPos = coordinate.x;
                        yPos = coordinate.y;
                    }
                    else
                    {
                        yPos = rnd.Next(0, 10);
                        xPos = rnd.Next(0, 10);
                        //if (yPos % 2 == 0) // odd numbers
                        //{
                        //    xPos = rnd.Next(0, 5) * 2 + 1;
                        //}
                        //else // even numbers
                        //{
                        //    xPos = rnd.Next(0, 5) * 2;
                        //}
                    }

                    hitStatus = MoveOnBoard(enemyBoard, xPos, yPos);
                } while (hitStatus == HitStatus.RETRY);
                if (hitStatus == HitStatus.HIT) // if it hit, add the coordinates to a tuple
                {
                    searchMode = SearchMode.HUNT;
                    currentHitCoordinate = new Coordinate(xPos, yPos);
                    currentShipCoordinates.Add(currentHitCoordinate);
                    AddSurroundingPossibleHitCoordinates(currentHitCoordinate);
                }
            }
            else if (searchMode == SearchMode.HUNT)
            {
                if (possibleHitCoordinates.Count == 0)
                {
                    GenerateHeatMap();
                    // Unless I'm messing this up, this will add all the current hits to the list if hunt has no more places to shoot
                    foreach (Coordinate c in currentShip.coordinates)
                    {
                        if (c != currentHitCoordinate)
                        {
                            AddSurroundingPossibleHitCoordinates(c);
                        }
                    }
                }
                Coordinate location = ChooseCoordinateFromFromList(possibleHitCoordinates);
                hitStatus = MoveOnBoard(enemyBoard, location.x, location.y);

                if (hitStatus == HitStatus.HIT)
                {
                    GenerateHeatMap();
                    possibleHitCoordinates.Clear();
                    currentShipCoordinates.Add(new Coordinate(location.x, location.y));
                    AddNarrowedPossibleHitCoordinates(location);
                    searchMode = SearchMode.NARROWEDHUNT;
                }
                else if (hitStatus == HitStatus.SUNK)
                {
                    currentShipCoordinates.Add(new Coordinate(location.x, location.y));

                    Ship sunkShip = new Ship(currentShipCoordinates.Count);
                    sunkShip.AddSunkShips(currentShipCoordinates);
                    currentShip = sunkShip;
                    currentShipCoordinates.Clear();

                    // There are more hits than should be needed to sink a ship.
                    if (sunkShip.coordinates.Count > GetLargestRemainingShipLength() || !sunkShip.AreCoordinatesAligned())
                    {
                        GenerateHeatMap();
                        searchMode = SearchMode.HUNT;
                        AddSurroundingPossibleHitCoordinates(currentHitCoordinate);
                    }
                    else
                    {
                        // clearing out the lists
                        possibleHitCoordinates.Clear();
                        currentShipCoordinates.Clear(); // redundant
                        RemoveDestroyedShip();
                        searchMode = SearchMode.SEARCH;
                    }
                    ReMarkBoardOnDestroyedShip();
                }
            }
            else if (searchMode == SearchMode.NARROWEDHUNT)
            {
                if (possibleHitCoordinates.Count == 0)
                {
                    GenerateHeatMap();
                    // this means that no ship was sunk, but we're out of possible hit spots...
                    AddSurroundingPossibleHitCoordinates(currentHitCoordinate);

                    if (possibleHitCoordinates.Count == 0)
                    {
                        // Unless I'm messing this up, this will add all the current hits to the list if hunt has no more places to shoot
                        foreach (Coordinate c in currentShip.coordinates)
                        {
                            if (c != currentHitCoordinate)
                            {
                                AddSurroundingPossibleHitCoordinates(c);
                            }
                        }
                        searchMode = SearchMode.HUNT;
                    }
                }

                Coordinate location = ChooseCoordinateFromFromList(possibleHitCoordinates);
                hitStatus = MoveOnBoard(enemyBoard, location.x, location.y);

                if (hitStatus == HitStatus.HIT)
                {
                    GenerateHeatMap();
                    searchMode = SearchMode.NARROWEDHUNT;
                    currentShipCoordinates.Add(new Coordinate(location.x, location.y));
                    AddNarrowedPossibleHitCoordinates(location);
                }
                else if (hitStatus == HitStatus.SUNK)
                {
                    // TODO: if ship of length that is greater than is actually possible is sunk, re-search those coordinates.
                    // Something that would also be cool, is that if a ship of length 4 is sunk, and then another ship of length 4 is sunk,
                    // it would search both spots to see what it missed.
                    currentShipCoordinates.Add(new Coordinate(location.x, location.y));
                    Console.WriteLine("Length: " + currentShipCoordinates.Count);
                    Console.ReadKey();

                    Ship sunkShip = new Ship(currentShipCoordinates.Count);
                    sunkShip.AddSunkShips(currentShipCoordinates);
                    currentShip = sunkShip;
                    currentShipCoordinates.Clear();

                    // There are more hits than should be needed to sink a ship.
                    if (sunkShip.coordinates.Count > GetLargestRemainingShipLength() || !sunkShip.AreCoordinatesAligned())
                    {
                        GenerateHeatMap();
                        searchMode = SearchMode.HUNT;
                        AddSurroundingPossibleHitCoordinates(currentHitCoordinate);
                    }
                    else
                    {
                        // clearing out the lists
                        possibleHitCoordinates.Clear();
                        currentShipCoordinates.Clear(); // redundant
                        RemoveDestroyedShip();
                        searchMode = SearchMode.SEARCH;
                    }

                    ReMarkBoardOnDestroyedShip();
                }
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
                //GenerateHeatMap();
                //PrintHeatMap();
                //Console.ReadKey();

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
                        BinaryFormatter bf = new BinaryFormatter();

                        using (FileStream fs = new FileStream("freq.tbl", FileMode.Open))
                            freqTable = (int[,])bf.Deserialize(fs);

                        // Create frequency map of hits
                        for (int i = 0; i < board.GetLength(0); i++)
                        {
                            for (int j = 0; j < board.GetLength(1); j++)
                            {
                                if (board[i, j] != "[ ]")
                                {
                                    freqTable[i, j] += 1;
                                }
                            }
                        }

                        // Serialize frequency table, this will allow us to learn strong positions for AI vs AI.  Will need to change this model to adapt against players
                        using (FileStream fs = new FileStream("freq.tbl", FileMode.Create))
                            bf.Serialize(fs, freqTable);
                        return true;
                    }
                }
                else if (hitStatus == HitStatus.SUNK)
                {
                    currentShipCoordinates.Clear();
                    possibleHitCoordinates.Clear();
                    Console.WriteLine("Player 1");
                    Console.WriteLine();
                    PrintPlayerView();
                    Console.WriteLine("Hit and sink!");
                    // Total hits means game is over
                    if (playerB.GetHits() >= playerB.GetMaxHits())
                    {
                        Console.WriteLine("Game Over, " + name + " wins!");
                        BinaryFormatter bf = new BinaryFormatter();

                        using (FileStream fs = new FileStream("freq.tbl", FileMode.Open))
                            freqTable = (int[,])bf.Deserialize(fs);

                        // Create frequency map of hits
                        for (int i = 0; i < board.GetLength(0); i++)
                        {
                            for (int j = 0; j < board.GetLength(1); j++)
                            {
                                if (board[i, j] != "[ ]")
                                {
                                    freqTable[i, j] += 1;
                                }
                            }
                        }

                        // Serialize frequency table, this will allow us to learn strong positions for AI vs AI.  Will need to change this model to adapt against players
                        using (FileStream fs = new FileStream("freq.tbl", FileMode.Create))
                            bf.Serialize(fs, freqTable);
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

        void CleanUpList(List<Coordinate> coordinates)
        {
            List<Coordinate> tempList = new List<Coordinate>();

            foreach (Coordinate c in coordinates)
            {
                if (heatMap[c.y, c.x] <= 0)
                {
                    tempList.Add(c);
                }
            }

            foreach (Coordinate c in tempList)
            {
                coordinates.Remove(c);
            }
        }

        /// <summary>
        /// Makes sure coordinates are on the board and that the value at the heatmap is greater than 0
        /// </summary>
        /// <param name="hitCoordinate"></param>
        /// <returns></returns>
        bool TestCoordinates(Coordinate hitCoordinate)
        {
            // need to iterate through possible hit coordinates and see if the coordinate is already in the list
            foreach (Coordinate c in possibleHitCoordinates)
            {
                if (c.CoordinatesAreEqual(hitCoordinate))
                {
                    return false;
                }
            }
            if (hitCoordinate.x < 0 || hitCoordinate.x > 9 || hitCoordinate.y < 0 || hitCoordinate.y > 9)
            {
                return false;
            }
            return !(heatMap[hitCoordinate.y, hitCoordinate.x] == 0);
        }

        void PrintPlayerView()
        {
            Console.WriteLine("   ------------- Enemy board -------------");
            PrintEnemyView();
            Console.WriteLine();
            Console.WriteLine("   ------------- Your board -------------");
            PrintBoard();
        }

        /// <summary>
        /// Remarks the board where a ship was destroyed to be all [O]
        /// This is necessary so that the heatmap doesn't favor places with a sunk ship
        /// </summary>
        private void ReMarkBoardOnDestroyedShip()
        {
            foreach (Coordinate c in currentShipCoordinates)
            {
                enemyView[c.y, c.x] = "[S]";
            }
        }

        /// <summary>
        /// This removes a destroyed ship from the remainingShipLengths list
        /// This allows the AI to know (usually) which ships are left.
        /// </summary>
        private void RemoveDestroyedShip()
        {
            foreach (int l in remainingShipLengths)
            {
                if (l == currentShipCoordinates.Count)
                {
                    remainingShipLengths.Remove(l);
                    break;
                }
            }
        }

        /// <summary>
        /// Gives you the highest coordinate to choose from and then removes it from the list (possibleHitCoordinates)
        /// </summary>
        /// <returns></returns>
        Coordinate ChooseCoordinateFromFromList(List<Coordinate> coordinates)
        {
            Coordinate highestCoordinates = new Coordinate(0, 0);
            float highestValue = 0;

            foreach (Coordinate t in coordinates)
            {
                if (heatMap[t.y, t.x] > highestValue)
                {
                    highestValue = heatMap[t.y, t.x];
                    highestCoordinates = t;
                }
            }
            coordinates.Remove(highestCoordinates);

            return highestCoordinates;
        }

        void AddSurroundingPossibleHitCoordinates(Coordinate hitCoordinates)
        {
            Coordinate coordinate = new Coordinate(hitCoordinates.x, hitCoordinates.y - 1);
            if (TestCoordinates(coordinate))
            {
                if (heatMap[coordinate.y, coordinate.x] == -1)
                {
                    coordinate = new Coordinate(hitCoordinates.x, hitCoordinates.y - 2);
                }
                if (TestCoordinates(coordinate) && heatMap[coordinate.y, coordinate.x] != -1)
                    possibleHitCoordinates.Add(coordinate);
            }

            coordinate = new Coordinate(hitCoordinates.x, hitCoordinates.y + 1);
            if (TestCoordinates(coordinate))
            {
                if (heatMap[coordinate.y, coordinate.x] == -1)
                {
                    coordinate = new Coordinate(hitCoordinates.x, hitCoordinates.y + 2);
                }
                if (TestCoordinates(coordinate) && heatMap[coordinate.y, coordinate.x] != -1)
                    possibleHitCoordinates.Add(coordinate);
            }

            coordinate = new Coordinate(hitCoordinates.x - 1, hitCoordinates.y);
            if (TestCoordinates(coordinate))
            {
                if (heatMap[coordinate.y, coordinate.x] == -1)
                {
                    coordinate = new Coordinate(hitCoordinates.x - 2, hitCoordinates.y);
                }
                if (TestCoordinates(coordinate) && heatMap[coordinate.y, coordinate.x] != -1)
                    possibleHitCoordinates.Add(coordinate);
            }

            coordinate = new Coordinate(hitCoordinates.x + 1, hitCoordinates.y);
            if (TestCoordinates(coordinate))
            {
                if (heatMap[coordinate.y, coordinate.x] == -1)
                {
                    coordinate = new Coordinate(hitCoordinates.x + 2, hitCoordinates.y);
                }
                if (TestCoordinates(coordinate) && heatMap[coordinate.y, coordinate.x] != -1)
                    possibleHitCoordinates.Add(coordinate);
            }
        }

        /// <summary>
        /// On a second hit, this function adds possible locations for the next hits (either horizontal or vertical)
        /// </summary>
        /// <param name="newHitCoordinate">The new hit</param>
        void AddNarrowedPossibleHitCoordinates(Coordinate newHitCoordinate)
        {
            // Up or down
            if (currentHitCoordinate.x == newHitCoordinate.x)
            {
                if (currentHitCoordinate.y > newHitCoordinate.y)
                {
                    Coordinate coordinate = new Coordinate(currentHitCoordinate.x, currentHitCoordinate.y + 1);

                    if (TestCoordinates(coordinate))
                    {
                        if (heatMap[coordinate.y, coordinate.x] == -1)
                        {
                            coordinate = new Coordinate(currentHitCoordinate.x, currentHitCoordinate.y + 2);
                        }
                        if (TestCoordinates(coordinate) && heatMap[coordinate.y, coordinate.x] != -1)
                            possibleHitCoordinates.Add(coordinate);
                    }

                    coordinate = new Coordinate(newHitCoordinate.x, newHitCoordinate.y - 1);
                    if (TestCoordinates(coordinate))
                    {
                        if (heatMap[coordinate.y, coordinate.x] == -1) // this checks to see if it is a hit. If it is, you should pass the hit, not ignore it
                        {
                            coordinate = new Coordinate(newHitCoordinate.x, newHitCoordinate.y - 2);
                        }
                        if (TestCoordinates(coordinate) && heatMap[coordinate.y, coordinate.x] != -1)
                            possibleHitCoordinates.Add(coordinate);
                    }
                }
                else
                {
                    Coordinate coordinate = new Coordinate(currentHitCoordinate.x, currentHitCoordinate.y - 1);

                    if (TestCoordinates(coordinate))
                    {
                        if (heatMap[coordinate.y, coordinate.x] == -1)
                        {
                            coordinate = new Coordinate(currentHitCoordinate.x, currentHitCoordinate.y - 2);
                        }
                        if (TestCoordinates(coordinate) && heatMap[coordinate.y, coordinate.x] != -1)
                            possibleHitCoordinates.Add(coordinate);
                    }

                    coordinate = new Coordinate(newHitCoordinate.x, newHitCoordinate.y + 1);

                    if (TestCoordinates(coordinate))
                    {
                        if (heatMap[coordinate.y, coordinate.x] == -1)
                        {
                            coordinate = new Coordinate(newHitCoordinate.x, newHitCoordinate.y + 2);
                        }
                        if (TestCoordinates(coordinate) && heatMap[coordinate.y, coordinate.x] != -1)
                            possibleHitCoordinates.Add(coordinate);
                    }
                }
            }
            // left or right
            else if (currentHitCoordinate.y == newHitCoordinate.y)
            {
                if (currentHitCoordinate.x > newHitCoordinate.x)
                {
                    Coordinate coordinate = new Coordinate(currentHitCoordinate.x + 1, currentHitCoordinate.y);

                    if (TestCoordinates(coordinate))
                    {
                        if (heatMap[coordinate.y, coordinate.x] == -1)
                        {
                            coordinate = new Coordinate(currentHitCoordinate.x + 2, currentHitCoordinate.y);
                        }
                        if (TestCoordinates(coordinate) && heatMap[coordinate.y, coordinate.x] != -1)
                            possibleHitCoordinates.Add(coordinate);
                    }

                    coordinate = new Coordinate(newHitCoordinate.x - 1, newHitCoordinate.y);

                    if (TestCoordinates(coordinate))
                    {
                        if (heatMap[coordinate.y, coordinate.x] == -1)
                        {
                            coordinate = new Coordinate(newHitCoordinate.x - 2, newHitCoordinate.y);
                        }
                        if (TestCoordinates(coordinate) && heatMap[coordinate.y, coordinate.x] != -1)
                            possibleHitCoordinates.Add(coordinate);
                    }
                }
                else
                {
                    Coordinate coordinate = new Coordinate(currentHitCoordinate.x - 1, currentHitCoordinate.y);
                    if (TestCoordinates(coordinate))
                    {
                        if (heatMap[coordinate.y, coordinate.x] == -1)
                        {
                            coordinate = new Coordinate(currentHitCoordinate.x - 2, currentHitCoordinate.y);
                        }
                        if (TestCoordinates(coordinate) && heatMap[coordinate.y, coordinate.x] != -1)
                            possibleHitCoordinates.Add(coordinate);
                    }

                    coordinate = new Coordinate(newHitCoordinate.x + 1, newHitCoordinate.y);
                    if (TestCoordinates(coordinate))
                    {
                        if (heatMap[coordinate.y, coordinate.x] == -1)
                        {
                            coordinate = new Coordinate(newHitCoordinate.x + 2, newHitCoordinate.y);
                        }
                        if (TestCoordinates(coordinate) && heatMap[coordinate.y, coordinate.x] != -1)
                            possibleHitCoordinates.Add(coordinate);
                    }
                }
            }
        }
    }
}
