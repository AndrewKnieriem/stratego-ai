using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameCore;

namespace GameCore.Controllers
{
    public interface IPlayerController
    {
        Move chooseMove(Game game, Player player);
        string GetControllerName();
    }

    public abstract class ControllerBase 
    {
        public string GetControllerName() => "Controller";
    }

    public class DepthFirstController : IPlayerController
    {
        public string GetControllerName() => "Depth First";
        public DepthFirstController()
        {
        }

        public Move chooseMove(Game game, Player player)
        {
            return game.CurrentBoard.GetLegalMovesForPlayer(player, game.rules).First();
        }
    }

    public class RandomController : IPlayerController
    {
        private static Random rand = new Random();
        public string GetControllerName() => "Random";
        public RandomController()
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
    
    public class MinimaxController : IPlayerController
    {
        public string GetControllerName() => "Minimax";
        public Move chooseMove(Game game, Player player)
        {
            throw new NotImplementedException();
            return null;
        }

        /*
        public float CalculateUtility(Board board) // this is a function for the heuristics and should be moved out of the board and into a controller
        {
            float utility = 0;
            foreach (Piece p in board.PieceSet)
            {
                utility += p.ComputeTotalHeuristicValue(board);
            }
            return utility;
        }
        */

        public int CalcFringeAtDepth(int branching, int depth) => (int)Math.Pow(branching, depth);

    }

    public class MonteCarloController : IPlayerController
    {
        public string GetControllerName() => "MonteCarlo";
        public Move chooseMove(Game game, Player player)
        {
            throw new NotImplementedException();
            return null;
        }
    }


}
