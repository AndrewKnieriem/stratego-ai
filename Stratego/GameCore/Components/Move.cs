using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCore
{
    public abstract class Transition
    {
        
    }


    /// <summary>
    /// transition between states, it has to be complex enough to support any kind of board change
    /// </summary>
    public class Move : Transition
    {
        public Piece movingPiece;
        public CoordAbs FromCoord; // keep a seperate record for board location, since original piece location will change
        public CoordAbs ToCoord;
        public Piece opponentPiece;
        public GameRules.MoveOutcomes outcome;
        public bool Legal { get; private set; }

        public Move(Piece p, CoordRel coord, Board currentBoard)
        {
            InitializeAndValidateMove(p, coord.ToAbs(p.pos.X, p.pos.Y), currentBoard);
        }

        public Move(Piece p, CoordAbs coord, Board currentBoard)
        {
            InitializeAndValidateMove(p, coord, currentBoard);
        }

        /// <summary>
        /// Given a pice and its new location, check if the move is allowed based on obstructions, limits, etc
        /// </summary>
        /// <param name="p"></param>
        /// <param name="coord"></param>
        /// <param name="currentBoard"></param>
        private void InitializeAndValidateMove(Piece p, CoordAbs coord, Board currentBoard)
        {
            FromCoord = new CoordAbs(p.pos.X, p.pos.Y);
            movingPiece = p;
            outcome = GameRules.MoveOutcomes.BadMove;
            opponentPiece = null;
            ToCoord = coord;


            // finish populating the Move and check the legality

            Legal = false;

            if (ToCoord.X >= currentBoard.Width // ensure the move stays on the board
                || ToCoord.X < 0
                || ToCoord.Y >= currentBoard.Height
                || ToCoord.Y < 0)
                return;

            opponentPiece = currentBoard.GetPieceAtCoord(ToCoord); // may be null

            if (opponentPiece?.Owner == movingPiece.Owner // cannot move into owned space
                || currentBoard.GetLocationAtCoord(ToCoord)?.Passable == false) // cannot move into obstacles
                return;

            // todo: is legality and battle result predictions premature at this stage?
            outcome = movingPiece.DetermineWinner(opponentPiece);//defaults to "move" if nothing is in the way

            Legal = true;
        }


        public override string ToString()
        {
            // -10(0,0)   move   (1,0)         -> move piece
            // -10(0,0)   won    +5(1,0)       -> move -10 and remove +5
            // -3(1,5)    tie    +3(2,5)       -> remove both -3 and +3
            return $"{movingPiece?.Owner.FriendlySymbol}{movingPiece.Type.SymbolForBoard}{FromCoord}"
                   + $" {outcome} "
                   + $"{opponentPiece?.Owner.FriendlySymbol}{opponentPiece?.Type.SymbolForBoard}{ToCoord}";
        }
    }
}
