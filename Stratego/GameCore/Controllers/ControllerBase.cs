using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameCore;

namespace GameCore.Controllers
{
    public interface IPlayerController
    {
        Move chooseMove(Game game, Player player);
        string GetControllerName();
    }

    public abstract class ControllerBase 
    {
        public string GetControllerName() => "Controller";
        public override string ToString()
        {
            return GetControllerName();
        }
    }
    

}
