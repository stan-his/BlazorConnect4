using System;
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
        protected static AI FromFile(string fileName)
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
        private int reward = 0;
        private GameEngine gameEngine;

        public QAgent(GameEngine gameEngine)
        {
            this.gameEngine = gameEngine;
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

        
        public void UpdateQValue(Cell[,] grid, int action)
        {
            String key = GameBoard.GetHashStringCode(grid);
            float alpha = 0;
            float gamma = 0;
            float currentValue = Qdict[key][action];
            float reward = 0;
            float maxOfNextMove = 0;
            SetQValue(grid, action, currentValue + alpha * (reward + gamma * (maxOfNextMove) - currentValue));
            

        }

        // Unnecessary?
        public double GetReward(Cell[,] grid, int action)
        {
            return 0;
        }
        /*public double GetReward(int action)
        {
            //GameBoard.GetHashStringCode(gameEngine.Board.Grid);
            return 0;
        }*/
        public int QLearning (Cell[,] grid)
        {
            String currentBoard = GameBoard.GetHashStringCode(grid);

            double qValue = 0F;
            int move = -1;
            //GameEngine gameEngine = new GameEngine();
            
            for (int col = 0; col < 7; col++)
            {
                // Search for the columns with the highest reward that is a legal move
                qValue = GetReward(grid, col) > qValue && gameEngine.IsValid(col) ? GetReward(grid, col) : qValue;
            }
            return 0;
        } 
        public override int SelectMove(Cell[,] grid)
        {
            return QLearning(grid);
        }

        public int[] GetValidActions(Cell[,] currentState)
        {
            List<int> validActions = new List<int>();
            for (int i = 0; i < 7; i++)
            {
                if (gameEngine.IsValid(i))
                    validActions.Add(i);
            }
            return validActions.ToArray();
        }


        public static void TrainAgents(int numberOfIterations)
        {
            for (int i = 0; i < numberOfIterations; i++)
            {

            }
        }

        private void InitializeEpisode (int initialState)
        {
            int currentState = initialState;
            while (true)
            {
                bool isFinished = false;
                currentState = TakeAction(currentState);
                if (isFinished)
                    break;
            }   
        }

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
        public static QAgent ConstructFromFile(string fileName)
        {
            QAgent temp = (QAgent)(AI.FromFile(fileName));
            // Eftersom generatorn inte var serialiserad.
            //temp.generator = new Random();
            return temp;
        }
    }
}
