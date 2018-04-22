using System;
using System.Collections.Generic;
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
            while (true)
            {

                var game = GameModes.tinyStratego();
                game.rules.LoggingSettings = new GameRules.LogSettings()
                {
                    logTime = false,
                    showEachPlayersPlanning = false,
                    showStatePerTurn = true,
                    pausePerMove = true,
                };
                game.rules.MaxPhysicalTurns = 300;

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
    }
}
