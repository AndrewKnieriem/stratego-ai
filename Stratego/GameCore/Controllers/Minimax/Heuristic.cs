using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCore
{
    public class Heuristic
    {
        public string Name { get; set; }
        public Func<Board, float> Function { get; set; }
         
        public Heuristic()
        {
            
        }



    }
}
