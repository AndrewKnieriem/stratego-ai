using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCore.Controllers
{
    public class ControllerMinimax : IPlayerController
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
}
