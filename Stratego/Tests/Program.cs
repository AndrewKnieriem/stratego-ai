using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameCore;

namespace Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            CompareWins();
        }
        
        static void CompareWins()
        {
            Console.WriteLine("Starting simulations, please wait...");
            Stopwatch totalTime = Stopwatch.StartNew();

            StringBuilder csv = new StringBuilder();
            string sessionName = "Mode-Classic_Agents-2Random_Turns-2000_Games-10000";

            // csv headers
            csv.AppendLine("Session,Game,Turns,Elapsed,Winners");

            string SessionTimestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            int countP1Wins = 0;
            int countDraws = 0;
            int maxGames = 10000;

            object statslock = new object();


            Parallel.For(0, maxGames, (int i) =>
            {
                // TODO any way to save the rules but rerun a new game?
                var game = GameModes.classicStratego();
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
                game.rules.MaxPhysicalTurns = 2000;


                // add custom event listeners to better integrate logging into the game events?
                var results = game.Run();
                
                
                if (results.Winners.Count == 0)
                    countDraws++;
                else if (results.Winners.Contains(GameModes.playerOne))
                    countP1Wins++;

                lock(statslock)
                    csv.AppendLine($"{SessionTimestamp},{i},{results.ToCSV()}");

                //Console.WriteLine($"Wins: {countP1Wins}/{i} ({(Math.Round((countP1Wins / (float)i) * 100, 1))}%)" +
                //                  $" Turns: {results.turnsElapsed}, Time: {results.timeElapsed.ToReadable()}");
            });


            

            Console.WriteLine($"---  Sim complete, {maxGames} evaluated. Total Time: {totalTime.Elapsed.ToReadable()}");
            Console.WriteLine($"Wins: {countP1Wins} ({(Math.Round((countP1Wins / (float)maxGames) * 100, 1))}%), Draws: {countDraws}");

            string logfilename = $"Session_{sessionName}_{SessionTimestamp}.csv";
            System.IO.File.WriteAllText(logfilename,csv.ToString());
            Console.WriteLine("Wrote log to " + logfilename);
            
            Console.WriteLine("Press <enter> to quit...");
            Console.ReadLine();
        }







        static void TurnLoops()
        {
            while (true)
            {
                var game = GameModes.classicStratego();
                game.rules.LoggingSettings = new GameRules.LogSettings()
                {
                    logTime = false,
                    showEachPlayersPlanning = false,
                    showStatePerTurn = true,
                    pausePerMove = false,
                };
                game.rules.MaxPhysicalTurns = 1000;

                Console.WriteLine("New game: ");
                Console.WriteLine(game.CurrentBoard.ToString());

                Console.WriteLine(" press enter to begin turns");
                Console.ReadLine();

                var results = game.Run();

                Console.WriteLine($"Turns: {results.turnsElapsed}, Time: {results.timeElapsed.ToReadable()},  Winners: ");
                foreach (Player p in results.Winners)
                {
                    Console.WriteLine("- " + p.FriendlyName);
                }

                Console.WriteLine("---- end of game ---- press <enter> to continue");
                Console.ReadLine();
            }
        }


        static void Debugs()
        {
            // Define the simulation, board, pieces, players, and rules
            Stopwatch fullProgramTime = Stopwatch.StartNew();

            Stopwatch watch = new Stopwatch();
            watch.Start();
            Game game = GameModes.classicStratego();

            Console.WriteLine("Board Setup: " + watch.Elapsed.TotalMilliseconds);

            watch.Restart();
            Console.WriteLine(Environment.NewLine + game.CurrentBoard.ToString(true));
            Console.WriteLine(game.CurrentBoard.ToString());
            Console.WriteLine("Board Output: " + watch.Elapsed.TotalMilliseconds);

            Stopwatch totalTurnTime = Stopwatch.StartNew();

            // easy configuration of loggings and tracking, independent of gamemodes or controllers (todo move to rules?)
            game.rules.LoggingSettings = new GameRules.LogSettings()
            {
                logTime = true,
                showEachPlayersPlanning = true,
                showStatePerTurn = true,
                pausePerMove = false,
                winLossReasons = true,
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



            Console.WriteLine("Press <enter> to quit");
            Console.ReadLine();
        }






    }
}
