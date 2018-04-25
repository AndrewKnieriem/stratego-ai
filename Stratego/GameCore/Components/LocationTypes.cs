using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCore
{
    // the properties of a spot on the board; crossable, hidden, high ground, etc
    
    public class LocationType
    {
        public string Name { get; set; }
        public bool Standable { get; set; } = true;
        public bool Passable { get; set; } = true;

        // allows player to use this location as a starting point for their piece
        public Player StarterPlace { get; set; } = null; 


        // example attributes for locations for more complex games
        /*
        public int PowerBoost { get; set; } = 0;
        public int DamageToTake { get; set; } = 0;
        public int DamageToHeal { get; set; } = 0;
        */

        // cache the hash so that it doesn't need to be recalculated with each hashset lookup
        private int? _hashCode;
        public override int GetHashCode() => _hashCode.HasValue ? _hashCode.Value : (int)(_hashCode = Name.GetHashCode());


        // example that a location on the board could cause a change in the board or the piece locations
        public Board CheckForPieceChanges()
        {
            throw new NotImplementedException();
        }
        public Board CheckForBoardChanges()
        {
            throw new NotImplementedException();
        }


        


        public LocationType()
        {
            
        }


    }
}
