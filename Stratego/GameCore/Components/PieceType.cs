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

        public PieceType()
        {
            CalculateBattleOutcome = (PieceType p) => DetermineWinner(p);
        }

        public Func<PieceType, GameRules.MoveOutcomes> CalculateBattleOutcome;

        public virtual GameRules.MoveOutcomes DetermineWinner(PieceType opponent)
        {
            if (opponent == null)
                return GameRules.MoveOutcomes.Move;
            
            if (this.Rank > opponent.Rank)
                return GameRules.MoveOutcomes.Win;
            else if (this.Rank < opponent.Rank)
                return GameRules.MoveOutcomes.Lose;
            else
                return GameRules.MoveOutcomes.Tie; // usually both die
        }

        
    }


    // this is only an example as a design-time class
    public class Pawn : PieceType
    {
        public Pawn(string name, int power) : base()
        {
            
        }
    }
}
