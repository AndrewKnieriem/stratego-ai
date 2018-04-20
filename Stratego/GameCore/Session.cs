using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCore
{
    // A Session is a collection of games, each with the same rules, pieces, heuristics, etc
    // The session is used to evaluate the progresion of learning 
    // (however changing minor rules with the same training set could allow for more general learning)
    // TODO: do we need to differentiate between training and execution sessions?
    public class Session
    { 
        // Game is a collection of the board, the pieces, and the heuristics
        public string Description { get; set; }

        public GameRules rules { get; set; }


        public Session()
        {
            

        }



        
    }
}
