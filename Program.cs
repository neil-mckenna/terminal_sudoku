
using System.Runtime.CompilerServices;
using System.Timers;
using TerminalSudoku_2;

namespace TerminalSudoko_2
{
    public enum BOARDSIZE
    {
        NINE = 9,
        TWEVLE = 12,
        FIFTEEN = 15
    }


    public class Program
    {
        #region variables & statics

        // debug option to show board
        public static bool DEBUG = false;

        // variable for global board size
        public static int bSize = (int)BOARDSIZE.NINE;

        // 2 dimensional int array to store data boards
        private static int[,]? solvedBoard;
        private static int[,]? riddleBoard;

        // this is the option for varying difficulty (81 with a BSIZE of 9, 15 is basic difficulty start)
        public static int piecesToErase = 15;

        // a link to a a class for generating random numbers and shorthand
        public static RandomNumber? rnd = null;

        // main game loop
        static bool gameRunning = false;

        // this hold keypress information
        static ConsoleKeyInfo cKey;

        // this a list of real board location in a tuple form
        public static List<Tuple<int, int>> zeroList = new List<Tuple<int, int>>();


        // this is a list of board location but with a plus 1
        // so it easier for human to count instead of starting at 0
        public static List<Tuple<int, int>> readTuples = new List<Tuple<int, int>>();

        // this is for the current selection location in the list tuple
        public static Tuple<int, int>? selectedTuple;

        // this is the default selection in the list, I use this to cycle through list
        public static int selectedIndex = 0;

        // was used to hide information
        private static bool shown = false;

        // this is to stop adding tuples to my list for tidiness
        private static bool addedTuples = true;

        // this is for showing menu
        private static bool inMenu = true;

        //
        private static string difficultyString = "";

        //
        private const int secondsInMin = 60;

        // chosen Mins
        private static int chosenMins = 7;

        private static int currentTimerCounter = 0;

        private static System.Timers.Timer myTimer = new System.Timers.Timer(1000);

        #endregion


        #region Main

        static void Main(string[] args)
        {
            solvedBoard = new int[bSize, bSize];
            riddleBoard = new int[bSize, bSize];


            // count up seconds
            currentTimerCounter = chosenMins * secondsInMin;

            // enable event
            myTimer.Elapsed += Tick;
            // 1 sec = 1000 milliseconds
            myTimer.Interval = 1000;


            // change console colors
            SetConsoleColor();

            // this is do while for teh selecting the difficulty, this affect the 0s in the grid and add the message to display
            do
            {
                DifficultyMenu();
            }
            while (inMenu == true);

            // clear top menu
            Console.Clear();


            // initialise the random class
            rnd = new RandomNumber();

            // crucial method to prepopulate
            SetupBoard();

            //Console.WriteLine("\tDebug solvedBoard\n");
            //DebugGrid(ref solvedBoard);

            //DisplayBoard(ref solvedBoard);

            // setups up riddle
            CreateRiddleBoard(ref solvedBoard, ref riddleBoard);

            //Console.WriteLine("\tDebug riddleBoard\n");
            //DebugGrid(ref riddleBoard);



            gameRunning = true;


            do
            {
                // Enable the Timer
                myTimer.Enabled = true;

                ReDrawScreen();

                //Console.ReadLine();

                // clear screen
                Console.Clear();
                // clear previous console history
                Console.WriteLine("\x1b[3J");


                //Thread.Sleep(deltaTime);
            }
            while (gameRunning);



            // need to show screen
            Console.ReadLine();
        }

        // This method will be called every 1 second
        private static void Tick(object? sender, ElapsedEventArgs e)
        {
            currentTimerCounter--;
            if (currentTimerCounter == 0)
            {
                myTimer.Stop();
            }

            UpdateCountdownTimerText();
            //ReDrawScreen();
            //Console.WriteLine($"COUNTER: {currentTimerCounter}");
        }

        private static void ReDrawScreen()
        {
            Console.Clear();

            DrawGameBoard();
            HeadsUpDisplay();

            if (DEBUG)
            {

                if(riddleBoard != null && solvedBoard != null)
                {
                    Console.WriteLine("\nriddleBoard\n");
                    DebugGrid(ref riddleBoard);
                    Console.WriteLine("\nanswersBoard\n");
                    DebugGrid(ref solvedBoard);
                    Console.WriteLine("\n");
                }


            }

            CheckForPlayerInput();
        }

