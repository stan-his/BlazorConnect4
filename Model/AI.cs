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

        public double GetReward(Cell[,] grid, int action)
        {
            return 0;
        }
        
        public int QLearning (Cell[,] grid)
        {
            return 0;
        }
        public override int SelectMove(Cell[,] grid)
        {
            double epsilon = 0.99;
            return EpsilonGreedyAction(epsilon, grid);
        }

        public int GetBestAction(Cell[,] state)
        {
            String key = GameBoard.GetHashStringCode(state);

            int action = 0;
            float value = Qdict[key][0];
            for (int i = 1; i < 7; i++)
            {
                if (Qdict[key][i] > value)
                {
                    action = i;
                    value = Qdict[key][i];
                }
            }
            return action;
        }

        private int EpsilonGreedyAction(double epsilon, Cell[,] state)
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
        
    }
        
}
