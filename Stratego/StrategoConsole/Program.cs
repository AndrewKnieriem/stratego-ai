using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameCore;
using GameCore.Tools;
using System.Diagnostics;


namespace StrategoConsole
{
    class Program
    {

        static void Main(string[] args)
        {
            // Define the simulation, board, pieces, players, and rules
            Stopwatch fullProgramTime = Stopwatch.StartNew();

            Stopwatch watch = new Stopwatch();
            watch.Start();
            Game game = GameModes.tinyStratego();
            
            Console.WriteLine("Board Setup: " + watch.Elapsed.TotalMilliseconds);

            watch.Restart();
            Console.WriteLine(Environment.NewLine + game.CurrentBoard.ToString(true));
            Console.WriteLine(game.CurrentBoard.ToString());
            Console.WriteLine("Board Output: " + watch.Elapsed.TotalMilliseconds);
            /*
            watch.Restart();
            int moveCount = 0;
            foreach (Piece piece in game.CurrentBoard.PieceSet)
            {
                foreach (Move m in piece.GetLegalMoves(game.CurrentBoard))
                {
                    moveCount++;
                }
            }

            Console.WriteLine($"Move generation: {watch.Elapsed.TotalMilliseconds}ms, {moveCount} total moves");
            //Console.WriteLine("Press <enter> to continue");
            //Console.ReadLine();
            */
            Stopwatch totalTurnTime = Stopwatch.StartNew();
            
            // easy configuration of loggings and tracking, independent of gamemodes or controllers (todo move to rules?)
            game.rules.LoggingSettings = new GameRules.LogSettings()
            {
                logTime = true,
                showEachPlayersPlanning = true,
                showStatePerTurn = true,
                pausePerMove = true,

            };
            game.rules.MaxPhysicalTurns = 300;

            var results = game.Run();

            Console.WriteLine($"Turns: {results.turnsElapsed}, Time: {results.timeElapsed.ToReadable()},  Winners: ");
            foreach (Player p in results.Winners)
            {
                Console.WriteLine("- " + p.FriendlyName);
            }

            /*
            moveCount = 0;
            foreach (Piece piece in game.CurrentBoard.PieceSet)
            {
                foreach (Move m in piece.GetLegalMoves(game.CurrentBoard))
                {
                    watch.Restart();

                    moveCount++;
                    // display the move, results, and calculated utility
                    Console.WriteLine($"Depth 1, Move {moveCount}: {m} => {game.CurrentBoard.applyMove(m)}, utility = {game.CurrentBoard.CalculateUtility()}");
                    // display the new board
                    Console.WriteLine($"{game.CurrentBoard.ToString()}");

                    game.CurrentBoard.undoMove(m);


                    Console.WriteLine("single turn Calculations: " + watch.Elapsed.TotalMilliseconds);
                    watch.Stop();
                    //Console.WriteLine("Press <enter> to continue");
                    //Console.ReadLine();
                }
            }

            Console.WriteLine("Full turn Calculations: " + totalTurnTime.Elapsed.TotalMilliseconds);
            */
            
            watch.Stop();
            fullProgramTime.Stop();

            Console.WriteLine("------------------------- DONE in " + fullProgramTime.Elapsed.ToReadable() + "----------------------------");
            

            Console.WriteLine("Press <enter> to continue");
            Console.ReadLine();
        }




    }
}

