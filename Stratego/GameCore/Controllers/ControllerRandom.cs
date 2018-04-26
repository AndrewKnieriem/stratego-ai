using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCore.Controllers
{
    public class ControllerRandom : IPlayerController
    {
        private static Random rand = new Random();
        public string GetControllerName() => "Random";
        public ControllerRandom()
        {
        }

        public Move chooseMove(Game game, Player player)
        {
            List<Move> moves = game.CurrentBoard.GetLegalMovesForPlayer(player, game.rules).ToList();

            if (moves.Count == 0)
            {
                if (game.rules.LoggingSettings.showEachPlayersPlanning)
                    Console.WriteLine("NO MOVES AVAILABLE");

                return null;
            }

            int choice = rand.Next(0, moves.Count); // second number is exlusive
            if (game.rules.LoggingSettings.showEachPlayersPlanning)
                Console.WriteLine("choosing option " + choice);

            return moves[choice];
        }
    }
}