        private static void DifficultyMenu()
        {
            Console.Clear();

            //Console.WriteLine("InMENU: \t" + inMenu + " gameRunning: \t" + gameRunning);

            Console.WriteLine("Please Select a difficulty option\n");
            Console.WriteLine("Press E => Easy");
            Console.WriteLine("Press M => Medium");
            Console.WriteLine("Press H => Hard");
            Console.WriteLine("Press I => Insane");
            Console.WriteLine("\n");
            Console.WriteLine("Press Escape => To Quit Application");

            Console.WriteLine("\nPress D => To Turn on DEBUG");



            ConsoleKeyInfo ckiInMenu = Console.ReadKey(true);

            if (ckiInMenu.Key == ConsoleKey.E)
            {
                piecesToErase = 0;
                // keep all difficulty string 6 letter long for display purposes
                difficultyString = " EASY ";
                inMenu = false;

            }
            else if (ckiInMenu.Key == ConsoleKey.M)
            {
                piecesToErase = 25;
                difficultyString = "MEDIUM";
                inMenu = false;
            }
            else if (ckiInMenu.Key == ConsoleKey.H)
            {
                piecesToErase = 35;
                difficultyString = " HARD ";
                inMenu = false;
            }
            else if (ckiInMenu.Key == ConsoleKey.I)
            {
                piecesToErase = 45;
                difficultyString = "INSANE";
                inMenu = false;
            }
            else if (ckiInMenu.Key == ConsoleKey.Escape)
            {
                Console.WriteLine("Exiting Game!");
                gameRunning = false;
                System.Environment.Exit(0);
                Console.ReadKey();

            }
            else if (ckiInMenu.Key == ConsoleKey.D)
            {
                DEBUG = !DEBUG;
                Console.WriteLine("DEBUG ON: " + DEBUG);
                Console.WriteLine("Debug Changed please selection a game option!");
                Console.ReadKey();

            }
            else
            {

            }
        }

        #endregion

        #region console & player input
        //--------------------------------------GAME-INPUT-----------------------------------
        // a method to handle player input

        private static void HeadsUpDisplay()
        {

            Console.WriteLine("\nDEBUG ON :\t" + DEBUG);
            Console.WriteLine("press U \t\t\t=>\t update selected");
            Console.WriteLine("press A or LeftArrow \t\t=>\t cycle left");
            Console.WriteLine("press D or RightArrow \t\t=>\t cycle right");
            Console.WriteLine("press L \t\t\t=>\t list all locations");
            Console.WriteLine("press C \t\t\t=>\t check if correct");
            Console.WriteLine("press N \t\t\t=>\t rebuild board and restart");
            Console.WriteLine("press Escape \t\t\t=>\t exit\n");


            zeroList.Sort();

            //Console.WriteLine("Zerolist count: " + zeroList.Count + " readTuples count: " + readTuples.Count);
            //Console.WriteLine("bool " +  addedTuples);


            while(addedTuples)
            {
                foreach (Tuple<int, int> tp in zeroList)
                {
                    //Console.WriteLine($"{tp.Item1 + 1},{tp.Item2 + 1}");
                    readTuples.Add(new Tuple<int, int>(tp.Item1 + 1, tp.Item2 + 1));
                }

                addedTuples = false;
            }

            //Console.WriteLine("bool " + addedTuples);


            if(readTuples.Count > 0)
            {
                selectedTuple = readTuples[selectedIndex];
                Console.OutputEncoding = System.Text.Encoding.Default;
                Console.WriteLine("Currently selected : ↓ , → : " + readTuples[selectedIndex]);
            }
            else
            {
                selectedTuple = new Tuple<int, int>(0, 0);
                Console.WriteLine("Nothing selected as no missing 0s in the grid");
            }


        }

        private static void CheckForPlayerInput()
        {
            ConsoleKeyInfo cki = Console.ReadKey(true);

            //Console.WriteLine("Key pressed: " + cki.Key);
            int num = 0;

            // add a new value to the riddleBoard
            if (cki.Key == ConsoleKey.U || cki.Key == ConsoleKey.Enter)
            {


                Console.OutputEncoding = System.Text.Encoding.Default;
                Console.WriteLine("Enter a new value (1 \u2192 , 9 \u2193 ) for the selected location");
                string? inputNum = Console.ReadLine();
                //Console.WriteLine("\nNumber entered " + inputNum);
                if (inputNum != null)
                {
                    bool res = int.TryParse(inputNum, out num);
                    if (res)
                    {
                        //Console.WriteLine("number given " + num);
                        if (selectedTuple != null)
                        {
                            if(riddleBoard != null)
                            {
                                riddleBoard[selectedTuple.Item1 - 1, selectedTuple.Item2 - 1] = num;
                            }

                            //Console.WriteLine("selected location updated to " + num);
                        }
                        else
                        {
                            Console.WriteLine("Selected is null!");
                        }

                    }
                    else
                    {
                        Console.WriteLine("Not a valid number, nothing changed");
                    }
                }
            }

            // cycle left through available options
            if (cki.Key == ConsoleKey.D || cki.Key == ConsoleKey.RightArrow)
            {
                if (selectedTuple != null && readTuples.Count > 0)
                {
                    if (selectedIndex >= readTuples.Count - 1)
                    {
                        selectedIndex = 0;
                        Console.WriteLine(readTuples[selectedIndex]);
                    }
                    else
                    {
                        selectedIndex++;
                        Console.WriteLine(readTuples[selectedIndex]);
                    }


                }
                else
                {
                    Console.WriteLine("Selected is null!");
                }
            }

            // cycle right through available options
            if (cki.Key == ConsoleKey.A || cki.Key == ConsoleKey.LeftArrow)
            {
                if (selectedTuple != null && readTuples.Count > 0)
                {
                    if (selectedIndex < 1)
                    {
                        selectedIndex = readTuples.Count - 1;
                        Console.WriteLine(readTuples[selectedIndex]);
                    }
                    else
                    {
                        selectedIndex--;
                        Console.WriteLine(readTuples[selectedIndex]);
                    }

                }
                else
                {
                    Console.WriteLine("Selected is null!");
                }
            }

            // show all available locations
            if (cki.Key == ConsoleKey.L)
            {
                Console.WriteLine("Available locations 1st number down , second along");
                foreach (Tuple<int, int> nt in readTuples)
                {
                    Console.Write(nt.Item1 + "," + nt.Item2 + "\t");

                }

                Console.ReadKey();
            }

            if (cki.Key == ConsoleKey.C)
            {
                CheckComplete();
                Console.ReadKey();
            }

            if (cki.Key == ConsoleKey.N)
            {
                Console.WriteLine("Rebuilding the board!");
                RestartBoard();
                Console.ReadKey();
            }

            if (cki.Key == ConsoleKey.Escape)
            {
                /*Console.WriteLine("Exiting Game!");
                gameRunning = false;
                System.Environment.Exit(0);*/

                // go back to main menu rather than kill program
                // reset bools
                inMenu = false;
                gameRunning = false;
                // super broken work round
                // recall main this likely will cause a memory problems
                Main([]);

                //Console.ReadKey();

            }
        }

