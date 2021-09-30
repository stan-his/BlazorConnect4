using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BlazorConnect4
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (!Directory.Exists("Data"))
            {
                Directory.CreateDirectory("./Data");
            }
            //CreateHostBuilder(args).Build().Run();



            Training();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });



        public static void Training()
            {
            


            AIModels.QAgent RedAi = (AIModels.QAgent)AIModels.AI.FromFile("Data/RedV2.bin");
            AIModels.QAgent YellowAi = (AIModels.QAgent)AIModels.AI.FromFile("Data/YellowV2.bin");

            //AIModels.QAgent RedAi = new AIModels.QAgent(Model.CellColor.Red);
            //AIModels.QAgent YellowAi = new AIModels.QAgent(Model.CellColor.Yellow);

            //AIModels.RandomAI randomAI = new AIModels.RandomAI();
            //RedAi.Workout( randomAI, 1000);


            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine(i);
                if (i % 2 == 0)
                {
                    YellowAi.Workout(RedAi, 1000);
                }
                else
                {
                    RedAi.Workout(YellowAi, 4000);
                }
            }

            RedAi.ToFile("Data/RedV2.bin");
            YellowAi.ToFile("Data/YellowV2.bin");



            Console.WriteLine();
        }
    }
}
