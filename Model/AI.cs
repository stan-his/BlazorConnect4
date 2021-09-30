using System;
using BlazorConnect4.Model;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace BlazorConnect4.AIModels
{
    [Serializable]
    public abstract class AI
    {
        // Funktion för att beskriva 
        public abstract int SelectMove(Cell[,] grid);

        // Funktion för att skriva till fil.
        public virtual void ToFile(string fileName)
        {
            using (Stream stream = File.Open(fileName, FileMode.Create))
            {
                var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                bformatter.Serialize(stream, this);
            }
        }

        // Funktion för att att läsa från fil.
        public static AI FromFile(string fileName)
        {
            AI returnAI;
            using (Stream stream = File.Open(fileName, FileMode.Open))
            {
                var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                returnAI = (AI)bformatter.Deserialize(stream);
            }
            return returnAI;

        }
    }


    [Serializable]
    public class RandomAI : AI
    {
        [NonSerialized] Random generator;

        public RandomAI()
        {
            generator = new Random();
        }

        public override int SelectMove(Cell[,] grid)
        {
            return generator.Next(7);
        }
    }

    [Serializable]
    public class QAgent : AI
    {

        public Dictionary<String, float[]> Qdict;
       
        private static CellColor PlayerColor;
        // Reward values
        public float InvalidMoveReward = -0.5F;
        public float WinningMoveReward = 1F;
        public float LosingMoveReward = -1F;
        public float DrawMoveReward = 0F;

        // Statistics
        public int wins = 0;
        public int losses = 0;
        public int ties = 0;
        public int invalidMoves = 0;

        


        public QAgent(CellColor playerColor)
        {
            PlayerColor = playerColor;
            Qdict = new Dictionary<String, float[]>();
            
        }





        public float GetQValue(Cell[,] grid, int action)
        {

            String key = GameBoard.GetHashStringCode(grid);
            if (Qdict.ContainsKey(key))
            {
                return Qdict[key][action];
            }
            else
            {
                float[] actions = { 0, 0, 0, 0, 0, 0, 0 }; // fill array with zeros //TODO , could be better with random values instead
                Qdict.Add(key, actions);
                return 0;
            }
        }

        public void SetQValue(Cell[,] grid, int action, float value)
        {
            String key = GameBoard.GetHashStringCode(grid);
            if (!Qdict.ContainsKey(key))
            {
                float[] actions = { 0, 0, 0, 0, 0, 0, 0 }; // fill array with zeros
                Qdict.Add(key, actions);

            }
            Qdict[key][action] = value;

        }


        public override int SelectMove(Cell[,] grid)
        {
            double epsilon = -0;
            int action = EpsilonGreedyAction(epsilon, grid);
            bool validMove = GameEngineTwo.IsValid(grid, action);
            // in the case that the best move is not a validmove, isntead randomize a move until a valid is found
            Random randomGen = new Random();
            while (!validMove)
            {

                action = randomGen.Next(7);
                validMove = GameEngineTwo.IsValid(grid, action);
            }
            Debug.Assert(GameEngineTwo.IsValid(grid, action));
            return action;
        }

        public int GetBestAction(Cell[,] state)
        {


            int action = 0;

            float value = GetQValue(state, 0);
            for (int i = 1; i < 7; i++)
            {
                if (GetQValue(state, i) > value)
                {
                    action = i;
                    value = GetQValue(state, i);
                }
            }
            bool validMove = GameEngineTwo.IsValid(state, action);
            Random randomGen = new Random();
            while (!validMove)
            {

                action = randomGen.Next(0,7);
                validMove = GameEngineTwo.IsValid(state, action);
            }

            return action;
        }

        public int EpsilonGreedyAction(double epsilon, Cell[,] state)
        {
            Random random = new Random();
            int action = -1;
            if (random.NextDouble() < epsilon)
            {
                action = random.Next(0, 7);
                while (!GameEngineTwo.IsValid(state, action))
                {
                    action = random.Next(0, 7);
                }
                
            }
            else
            {
                // Make highest valued move
                action = GetBestAction(state);
                while (!GameEngineTwo.IsValid(state, action))
                {
                    action = random.Next(0, 7);
                }
            }
            Debug.Assert(GameEngineTwo.IsValid(state, action));
            return action;
        }

        public static QAgent ConstructFromFile(string fileName)
        {
            QAgent temp = (QAgent)(AI.FromFile(fileName));
            // Eftersom generatorn inte var serialiserad.
            //temp.generator = new Random();
            return temp;
        }

        public bool IsTerminalState(GameEngineTwo gameEngine, int action, CellColor playerColor)
        {
            bool isTerminalState = false;
            if (GameEngineTwo.IsWin(gameEngine.Board,action,playerColor)) // If the game is terminal quit here and give the reward for all actions in this state.
            {
                wins++;
                SetQValue(gameEngine.Board.Grid, action, WinningMoveReward);
                isTerminalState = true;
            }
            else if (GameEngineTwo.IsWin(gameEngine.Board, action, GameEngineTwo.OtherPlayer(playerColor)))
            {
                losses++;
                SetQValue(gameEngine.Board.Grid, action, LosingMoveReward);
                isTerminalState = true;
            }
            else if (GameEngineTwo.IsDraw(gameEngine.Board,action))
            {
                ties++;
                SetQValue(gameEngine.Board.Grid, action, DrawMoveReward);
                isTerminalState = true;
            }
            return isTerminalState;
        }

        public void Workout(AI oppositeAi, int iterations)
        {
            double epsilon = 0.7F;
            float gamma = 0.9F;
            float alpha = 0.5F;

            GameEngineTwo gameEngine = new GameEngineTwo();
            int opponentAction;
            CellColor opponenentColor = GameEngineTwo.OtherPlayer(PlayerColor);
            for (int i = 0; i < iterations; i++)
            {
                // new iteration, reset the gameBoard and playerturn
                gameEngine.Reset();
                bool terminal = false;

                if (PlayerColor == CellColor.Yellow)
                {
                    //opponentAction = oppositeAi.SelectMove(gameEngine.Board.Grid);
                    opponentAction = EpsilonGreedyAction(1,gameEngine.Board.Grid); 
                    gameEngine.MakeMove(opponentAction);
                }

                int action = EpsilonGreedyAction(epsilon, gameEngine.Board.Grid);
                Debug.Assert(GameEngineTwo.IsValid(gameEngine.Board.Grid, action));

                while (!terminal)
                {

                    float currentVal = GetQValue(gameEngine.Board.Grid, action);  //Q(s,a)

                    //The Q value for best next move
                    //Q(a',s')
                    GameBoard nextBoardState = gameEngine.Board.Copy();
                    int bestAction1 = EpsilonGreedyAction(-1,gameEngine.Board.Grid); // best action by epsilon -1
                    Debug.Assert(GameEngineTwo.IsValid(gameEngine.Board.Grid, bestAction1));
                    GameEngineTwo.MakeMove(ref nextBoardState, gameEngine.PlayerTurn, bestAction1);


                    int oppositeAction = oppositeAi.SelectMove(nextBoardState.Grid);
                    Debug.Assert(GameEngineTwo.IsValid(gameEngine.Board.Grid, oppositeAction));
                    GameEngineTwo.MakeMove(ref nextBoardState, GameEngineTwo.OtherPlayer(gameEngine.PlayerTurn), oppositeAction);

                    int bestAction2 = EpsilonGreedyAction(-1, gameEngine.Board.Grid);
                    Debug.Assert(GameEngineTwo.IsValid(gameEngine.Board.Grid, bestAction2));
                    float maxQvalueNextState = GetQValue(nextBoardState.Grid, bestAction2);
                    //update value

                    //                                        Q(a,s)    + alpha * (gamma * Max(Q(a',s))       - Q(s,a)                                  
                    SetQValue(gameEngine.Board.Grid, action, currentVal + alpha * (gamma * maxQvalueNextState - currentVal));

                    //we should make a new move and then let the opponent make a move

                    action = EpsilonGreedyAction(epsilon, gameEngine.Board.Grid);
                    
                    Debug.Assert(GameEngineTwo.IsValid(gameEngine.Board.Grid, action));
                    terminal = IsTerminalState(gameEngine, action, PlayerColor);
                    gameEngine.MakeMove(action);
                    // If players move is not terminal, make opponentAi move
                    if (!terminal)
                    {
                        opponentAction = oppositeAi.SelectMove(gameEngine.Board.Grid);
                        Debug.Assert(GameEngineTwo.IsValid(gameEngine.Board.Grid, opponentAction));
                        terminal = IsTerminalState(gameEngine, opponentAction, opponenentColor);
                        gameEngine.MakeMove(opponentAction);
                        
                    }
                }
            }
        }
        /*
        public void WorkoutRed(AI oppositeAi, int iterations)
        {

            GameEngineTwo gameEngine = new GameEngineTwo();
            for (int i = 0; i < iterations; i++)
            {
                gameEngine.Reset();
                Console.WriteLine(i);
                //new Iteration => choose a new action and check if the action is valid
                bool terminal = false;
                // Reward defaults to 

                int action = this.EpsilonGreedyAction(1, gameEngine.Board.Grid);
                GameBoard boardCopy = gameEngine.Board.Copy();
                bool isValidAction = GameEngineTwo.MakeMove(ref boardCopy, gameEngine.PlayerTurn, action);

                while (!terminal)
                {
                    // TODO look over below statements, cleanup?
                    if (!isValidAction) // If the game was a invalid action give all moves from here a negative reward.
                    {
                        invalidMoves++;
                        SetQValue(gameEngine.Board.Grid, action, InvalidMoveReward);
                        terminal = true; // The move was terminal becuase it was a wrong move;
                    }
                    else if (gameEngine.IsWin(gameEngine.PlayerTurn)) // If the game is terminal quit here and give the reward for all actions in this state.
                    {
                        wins++;
                        SetQValue(gameEngine.Board.Grid, action, WinningMoveReward);
                        terminal = true;
                    }
                    else if (gameEngine.IsWin(GameEngineTwo.OtherPlayer(gameEngine.PlayerTurn)))
                    {
                        losses++;
                        SetQValue(gameEngine.Board.Grid, action, LosingMoveReward);
                        terminal = true;
                    }
                    else if (gameEngine.IsDraw())
                    {
                        ties++;
                        SetQValue(gameEngine.Board.Grid, action, DrawMoveReward);
                        terminal = true;
                    }
                    else
                    {
                        float gamma = 0.9F;
                        float alpha = 0.5F;
                        float currentVal = GetQValue(gameEngine.Board.Grid, action);

                        //The Q value for best next move
                        GameBoard nextBoardState = gameEngine.Board.Copy();
                        int bestAction = GetBestAction(nextBoardState.Grid);
                        GameEngineTwo.MakeMove(ref nextBoardState, gameEngine.PlayerTurn, bestAction);
                        int oppositeAction = oppositeAi.SelectMove(nextBoardState.Grid);
                        GameEngineTwo.MakeMove(ref nextBoardState, GameEngineTwo.OtherPlayer(gameEngine.PlayerTurn), oppositeAction);
                        float maxQvalueNextState = GetQValue(nextBoardState.Grid, bestAction);
                        //update value
                        SetQValue(gameEngine.Board.Grid, action, currentVal + alpha * (gamma * maxQvalueNextState - currentVal));

                        //we should make a new move and then let the opponent make a move

                        action = this.EpsilonGreedyAction(0.15, gameEngine.Board.Grid);
                        gameEngine.MakeMove(action);
                        int opponentACtion = oppositeAi.SelectMove(gameEngine.Board.Grid);
                        gameEngine.MakeMove(opponentACtion);
                        isValidAction = gameEngine.MakeMove(action);
                    }
                }
            }
        }
        */
    }

}