        public static void RestartBoard()
        {
            zeroList.Clear();
            readTuples.Clear();

            addedTuples = true;

            SetupBoard();
            if(solvedBoard != null && riddleBoard != null)
            {
                CreateRiddleBoard(ref solvedBoard, ref riddleBoard);
            }


        }


        private static void SetConsoleColor()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Yellow;
        }

        private static void SetupBoard()
        {
            // create x and y edges of the board between 1 - 9
            if(solvedBoard != null)
            {

                FillBoardEdges(ref solvedBoard);

                SolvedInnerGrid(ref solvedBoard);
                //DebugGrid(ref solvedBoard);
            }
        }

        public static void DrawGameBoard()
        {
            if(riddleBoard != null)
            {
                DisplayBoard(ref riddleBoard);
            }


        }


        #endregion

        #region board functions
        //--------------------------------------BOARD-FUNCTIONS----------------------------------
        private static int[,] InitGrid(ref int[,] board)
        {
            for (int i = 0; i < bSize; i++)
            {
                for (int j = 0; j < bSize; j++)
                {

                    board[i, j] = (i * 3 + i / 3 + j) % 9 + 1;
                    //board[i, j] = 9;

                }

            }

            return board;
        }

        public static void DebugGrid(ref int[,] board)
        {
            string s = "";
            int sep = 0;

            for (int i = 0; i < bSize; i++)
            {
                s += "|";
                for (int j = 0; j < bSize; j++)
                {
                    s += " " + board[i, j].ToString() + " ";

                    sep = j % 3;
                    if (sep == 2)
                    {
                        s += "|";
                    }
                }

                s += "\n";


            }

            Console.WriteLine(s);

        }
        #endregion

