using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameCore;
using GameCore.Controllers;

namespace Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            Adversarial();
        }

        static void HugeMonteCarlo()
        {
            var hugeMonte = new MonteCarloController(1000, 100);
            var rando = new RandomController();

            float winrate = CompareWins(hugeMonte, rando, 100, 2000);
            Console.WriteLine($"{hugeMonte.GetControllerName()} vs {rando.GetControllerName()} = {(Math.Round(winrate * 100, 2))}");
        }

        static void Adversarial()
        {
            // create a matrix of comparisons of algorithms against each other
            List<IPlayerController> Controllers = new List<IPlayerController>()
            {
                new RandomController(),
                new DepthFirstController(),
                new MonteCarloController(10, 10),
            };

            foreach (IPlayerController controller1 in Controllers)
            foreach (IPlayerController controller2 in Controllers)
            {
                float winrate = CompareWins(controller1, controller2);
                Console.WriteLine($"{controller1.GetControllerName()} vs {controller2.GetControllerName()} = {(Math.Round(winrate * 100, 2))}");
            }


        }


        static float CompareWins(IPlayerController controller1, IPlayerController controller2, int maxgames = 250, int maxphysturns = 2000, bool showGameResults = false)
        {
            int countP1Wins = 0;
            int countDraws = 0;
            int maxGames = maxgames;
            int maxPhysTurns = maxphysturns;
            
            // since these are statics we cant parallel them without risking changing their controller midway
            var p1 = new Player()
            {
                FriendlyName = "Player 1",
                FriendlySymbol = "+",
                Controller = controller1,
            };

            var p2 = new Player()
            {
                FriendlyName = "Player 2",
                FriendlySymbol = "-",
                Controller = controller2,
            };

            Console.WriteLine("Starting simulations, please wait...");
            Stopwatch totalTime = Stopwatch.StartNew();

            StringBuilder csv = new StringBuilder();
            string sessionName = $"ModeFULL-" +
                                 $"Agents{p1.Controller.GetControllerName()}Vs{p2.Controller.GetControllerName()}-" +
                                 $"Turns{maxPhysTurns}-Games{maxGames}";

            // csv headers
            string SessionTimestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string logfilename = $"Session_{sessionName}_{SessionTimestamp}.csv";
            csv.AppendLine("Session,Game,Turns,Elapsed,Winners");
            System.IO.File.AppendAllText(logfilename, csv.ToString());

            object statslock = new object();


            //Parallel.For(0, maxGames, (int i) =>
            for(int i = 0; i < maxGames; i++)
            {
                csv.Clear();

                // TODO any way to save the rules but rerun a new game?
                var game = GameModes.FullNewStratego(p1, p2);
                game.rules.LoggingSettings = new GameRules.LogSettings()
                {
                    logTime = true,
                    showEachPlayersPlanning = false,
                    showStatePerTurn = false,
                    pausePerMove = false,
                    listMoveSeqenceAtEnd = false,
                    showMovePerTurn = false,
                    winLossReasons = false,
                };
                game.rules.MaxPhysicalTurns = maxPhysTurns;


                // add custom event listeners to better integrate logging into the game events?
                var results = game.Run();
                
                
                if (results.Winners.Count == 0)
                    countDraws++;
                else if (results.Winners.Contains(p1))
                    countP1Wins++;

                lock(statslock)
                    csv.AppendLine($"{SessionTimestamp},{i},{results.ToCSV()}");


                System.IO.File.AppendAllText(logfilename, csv.ToString());

                if(showGameResults)
                    Console.WriteLine($"#{i} Turns: {results.turnsElapsed}, Time: {results.timeElapsed.ToReadable()}");
                
            };

            
            Console.WriteLine($"---  Sim complete, {maxGames} evaluated. Total Time: {totalTime.Elapsed.ToReadable()}");
            Console.WriteLine($"Wins: {countP1Wins} ({(Math.Round((countP1Wins / (float)maxGames) * 100, 1))}%), " +
                              $"Draws: {countDraws} ({(Math.Round((countDraws / (float)maxGames) * 100, 1))}%)");

            if (showGameResults)
                Console.WriteLine("Wrote log to " + logfilename);

            return countP1Wins / (float) maxGames;
        }







        static void TurnLoops()
        {
            while (true)
            {
                var game = GameModes.FullNewStratego(null, null);
                game.rules.LoggingSettings = new GameRules.LogSettings()
                {
                    logTime = false,
                    showEachPlayersPlanning = false,
                    showStatePerTurn = false,
                    showMovePerTurn = false,
                    pausePerMove = false,
                    listMoveSeqenceAtEnd = true,
                };
                game.rules.MaxPhysicalTurns = 200;

                Console.WriteLine("New game: ");
                Console.WriteLine(game.CurrentBoard.ToAsciiLayout());

                Console.WriteLine(" press enter to begin turns");
                //Console.ReadLine();

                var results = game.Run();

                Console.WriteLine($"Turns: {results.turnsElapsed}, Time: {results.timeElapsed.ToReadable()},  Winners: ");
                foreach (Player p in results.Winners)
                {
                    Console.WriteLine("- " + p.FriendlyName);
                }

                Console.WriteLine("---- end of game ---- press <enter> to continue");
                //Console.ReadLine();
            }
        }


        static void Debugs()
        {
            // Define the simulation, board, pieces, players, and rules
            Stopwatch fullProgramTime = Stopwatch.StartNew();

            Stopwatch watch = new Stopwatch();
            watch.Start();
            Game game = GameModes.FullNewStratego(null, null);

            Console.WriteLine("Board Setup: " + watch.Elapsed.TotalMilliseconds);

            watch.Restart();
            Console.WriteLine(Environment.NewLine + game.CurrentBoard.ToAsciiLayout(null, true));
            Console.WriteLine(game.CurrentBoard.ToAsciiLayout());
            Console.WriteLine("Board Output: " + watch.Elapsed.TotalMilliseconds);

            Stopwatch totalTurnTime = Stopwatch.StartNew();

            // easy configuration of loggings and tracking, independent of gamemodes or controllers (todo move to rules?)
            game.rules.LoggingSettings = new GameRules.LogSettings()
            {
                logTime = true,
                showEachPlayersPlanning = false,
                showStatePerTurn = true,
                pausePerMove = true,
                winLossReasons = true,
                debugJumpchecks = false,
                showMovePerTurn = true,
                listMoveSeqenceAtEnd = true,
                showBombDefusals = true,
                showHiddenPieces = false,
            };
            game.rules.MaxPhysicalTurns = 600;

            var results = game.Run();

            Console.WriteLine($"Turns: {results.turnsElapsed}, Time: {results.timeElapsed.ToReadable()},  Winners: ");
            foreach (Player p in results.Winners)
            {
                Console.WriteLine("- " + p.FriendlyName);
            }


            watch.Stop();
            fullProgramTime.Stop();

            Console.WriteLine("------------------------- DONE in " + fullProgramTime.Elapsed.ToReadable() + "----------------------------");



            Console.WriteLine("Press <space> to quit");

            while (Console.ReadKey().Key != ConsoleKey.Spacebar) ;
        }






    }
}
