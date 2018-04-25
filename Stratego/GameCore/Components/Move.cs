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

        public Move(Piece p, CoordRel targetCoord, Board currentBoard, GameRules rules)
        {
            InitializeAndValidateMove(p, targetCoord.ToAbs(p.pos.X, p.pos.Y), currentBoard, rules);
        }

        public Move(Piece p, CoordAbs targetCoord, Board currentBoard, GameRules rules)
        {
            InitializeAndValidateMove(p, targetCoord, currentBoard, rules);
        }

        /// <summary>
        /// Given a pice and its new location, check if the move is allowed based on obstructions, limits, etc
        /// </summary>
        /// <param name="p"></param>
        /// <param name="targetCoord"></param>
        /// <param name="currentBoard"></param>
        private bool InitializeAndValidateMove(Piece p, CoordAbs targetCoord, Board currentBoard, GameRules rules)
        {
            FromCoord = new CoordAbs(p.pos.X, p.pos.Y);
            movingPiece = p;
            outcome = GameRules.MoveOutcomes.Unknown;
            opponentPiece = null;
            ToCoord = targetCoord;

            // finish populating the Move and check the legality

            Legal = false;

            // ensure the move stays on the board
            if (ToCoord.X >= currentBoard.Width
                || ToCoord.X < 0
                || ToCoord.Y >= currentBoard.Height
                || ToCoord.Y < 0)
                return false;

            opponentPiece = currentBoard.GetPieceAtCoord(ToCoord); // may be null

            if (opponentPiece?.Owner == movingPiece.Owner // cannot move into owned space
                || currentBoard.GetLocationAtCoord(ToCoord)?.Passable == false) // cannot move into obstacles
                return false;

            if ((Math.Abs(FromCoord.X - ToCoord.X) > 1 || Math.Abs(FromCoord.Y - ToCoord.Y) > 1) 
                && movingPiece.Owner == GameModes.playerTwo
                && rules.LoggingSettings.debugJumpchecks)
                System.Diagnostics.Debugger.Break();
            
            // check if theres is an empty line to the target location
            if (p.Type.CanJump == false)
            {
                if (rules.LoggingSettings.debugJumpchecks)
                    Console.WriteLine($"Checking space between {FromCoord} to {ToCoord}...");
                
                int xmin = Math.Min(FromCoord.X, ToCoord.X);
                int xmax = Math.Max(FromCoord.X, ToCoord.X);
                int ymin = Math.Min(FromCoord.Y, ToCoord.Y);
                int ymax = Math.Max(FromCoord.Y, ToCoord.Y);

                for (int y = ymin; y <= ymax; y++)
                    for (int x = xmin; x <= xmax; x++)
                    {
                        // dont include the coords of FromCoord nor ToCoord, just between
                        if (FromCoord.X == x && FromCoord.Y == y ||
                            ToCoord.X == x && ToCoord.Y == y)
                            continue;
                        
                        // check if the path is clear of empty pieces and is traversable
                        if (currentBoard.PiecesLayout[x, y] != null || !currentBoard.LocationsLayout[x, y].Passable)
                        {
                            Legal = false;
                            return false;
                        }

                        if (rules.LoggingSettings.debugJumpchecks)
                            Console.WriteLine($"{x} {y} is " + currentBoard.PiecesLayout[x, y]
                                              + " / " + currentBoard.LocationsLayout[x, y].Passable);

                    }
            }


            // todo: is legality and battle result predictions premature at this stage?
            // outcome = movingPiece.PredictWinner(opponentPiece, rules);//defaults to "move" if nothing is in the way

            Legal = true;
            return true;
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
