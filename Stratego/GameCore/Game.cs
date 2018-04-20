using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCore
{
    public class GameRules
    {
        public Board InitialBoard { get; private set; } // please don't change after game starts
        public int MaxPhysicalTurns = 1000;

        // this provides the pieces, count, type, and affiliations of each
        public List<Arsenal> Arsenals { get; set; }

        /// <summary>
        /// the players in turn order
        /// </summary>
        public List<Player> PlayerOrder { get; set; }
        
        // TODO: replace Arsenal with piece collections directly under player object
        // as combining the lists is much easier than filtering them out constantly 
        // ... or at least doubly link / reference them for faster finding
        public class Arsenal
        {
            // min, max, and start define the range of pieces a player can place to start the game
            public int CountMin { get; set; }
            public int CountMax { get; set; }
            public int CountStart { get; set; }

            [Obsolete("Not used for now as updating arsenal listing could be pricey")]
            public int CountCurrent { get; private set; }
            public Player Owner { get; set; }
            public PieceType Type { get; set; }
            public Arsenal()
            {
            }

            public Arsenal(int min, int max, int start, Player owner, PieceType type)
            {
                CountMin = min;
                CountMax = max;
                CountStart = start;
                Owner = owner;
                Type = type;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_arsenal"></param>
        /// <param name="_players">The players in order of turn sequence</param>
        /// <param name="_startingBoard"></param>
        public GameRules(List<Arsenal> _arsenal, List<Player> _players, Board _startingBoard)
        {
            Arsenals = _arsenal;
            PlayerOrder = _players;
            InitialBoard = _startingBoard;

            
            // cross reference the objects between the collections to reduce lookups later
            // TODO: test that all references are updated in all sources
            foreach (Player p in PlayerOrder)
            {
                p.Arsenal = Arsenals.Where(x => x.Owner == p);
            }

            // update starting counts based on how the initial board is layed out, ie if 2 pawns are already out, set that in the arsenal
            //foreach (Arsenal a in Arsenals)
            //{
            //    a.CountStart = InitialBoard.PieceSet.Where(x=>x.Owner == a.Owner && x.PieceType == a.PieceType).Count()
            //}

            
            InitialBoard.PiecesLayout = new Piece[InitialBoard.Width, InitialBoard.Height];
            

            foreach (Piece p in InitialBoard.PieceSet)
            {
                if (InitialBoard.PiecesLayout[p.pos.X, p.pos.Y] == null)
                {
                    InitialBoard.PiecesLayout[p.pos.X, p.pos.Y] = p;
                }
                else
                {
                    Console.WriteLine("ERROR: cannot place pieces in the same location");
                    throw new Exception("ERROR: cannot place pieces in the same location");
                }
            }


        }

        public enum MoveOutcomes
        {
            Win = 1,
            Lose = -1,
            Tie = 0,
            /// <summary>
            /// Since we expect a loss with unknown battles, an Unknown BattleOutcome means its not set
            /// </summary>
            BadMove = 2,
            /// <summary>
            /// Did not attack an enemy, moved successfully
            /// </summary>
            Move = 3,
        }

        /// <summary>
        /// Function to determine if a player automatically wins (true), lost (false), or is still active in game (null)
        /// </summary>
        public Func<GameRules, Board, Player, bool?> TerminalStateFunction { get; set; } = (rules, board, player) =>
        {
            // TODO: it would be nice if a "reason" could be added to each win/lose scenario to better trace

            // in this case there is no instant win scenario
            bool stillHaspieces = board.PieceSet.Where(x => x.Owner == player).All(x => x.pos == Piece.removedPos);
            if (stillHaspieces)
                return null; // still in game
            else
                return false; // lost
        };


        public LogSettings LoggingSettings;

        public class LogSettings
        {
            public bool logTime = true;
            public bool showStatePerTurn = true;
            public bool showMovePerTurn = true;
            public bool showEachPlayersPlanning = false;
            public bool pausePerMove = false;
            public bool winLossReasons = true;
            public bool listMoveSeqenceAtEnd = true;
        }
    }


    public class Game
    {

        // for the same to start we need an initial board layout
        // which game within this session has occured
        public int GameNumber { get; set; }
        public Board CurrentBoard { get; set; }
        public GameRules rules { get; set; }
        public Player PlayersTurn => rules.PlayerOrder[(CurrentBoard.TurnNumber - 1) % rules.PlayerOrder.Count]; // circular index of players (trouble with double turns)

        public Dictionary<int, Move> MoveSequence { get; set; } = new Dictionary<int, Move>();
        public Game()
        {

        }
        
        // todo: restructure this function since it currently changes the state of the object after running
        public GameResults Run() // yield, async, parallel, task.run() ? 
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

            if (rules.LoggingSettings.logTime)
                watch.Start();

            List<Player> activePlayers = null;

            // allow each player to make a move, checking for end state or limit
            for (CurrentBoard.TurnNumber = 1; CurrentBoard.TurnNumber <= rules.MaxPhysicalTurns; CurrentBoard.TurnNumber++)
            {
                // have the player decide on a move
                Move playersMove = PlayersTurn.chooseMove(this);

                // automatic forfeit if player is unable to make a move
                if (playersMove == null)
                    rules.PlayerOrder.Remove(PlayersTurn);
                else
                    CurrentBoard.applyMove(playersMove); // apply the move

                // add the move to the current game's seqence of moves
                MoveSequence.Add(CurrentBoard.TurnNumber, playersMove);

                if (rules.LoggingSettings.showMovePerTurn)
                    Console.WriteLine(playersMove?.ToString());
                
                if (rules.LoggingSettings.showStatePerTurn)
                    Console.WriteLine(CurrentBoard.ToString());

                if (rules.LoggingSettings.pausePerMove)
                {
                    Console.WriteLine("Press <enter> to continue...");
                    Console.ReadLine();
                }

                // check if after the move if any players are remaning
                activePlayers = CurrentBoard.TerminalStateCheck(rules);

                if (activePlayers.Count <= 1) // no win scenario is possible, so activePlayers might be 0
                    break;
            }

            watch.Stop();

            if (rules.LoggingSettings.listMoveSeqenceAtEnd)
            {
                Console.WriteLine("Move Sequence:");
                foreach (KeyValuePair<int, Move> sequence in MoveSequence)
                    Console.WriteLine($"Turn {sequence.Key} = {sequence.Value.ToString()}");
            }

            return new GameResults()
            {
                turnsElapsed = CurrentBoard.TurnNumber,
                Winners = activePlayers,
                timeElapsed = watch.Elapsed,
            };
        }

        

        public class GameResults
        {
            public List<Player> Winners;
            public int turnsElapsed;
            public TimeSpan timeElapsed;
        }

}




}
