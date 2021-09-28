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
            Model.GameEngineTwo gameEngine = new Model.GameEngineTwo();
            AIModels.QAgent leAgent = new AIModels.QAgent();
            AIModels.RandomAI randomAI = new AIModels.RandomAI();
            leAgent.Workout(gameEngine, randomAI, 10000000);


            leAgent.ToFile("Data/AwesomeAgent.bin");


            AIModels.QAgent newAgent = (AIModels.QAgent)AIModels.AI.FromFile("Data/AwesomeAgent.bin");
            Console.WriteLine();
        }
    }
}