        #region display board
        //-------------------------------------DISPLAY-BOARD-----------------------------------
        private static void DisplayBoard(ref int[,] board)
        {



            Console.WriteLine("##\t\t       Neil's Sudoku            \t##");
            Console.WriteLine("##\t\t         " + difficultyString + "               \t\t##");
            Console.WriteLine(UpdateCountdownTimerText());
            Console.WriteLine("##\t|---------------------------------------|\t##");


            if (bSize == (int)BOARDSIZE.NINE)
            {
                // row 0
                //Console.WriteLine($"##\t| {board[0, 0]} {board[0, 1]} {board[0, 2]} | {board[0, 3]} {board[0, 4]} {board[0, 5]} | {board[0, 6]} {board[0, 7]} {board[0, 8]} | \t##");
                RowDisplayer(board[0, 0], board[0, 1], board[0, 2], board[0, 3], board[0, 4], board[0, 5], board[0, 6], board[0, 7], board[0, 8], 0, 0, 0, 0, 0, 0);


                // row 1
                //Console.WriteLine($"##\t| {board[1, 0]} {board[1, 1]} {board[1, 2]} | {board[1, 3]} {board[1, 4]} {board[1, 5]} | {board[1, 6]} {board[1, 7]} {board[1, 8]} | \t##");
                RowDisplayer(board[1, 0], board[1, 1], board[1, 2], board[1, 3], board[1, 4], board[1, 5], board[1, 6], board[1, 7], board[1, 8], 0, 0, 0, 0, 0, 0);

                //row 2
                //Console.WriteLine($"##\t| {board[2, 0]} {board[2, 1]} {board[2, 2]} | {board[2, 3]} {board[2, 4]} {board[2, 5]} | {board[2, 6]} {board[2, 7]} {board[2, 8]} | \t##");
                RowDisplayer(board[2, 0], board[2, 1], board[2, 2], board[2, 3], board[2, 4], board[2, 5], board[2, 6], board[2, 7], board[2, 8], 0, 0, 0, 0, 0, 0);

                // break
                Console.WriteLine("##\t|---------------------------------------|\t##");

                //row 3
                //Console.WriteLine($"##\t| {board[3, 0]} {board[3, 1]} {board[3, 2]} | {board[3, 3]} {board[3, 4]} {board[3, 5]} | {board[3, 6]} {board[3, 7]} {board[3, 8]} | \t##");
                RowDisplayer(board[3, 0], board[3, 1], board[3, 2], board[3, 3], board[3, 4], board[3, 5], board[3, 6], board[3, 7], board[3, 8], 0, 0, 0, 0, 0, 0);

                //row 4
                //Console.WriteLine($"##\t| {board[4, 0]} {board[4, 1]} {board[4, 2]} | {board[4, 3]} {board[4, 4]} {board[4, 5]} | {board[4, 6]} {board[4, 7]} {board[4, 8]} | \t##");
                RowDisplayer(board[4, 0], board[4, 1], board[4, 2], board[4, 3], board[4, 4], board[4, 5], board[4, 6], board[4, 7], board[4, 8], 0, 0, 0, 0, 0, 0);


                //row 5
                //Console.WriteLine($"##\t| {board[5, 0]} {board[5, 1]} {board[5, 2]} | {board[5, 3]} {board[5, 4]} {board[5, 5]} | {board[5, 6]} {board[5, 7]} {board[5, 8]} | \t##");
                RowDisplayer(board[5, 0], board[5, 1], board[5, 2], board[5, 3], board[5, 4], board[5, 5], board[5, 6], board[5, 7], board[5, 8], 0, 0, 0, 0, 0, 0);


                // break
                Console.WriteLine("##\t|---------------------------------------|\t##");

                // row 6
                //Console.WriteLine($"##\t| {board[6, 0]} {board[6, 1]} {board[6, 2]} | {board[6, 3]} {board[6, 4]} {board[6, 5]} | {board[6, 6]} {board[6, 7]} {board[6, 8]} | \t##");
                RowDisplayer(board[6, 0], board[6, 1], board[6, 2], board[6, 3], board[6, 4], board[6, 5], board[6, 6], board[6, 7], board[6, 8], 0, 0, 0, 0, 0, 0);


                //row 7
                //Console.WriteLine($"##\t| {board[7, 0]} {board[7, 1]} {board[7, 2]} | {board[7, 3]} {board[7, 4]} {board[7, 5]} | {board[7, 6]} {board[7, 7]} {board[7, 8]} | \t##");
                RowDisplayer(board[7, 0], board[7, 1], board[7, 2], board[7, 3], board[7, 4], board[7, 5], board[7, 6], board[7, 7], board[7, 8], 0, 0, 0, 0, 0, 0);


                // row 8
                //Console.WriteLine($"##\t| {board[8, 0]} {board[8, 1]} {board[8, 2]} | {board[8, 3]} {board[8, 4]} {board[8, 5]} | {board[8, 6]} {board[8, 7]} {board[8, 8]} | \t##");
                RowDisplayer(board[8, 0], board[8, 1], board[8, 2], board[8, 3], board[8, 4], board[8, 5], board[8, 6], board[8, 7], board[8, 8], 0, 0, 0, 0, 0, 0);


                // break
                Console.WriteLine("##\t|---------------------------------------|\t##");

            }
            if (bSize == (int)BOARDSIZE.TWEVLE)
            {
                // row 0
                //Console.WriteLine($"##\t| {board[0, 0]} {board[0, 1]} {board[0, 2]} | {board[0, 3]} {board[0, 4]} {board[0, 5]} | {board[0, 6]} {board[0, 7]} {board[0, 8]} | \t##");
                RowDisplayer(board[0, 0], board[0, 1], board[0, 2], board[0, 3], board[0, 4], board[0, 5], board[0, 6], board[0, 7], board[0, 8], board[0, 9], board[0, 10], board[0, 11], 0, 0, 0);


                // row 1
                //Console.WriteLine($"##\t| {board[1, 0]} {board[1, 1]} {board[1, 2]} | {board[1, 3]} {board[1, 4]} {board[1, 5]} | {board[1, 6]} {board[1, 7]} {board[1, 8]} | \t##");
                RowDisplayer(board[1, 0], board[1, 1], board[1, 2], board[1, 3], board[1, 4], board[1, 5], board[1, 6], board[1, 7], board[1, 8], board[1, 9], board[1, 10], board[1, 11], 0, 0, 0);

                //row 2
                //Console.WriteLine($"##\t| {board[2, 0]} {board[2, 1]} {board[2, 2]} | {board[2, 3]} {board[2, 4]} {board[2, 5]} | {board[2, 6]} {board[2, 7]} {board[2, 8]} | \t##");
                RowDisplayer(board[2, 0], board[2, 1], board[2, 2], board[2, 3], board[2, 4], board[2, 5], board[2, 6], board[2, 7], board[2, 8], board[2, 9], board[2, 10], board[2, 11], 0, 0, 0);

                // break
                Console.WriteLine("##\t|-------------------------------|\t##");

                //row 3
                //Console.WriteLine($"##\t| {board[3, 0]} {board[3, 1]} {board[3, 2]} | {board[3, 3]} {board[3, 4]} {board[3, 5]} | {board[3, 6]} {board[3, 7]} {board[3, 8]} | \t##");
                RowDisplayer(board[3, 0], board[3, 1], board[3, 2], board[3, 3], board[3, 4], board[3, 5], board[3, 6], board[3, 7], board[3, 8], board[3, 9], board[3, 10], board[3, 11], 0, 0, 0);

                //row 4
                //Console.WriteLine($"##\t| {board[4, 0]} {board[4, 1]} {board[4, 2]} | {board[4, 3]} {board[4, 4]} {board[4, 5]} | {board[4, 6]} {board[4, 7]} {board[4, 8]} | \t##");
                RowDisplayer(board[4, 0], board[4, 1], board[4, 2], board[4, 3], board[4, 4], board[4, 5], board[4, 6], board[4, 7], board[4, 8], board[4, 9], board[4, 10], board[4, 11], 0, 0, 0);


                //row 5
                //Console.WriteLine($"##\t| {board[5, 0]} {board[5, 1]} {board[5, 2]} | {board[5, 3]} {board[5, 4]} {board[5, 5]} | {board[5, 6]} {board[5, 7]} {board[5, 8]} | \t##");
                RowDisplayer(board[5, 0], board[5, 1], board[5, 2], board[5, 3], board[5, 4], board[5, 5], board[5, 6], board[5, 7], board[5, 8], board[5, 9], board[5, 10], board[5, 11], 0, 0, 0);


                // break
                Console.WriteLine("##\t|-------------------------------|\t##");

                // row 6
                //Console.WriteLine($"##\t| {board[6, 0]} {board[6, 1]} {board[6, 2]} | {board[6, 3]} {board[6, 4]} {board[6, 5]} | {board[6, 6]} {board[6, 7]} {board[6, 8]} | \t##");
                RowDisplayer(board[6, 0], board[6, 1], board[6, 2], board[6, 3], board[6, 4], board[6, 5], board[6, 6], board[6, 7], board[6, 8], board[6, 9], board[6, 10], board[6, 11], 0, 0, 0);


                //row 7
                //Console.WriteLine($"##\t| {board[7, 0]} {board[7, 1]} {board[7, 2]} | {board[7, 3]} {board[7, 4]} {board[7, 5]} | {board[7, 6]} {board[7, 7]} {board[7, 8]} | \t##");
                RowDisplayer(board[7, 0], board[7, 1], board[7, 2], board[7, 3], board[7, 4], board[7, 5], board[7, 6], board[7, 7], board[7, 8], board[7, 9], board[7, 10], board[7, 11], 0, 0, 0);


                // row 8
                //Console.WriteLine($"##\t| {board[8, 0]} {board[8, 1]} {board[8, 2]} | {board[8, 3]} {board[8, 4]} {board[8, 5]} | {board[8, 6]} {board[8, 7]} {board[8, 8]} | \t##");
                RowDisplayer(board[8, 0], board[8, 1], board[8, 2], board[8, 3], board[8, 4], board[8, 5], board[8, 6], board[8, 7], board[8, 8], board[8, 9], board[8, 10], board[8, 11], 0, 0, 0);

                // break
                Console.WriteLine("##\t|-------------------------------|\t##");

                // row 9
                RowDisplayer(board[9, 0], board[9, 1], board[9, 2], board[9, 3], board[9, 4], board[9, 5], board[9, 6], board[9, 7], board[9, 8], board[9, 9], board[9, 10], board[9, 11], 0, 0, 0);

                // row 10
                RowDisplayer(board[10, 0], board[10, 1], board[10, 2], board[10, 3], board[10, 4], board[10, 5], board[10, 6], board[10, 7], board[10, 8], board[10, 9], board[10, 10], board[10, 11], 0, 0, 0);

                // row 11
                RowDisplayer(board[11, 0], board[11, 1], board[11, 2], board[11, 3], board[11, 4], board[11, 5], board[11, 6], board[11, 7], board[11, 8], board[11, 9], board[11, 10], board[11, 11], 0, 0, 0);

                // break
                Console.WriteLine("##\t|-------------------------------|\t##");

            }

            if (bSize == (int)BOARDSIZE.FIFTEEN)
            {
                // row 0
                //Console.WriteLine($"##\t| {board[0, 0]} {board[0, 1]} {board[0, 2]} | {board[0, 3]} {board[0, 4]} {board[0, 5]} | {board[0, 6]} {board[0, 7]} {board[0, 8]} | \t##");
                RowDisplayer(board[0, 0], board[0, 1], board[0, 2], board[0, 3], board[0, 4], board[0, 5], board[0, 6], board[0, 7], board[0, 8], board[0, 9], board[0, 10], board[0, 11], board[0, 12], board[0, 13], board[0, 14]);


                // row 1
                //Console.WriteLine($"##\t| {board[1, 0]} {board[1, 1]} {board[1, 2]} | {board[1, 3]} {board[1, 4]} {board[1, 5]} | {board[1, 6]} {board[1, 7]} {board[1, 8]} | \t##");
                RowDisplayer(board[1, 0], board[1, 1], board[1, 2], board[1, 3], board[1, 4], board[1, 5], board[1, 6], board[1, 7], board[1, 8], board[1, 9], board[1, 10], board[1, 11], board[1, 12], board[1, 13], board[1, 14]);

                //row 2
                //Console.WriteLine($"##\t| {board[2, 0]} {board[2, 1]} {board[2, 2]} | {board[2, 3]} {board[2, 4]} {board[2, 5]} | {board[2, 6]} {board[2, 7]} {board[2, 8]} | \t##");
                RowDisplayer(board[2, 0], board[2, 1], board[2, 2], board[2, 3], board[2, 4], board[2, 5], board[2, 6], board[2, 7], board[2, 8], board[2, 9], board[2, 10], board[2, 11], board[2, 12], board[2, 13], board[2, 14]);

                // break
                Console.WriteLine("##\t|---------------------------------------|\t##");

                //row 3
                //Console.WriteLine($"##\t| {board[3, 0]} {board[3, 1]} {board[3, 2]} | {board[3, 3]} {board[3, 4]} {board[3, 5]} | {board[3, 6]} {board[3, 7]} {board[3, 8]} | \t##");
                RowDisplayer(board[3, 0], board[3, 1], board[3, 2], board[3, 3], board[3, 4], board[3, 5], board[3, 6], board[3, 7], board[3, 8], board[3, 9], board[3, 10], board[3, 11], board[3, 12], board[3, 13], board[3, 14]);

                //row 4
                //Console.WriteLine($"##\t| {board[4, 0]} {board[4, 1]} {board[4, 2]} | {board[4, 3]} {board[4, 4]} {board[4, 5]} | {board[4, 6]} {board[4, 7]} {board[4, 8]} | \t##");
                RowDisplayer(board[4, 0], board[4, 1], board[4, 2], board[4, 3], board[4, 4], board[4, 5], board[4, 6], board[4, 7], board[4, 8], board[4, 9], board[4, 10], board[4, 11], board[4, 12], board[4, 13], board[4, 14]);


                //row 5
                //Console.WriteLine($"##\t| {board[5, 0]} {board[5, 1]} {board[5, 2]} | {board[5, 3]} {board[5, 4]} {board[5, 5]} | {board[5, 6]} {board[5, 7]} {board[5, 8]} | \t##");
                RowDisplayer(board[5, 0], board[5, 1], board[5, 2], board[5, 3], board[5, 4], board[5, 5], board[5, 6], board[5, 7], board[5, 8], board[5, 9], board[5, 10], board[5, 11], board[5, 12], board[5, 13], board[5, 14]);


                // break
                Console.WriteLine("##\t|---------------------------------------|\t##");

                // row 6
                //Console.WriteLine($"##\t| {board[6, 0]} {board[6, 1]} {board[6, 2]} | {board[6, 3]} {board[6, 4]} {board[6, 5]} | {board[6, 6]} {board[6, 7]} {board[6, 8]} | \t##");
                RowDisplayer(board[6, 0], board[6, 1], board[6, 2], board[6, 3], board[6, 4], board[6, 5], board[6, 6], board[6, 7], board[6, 8], board[6, 9], board[6, 10], board[6, 11], board[6, 12], board[6, 13], board[6, 14]);


                //row 7
                //Console.WriteLine($"##\t| {board[7, 0]} {board[7, 1]} {board[7, 2]} | {board[7, 3]} {board[7, 4]} {board[7, 5]} | {board[7, 6]} {board[7, 7]} {board[7, 8]} | \t##");
                RowDisplayer(board[7, 0], board[7, 1], board[7, 2], board[7, 3], board[7, 4], board[7, 5], board[7, 6], board[7, 7], board[7, 8], board[7, 9], board[7, 10], board[7, 11], board[7, 12], board[7, 13], board[7, 14]);


                // row 8
                //Console.WriteLine($"##\t| {board[8, 0]} {board[8, 1]} {board[8, 2]} | {board[8, 3]} {board[8, 4]} {board[8, 5]} | {board[8, 6]} {board[8, 7]} {board[8, 8]} | \t##");
                RowDisplayer(board[8, 0], board[8, 1], board[8, 2], board[8, 3], board[8, 4], board[8, 5], board[8, 6], board[8, 7], board[8, 8], board[8, 9], board[8, 10], board[8, 11], board[8, 12], board[8, 13], board[8, 14]);

                // break
                Console.WriteLine("##\t|---------------------------------------|\t##");

                // row 9
                RowDisplayer(board[9, 0], board[9, 1], board[9, 2], board[9, 3], board[9, 4], board[9, 5], board[9, 6], board[9, 7], board[9, 8], board[9, 9], board[9, 10], board[9, 11], board[9, 12], board[9, 13], board[9, 14]);

                // row 10
                RowDisplayer(board[10, 0], board[10, 1], board[10, 2], board[10, 3], board[10, 4], board[10, 5], board[10, 6], board[10, 7], board[10, 8], board[10, 9], board[10, 10], board[10, 11], board[10, 12], board[10, 13], board[10, 14]);

                // row 11
                RowDisplayer(board[11, 0], board[11, 1], board[11, 2], board[11, 3], board[11, 4], board[11, 5], board[11, 6], board[11, 7], board[11, 8], board[11, 9], board[11, 10], board[11, 11], board[11, 12], board[11, 13], board[11, 14]);

                // break
                Console.WriteLine("##\t|---------------------------------------|\t##");

                // row 12
                RowDisplayer(board[12, 0], board[12, 1], board[12, 2], board[12, 3], board[12, 4], board[12, 5], board[12, 6], board[12, 7], board[12, 8], board[12, 9], board[12, 10], board[12, 11], board[12, 12], board[12, 13], board[12, 14]);

                // row 13
                RowDisplayer(board[13, 0], board[13, 1], board[13, 2], board[13, 3], board[13, 4], board[13, 5], board[13, 6], board[13, 7], board[13, 8], board[13, 9], board[13, 10], board[13, 11], board[13, 12], board[13, 13], board[13, 14]);

                // row 14
                RowDisplayer(board[14, 0], board[14, 1], board[14, 2], board[14, 3], board[14, 4], board[14, 5], board[14, 6], board[14, 7], board[14, 8], board[14, 9], board[14, 10], board[14, 11], board[14, 12], board[14, 13], board[14, 14]);

                // break
                Console.WriteLine("##\t|---------------------------------------|\t##");

            }

        }

