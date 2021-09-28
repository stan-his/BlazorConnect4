﻿using System;
using BlazorConnect4.Model;
using System.Collections.Generic;
using System.IO;

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

        public Dictionary<String,float[]> Qdict;
        private float reward = 0F;

        public float InvalidMoveReward = -1.0F;
        public float WinningMoveReward = 1F;
        public float LosingMoveReward = -1F;


        public QAgent()
        {
           
            Qdict = new Dictionary<String,float[]>();
        }

       
        


        public float GetQValue(Cell[,] grid , int action)
        {

            String key = GameBoard.GetHashStringCode(grid);
            if (Qdict.ContainsKey(key))
            {
                return Qdict[key][action];
            }
            else
            {
                float[] actions = { 0, 0, 0, 0, 0, 0, 0 }; // fill array with zeros
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

        


        public double GetReward(Cell[,] grid, int action)
        {
            return reward;
        }
        public void SetReward(float terminalReward=0)
        {
            reward = terminalReward;
        }
        

        public override int SelectMove(Cell[,] grid)
        {
            double epsilon = 0.99;
            return EpsilonGreedyAction(epsilon, grid);
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
            return action;
        }

        public int EpsilonGreedyAction(double epsilon, Cell[,] state)
        {
            Random random = new Random();
            if (random.NextDouble() < epsilon)
            {
                // Make a random move
                return random.Next(0, 7);
            }
            else
            {
                // Make highest valued move
                return GetBestAction(state);
            }
        }

        /*
        public int[] GetValidActions(Cell[,] currentState)
        {
            List<int> validActions = new List<int>();
            for (int i = 0; i < 7; i++)
            {
                if (gameEngine.IsValid(i))
                    validActions.Add(i);
            }
            return validActions.ToArray();
        }*/

        /*
        public void InitializeEpisode (Cell[,] initialState)
        {

            Cell[,] currentState = initialState;
            while (true)
            {
                bool isFinished = false;
                currentState = TakeAction(currentState);
                if (gameEngine.)
                    break;
            }   
        }
        */
        /*
        public static void TrainAgents(int numberOfIterations)
        {
            for (int i = 0; i < numberOfIterations; i++)
            {
                String ai = "ai";
                GameEngine initialState = new GameEngine();
                QAgent s = new QAgent(initialState);
                
            }
        }
        */

        /*
        private int TakeAction (Cell[,] currentState)
        {
            int[] validActions = GetValidActions(currentState);
            int randomIndexAction = random.Next(0, validActions.Length);
            int action = validActions[randomIndexAction];
            int gamma = 0;

            double saReward = GetReward(currentState, action);
            double nsReward = qTable[action].Max();
            double qCurrentState = saReward + (gamma * nsReward);
            qTable[currentState][action] = qCurrentState;
            int newState = action;
            return newState;
        }
        */

        public static QAgent ConstructFromFile(string fileName)
        {
            QAgent temp = (QAgent)(AI.FromFile(fileName));
            // Eftersom generatorn inte var serialiserad.
            //temp.generator = new Random();
            return temp;
        }


       public void Workout(GameEngineTwo gameEngine, AI oppositeAi, int Iterations)
            {
            
            
            for(int i = 0; i < Iterations; i++)
                {
                Console.WriteLine(i);
                //new Iteration => choose a new action and check if the action is valid
                gameEngine.Board = new GameBoard();
                bool terminal = false;
                // Reward defaults to zero
               
                int action = this.EpsilonGreedyAction(1, gameEngine.Board.Grid);
                GameBoard boardCopy = gameEngine.Board.Copy();
                bool isValidAction = GameEngineTwo.MakeMove(ref boardCopy,gameEngine.PlayerTurn,action);

                while (!terminal) 
                    {
                    
                   
                    
                    
                    if (!isValidAction)//if the game was a invalid action give all moves from here a negative reward.
                        {
                        
                        SetQValue(gameEngine.Board.Grid, action, InvalidMoveReward);
                        
                        terminal = true; //the move was terminal becuase it was a wrong move;
                        }
                    else if (gameEngine.IsWin(gameEngine.PlayerTurn))//TODO  if the game is terminal quit here and give the reward for all actions in this state.
                        {
                        
                        SetQValue(gameEngine.Board.Grid, action, WinningMoveReward);
                       
                        terminal = true;
                        }
                    else if (gameEngine.IsWin(GameEngineTwo.OtherPlayer(gameEngine.PlayerTurn)))
                    {
                        
                        SetQValue(gameEngine.Board.Grid, action, LosingMoveReward);
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
                        int oppositeAction =  oppositeAi.SelectMove(nextBoardState.Grid);
                        GameEngineTwo.MakeMove(ref nextBoardState, GameEngineTwo.OtherPlayer(gameEngine.PlayerTurn),oppositeAction);
                        float maxQvalueNextState = GetQValue(nextBoardState.Grid,bestAction);
                        //update value
                        SetQValue(gameEngine.Board.Grid, action, currentVal + alpha * (gamma + maxQvalueNextState - currentVal));
                        
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
    }
        
}
