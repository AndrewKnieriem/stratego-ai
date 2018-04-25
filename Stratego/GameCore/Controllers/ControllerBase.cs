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
    }

    public class DepthFirstController : IPlayerController
    {
        public string GetControllerName() => "Depth First";
        public DepthFirstController()
        {
        }

        public Move chooseMove(Game game, Player player)
        {
            return game.CurrentBoard.GetLegalMovesForPlayer(player, game.rules).FirstOrDefault();
        }
    }

    public class RandomController : IPlayerController
    {
        private static Random rand = new Random();
        public string GetControllerName() => "Random";
        public RandomController()
        {
        }

        public Move chooseMove(Game game, Player player)
        {
            List<Move> moves = game.CurrentBoard.GetLegalMovesForPlayer(player, game.rules).ToList();

            if (moves.Count == 0)
            {
                if (game.rules.LoggingSettings.showEachPlayersPlanning)
                    Console.WriteLine("NO MOVES AVAILABLE");

                return null;
            }


            int choice = rand.Next(0, moves.Count); // second number is exlusive
            if (game.rules.LoggingSettings.showEachPlayersPlanning)
                Console.WriteLine("choosing option " + choice);

            return moves[choice];
        }
    }
    
    public class MinimaxController : IPlayerController
    {
        public string GetControllerName() => "Minimax";
        public Move chooseMove(Game game, Player player)
        {
            throw new NotImplementedException();
            return null;
        }

        /*
        public float CalculateUtility(Board board) // this is a function for the heuristics and should be moved out of the board and into a controller
        {
            float utility = 0;
            foreach (Piece p in board.PieceSet)
            {
                utility += p.ComputeTotalHeuristicValue(board);
            }
            return utility;
        }
        */

        public int CalcFringeAtDepth(int branching, int depth) => (int)Math.Pow(branching, depth);

    }

    public class MonteCarloController : IPlayerController
    {
        public string GetControllerName() => "MonteCarlo";
        public int GamesToRun { get; private set; }
        public bool ShowSubGames = false;
        public bool ShowSubResults = false;

        public MonteCarloController(int maxGamesToRun)
        {
            GamesToRun = maxGamesToRun;
        }

        public Move chooseMove(Game game, Player player)
        {
            Dictionary<simpleMove, int> moveResults = new Dictionary<simpleMove, int>();

            // have to treat this player as if they will play random in these sub-games, not as monte carlo
            var currentController = player.Controller;
            player.Controller = new RandomController();

            var currentLogSettings = game.rules.LoggingSettings;


            // for monte carlo we should play out n random games and track how our next move might win or lose
            for (int i = 0; i < GamesToRun; i++)
            {
                System.Diagnostics.Debug.Indent();

                if (game.rules.LoggingSettings.showEachPlayersPlanning || ShowSubResults)
                    Console.WriteLine("Running sub-game");

                
                // create a copy of the game and play it through with what we know so far
                Game testgame = new Game(game); // becomes an infinitely recursive problem since every sub-game is also monte carlo

                if (!ShowSubGames)
                {
                    testgame.rules.LoggingSettings = new GameRules.LogSettings()
                    {
                        logTime = false,
                        showEachPlayersPlanning = false,
                        showStatePerTurn = false,
                        pausePerMove = false,
                        winLossReasons = false,
                        debugJumpchecks = false,
                        showMovePerTurn = false,
                        listMoveSeqenceAtEnd = false,
                        showBombDefusals = false,
                        showHiddenPieces = false,
                    };
                }


                // randomize any unknown pieces on the board
                if (testgame.CurrentBoard.PieceSet.Any(x => x.IsRevealed == false && x.Owner != player))
                {
                    List<CoordAbs> availableSpaces = new List<CoordAbs>();
                    // randomize any unknown piece and play through
                    foreach (Piece unknownPiece in testgame.CurrentBoard.PieceSet.Where(x => x.IsRevealed == false && x.Owner != player))
                    {
                        availableSpaces.Add(unknownPiece.pos);
                        unknownPiece.turnRevealed = -1; // place holder to indicate piece is being moved
                        testgame.CurrentBoard.PiecesLayout[unknownPiece.pos.X, unknownPiece.pos.Y] = null;
                    }

                    Random rand = new Random();

                    foreach (Piece unknownPiece in testgame.CurrentBoard.PieceSet.Where(x => x.turnRevealed == -1))
                    {
                        // hide the piece again
                        unknownPiece.turnRevealed = null; 

                        // assign it a random location from the spots available
                        int ran = rand.Next(0, availableSpaces.Count);
                        unknownPiece.pos = availableSpaces[ran];
                        availableSpaces.RemoveAt(ran);
                        testgame.CurrentBoard.PiecesLayout[unknownPiece.pos.X, unknownPiece.pos.Y] = unknownPiece;
                    }
                }

                /*
                if (game.rules.LoggingSettings.showEachPlayersPlanning || ShowSubResults)
                {
                    Console.WriteLine("--- subgame new board: ");
                    Console.WriteLine(testgame.CurrentBoard.ToAsciiLayout());
                }
                */
                
                var results = testgame.Run();

               //if (results != null)
               //     System.Diagnostics.Debugger.Break();

                int score = results.Winners.Contains(player) ? 1 : -1;
                
                // lookup via string instead of object
                Move firstMove = testgame.MoveSequence.FirstOrDefault().Value; // remember move sequence is <turn, move>

                simpleMove simple = new simpleMove()
                {
                    from = firstMove.FromCoord,
                    to = firstMove.ToCoord,
                };


                if (moveResults.Keys.Contains(simple))
                    moveResults[simple] += score;
                else
                    moveResults.Add(simple, score);

                if (game.rules.LoggingSettings.showEachPlayersPlanning || ShowSubResults)
                    Console.WriteLine($"{firstMove} ... => {score}, Total = {moveResults[simple]}");

                System.Diagnostics.Debug.Unindent();
            }

            // reset back to original settings
            player.Controller = currentController;
            game.rules.LoggingSettings = currentLogSettings;

            // do the move that has the most win options
            // Note: the liklihood of making the same move, with 1000 randomized boards seems infinitely tiny, let alone useful
            var bestMoveStats = moveResults.OrderByDescending(x => x.Value).FirstOrDefault();
            if (game.rules.LoggingSettings.showEachPlayersPlanning || ShowSubResults)
                Console.WriteLine("Best move from subgames: " + bestMoveStats.Key.ToString() + " w/ score " + bestMoveStats.Value);

            var bestMove = bestMoveStats.Key;

            // convert the sub-game move into a move that is applicable to the current game
            var relevantMove = new Move(
                game.CurrentBoard.GetPieceAtCoord(bestMove.from), 
                bestMove.to,
                game.CurrentBoard, game.rules
            );
            
            return relevantMove;
        }

        private struct simpleMove
        {
            public CoordAbs from;
            public CoordAbs to;
        }
    }


}