        private static void RowDisplayer(int pos0, int pos1, int pos2,int pos3, int pos4, int pos5, int pos6, int pos7, int pos8, int pos9, int pos10 = 0, int pos11 = 0, int pos12 = 0, int pos13 = 0, int pos14 = 0)
        {

            string rowString = "";


            if (bSize == 9)
            {
                rowString = $"##\t|  {pos0}  {pos1}  {pos2}  |  {pos3}  {pos4}  {pos5}  |  {pos6}  {pos7}  {pos8}  \t| \t##";
            }
            else if (bSize == 12)
            {
                rowString = $"##\t| {pos0} {pos1} {pos2} | {pos3} {pos4} {pos5} | {pos6} {pos7} {pos8} | {pos9} {pos10} {pos11} | \t##";
            }
            else if (bSize == 15)
            {
                rowString = $"##\t| {pos0} {pos1} {pos2} | {pos3} {pos4} {pos5} | {pos6} {pos7} {pos8} | {pos9} {pos10} {pos11} | {pos12} {pos13} {pos14} |\t##";
            }
            else
            {
                rowString = "";
            }


            Console.WriteLine(rowString);

        }

        private static string UpdateCountdownTimerText()
        {
            return "##\t     " + currentTimerCounter + " seconds remaining          \t\t##";
        }
        #endregion

