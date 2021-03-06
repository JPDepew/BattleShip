﻿using System;
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

        public bool CoordinateIsNeighbor(Coordinate c)
        {
            if ((c.x == x + 1 || c.x == x - 1) && c.y == y)
            {
                return true;
            }
            else if ((c.y == y + 1 || c.y == y - 1) && c.x == x)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CoordinateIsAligned(Coordinate c)
        {
            if (c.x == x || c.y == y)
            {
                return true;
            }
            else
            {
                return false;
            }
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
        private List<Coordinate> foundShipCoordinates;
        private List<int> remainingShipLengths;
        private List<Coordinate> possibleRemainingSpots;
        private List<Ship> sunkShips;
        private Ship currentShip;
        float[,] heatMap;
        private bool hitFiveCoordinates = false;
        Random rnd;

        public string[,] board;
        string[,] enemyView;
        int[] shipLengths = { 2, 3, 3, 4, 5 };
        Ship[] ships = new Ship[5];

        public int[,] freqTable = new int[10, 10];
        public string freqFile;

        SearchMode searchMode;
        int boardSize = 10;
        int hits;
        int maxHits = 0;
        int turnCounter = 0;

        public Board(string _name)
        {
            // AI stuff
            rnd = new Random();
            possibleHitCoordinates = new List<Coordinate>();
            currentShipCoordinates = new List<Coordinate>();
            startingCoordinates = new List<Coordinate>();
            cleanupCoordinates = new List<Coordinate>();
            foundShipCoordinates = new List<Coordinate>();
            possibleRemainingSpots = new List<Coordinate>();
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
            InitializeCleanupCoordinates();
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
            int maxShipLength = 5; //GetLargestRemainingShipLength(); // as much as I hate to comment out this stuff, it causes problems with clustering ships.
            int minShipLength = 2; //GetSmallestRemainingShipLength();

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

            for (int _x = x, counter = 0; _x < enemyView.GetLength(1) && counter < maxShipLength; _x++, counter++)
            {
                if (enemyView[y, _x] == "[X]")
                {
                    nextToHit = true;
                    sum += (addValue + bonus);
                    break;
                }
                else if (enemyView[y, _x] == "[O]")
                {
                    //sum -= subtractValue;
                    break;
                }
                else if (enemyView[y, _x] == "[S]")
                {
                    nextToHit = true;
                    // add value here?
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
                    //sum -= subtractValue;
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
                    //sum -= subtractValue;
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
                    //sum -= subtractValue;
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
                cleanupCoordinates.Add(new Coordinate(i + 3, i));
            }
            for (int i = 0; i < boardSize - 6; i++)
            {
                // starting at (6,0)
                cleanupCoordinates.Add(new Coordinate(i + 6, i));
            }
            startingCoordinates.Add(new Coordinate(9, 0));
            for (int i = 0; i < boardSize - 1; i++)
            {
                // starting at (0,1)
                cleanupCoordinates.Add(new Coordinate(i, i + 1));
            }
            for (int i = 0; i < boardSize - 4; i++)
            {
                // starting at (0,4)
                cleanupCoordinates.Add(new Coordinate(i, i + 4));
            }
            for (int i = 0; i < boardSize - 7; i++)
            {
                // starting at (0,7)
                cleanupCoordinates.Add(new Coordinate(i, i + 7));
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
                List<Coordinate> triedShips = new List<Coordinate>();

                //Console.Write("  ");
                //for (int i = 0; i < board.GetLength(0); i++)
                //{
                //    Console.Write("  " + i + " ");
                //}
                //Console.WriteLine();
                //for (int i = 0; i < board.GetLength(1); i++)
                //{
                //    Console.Write(i + "  ");
                //    for (int j = 0; j < board.GetLength(0); j++)
                //    {
                //        Console.Write(freqTable[i, j] + " ");
                //    }
                //    Console.WriteLine();
                //}

                List<Coordinate> placedShips = new List<Coordinate>();

                getMinPlacementVal(freqTable, placedShips);

                while (ind < shipLengths.GetLength(0))
                {
                    int x;
                    int y;
                    Coordinate shipToPlace;

                    Random rand = new Random();
                    //y = rand.Next(0, board.GetLength(0));
                    //x = rand.Next(0, board.GetLength(1));

                    // Chooses a random location from the list of cells with the lowest frequencies of being hit
                    int randShip = rand.Next(placedShips.Count);
                    shipToPlace = placedShips[randShip];

                    //  This prevents an infinite loop if a ship can't be placed
                    //  If there are less locations in the list to choose from
                    //  than there are ships. a random location is chosen
                    if (placedShips.Count < 5 || triedShips.Contains(shipToPlace))
                    {
                        //Console.WriteLine("Not enough locations in list, choosing randomly");
                        x = rand.Next(0, board.GetLength(0));
                        y = rand.Next(0, board.GetLength(1));
                    }
                    else
                    {
                        x = shipToPlace.x;
                        y = shipToPlace.y;
                    }

                    triedShips.Add(shipToPlace);
                    int chance = rand.Next(0, 9);
                    string orientation;
                    if (chance > 4)
                    {
                        orientation = "d";
                    }
                    else
                    {
                        orientation = "r";
                    }

                    // Tries to correct placement of ship if it isn't able to be placed in a cell
                    // If it can't, continues and tries again
                    if (!VerifyShipPlacement(x, y, orientation, ind))
                    {
                        if (orientation == "d")
                        {
                            orientation = "r";
                        }
                        else if (orientation == "r")
                        {
                            orientation = "d";
                        }
                        if (!VerifyShipPlacement(x, y, orientation, ind))
                        {
                            continue;
                        }
                        // error in ship placement
                        else
                        {
                            continue;
                        }
                    }

                    // If all is well, place the ship.
                    PlaceShip(board, ind, x, y, orientation);
                    ind++;
                }
                Console.WriteLine("AI board setup: ");
                PrintBoard();
                Console.ReadLine();
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
                    Console.Write("Enter Letter coordinate: ");
                    do
                    {
                        _y = Console.ReadLine();
                    } while (StupidSwitchLettersToNumbersFunction(_y) < 0);
                    y = StupidSwitchLettersToNumbersFunction(_y);

                    Console.Write("Enter Number coordinate: ");
                    do
                    {
                        _x = Console.ReadLine();
                    } while (!int.TryParse(_x, out x));
                    x--;

                    // Getting orientation
                    Console.Write("Enter d to orient the ship downwards, or r to orient the ship to the right: ");
                    string orientation;
                    do
                    {
                        orientation = Console.ReadLine();
                    } while (orientation != "d" && orientation != "r");

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

        bool checkCoordinates(Coordinate c, List<Coordinate> placedShips)
        {
            foreach (Coordinate coord in placedShips)
            {
                if (c.CoordinatesAreEqual(coord))
                {
                    return false;
                }
            }

            return true;
        }

        //  Find lowest value position in frequency table, and try to place ship at location.  If it doesn't work, keep trying until a coordinate is found that does
        List<Coordinate> getMinPlacementVal(int[,] freqTable, List<Coordinate> placedShips)
        {
            int minValue = int.MaxValue;
            int minCol = -1;
            int minRow = -1;

            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if ((freqTable[i, j] <= minValue) && (checkCoordinates(new Coordinate(i, j), placedShips)))
                    {
                        minValue = freqTable[i, j];
                        minRow = i;
                        minCol = j;
                        placedShips.Add(new Coordinate(minRow, minCol));
                    }
                }
            }

            return placedShips;
        }

        bool VerifyShipPlacement(int x, int y, string orientation, int shipIndex)
        {
            if (x > board.GetLength(1) || x < 0)
            {
                return false;
            }
            if (y > board.GetLength(0) || y < 0)
            {
                return false;
            }
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
            string shipType = "[" + (shipIndex + 1).ToString() + "]"; // could that 5 be changed to ships.Length?
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
                Console.Write("  " + (i + 1) + " ");
            }
            Console.WriteLine();
            for (int i = 0; i < board.GetLength(1); i++)
            {
                Console.Write(StupidSwitchNumbersToLettersFunction(i) + "  ");
                for (int j = 0; j < board.GetLength(0); j++)
                {
                    Console.Write(board[i, j] + " ");
                }
                Console.WriteLine();
            }
        }

        int StupidSwitchLettersToNumbersFunction(string letter)
        {
            letter = letter.ToUpper();
            int number = -100000;
            switch (letter)
            {
                case "A":
                    number = 0;
                    break;
                case "B":
                    number = 1;
                    break;
                case "C":
                    number = 2;
                    break;
                case "D":
                    number = 3;
                    break;
                case "E":
                    number = 4;
                    break;
                case "F":
                    number = 5;
                    break;
                case "G":
                    number = 6;
                    break;
                case "H":
                    number = 7;
                    break;
                case "I":
                    number = 8;
                    break;
                case "J":
                    number = 9;
                    break;
            }
            return number;
        }

        string StupidSwitchNumbersToLettersFunction(int number)
        {
            string letter = "";
            switch (number)
            {
                case 0:
                    letter = "A";
                    break;
                case 1:
                    letter = "B";
                    break;
                case 2:
                    letter = "C";
                    break;
                case 3:
                    letter = "D";
                    break;
                case 4:
                    letter = "E";
                    break;
                case 5:
                    letter = "F";
                    break;
                case 6:
                    letter = "G";
                    break;
                case 7:
                    letter = "H";
                    break;
                case 8:
                    letter = "I";
                    break;
                case 9:
                    letter = "J";
                    break;
            }
            return letter;
        }

        public void PrintEnemyView()
        {
            Console.Write("  ");
            for (int i = 0; i < enemyView.GetLength(0); i++)
            {
                Console.Write("  " + (i + 1) + " ");
            }
            Console.WriteLine();
            for (int i = 0; i < enemyView.GetLength(1); i++)
            {
                Console.Write(StupidSwitchNumbersToLettersFunction(i) + "  ");
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

            turnCounter++;

            GenerateHeatMap();

            // makes sure that these lists don't contain any values that are equal to 0. No need to search there in that case.
            CleanUpList(startingCoordinates);
            CleanUpList(possibleHitCoordinates);
            CleanUpList(cleanupCoordinates);

            int counter = 0;
            if (searchMode == SearchMode.SEARCH)
            {
                do
                {
                    counter++;
                    if (counter > 300)
                    {
                        Console.WriteLine("Too many iterations through Search");
                        Console.ReadLine();
                    }
                    // This is the strafing pattern from the article
                    if (startingCoordinates.Count > 0)
                    {
                        if (turnCounter % 3 == 0)
                        {
                            Coordinate coordinate = ChooseSideCoordinateFromList(startingCoordinates);
                            yPos = coordinate.y;
                            xPos = coordinate.x;
                        }
                        else
                        {
                            Coordinate coordinate = ChooseCoordinateFromFromList(startingCoordinates);
                            yPos = coordinate.y;
                            xPos = coordinate.x;
                        }
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
                        possibleRemainingSpots.Clear();
                        AddPossibleRemainingSpotsToList(possibleRemainingSpots);

                        if (possibleRemainingSpots.Count == 0)
                        {
                            yPos = rnd.Next(0, 10);
                            xPos = rnd.Next(0, 10);
                        }
                        else
                        {
                            Coordinate coordinate = ChooseCoordinateFromFromList(possibleRemainingSpots);
                            yPos = coordinate.y;
                            xPos = coordinate.x;
                        }

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
                    Coordinate coordinate = new Coordinate(xPos, yPos);

                    if (hitFiveCoordinates)
                    {
                        searchMode = SearchMode.HUNT;
                        currentHitCoordinate = new Coordinate(xPos, yPos);
                        AddSurroundingPossibleHitCoordinates(currentHitCoordinate);
                        currentShipCoordinates.Add(currentHitCoordinate);
                    }
                    else if (foundShipCoordinates.Count + 1 >= 5)
                    {
                        hitFiveCoordinates = true;
                        searchMode = SearchMode.HUNT;
                        currentHitCoordinate = new Coordinate(xPos, yPos);
                        currentShipCoordinates.Add(currentHitCoordinate);
                        AddSurroundingPossibleHitCoordinates(currentHitCoordinate);
                    }
                    // then we're going to keep searching, so just add this to foundShipCoordinates
                    else
                    {
                        foundShipCoordinates.Add(coordinate);
                    }
                }
                else if (hitStatus == HitStatus.SUNK)
                {

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

                Coordinate location;
                do
                {
                    counter++;
                    if (counter > 300)
                    {
                        Console.WriteLine("Too many iterations through Search");
                        Console.ReadLine();
                    }
                    if (possibleHitCoordinates.Count > 0)
                    {
                        location = ChooseCoordinateFromFromList(possibleHitCoordinates);
                    }
                    else
                    {
                        // we'll just have to search. this sucks
                        if (startingCoordinates.Count > 0)
                        {
                            location = ChooseCoordinateFromFromList(startingCoordinates);
                        }
                        else if (cleanupCoordinates.Count > 0)
                        {
                            location = ChooseCoordinateFromFromList(cleanupCoordinates);
                        }
                        else
                        {
                            int y = rnd.Next(0, 10);
                            int x = rnd.Next(0, 10);
                            location = new Coordinate(x, y);
                        }
                        searchMode = SearchMode.SEARCH;
                    }
                    hitStatus = MoveOnBoard(enemyBoard, location.x, location.y);
                } while (hitStatus == HitStatus.RETRY);

                if (hitStatus == HitStatus.HIT)
                {
                    GenerateHeatMap();
                    possibleHitCoordinates.Clear();
                    currentShipCoordinates.Add(new Coordinate(location.x, location.y));

                    // before adding the narrowed possible hit coordinates, I'm checking to make sure that 
                    // location and currentHitCoordinate are aligned. 
                    if (currentHitCoordinate.CoordinateIsAligned(location))
                    {
                        AddNarrowedPossibleHitCoordinates(location);
                        searchMode = SearchMode.NARROWEDHUNT;
                    }
                    // If they're not aligned, we need to search around them.
                    else
                    {
                        AddSurroundingPossibleHitCoordinates(currentHitCoordinate);
                        searchMode = SearchMode.HUNT;
                    }

                    // So if there were not coordinates surrounding currentHitCoordinate, check location
                    // Problem is this makes it behave worse in some clustering situations. I'm not sure if we should keep it or not.
                    // It kind of makes the AI smarter, but the dumber AI is more thorough
                    if (possibleHitCoordinates.Count == 0)
                    {
                        AddSurroundingPossibleHitCoordinates(location);
                        searchMode = SearchMode.HUNT;
                    }
                }
                else if (hitStatus == HitStatus.SUNK)
                {
                    currentShipCoordinates.Add(new Coordinate(location.x, location.y));

                    AddNeighboringFoundCoordinatesToCurrentHitCoordinates();

                    Ship sunkShip = new Ship(currentShipCoordinates.Count);
                    sunkShip.AddSunkShips(currentShipCoordinates);
                    sunkShips.Add(sunkShip);
                    currentShip = sunkShip;

                    // There are more hits than should be needed to sink a ship.
                    if (sunkShip.coordinates.Count > GetLargestRemainingShipLength() || !sunkShip.AreCoordinatesAligned())
                    {
                        RemoveDestroyedShip();
                        ReMarkBoardOnDestroyedShip();
                        GenerateHeatMap();
                        searchMode = SearchMode.HUNT;
                        AddSurroundingPossibleHitCoordinates(currentHitCoordinate);

                        // if coordinates are still 0 - Problem is this makes it behave worse in some clustering situations. I'm not sure if we should keep it or not.
                        if (possibleHitCoordinates.Count == 0)
                        {
                            AddSurroundingPossibleHitCoordinates(location);
                        }
                    }
                    else
                    {
                        // clearing out the lists
                        RemoveDestroyedShip();
                        ReMarkBoardOnDestroyedShip();
                        possibleHitCoordinates.Clear();
                        currentShipCoordinates.Clear();

                        // got back to hunt with next coordinate from foundShipCoordinates

                        if (foundShipCoordinates.Count <= 0)
                        {
                            searchMode = SearchMode.SEARCH;
                        }
                        else
                        {
                            currentHitCoordinate = foundShipCoordinates[0];
                            foundShipCoordinates.Remove(currentHitCoordinate);
                            currentShipCoordinates.Add(currentHitCoordinate);
                            AddSurroundingPossibleHitCoordinates(currentHitCoordinate);
                            searchMode = SearchMode.HUNT;
                        }
                    }
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

                Coordinate location;
                do
                {
                    counter++;
                    if (counter > 300)
                    {
                        Console.WriteLine("Too many iterations through Search");
                        Console.ReadLine();
                    }

                    // This check is for the edge-case where there are no coordinates in possibleHitCoordinates even when 
                    // all the surrounding coordinates have been attempted to be added.
                    if (possibleHitCoordinates.Count > 0)
                    {
                        location = ChooseCoordinateFromFromList(possibleHitCoordinates);
                    }
                    else
                    {
                        // we'll just have to search. this sucks
                        if (startingCoordinates.Count > 0)
                        {
                            location = ChooseCoordinateFromFromList(startingCoordinates);
                        }
                        else if (cleanupCoordinates.Count > 0)
                        {
                            location = ChooseCoordinateFromFromList(cleanupCoordinates);
                        }
                        else
                        {
                            int y = rnd.Next(0, 10);
                            int x = rnd.Next(0, 10);
                            location = new Coordinate(x, y);
                        }
                        searchMode = SearchMode.SEARCH;
                    }

                    hitStatus = MoveOnBoard(enemyBoard, location.x, location.y);
                } while (hitStatus == HitStatus.RETRY);

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

                    AddNeighboringFoundCoordinatesToCurrentHitCoordinates();

                    Ship sunkShip = new Ship(currentShipCoordinates.Count);
                    sunkShip.AddSunkShips(currentShipCoordinates);
                    sunkShips.Add(sunkShip);
                    currentShip = sunkShip;

                    // There are more hits than should be needed to sink a ship.
                    if (sunkShip.coordinates.Count > GetLargestRemainingShipLength() || !sunkShip.AreCoordinatesAligned())                           // This checks if the coordinates are aligned
                    {
                        RemoveDestroyedShip();
                        ReMarkBoardOnDestroyedShip();
                        currentShipCoordinates.Clear();
                        GenerateHeatMap();
                        searchMode = SearchMode.HUNT;
                        AddSurroundingPossibleHitCoordinates(currentHitCoordinate);
                        if (possibleHitCoordinates.Count == 0)
                        {
                            AddSurroundingPossibleHitCoordinates(location);
                        }
                    }
                    else
                    {
                        // clearing out the lists
                        RemoveDestroyedShip();
                        ReMarkBoardOnDestroyedShip();
                        possibleHitCoordinates.Clear();
                        currentShipCoordinates.Clear();

                        // got back to hunt with next coordinate from foundShipCoordinates
                        if (foundShipCoordinates.Count <= 0)
                        {
                            searchMode = SearchMode.SEARCH;
                        }
                        else
                        {
                            currentHitCoordinate = foundShipCoordinates[0];
                            foundShipCoordinates.Remove(currentHitCoordinate);
                            currentShipCoordinates.Add(currentHitCoordinate);
                            AddSurroundingPossibleHitCoordinates(currentHitCoordinate);
                            searchMode = SearchMode.HUNT;
                        }
                    }
                }
            }

            if (enemyBoard.GetHits() >= enemyBoard.GetMaxHits())
            {
                Console.WriteLine("Game Over, " + name + " wins!");
                Console.ReadLine();
                return true;
            }
            else
            {
                return false;
            }
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
            Console.Write("Enter Letter coordinate: ");
            string str_y = Console.ReadLine();
            Console.Write("Enter Number coordinate: ");
            string str_x = Console.ReadLine();

            int x;
            int y;

            if (!Int32.TryParse(str_x, out x))
                return HitStatus.RETRY;
            //else if (!Int32.TryParse(str_y, out y))
            //    return HitStatus.RETRY;
            y = StupidSwitchLettersToNumbersFunction(str_y);

            if (x > 9 || y > 9 || y < 0 || x < 0)
            {
                Console.WriteLine("Coordinates are outside of bounds. Enter coordinates between 0 and 9.");
                Console.WriteLine("Press enter to continue.");
                Console.ReadKey();
                return HitStatus.RETRY;
            }

            // x decrementing correctly
            x--;

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
                    ships[4].hits++;
                    hitStatus = ships[4].hits >= ships[4].length ? HitStatus.SUNK : HitStatus.HIT;

                    enemyBoard.SetLocation(x, y, "[X]");
                    enemyView[y, x] = "[X]";
                    break;
                case "[4]":
                    ships[3].hits++;
                    hitStatus = ships[3].hits >= ships[3].length ? HitStatus.SUNK : HitStatus.HIT;

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
                    ships[1].hits++;
                    hitStatus = ships[1].hits >= ships[1].length ? HitStatus.SUNK : HitStatus.HIT;

                    enemyBoard.SetLocation(x, y, "[X]");
                    enemyView[y, x] = "[X]";
                    break;
                case "[1]":
                    ships[0].hits++;
                    hitStatus = ships[0].hits >= ships[0].length ? HitStatus.SUNK : HitStatus.HIT;

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
                        BinaryFormatter bf = new BinaryFormatter();

                        //if (File.Exists(freqFile))
                        //{
                        //    using (FileStream fs = new FileStream(freqFile, FileMode.Open))
                        //        freqTable = (int[,])bf.Deserialize(fs);
                        //}

                        // Create frequency map of hits
                        //for (int i = 0; i < board.GetLength(0); i++)
                        //{
                        //    for (int j = 0; j < board.GetLength(1); j++)
                        //    {
                        //        if (board[i, j] != "[ ]")
                        //        {
                        //            freqTable[i, j] += 1;
                        //        }
                        //    }
                        //}

                        // Serialize frequency table, this will allow us to learn strong positions for AI vs AI.  Will need to change this model to adapt against players
                        //using (FileStream fs = new FileStream(freqFile, FileMode.Create))
                        //    bf.Serialize(fs, freqTable);
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

                        // Check for file existing
                        //if (File.Exists(freqFile))
                        //{
                        //    using (FileStream fs = new FileStream(freqFile, FileMode.Open))
                        //        freqTable = (int[,])bf.Deserialize(fs);
                        //}

                        // Create frequency map of hits
                        //for (int i = 0; i < board.GetLength(0); i++)
                        //{
                        //    for (int j = 0; j < board.GetLength(1); j++)
                        //    {
                        //        if (board[i, j] != "[ ]")
                        //        {
                        //            freqTable[i, j] += 1;
                        //        }
                        //    }
                        //}

                        //// Serialize frequency table, this will allow us to learn strong positions for AI vs AI.  Will need to change this model to adapt against players
                        //using (FileStream fs = new FileStream(freqFile, FileMode.Create))
                        //    bf.Serialize(fs, freqTable);
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

        /// <summary>
        /// This removes all elements from the list that are less than or equal to zero
        /// </summary>
        /// <param name="coordinates">The list to be cleaned up</param>
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
        /// Chooses a coordinate that is on the side of the board.
        /// </summary>
        /// <param name="coordinates"></param>
        /// <returns></returns>
        Coordinate ChooseSideCoordinateFromList(List<Coordinate> coordinates)
        {
            Coordinate sideCoordinate = new Coordinate(0, 0);
            bool found = false;

            foreach (Coordinate c in coordinates)
            {
                if (c.y == 9 || c.y == 0 || c.x == 9 || c.x == 0)
                {
                    sideCoordinate = c;
                    found = true;
                    break;
                }
            }

            if (found)
            {
                coordinates.Remove(sideCoordinate);
            }
            else
            {
                sideCoordinate = ChooseCoordinateFromFromList(coordinates);
            }

            return sideCoordinate;
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

        private void RemoveCoordinateFromList(List<Coordinate> list, Coordinate coordinate)
        {
            Coordinate coordinateToBeRemoved = null;
            foreach (Coordinate c in list)
            {
                if (c.x == coordinate.x && c.y == coordinate.y)
                {
                    coordinateToBeRemoved = c;
                }
            }

            if (coordinateToBeRemoved != null)
            {
                list.Remove(coordinateToBeRemoved);
            }
        }

        Coordinate ChooseRandomCoordinateFromList(List<Coordinate> coordinates)
        {
            int i = rnd.Next(0, coordinates.Count);
            Coordinate coordinate = coordinates[i];
            coordinates.Remove(coordinate);
            return coordinate;
        }

        /// if there is a neighbor of any object in currentShipCoordinates in foundShipCoordinates,
        /// add that neighbor to currentShipCoordinates
        private void AddNeighboringFoundCoordinatesToCurrentHitCoordinates()
        {
            List<Coordinate> coordinatesToAdd = new List<Coordinate>();

            foreach (Coordinate cs in currentShipCoordinates)
            {
                foreach (Coordinate cf in foundShipCoordinates)
                {
                    if (cs.CoordinateIsNeighbor(cf))
                    {
                        coordinatesToAdd.Add(cf);
                    }
                }
            }

            foreach (Coordinate c in coordinatesToAdd)
            {
                currentShipCoordinates.Add(c);
                foundShipCoordinates.Remove(c);
            }
        }

        private bool ListContainsCoordinate(List<Coordinate> coordinates, Coordinate coordinate)
        {
            foreach (Coordinate c in coordinates)
            {
                if (c.x == coordinate.x && c.y == coordinate.y)
                {
                    return true;
                }
            }
            return false;
        }

        void AddSurroundingPossibleHitCoordinates(Coordinate hitCoordinates)
        {
            Coordinate coordinate = new Coordinate(hitCoordinates.x, hitCoordinates.y - 1);
            if (TestCoordinates(coordinate))
            {
                if (heatMap[coordinate.y, coordinate.x] == -1)
                {
                    RemoveCoordinateFromList(foundShipCoordinates, coordinate);
                    if (!ListContainsCoordinate(currentShipCoordinates, coordinate))
                    {
                        currentShipCoordinates.Add(coordinate);
                    }
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
                    RemoveCoordinateFromList(foundShipCoordinates, coordinate);
                    if (!ListContainsCoordinate(currentShipCoordinates, coordinate))
                    {
                        currentShipCoordinates.Add(coordinate);
                    }
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
                    RemoveCoordinateFromList(foundShipCoordinates, coordinate);
                    if (!ListContainsCoordinate(currentShipCoordinates, coordinate))
                    {
                        currentShipCoordinates.Add(coordinate);
                    }
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
                    RemoveCoordinateFromList(foundShipCoordinates, coordinate);
                    if (!ListContainsCoordinate(currentShipCoordinates, coordinate))
                    {
                        currentShipCoordinates.Add(coordinate);
                    }
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
                            RemoveCoordinateFromList(foundShipCoordinates, coordinate);
                            if (!ListContainsCoordinate(currentShipCoordinates, coordinate))
                            {
                                currentShipCoordinates.Add(coordinate);
                            }
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
                            RemoveCoordinateFromList(foundShipCoordinates, coordinate);
                            if (!ListContainsCoordinate(currentShipCoordinates, coordinate))
                            {
                                currentShipCoordinates.Add(coordinate);
                            }
                            coordinate = new Coordinate(newHitCoordinate.x, newHitCoordinate.y - 2);
                        }
                        if (TestCoordinates(coordinate) && heatMap[coordinate.y, coordinate.x] != -1)
                            possibleHitCoordinates.Add(coordinate);
                    }
                }
                else // currentHitCoordinate.y < newHitCoordinate.y
                {
                    Coordinate coordinate = new Coordinate(currentHitCoordinate.x, currentHitCoordinate.y - 1);

                    if (TestCoordinates(coordinate))
                    {
                        if (heatMap[coordinate.y, coordinate.x] == -1)
                        {
                            RemoveCoordinateFromList(foundShipCoordinates, coordinate);
                            if (!ListContainsCoordinate(currentShipCoordinates, coordinate))
                            {
                                currentShipCoordinates.Add(coordinate);
                            }
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
                            RemoveCoordinateFromList(foundShipCoordinates, coordinate);
                            if (!ListContainsCoordinate(currentShipCoordinates, coordinate))
                            {
                                currentShipCoordinates.Add(coordinate);
                            }
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
                            RemoveCoordinateFromList(foundShipCoordinates, coordinate);
                            if (!ListContainsCoordinate(currentShipCoordinates, coordinate))
                            {
                                currentShipCoordinates.Add(coordinate);
                            }
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
                            RemoveCoordinateFromList(foundShipCoordinates, coordinate);
                            if (!ListContainsCoordinate(currentShipCoordinates, coordinate))
                            {
                                currentShipCoordinates.Add(coordinate);
                            }
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
                            RemoveCoordinateFromList(foundShipCoordinates, coordinate);
                            if (!ListContainsCoordinate(currentShipCoordinates, coordinate))
                            {
                                currentShipCoordinates.Add(coordinate);
                            }
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
                            RemoveCoordinateFromList(foundShipCoordinates, coordinate);
                            if (!ListContainsCoordinate(currentShipCoordinates, coordinate))
                            {
                                currentShipCoordinates.Add(coordinate);
                            }
                            coordinate = new Coordinate(newHitCoordinate.x + 2, newHitCoordinate.y);
                        }
                        if (TestCoordinates(coordinate) && heatMap[coordinate.y, coordinate.x] != -1)
                            possibleHitCoordinates.Add(coordinate);
                    }
                }
            }
        }

        void AddPossibleRemainingSpotsToList(List<Coordinate> coordinates)
        {
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if (heatMap[j, i] != 0)
                    {
                        coordinates.Add(new Coordinate(i, j));
                    }
                }
            }
        }
    }
}