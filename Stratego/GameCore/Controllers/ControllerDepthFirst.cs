using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCore.Controllers
{
    public class ControllerDepthFirst : IPlayerController
    {
        public string GetControllerName() => "DepthFirst";
        public ControllerDepthFirst()
        {
        }

        public Move chooseMove(Game game, Player player)
        {
            return game.CurrentBoard.GetLegalMovesForPlayer(player, game.rules).FirstOrDefault();
        }
    }
}
