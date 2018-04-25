using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCore
{
    
    public class PieceType
    {
        public string Name { get; set; }
        public int Rank { get; set; }
        public string SymbolForBoard { get; set; }
        public string ToSymbol() => SymbolForBoard.PadLeft(3);
        public bool Movable { get; set; } = true;
        public bool CanJump { get; set; } = false; // can jump over other pieces

        public PieceType()
        {
            //CalculateBattleOutcome = (PieceType p) => DetermineWinner(p);
        }

        public PieceType(string name, int rank, string symbolforboard)
        {
            Name = name;
            Rank = rank;
            SymbolForBoard = symbolforboard;

            //CalculateBattleOutcome = (PieceType p) => DetermineWinner(p);
        }

        //public Func<PieceType, GameRules.MoveOutcomes> CalculateBattleOutcome;

        public Func<Board, List<CoordRel>> PossibleMovementFunction = (board) =>  new List<CoordRel>()
            {
                new CoordRel(0, 1),
                new CoordRel(0, -1),
                new CoordRel(-1, 0),
                new CoordRel(1, 0),
            };
        
    }


    // this is only an example as a design-time class
    public class Pawn : PieceType
    {
        public Pawn(string name, int power) : base()
        {
            
        }
    }
}
