using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameCore.Controllers;

namespace GameCore
{
    // class for both human and computer players
    
    public class Player
    {
        public string FriendlyName { get; set; }
        public string FriendlySymbol { get; set; }
        public IEnumerable<GameCore.GameRules.Arsenal> Arsenal;
        public IPlayerController Controller { get; set; }

        // a collection of the pieces owned by the user would be nice for faster "per-player" actions,
        // however when projecting moves the piece collection is altered and we don't carry the player objects
        // we only carry the board for future states, which has the pieces
        public override string ToString() => FriendlyName + " (" + Controller?.GetControllerName() + ")";

        public Player()
        {
            
        }

        public Move chooseMove(Game game) => Controller.chooseMove(game, this);
    }
}
