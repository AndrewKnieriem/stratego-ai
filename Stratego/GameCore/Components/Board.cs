using System;
using System.Collections.Generic;
using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameCore;
using GameCore.Tools;
using System.Diagnostics;

namespace GameCore
{
    
    public abstract class State
    {
        private int dosomething;
    }

    /// <summary>
    /// The Board acts as a scratchpad to explore moves and to calculate heuristics, but is not stored in the nodes
    /// </summary>
    
    public class Board : State
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public int TurnNumber { get; set; }

        // cache the hash so that it doesn't need to be recalculated with each hashset lookup
        private int? _hashCode;

        public override int GetHashCode() => _hashCode.HasValue
            ? _hashCode.Value
            : (int) (_hashCode = base.GetHashCode());



        // Complete lists, can manage pieces not on board
        public List<Piece> PieceSet { get; set; }

        public LocationType[] LocationSet { get; set; }

        // Spatial references, pieces in play
        public Piece[,] PiecesLayout { get; set; }

        public LocationType[,] LocationsLayout { get; set; } // probably static per session


        public Piece GetPieceAtCoord(CoordAbs c) => PiecesLayout.TryIndex(c.X, c.Y); // can return null
        public LocationType GetLocationAtCoord(CoordAbs c) => LocationsLayout.TryIndex(c.X, c.Y); // can return null

        /// <summary>
        /// Returns the players that are still in the game, checking each by the "active" requirements
        /// </summary>
        /// <param name="rules"></param>
        /// <param name="board"></param>
        /// <returns></returns>
        public List<Player> TerminalStateCheck(GameRules rules)
        {
            List<Player> activePlayers = new List<Player>(rules.PlayerOrder);

            foreach (Player p in rules.PlayerOrder)
            {
                // remove a player that no longer satisfies the "active" requirements
                var playerResult = rules.TerminalStateFunction(rules, this, p);
                if (playerResult == true) // instant win
                    return new List<Player>(){ p };
                else if (playerResult == false) // removed from game
                    activePlayers.Remove(p);
                else
                    continue; // still in game if null
            }
            return activePlayers;
        }


        public Board()
        {
        }

        /// <summary>
        /// Creates a deep copy of the board at the current state
        /// </summary>
        /// <param name="otherBoard"></param>
        public Board(Board otherBoard)
        {
            this.Height = otherBoard.Height;
            this.Width = otherBoard.Width;
            this.TurnNumber = otherBoard.TurnNumber;

            // locations types wont change during game, so its ok to keep original references
            this.LocationsLayout = otherBoard.LocationsLayout; 
            this.LocationSet = otherBoard.LocationSet;
            
            // copy pieces
            this.PiecesLayout = new Piece[this.Width, this.Height];
            this.PieceSet = new List<Piece>();
            foreach (Piece oldPiece in otherBoard.PieceSet)
            {
                // copy the piece
                var newP = new Piece(oldPiece.pos.X, oldPiece.pos.Y, oldPiece.Owner, oldPiece.Type)
                {
                    turnRevealed = oldPiece.turnRevealed,
                };

                // add the piece to the list and layout
                this.PieceSet.Add(newP);
                this.PiecesLayout[newP.pos.X, newP.pos.Y] = newP;
            }

            
            // https://stackoverflow.com/questions/18547354/c-sharp-linq-find-duplicates-in-list
            var dupes = PieceSet.GroupBy(x => x.pos).Where(g => g.Count() > 1).Select(y => y.Key).ToList();

            if (dupes.Count > 0)
            {
                Console.WriteLine(" -------------- DUPES");
                System.Diagnostics.Debugger.Break();
            }
            
        }

        public IEnumerable<Move> GetLegalMovesForPlayer(Player p, GameRules rules)
        {
            // todo: itd be nice to index pieces by player, but we dont persist players into projected board states
            foreach (Piece piece in this.PieceSet.Where(x=>x.Owner == p)) 
            {
                foreach (Move m in piece.GetLegalMoves(this, rules))
                {
                    yield return m;
                }
            }
        }

        public string ToAsciiLayout(Player showPiecesForPlayer = null, bool showCords = false)
        {
            StringBuilder str = new StringBuilder();

            for (int y = this.Height - 1; y >= 0; y--)
            {
                for (int x = 0; x < this.Width; x++)
                {
                    if (showCords)
                        str.Append($"[{x},{y}]");
                    else
                    {
                        if (this.LocationsLayout[x, y]?.Passable == false)
                            str.Append("[###]");
                        else
                        {
                            var pieceAtLocation = PiecesLayout[x, y];
                            if (pieceAtLocation == null)
                            {
                                str.Append("[   ]");
                                continue;
                            }
                            
                            // if the piece is hidden
                            if (showPiecesForPlayer != null 
                                && pieceAtLocation?.Owner != showPiecesForPlayer
                                && pieceAtLocation.IsRevealed == false)
                                str.Append("[" + pieceAtLocation?.Owner.FriendlySymbol + "??" + "]");
                            else
                                str.Append("[" + (pieceAtLocation?.ToSymbol()) + "]");
                        }
                            
                    }
                }

                str.Append(Environment.NewLine);
            }

            return str.ToString();
        }
        
