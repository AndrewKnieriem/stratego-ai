using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCore
{
    public static class GameModes
    {
        public static Game classicStratego()
        {
            Player playerHuman = new Player()
            {
                FriendlyName = "Human",
                FriendlySymbol = "H",
            };

            Player playerComputer = new Player()
            {
                FriendlyName = "CPU",
                FriendlySymbol = "C",
            };

            PieceType pieceTypePawn = new PieceType()
            {
                Name = "Pawn",
                Rank = 1,
                SymbolForBoard = "1",
            };

            LocationType _____Space = new LocationType();
            LocationType waterSpace = new LocationType()
            {
                Passable = false,
                Standable = false,
            };

            int pieceCount = 0;
            List<Piece> pieces = new List<Piece>();

            // populate the human player's pieces
            for (int y = 6; y < 10; y++)
                for (int x = 0; x < 10; x++)
                {
                    pieces.Add(new Piece(x, y)
                    {
                        IsRevealed = false,
                        Owner = playerHuman,
                        Type = pieceTypePawn,
                    });
                    pieceCount++;
                }

            // populate the computer player's pieces
            for (int y = 0; y < 4; y++)
                for (int x = 0; x < 10; x++)
                {
                    pieces.Add(new Piece(x, y)
                    {
                        IsRevealed = false,
                        Owner = playerComputer,
                        Type = pieceTypePawn,
                    });
                    pieceCount++;
                }

            GameRules rules = new GameRules(

                _arsenal: new List<GameRules.Arsenal>()
                {
                    new GameRules.Arsenal()
                    {
                        CountMax = 20,
                        CountMin = 0,
                        CountStart = 20,
                        Owner = playerHuman,
                        Type = pieceTypePawn,
                    },
                    new GameRules.Arsenal()
                    {
                        CountMax = 20,
                        CountMin = 0,
                        CountStart = 20,
                        Owner = playerComputer,
                        Type = pieceTypePawn,
                    }
                },

                _players: new List<Player>()
                {
                    playerHuman,
                    playerComputer
                },

                _startingBoard: new Board()
                {
                    Height = 10,
                    Width = 10,
                    LocationsLayout = new[,]
                    {
                        // x, y     so first row is actually the first column
                        {_____Space, _____Space, _____Space, _____Space, _____Space, _____Space, _____Space, _____Space, _____Space, _____Space},
                        {_____Space, _____Space, _____Space, _____Space, _____Space, _____Space, _____Space, _____Space, _____Space, _____Space},
                        {_____Space, _____Space, _____Space, _____Space, waterSpace, waterSpace, _____Space, _____Space, _____Space, _____Space},
                        {_____Space, _____Space, _____Space, _____Space, waterSpace, waterSpace, _____Space, _____Space, _____Space, _____Space},
                        {_____Space, _____Space, _____Space, _____Space, _____Space, _____Space, _____Space, _____Space, _____Space, _____Space},
                        {_____Space, _____Space, _____Space, _____Space, _____Space, _____Space, _____Space, _____Space, _____Space, _____Space},
                        {_____Space, _____Space, _____Space, _____Space, waterSpace, waterSpace, _____Space, _____Space, _____Space, _____Space},
                        {_____Space, _____Space, _____Space, _____Space, waterSpace, waterSpace, _____Space, _____Space, _____Space, _____Space},
                        {_____Space, _____Space, _____Space, _____Space, _____Space, _____Space, _____Space, _____Space, _____Space, _____Space},
                        {_____Space, _____Space, _____Space, _____Space, _____Space, _____Space, _____Space, _____Space, _____Space, _____Space},
                    },
                    PiecesLayout = new Piece[10, 10],
                    PieceSet = pieces,


                }
            );


            Game game = new Game()
            {
                GameNumber = 1,
                rules = rules,
                CurrentBoard = rules.InitialBoard, // need to ensure it makes a deep copy
            };

            return game;
        }
        
        public static Game tinyStratego()
        {
            
            LocationType _____Space = new LocationType();


            GameRules rules = new GameRules(

                _arsenal: new List<GameRules.Arsenal>()
                {
                    // min, max, and start define the range of pieces a player can place to start the game
                    new GameRules.Arsenal(0, 1, 1, playerHuman, pieceTypePawn1),
                    new GameRules.Arsenal(0, 1, 1, playerHuman, pieceTypePawn2),
                    new GameRules.Arsenal(0, 1, 1, playerHuman, pieceTypeFlag),

                    new GameRules.Arsenal(0, 1, 1, playerComputer, pieceTypePawn1),
                    new GameRules.Arsenal(0, 1, 1, playerComputer, pieceTypePawn2),
                    new GameRules.Arsenal(0, 1, 1, playerComputer, pieceTypeFlag),
                },

                _players: new List<Player>()
                {
                    playerHuman,
                    playerComputer
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
                        new Piece(0, 0, playerHuman, pieceTypeFlag),
                        new Piece(1, 0, playerHuman, pieceTypePawn2),
                        new Piece(0, 1, playerHuman, pieceTypePawn1),

                        new Piece(2, 1, playerComputer, pieceTypePawn1),
                        new Piece(1, 2, playerComputer, pieceTypePawn2),
                        new Piece(2, 2, playerComputer, pieceTypeFlag)
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
            };


            Game game = new Game()
            {
                GameNumber = 1,
                rules = rules,
                CurrentBoard = new Board(rules.InitialBoard),
            };

            return game;
        }






        #region Common components
        public static Player playerHuman = new Player()
        {
            FriendlyName = "Human",
            FriendlySymbol = "+",
            Controller = new Controllers.RandomController(),
        };

        public static Player playerComputer = new Player()
        {
            FriendlyName = "CPU",
            FriendlySymbol = "-",
            Controller = new Controllers.RandomController(),
        };

        public static PieceType pieceTypePawn1 = new PieceType()
        {
            Name = "Pawn1",
            Rank = 1,
            SymbolForBoard = "1",
        };

        public static PieceType pieceTypePawn2 = new PieceType()
        {
            Name = "Pawn2",
            Rank = 2,
            SymbolForBoard = "2",
        };

        public static PieceType pieceTypeFlag = new PieceType()
        {
            Name = "Flag",
            Rank = 0,
            SymbolForBoard = "F",
            Movable = false,
        };
        #endregion
    }
}
