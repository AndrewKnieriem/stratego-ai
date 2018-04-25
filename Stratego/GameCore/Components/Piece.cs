using System;
using System.Collections.Generic;

namespace GameCore
{
    
    public class Piece
    {
        public CoordAbs pos { get; set; }
        public PieceType Type { get; set; }
        public Player Owner { get; set; }

        public static readonly CoordAbs removedPos = new CoordAbs(-10, -10); // all moves are invalid if accidently accessed


        #region Probability predictions of opponent pieces, 

        // OPTION 1 : unknown until known 100%
        public bool IsRevealed => turnRevealed.HasValue;

        public int? turnRevealed = null;



        // OPTION 2: a table of probabilities, at least so eliminated possibilities won't factor in
        private HashSet<PieceProbability> PieceProbabilities;
        private class PieceProbability
        {
            public PieceType Type; // hash based on type
            public double Probability;

            public override int GetHashCode()
            {
                return Type.Name.GetHashCode();
            }
        }


        // OPTION 3: an easier way to store option 2, but probably much worse performance
        private Dictionary<PieceType, double> PieceProbabilitiesOther;

        #endregion

        public string ToSymbol()
        {
            return (this.Owner.FriendlySymbol + this.Type.SymbolForBoard ?? "").PadLeft(3);
        }


        public Piece(int x, int y)
        {
            this.pos = new CoordAbs(x, y);
        }

        public Piece(int x, int y, Player owner, PieceType type)
        {
            this.pos = new CoordAbs(x, y);
            Owner = owner;
            Type = type;
        }

        // TODO MOVE THESE HEURISTIC FUNCTIONS
        #region these only belong to a particular control model - ie heuristics / minimax
        /*
        public float ComputeTotalHeuristicValue(Board currentBoard)
        {
            return SampleHeuristic_SpacesAvailable(currentBoard) + SampleHeuristic_AttacksAvailable(currentBoard);
        }
        
        public float SampleHeuristic_SpacesAvailable(Board currentBoard)
        {
            int spacesAvailable = 0;

            foreach (CoordRel move in GetPossibleMovement())
            {
               // if (this.pos.X == 0 && this.pos.Y == 6)
               //     System.Diagnostics.Debugger.Break();

                var ToCoord = move.ToAbs(this.pos.X, this.pos.Y);
                if (ToCoord.X < currentBoard.Width // ensure the move stays on the board
                    && ToCoord.X >= 0
                    && ToCoord.Y < currentBoard.Height
                    && ToCoord.Y >= 0
                    && currentBoard.PiecesLayout[ToCoord.X, ToCoord.Y] == null // unoccupied space
                    && currentBoard.LocationsLayout[ToCoord.X, ToCoord.Y].Passable) // enterable space
                    spacesAvailable++;
            }
            
            return spacesAvailable;
        }

        public float SampleHeuristic_AttacksAvailable(Board currentBoard)
        {
            int attacksAvailable = 0;

            foreach (CoordRel move in GetPossibleMovement())
            {
                var ToCoord = move.ToAbs(this.pos.X, this.pos.Y);
                if (ToCoord.X < currentBoard.Width // ensure the move stays on the board
                    && ToCoord.X >= 0
                    && ToCoord.Y < currentBoard.Height
                    && ToCoord.Y >= 0
                    && currentBoard.PiecesLayout[ToCoord.X, ToCoord.Y] != null // occupied space
                    && currentBoard.PiecesLayout[ToCoord.X, ToCoord.Y]?.Owner != this.Owner // enemy piece 
                    && currentBoard.LocationsLayout[ToCoord.X, ToCoord.Y].Passable) // enterable space 
                    attacksAvailable++;
            }

            return attacksAvailable;
        }
        */
        #endregion


        

        


        private IEnumerable<CoordRel> GetPossibleMovement(Board board, int maxMove = 1)
        {
            // TODO change movement patterns into an initialization parameter for piece types, needed to validate/prompt user's moves
            // unfortunately cant have anonymous functions with yield...

            if (this.Type.Movable)
            {
                foreach (var c in this.Type.PossibleMovementFunction(board))
                    yield return c;
            }
            else
                yield break; //return Enumerable.Empty<CoordRel>();
        }
        

        public IEnumerable<Move> GetLegalMoves(Board currentBoard, GameRules rules)
        {
            foreach (CoordRel newCoord in GetPossibleMovement(currentBoard))
            {
                // generate the move and validate it
                Move m = new Move(this, newCoord, currentBoard, rules);

                if (m.Legal)
                {
                    if (rules.LoggingSettings.showEachPlayersPlanning)
                        Console.WriteLine("possible move: " + m.ToString());

                    yield return m;
                }
                    
            }
        }

        /// <summary>
        /// Wraps the piece type's check for winner and adds if the piece is revealed
        /// </summary>
        /// <param name="opponent"></param>
        /// <returns></returns>
        public GameRules.MoveOutcomes PredictWinner(Piece opponent, GameRules rules)
        {
            if (opponent == null)
                return GameRules.MoveOutcomes.Move;

            if (opponent.IsRevealed)
                return rules.BattleFunction(this, opponent);

            // TODO CHANGE winner determiniation function according to controller
            // according to minimax rules, if we dont know we should assume a loss
            return GameRules.MoveOutcomes.Lose;
        }





    }


}