        #region location checks
        //--------------------------------------LOCATION-CHECKS--------------------------------------

        //this column contain the number
        static bool ColumnContainsNumber(int y, int value, ref int[,] board)
        {
            for (int x = 0; x < bSize; x++)
            {
                if (board[x, y] == value)
                {
                    return true;
                }
            }
            return false;
        }

        // this row contains the number
        static bool RowContainsNumber(int x, int value, ref int[,] board)
        {
            for (int y = 0; y < bSize; y++)
            {
                if (board[x, y] == value)
                {
                    return true;
                }
            }
            return false;
        }

        // this local 3 x 3 block contains the number
        private static bool BlockContainsNumber(int x, int y, int value, ref int[,] board)
        {

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (board[x - (x % 3) + i, y - (y % 3) + j] == value)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        // check all
        static bool CheckAll(int x, int y, int value, ref int[,] board)
        {
            if (ColumnContainsNumber(y, value, ref board)) { return false; }
            if (RowContainsNumber(x, value, ref board)) { return false; }
            if (BlockContainsNumber(x, y, value, ref board)) { return false; }

            return true;
        }

        // non 0s
        static bool IsValidGrid(ref int[,] board)
        {
            for (int i = 0; i < bSize; i++)
            {
                for (int j = 0; j < bSize; j++)
                {
                    if (board[i, j] == 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        #endregion

        #region generate board
        //--------------------------------------GENERATE-BOARD-----------------------------------
        private static bool FillBoardEdges(ref int[,] board)
        {
            if (rnd == null)
            {
                throw new Exception("Random Number class is is null");
            }

            List<int> rowValues = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            List<int> columnValues = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            if (bSize == (int)BOARDSIZE.TWEVLE)
            {
                rowValues.Add(10);
                rowValues.Add(11);
                rowValues.Add(12);
                columnValues.Add(10);
                columnValues.Add(11);
                columnValues.Add(12);

            }

            if (bSize == (int)BOARDSIZE.FIFTEEN)
            {
                rowValues.Add(10);
                rowValues.Add(11);
                rowValues.Add(12);
                rowValues.Add(13);
                rowValues.Add(14);
                rowValues.Add(15);
                columnValues.Add(10);
                columnValues.Add(11);
                columnValues.Add(12);
                columnValues.Add(13);
                columnValues.Add(14);
                columnValues.Add(15);
            }

            /*foreach (int i in rowValues)
            {
                Console.WriteLine(i);
            }

            foreach (int i in columnValues)
            {
                Console.WriteLine(i);
            }*/



            int value = rowValues[rnd.Next(0, rowValues.Count)];
            //Console.WriteLine("VALUE: " + value);

            board[0, 0] = value;

            // 0, 0 remove in both directions
            rowValues.Remove(value);
            columnValues.Remove(value);

            // ROW first
            for (int r = 1; r < bSize; r++)
            {
                value = rowValues[rnd.Next(0, rowValues.Count)];
                board[r, 0] = value;
                // remove from row
                rowValues.Remove(value);

            }

            // COLUMNS
            for (int c = 1; c < bSize; c++)
            {
                value = columnValues[rnd.Next(0, columnValues.Count)];
                if (c < 3)
                {
                    while (BlockContainsNumber(0, 0, value, ref board))
                    {
                        value = columnValues[rnd.Next(0, columnValues.Count)];

                    }
                }
                board[0, c] = value;
                columnValues.Remove(value);
            }

            // useful to see how the edges are working
            //DebugGrid(ref board);

            return true;
        }

        static bool SolvedInnerGrid(ref int[,] board)
        {
            //DebugGrid(ref board);

            if (IsValidGrid(ref board))
            {
                return true;
            }

            // FIND FIRST FREE CELL
            int x = 0;
            int y = 0;

            for (int i = 0; i < bSize; i++)
            {
                for (int j = 0; j < bSize; j++)
                {
                    if (board[i, j] == 0)
                    {
                        x = i;
                        y = j;
                        //Console.Write(x + " , " + y);
                        break;
                    }
                }
            }

            List<int> possibilities = new List<int>();
            possibilities = GetAllPossibilities(x, y, ref board);

            for (int ps = 0; ps < possibilities.Count; ps++)
            {
                // SET A POSSIBLE VALUE
                board[x, y] = possibilities[ps];
                // BACKTRACK
                if (SolvedInnerGrid(ref board))
                {

                    return true;
                }

                // handy to test mid solving
                //DebugGrid(ref board);

                // reset to 0 as false
                board[x, y] = 0;


            }



            return false;
        }

        static List<int> GetAllPossibilities(int x, int y, ref int[,] board)
        {
            List<int> possibilities = new List<int>();
            for (int val = 1; val <= bSize; val++)
            {
                if (CheckAll(x, y, val, ref board))
                {
                    possibilities.Add(val);
                }

            }

            return possibilities;
        }

        #endregion

        #region generate riddleboard
        //--------------------------------------GENERATE-RIDDLE-BOARD-----------------------------------
        public static void CreateRiddleBoard(ref int[,] solvedBoard, ref int[,] riddleBoard)
        {
            if (rnd == null)
            {
                throw new Exception("Random Number class is is null");
            }


            // COPY SOLVED GRID
            for (int i = 0; i < bSize ; i++)
            {
                for (int j = 0; j < bSize; j++)
                {
                    // copy solved to riddle
                    riddleBoard[i, j] = solvedBoard[i, j];

                }
            }

            // ERASE FROM RIDDLE GRID
            for (int i = 0; i < piecesToErase; i++)
            {
                int x1 = rnd.Next(0, bSize);
                int y1 = rnd.Next(0, bSize);

                // REROLL UNTIL WE FIND ONE WITHOUT A ZERO IN BETWWEN
                while (riddleBoard[x1, y1] == 0)
                {
                    x1 = rnd.Next(0, bSize);
                    y1 = rnd.Next(0, bSize);
                }

                // Once we found one with NO
                riddleBoard[x1, y1] = 0;

                // add to list for ui manipulation
                zeroList.Add(new Tuple<int, int>(x1, y1));

            }

            //DebugGrid(ref riddleBoard);
        }

        #endregion

        #region win conditions
        //--------------------------------------WIN CONDITIONS-----------------------------------
        // a method to handle player input
        public static void SetInputInRiddle(int x, int y, int value)
        {
            if(riddleBoard != null)
            {
                riddleBoard[x, y] = value;
            }


        }

        // check if won bool
        public static bool CheckIfWon()
        {
            for (int i = 0; i < bSize; i++)
            {
                for (int j = 0; j < bSize; j++)
                {
                    if(riddleBoard != null && solvedBoard != null)
                    {
                        if (riddleBoard[i, j] != solvedBoard[i, j])
                        {
                            return false;
                        }
                    }


                }
            }

            return true;

        }

        // check if won show display
        public static void CheckComplete()
        {
            if (CheckIfWon())
            {
                Console.WriteLine("Well done, YOU WON!");


            }
            else
            {
                Console.WriteLine("You Failed, Try Again!");

            }
        }

        #endregion


    }// program end

}// namespace end