        /// <summary>
        /// Serialize into a simple string, for easy hashing or state comparisons
        /// </summary>
        /// <returns></returns>
        public string Serialize()
        {
            StringBuilder str = new StringBuilder();
            for (int y = this.Height - 1; y >= 0; y--)
            {
                for (int x = 0; x < this.Width; x++)
                {
                   str.Append(PiecesLayout[x, y]?.ToSymbol() ?? "|");
                }
            }
            return str.ToString();
        }
        
        public GameRules.MoveOutcomes applyMove(Move m, GameRules rules) // move must be legal
        {
            // apply the movement
            m.movingPiece.pos = m.ToCoord;
            this.PiecesLayout[m.FromCoord.X, m.FromCoord.Y] = null;
            this.PiecesLayout[m.ToCoord.X, m.ToCoord.Y] = null;

            // using a single battle function allows for easier customization
            
            if (m.movingPiece != null)
                m.movingPiece.turnRevealed = TurnNumber;

            if (m.opponentPiece != null)
                m.opponentPiece.turnRevealed = TurnNumber;

            m.outcome = rules.BattleFunction(m.movingPiece, m.opponentPiece);

            switch (m.outcome)
            {
                case GameRules.MoveOutcomes.Move:
                    this.PiecesLayout[m.ToCoord.X, m.ToCoord.Y] = m.movingPiece;
                    break;
                case GameRules.MoveOutcomes.Win:
                    this.PiecesLayout[m.ToCoord.X, m.ToCoord.Y] = m.movingPiece;

                    // remove the opponent
                    m.opponentPiece.pos = Piece.removedPos;
                    PieceSet.Remove(m.opponentPiece);
                    break;
                case GameRules.MoveOutcomes.Lose:
                    this.PiecesLayout[m.ToCoord.X, m.ToCoord.Y] = m.opponentPiece;

                    // remove our piece
                    m.movingPiece.pos = Piece.removedPos;
                    PieceSet.Remove(m.movingPiece);
                    break;
                case GameRules.MoveOutcomes.Tie:
                    // remove the opponent
                    m.opponentPiece.pos = Piece.removedPos;
                    PieceSet.Remove(m.opponentPiece);
                    // remove our piece
                    m.movingPiece.pos = Piece.removedPos;
                    PieceSet.Remove(m.movingPiece);
                    break;
            }
            
            return m.outcome;
        }

        public bool undoMove(Move m)
        {
            // undo the movement
            m.movingPiece.pos = m.FromCoord;
            this.PiecesLayout[m.ToCoord.X, m.ToCoord.Y] = m.opponentPiece; // remember, can be null
            this.PiecesLayout[m.FromCoord.X, m.FromCoord.Y] = m.movingPiece;

            // re-hide the pieces if they were just revealed
            if (m.movingPiece.turnRevealed == this.TurnNumber)
                m.movingPiece.turnRevealed = null;

            if (m.opponentPiece.turnRevealed == this.TurnNumber)
                m.opponentPiece.turnRevealed = null;



            // only add the moving piece back into the piece set IF it was removed previously
            if (m.outcome == GameRules.MoveOutcomes.Lose || m.outcome == GameRules.MoveOutcomes.Tie)
                PieceSet.Add(m.movingPiece);
            
            m.movingPiece.pos = m.FromCoord;
            if (m.opponentPiece != null)
            {
                m.opponentPiece.pos = m.ToCoord;

                // only add the opponent piece back into the piece set IF it was removed previously
                if (m.outcome == GameRules.MoveOutcomes.Win || m.outcome == GameRules.MoveOutcomes.Tie)
                    PieceSet.Add(m.opponentPiece);
            }
            
            return true;
        }

        
    }
















    public interface ICoord
    {
    }
    
    public class CoordAbs : ICoord
    {
        public int X;
        public int Y;

        public override string ToString()
        {
            return $"({X}, {Y})";
        }

        public CoordAbs(int x, int y)
        {
            X = x;
            Y = y;
        }

        public CoordRel ToRel(int x, int y) => new CoordRel(x - X, y - Y);
    }
    
    public class CoordRel : ICoord
    {
        public int DeltaX;
        public int DeltaY;

        public override string ToString()
        {
            return $"{{{DeltaX}, {DeltaY}}}";
        }

        public CoordRel(int delX, int delY)
        {
            DeltaX = delX;
            DeltaY = delY;
        }

        public CoordAbs ToAbs(int x, int y) => new CoordAbs(x + DeltaX, y + DeltaY);
    }
}
