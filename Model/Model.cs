using System;
using System.IO;
using BlazorConnect4.AIModels;

namespace BlazorConnect4.Model
{
    public enum CellColor
    {
        Red,
        Yellow,
        Blank
    }


    public class Cell
    {
        public CellColor Color { get; set; }

        public Cell(CellColor color)
        {
            Color = color;
        }

    }

    public class GameBoard
    {
        public Cell[,] Grid { get; set; }

        public GameBoard()
        {
            Grid = new Cell[7, 6];

            //Populate the Board with blank pieces
            for (int i = 0; i <= 6; i++)
            {
                for (int j = 0; j <= 5; j++)
                {
                    Grid[i, j] = new Cell(CellColor.Blank);
                }
            }
        }

        public static String GetHashStringCode(Cell[,] grid)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i <= 6; i++)
            {
                for (int j = 0; j <= 5; j++)
                {
                    sb.Append(grid[i, j]);
                }
            }
            return sb.ToString();
        }

    }

    public class GameEngineTwo
    {
        public GameBoard Board { get; set; }
        public CellColor PlayerTurn { get; set; }

        public AI playerOne;
        public AI playerTwo;

        public GameEngineTwo()
        {
            Board = new GameBoard();
        }

        public bool IsValid(int col)
        {
            return Board.Grid[col, 0].Color == CellColor.Blank;
        }

        public bool IsDraw()
        {
            for (int i = 0; i < 7; i++)
            {
                if (Board.Grid[i, 0].Color == CellColor.Blank)
                {
                    return false;
                }
            }
            return true;
        }

        public CellColor OtherPlayer(CellColor player)
        {
            return player == CellColor.Red ? CellColor.Yellow : CellColor.Red;
        }

        public bool IsWin(CellColor player)
        {
            int height = 6;
            int width = 7;
            bool isWin = false;
            // horizontalCheck 
            for (int j = 0; j < height - 3; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    if (Board.Grid[i, j].Color == player && Board.Grid[i, j + 1].Color == player && Board.Grid[i, j + 2].Color == player && Board.Grid[i, j + 3].Color == player)
                    {
                        isWin = true;
                    }
                }
            }
            if (isWin == false)
            {
                // verticalCheck
                for (int i = 0; i < width - 3; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        if (Board.Grid[i, j].Color == player && Board.Grid[i + 1, j].Color == player && Board.Grid[i + 2, j].Color == player && Board.Grid[i + 3, j].Color == player)
                        {
                            isWin = true;
                        }
                    }
                }
            }
            if (isWin == false)
            {
                // ascendingDiagonalCheck 
                for (int i = 3; i < width; i++)
                {
                    for (int j = 0; j < height - 3; j++)
                    {
                        if (Board.Grid[i, j].Color == player && Board.Grid[i - 1, j + 1].Color == player && Board.Grid[i - 2, j + 2].Color == player && Board.Grid[i - 3, j + 3].Color == player)
                            isWin = true;
                    }
                }
            }

            if (isWin == false)
            {
                // descendingDiagonalCheck
                for (int i = 3; i < width; i++)
                {
                    for (int j = 3; j < height; j++)
                    {
                        if (Board.Grid[i, j].Color == player && Board.Grid[i - 1, j - 1].Color == player && Board.Grid[i - 2, j - 2].Color == player && Board.Grid[i - 3, j - 3].Color == player)
                            isWin = true;
                    }
                }
            }
            return isWin;
        }

    }

    public class GameEngine
    {
        public GameBoard Board { get; set; }
        public CellColor Player { get; set; }
        public bool active;
        public String message;
        private AI ai;


        public GameEngine()
        {
            Reset("Human");
        }



        // Reset the game and creats the opponent.
        // TODO change the code so new RL agents are created.
        public void Reset(String playAgainst)
        {
            Board = new GameBoard();
            Player = CellColor.Red;
            active = true;
            message = "Starting new game";

            if (playAgainst == "Human")
            {
                ai = null;
            }
            else if (playAgainst == "Random")
            {
                ai = new RandomAI();
            }
            else if (playAgainst == "Q1")
            {
                // Change filename to our filename
                if (File.Exists("Data/Random.bin"))
                {
                    // Remove below line and uncomment the one below that
                    ai = new RandomAI();
                    //ai = QAgent.ConstructFromFile("Data/Random.bin");
                }
                else
                {
                    // Change below line to our ai when complete
                    ai = new RandomAI();
                    ai.ToFile("Data/Random.bin");
                }
            }
            else if (playAgainst == "Q2")
            {
                ai = new RandomAI();
            }
            else if (playAgainst == "Q3")
            {
                ai = new RandomAI();
            }

        }

        public bool IsValid(int col)
        {
            return Board.Grid[col, 0].Color == CellColor.Blank;
        }


        public bool IsDraw()
        {
            for (int i = 0; i < 7; i++)
            {
                if (Board.Grid[i, 0].Color == CellColor.Blank)
                {
                    return false;
                }
            }
            return true;
        }

        public CellColor OtherPlayer(CellColor player)
        {
            return player == CellColor.Red ? CellColor.Yellow : CellColor.Red;
        }

        public bool IsWin(CellColor player)
        {
            int height = 6;
            int width = 7;
            bool isWin = false;
            // horizontalCheck 
            for (int j = 0; j < height - 3; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    if (Board.Grid[i, j].Color == player && Board.Grid[i, j + 1].Color == player && Board.Grid[i, j + 2].Color == player && Board.Grid[i, j + 3].Color == player)
                    {
                        isWin = true;
                    }
                }
            }
            if (isWin == false)
            {
                // verticalCheck
                for (int i = 0; i < width - 3; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        if (Board.Grid[i, j].Color == player && Board.Grid[i + 1, j].Color == player && Board.Grid[i + 2, j].Color == player && Board.Grid[i + 3, j].Color == player)
                        {
                            isWin = true;
                        }
                    }
                }
            }
            if (isWin == false)
            {
                // ascendingDiagonalCheck 
                for (int i = 3; i < width; i++)
                {
                    for (int j = 0; j < height - 3; j++)
                    {
                        if (Board.Grid[i, j].Color == player && Board.Grid[i - 1, j + 1].Color == player && Board.Grid[i - 2, j + 2].Color == player && Board.Grid[i - 3, j + 3].Color == player)
                            isWin = true;
                    }
                }
            }

            if (isWin == false)
            {
                // descendingDiagonalCheck
                for (int i = 3; i < width; i++)
                {
                    for (int j = 3; j < height; j++)
                    {
                        if (Board.Grid[i, j].Color == player && Board.Grid[i - 1, j - 1].Color == player && Board.Grid[i - 2, j - 2].Color == player && Board.Grid[i - 3, j - 3].Color == player)
                            isWin = true;
                    }
                }
            }
            return isWin;
        }

        public bool Play(int col)
        {
            if (IsValid(col) && active)
            {

                for (int i = 5; i >= 0; i--)
                {
                    if (Board.Grid[col, i].Color == CellColor.Blank)
                    {
                        Board.Grid[col, i].Color = Player;

                        if (IsWin(Player))
                        //if (IsWin(col, i, Player))
                        {
                            message = Player.ToString() + " Wins";
                            active = false;
                            return true;
                        }

                        if (IsDraw())
                        {
                            message = "Draw";
                            active = false;
                            return true;
                        }
                        break;
                    }
                }
                return PlayNext();
            }

            return false;
        }




        private bool PlayNext()
        {

            if (Player == CellColor.Red)
            {
                Player = CellColor.Yellow;
            }
            else
            {
                Player = CellColor.Red;
            }

            if (ai != null && Player == CellColor.Yellow)
            {
                int move = ai.SelectMove(Board.Grid);

                while (!IsValid(move))
                {
                    move = ai.SelectMove(Board.Grid);
                }

                return Play(move);
            }

            return false;
        }
    }


}
