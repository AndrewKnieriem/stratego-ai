using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCore.Controllers
{
    public class ControllerMonteUtility : IPlayerController
    {
        /// <summary>
        /// Monte Carlo (games x depth)
        /// </summary>
        /// <returns></returns>
        public string GetControllerName() => $"MonteCarlo{GamesToRun}x{MaxSubGameDepth}";
        public int GamesToRun { get; private set; }
        public bool ShowSubGames = false;
        public bool ShowSubResults = false;
        public int MaxSubGameDepth = 300;

        public ControllerMonteUtility(int maxGamesToRun, int maxSubDepth)
        {
            GamesToRun = maxGamesToRun;
            MaxSubGameDepth = maxSubDepth;
        }

        public Move chooseMove(Game game, Player player)
        {
            Dictionary<simpleMove, MoveScore> moveResults = new Dictionary<simpleMove, MoveScore>();

            // have to treat this player as if they will play random in these sub-games, not as monte carlo
            var currentController = player.Controller;
            player.Controller = new ControllerRandom();

            var currentLogSettings = game.rules.LoggingSettings;

            object scorelock = new object();

            // for monte carlo we should play out n random games and track how our next move might win or lose
            for (int i = 0; i < GamesToRun; i++)
                //Parallel.For(0, GamesToRun, (i) =>
            {
                System.Diagnostics.Debug.Indent();

                if (game.rules.LoggingSettings.showEachPlayersPlanning || ShowSubResults)
                    Console.WriteLine("Running sub-game");


                // create a copy of the game and play it through with what we know so far
                Game testgame = new Game(game); // becomes an infinitely recursive problem since every sub-game is also monte carlo
                testgame.rules.MaxPhysicalTurns = MaxSubGameDepth;

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
                    foreach (Piece unknownPiece in testgame.CurrentBoard.PieceSet.Where(
                        x => x.IsRevealed == false && x.Owner != player))
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


                // DIFFERENCE FROM MONTE CARLO - 
                // weigh the state based on number of pieces we own instead of win/lose
                var results = testgame.Run();

                int score = 0;

                if (results.Winners.Count == 1 && results.Winners.Contains(player)) // WIN
                    score = 100;
                else if (results.Winners.Count == 1 && results.Winners.Contains(player)) // LOSS
                    score = -100;
                else
                    score = testgame.CurrentBoard.PieceSet.Count(x => x.Owner == player);

                // lookup via string instead of object
                Move firstMove = testgame.MoveSequence.FirstOrDefault().Value; // remember move sequence is <turn, move>

                if (firstMove == null)
                    continue;

                simpleMove simple = new simpleMove()
                {
                    from = firstMove.FromCoord,
                    to = firstMove.ToCoord,
                };

                MoveScore mscore = new MoveScore();

                lock (scorelock)
                {
                    if (moveResults.Keys.Contains(simple))
                    {
                        mscore = moveResults[simple];
                        mscore.occurances++;
                        mscore.totalScore += score;
                        moveResults[simple] = mscore;
                    }
                    else
                    {
                        mscore.totalScore = score;
                        mscore.occurances = 1;
                        moveResults.Add(simple, mscore);
                    }
                        
                }


                if (game.rules.LoggingSettings.showEachPlayersPlanning || ShowSubResults)
                    Console.WriteLine($"{firstMove} ... => {score}, x{moveResults[simple].occurances} Total = {(moveResults[simple].totalScore / moveResults[simple].occurances)}");

                System.Diagnostics.Debug.Unindent();
            };

            // reset back to original settings
            player.Controller = currentController;
            game.rules.LoggingSettings = currentLogSettings;

            // do the move that has the most win options
            // Note: the liklihood of making the same move, with 1000 randomized boards seems infinitely tiny, let alone useful
            var bestMoveStats = moveResults.OrderByDescending(x => x.Value.totalScore / x.Value.occurances).FirstOrDefault();
            if (game.rules.LoggingSettings.showEachPlayersPlanning || ShowSubResults)
                Console.WriteLine("Best move from subgames: " + bestMoveStats.Key.from 
                    + " to " + bestMoveStats.Key.to
                    + " w/ avg score " + (bestMoveStats.Value.totalScore / bestMoveStats.Value.occurances));

            var bestMove = bestMoveStats.Key;
            if (bestMove.from == null) // if there is no moves available then return null and be removed from game
                return null;

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

        private struct MoveScore
        {
            public int occurances;
            public int totalScore;
        }
    }
}
