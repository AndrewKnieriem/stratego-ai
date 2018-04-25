using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCore
{
    public static class GameModes
    {
        public static Game FullNewStratego()
        {
            LocationType _____Space = new LocationType();
            LocationType plyr1Space = new LocationType()
            {
                StarterPlace = playerOne,
            };
            LocationType plyr2Space = new LocationType()
            {
                StarterPlace = playerTwo,
            };

            LocationType waterSpace = new LocationType()
            {
                Passable = false,
                Standable = false,
            };

            int pieceCount = 0;
            List<Piece> pieces = new List<Piece>();

            #region Board Setup

            var Arsenal = new List<GameRules.Arsenal>()
            {
                // https://en.wikipedia.org/wiki/Stratego#Pieces
                // min, max, and start define the range of pieces a player can place to start the game
                new GameRules.Arsenal(0, 1, 1, playerOne, pieceTypeFlag),
                new GameRules.Arsenal(0, 6, 6, playerOne, pieceTypeBomb),
                new GameRules.Arsenal(0, 1, 1, playerOne, pieceTypeSpy),
                new GameRules.Arsenal(0, 8, 8, playerOne, pieceTypeScout),
                new GameRules.Arsenal(0, 5, 5, playerOne, pieceTypeMiner),
                new GameRules.Arsenal(0, 4, 4, playerOne, pieceType4),
                new GameRules.Arsenal(0, 4, 4, playerOne, pieceType4),
                new GameRules.Arsenal(0, 4, 4, playerOne, pieceType6),
                new GameRules.Arsenal(0, 3, 3, playerOne, pieceType7),
                new GameRules.Arsenal(0, 2, 2, playerOne, pieceType8),
                new GameRules.Arsenal(0, 1, 1, playerOne, pieceType9),
                new GameRules.Arsenal(0, 1, 1, playerOne, pieceTypeM),

                new GameRules.Arsenal(0, 1, 1, playerTwo, pieceTypeFlag),
                new GameRules.Arsenal(0, 6, 6, playerTwo, pieceTypeBomb),
                new GameRules.Arsenal(0, 1, 1, playerTwo, pieceTypeSpy),
                new GameRules.Arsenal(0, 8, 8, playerTwo, pieceTypeScout),
                new GameRules.Arsenal(0, 5, 5, playerTwo, pieceTypeMiner),
                new GameRules.Arsenal(0, 4, 4, playerTwo, pieceType4),
                new GameRules.Arsenal(0, 4, 4, playerTwo, pieceType4),
                new GameRules.Arsenal(0, 4, 4, playerTwo, pieceType6),
                new GameRules.Arsenal(0, 3, 3, playerTwo, pieceType7),
                new GameRules.Arsenal(0, 2, 2, playerTwo, pieceType8),
                new GameRules.Arsenal(0, 1, 1, playerTwo, pieceType9),
                new GameRules.Arsenal(0, 1, 1, playerTwo, pieceTypeM),
            };

            var boardlayout = new[,]
            {
                // x, y     so first row is actually the first column
                {plyr1Space, plyr1Space, plyr1Space, plyr1Space, _____Space, _____Space, plyr2Space, plyr2Space, plyr2Space, plyr2Space},
                {plyr1Space, plyr1Space, plyr1Space, plyr1Space, _____Space, _____Space, plyr2Space, plyr2Space, plyr2Space, plyr2Space},
                {plyr1Space, plyr1Space, plyr1Space, plyr1Space, waterSpace, waterSpace, plyr2Space, plyr2Space, plyr2Space, plyr2Space},
                {plyr1Space, plyr1Space, plyr1Space, plyr1Space, waterSpace, waterSpace, plyr2Space, plyr2Space, plyr2Space, plyr2Space},
                {plyr1Space, plyr1Space, plyr1Space, plyr1Space, _____Space, _____Space, plyr2Space, plyr2Space, plyr2Space, plyr2Space},
                {plyr1Space, plyr1Space, plyr1Space, plyr1Space, _____Space, _____Space, plyr2Space, plyr2Space, plyr2Space, plyr2Space},
                {plyr1Space, plyr1Space, plyr1Space, plyr1Space, waterSpace, waterSpace, plyr2Space, plyr2Space, plyr2Space, plyr2Space},
                {plyr1Space, plyr1Space, plyr1Space, plyr1Space, waterSpace, waterSpace, plyr2Space, plyr2Space, plyr2Space, plyr2Space},
                {plyr1Space, plyr1Space, plyr1Space, plyr1Space, _____Space, _____Space, plyr2Space, plyr2Space, plyr2Space, plyr2Space},
                {plyr1Space, plyr1Space, plyr1Space, plyr1Space, _____Space, _____Space, plyr2Space, plyr2Space, plyr2Space, plyr2Space},
            };

            // populate the piece colleciton with the rule's number of pieces
            foreach (GameRules.Arsenal ars in Arsenal)
            {
                for (int i = 0; i < ars.CountStart; i++)
                {
                    pieces.Add(new Piece(-1, -1, ars.Owner, ars.Type)
                    {
                        pos = Piece.removedPos // ensure unplaced pieces are flagged correctly
                    });
                }
            }

            Random rand = new Random();

            int countPlaced = 0;
            // place the pieces on the board
            for (int x = 0; x < 10; x++)
            for (int y = 0; y < 10; y++)
            {
                var unplacedPieces = pieces.Where(q => q.Owner == boardlayout[x, y].StarterPlace && q.pos.X < 0).ToList();

                // if the selected board location does not belong to a player
                if (unplacedPieces.Count == 0)
                    continue;

                // get a random piece from the collection that has not been placed yet
                Piece randomPiece = unplacedPieces.ElementAt(rand.Next(0, unplacedPieces.Count));
                randomPiece.pos = new CoordAbs(x, y);
                countPlaced++;
            }

            // no piece should be remaining off the board
            System.Diagnostics.Debug.Assert(!pieces.Any(q => q.pos.X < 0));

            #endregion

            GameRules rules = new GameRules(

                    _arsenal: Arsenal,

                    _players: new List<Player>()
                    {
                        playerOne,
                        playerTwo
                    },

                    _startingBoard: new Board()
                    {
                        Height = 10,
                        Width = 10,
                        LocationsLayout = boardlayout,
                        PiecesLayout = new Piece[10, 10],
                        PieceSet = pieces,


                    }
                )
            {
                // Function to determine if a player automatically wins (true), lost (false), or is still active in game (null)
                TerminalStateFunction = (theRules, board, player) =>
                {
                    bool? result = null;
                    string reason = "unknown";

                    // over turn limit is a loss - used for turn planning
                    if (board.TurnNumber >= theRules.MaxPhysicalTurns)
                    {
                        reason = "Exceeded maximum turns";
                        result = false;
                    }

                    // game is over if player lost their flag
                    if (!board.PieceSet.Exists(x => x.Type == pieceTypeFlag
                                                && x.pos != Piece.removedPos
                                                && x.Owner == player))
                    {
                        reason = "Lost flag";
                        result = false;
                    }

                    // if we lost all our movable pieces
                    bool lostAllPieces = board.PieceSet.Where(x => x.Owner == player
                                                               && x.Type.Movable)
                    .All(x => x.pos == Piece.removedPos);
                    if (lostAllPieces)
                    {
                        reason = "Lost all mobile pieces";
                        result = false;
                    }

                    if (theRules.LoggingSettings.winLossReasons)
                    {
                        if (result == true)
                            Console.WriteLine(player.FriendlyName + " won ... " + reason);
                        if (result == false)
                            Console.WriteLine(player.FriendlyName + " lost ... " + reason);
                    }

                    return result; // otherwise still in game
                },

            };
            

            Game game = new Game()
            {
                GameNumber = 1,
                rules = rules,
                CurrentBoard = rules.InitialBoard, // need to ensure it makes a deep copy
            };

            return game;
        }


        



        #region Common components

        public static Player playerOne = new Player()
        {
            FriendlyName = "Player 1",
            FriendlySymbol = "+",
            Controller = new Controllers.MonteCarloController(10),
        };

        public static Player playerTwo = new Player()
        {
            FriendlyName = "Player 2",
            FriendlySymbol = "-",
            Controller = new Controllers.RandomController(),
        };


        

        public static PieceType pieceTypeScout = new PieceType("Scout", 2, "2")
        {
            PossibleMovementFunction = (board) =>
            {
                // switch axis if a path becomes blocked, scout cannot hop over any other piece

                var moves = new List<CoordRel>();
                for (int axis = 0; axis < 3; axis++)
                    for (int i = 1; i <= 10; i++)
                    {
                        switch (axis)
                        {
                            case 0:
                                moves.Add(new CoordRel(0, i));
                                break;
                            case 1:
                                moves.Add(new CoordRel(0, i * -1));
                                break;
                            case 2:
                                moves.Add(new CoordRel(i, 0));
                                break;
                            case 3:
                                moves.Add(new CoordRel(i * -1, 0));
                                break;
                        }
                    }


                return moves;
            },
        };
        public static PieceType pieceTypeMiner = new PieceType("Miner", 3, "3");
        public static PieceType pieceType4 = new PieceType("Sergeant", 4, "4");
        public static PieceType pieceType5 = new PieceType("Lieutenant", 5, "5");
        public static PieceType pieceType6 = new PieceType("Captain", 6, "6");
        public static PieceType pieceType7 = new PieceType("Major", 7, "7");
        public static PieceType pieceType8 = new PieceType("Colonel", 8, "8");
        public static PieceType pieceType9 = new PieceType("General", 9, "9");
        public static PieceType pieceTypeM = new PieceType("Marshall", 10, "M");
        public static PieceType pieceTypeFlag = new PieceType("Flag", 0, "F")
        {
            Movable = false,
        };
        public static PieceType pieceTypeSpy = new PieceType("Spy", 1, "S");
        public static PieceType pieceTypeBomb = new PieceType("Bomb", 11, "B")
        {
            Movable = false,
        };

        // placeholder piece
        public static PieceType pieceType1 = new PieceType("Pawn", 1, "1");

        // for now custom attack actions are handled in the rules BattleFunction instead of piece types
        // since these rules would apply to both attackers or defenders and would have to be copied to all
        public static Func<Piece, Piece, GameRules.MoveOutcomes> FullBattleFunction = (movingPiece, opponentPiece) =>
        {
            GameRules.MoveOutcomes outcome = GameRules.MoveOutcomes.Unknown;




            // special bomb rules
            if (opponentPiece?.Type == pieceTypeBomb)
            {
                if (movingPiece.Type == pieceTypeMiner)
                {
                    // in small games this happens a lot, oddly with full it hardly ever does
                    return GameRules.MoveOutcomes.Win;
                }
                else
                    return GameRules.MoveOutcomes.Lose;
            }

            // special spy rules (notice AFTER the bomb rules)
            if (movingPiece?.Type == pieceTypeSpy)
                return GameRules.MoveOutcomes.Win;
            if (opponentPiece?.Type == pieceTypeSpy)
                return GameRules.MoveOutcomes.Win;


            // use the default mechanics otherwise
            return GameRules.VanillaBattleFunction(movingPiece, opponentPiece);
        };


        #endregion


        [Obsolete("Classic stratego has all the rules, and appears to perform fast enough")]
        public static Game tinyStratego()
        {

            LocationType _____Space = new LocationType();


            GameRules rules = new GameRules(

                _arsenal: new List<GameRules.Arsenal>()
                {
                    // min, max, and start define the range of pieces a player can place to start the game
                    new GameRules.Arsenal(0, 1, 1, playerOne, pieceTypeBomb),
                    new GameRules.Arsenal(0, 1, 1, playerOne, pieceTypeMiner),
                    new GameRules.Arsenal(0, 1, 1, playerOne, pieceTypeFlag),

                    new GameRules.Arsenal(0, 1, 1, playerTwo, pieceTypeBomb),
                    new GameRules.Arsenal(0, 1, 1, playerTwo, pieceTypeMiner),
                    new GameRules.Arsenal(0, 1, 1, playerTwo, pieceTypeFlag),
                },

                _players: new List<Player>()
                {
                    playerOne,
                    playerTwo
                },

                _startingBoard: new Board()
                {
                    Height = 3,
                    Width = 3,
                    LocationsLayout = new[,]
                    {
                        // x, y     so first row is actually the first column
                        {_____Space, _____Space, _____Space},
                        {_____Space, _____Space, _____Space},
                        {_____Space, _____Space, _____Space},
                    },
                    PiecesLayout = new Piece[3, 3],
                    PieceSet = new List<Piece>
                    {
                        /*
                         * [      ,  -2  ,  -f ]
                         * [  +1  ,      ,  -1 ]
                         * [  +f  ,  +2  ,     ]
                         */
                        new Piece(0, 0, playerOne, pieceTypeFlag),
                        new Piece(1, 0, playerOne, pieceTypeMiner),
                        new Piece(0, 1, playerOne, pieceTypeBomb),

                        new Piece(2, 1, playerTwo, pieceTypeBomb),
                        new Piece(1, 2, playerTwo, pieceTypeMiner),
                        new Piece(2, 2, playerTwo, pieceTypeFlag)
                    }
                }
            )
            {
                // Function to determine if a player automatically wins (true), lost (false), or is still active in game (null)
                TerminalStateFunction = (theRules, board, player) =>
                {
                    bool? result = null;
                    string reason = "unknown";

                    // over turn limit is a loss - used for turn planning
                    if (board.TurnNumber >= theRules.MaxPhysicalTurns)
                    {
                        reason = "Exceeded maximum turns";
                        result = false;
                    }

                    // game is over if player lost their flag
                    if (!board.PieceSet.Exists(x => x.Type == pieceTypeFlag
                                                    && x.pos != Piece.removedPos
                                                    && x.Owner == player))
                    {
                        reason = "Lost flag";
                        result = false;
                    }

                    // if we lost all our movable pieces
                    bool lostAllPieces = board.PieceSet.Where(x => x.Owner == player
                                                                   && x.Type.Movable)
                        .All(x => x.pos == Piece.removedPos);
                    if (lostAllPieces)
                    {
                        reason = "Lost all mobile pieces";
                        result = false;
                    }

                    if (theRules.LoggingSettings.winLossReasons)
                    {
                        if (result == true)
                            Console.WriteLine(player.FriendlyName + " won ... " + reason);
                        if (result == false)
                            Console.WriteLine(player.FriendlyName + " lost ... " + reason);
                    }

                    return result; // otherwise still in game
                },
                BattleFunction = FullBattleFunction,
            };


            Game game = new Game()
            {
                GameNumber = 1,
                rules = rules,
                CurrentBoard = new Board(rules.InitialBoard),
            };

            return game;
        }
    }
}
