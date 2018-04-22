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
            
            LocationType _____Space = new LocationType();
            LocationType startSpace = new LocationType()
            {
                StarterPlace = true,
            };

            LocationType waterSpace = new LocationType()
            {
                Passable = false,
                Standable = false,
            };

            int pieceCount = 0;
            List<Piece> pieces = new List<Piece>();
            
            var arsenal = new List<GameRules.Arsenal>()
            {
                // min, max, and start define the range of pieces a player can place to start the game
                new GameRules.Arsenal(0, 10, 10, playerOne, pieceType1),
                new GameRules.Arsenal(0, 10, 10, playerOne, pieceType2),
                new GameRules.Arsenal(0, 1, 1, playerOne, pieceTypeFlag),

                new GameRules.Arsenal(0, 10, 1, PlayerTwo, pieceType1),
                new GameRules.Arsenal(0, 10, 1, PlayerTwo, pieceType2),
                new GameRules.Arsenal(0, 1, 1, PlayerTwo, pieceTypeFlag),
            };




            // populate the human player's pieces
            for (int y = 6; y < 10; y++)
                for (int x = 0; x < 10; x++)
                {
                    pieces.Add(new Piece(x, y)
                    {
                        IsRevealed = false,
                        Owner = playerOne,
                        Type = pieceType1,
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
                        Owner = PlayerTwo,
                        Type = pieceType1,
                    });
                    pieceCount++;
                }



            GameRules rules = new GameRules(

                _arsenal: arsenal,

                _players: new List<Player>()
                {
                    playerOne,
                    PlayerTwo
                },

                _startingBoard: new Board()
                {
                    Height = 10,
                    Width = 10,
                    LocationsLayout = new[,]
                    {
                        // x, y     so first row is actually the first column
                        {startSpace, startSpace, startSpace, _____Space, _____Space, _____Space, _____Space, startSpace, startSpace, startSpace},
                        {startSpace, startSpace, startSpace, _____Space, _____Space, _____Space, _____Space, startSpace, startSpace, startSpace},
                        {startSpace, startSpace, startSpace, _____Space, waterSpace, waterSpace, _____Space, startSpace, startSpace, startSpace},
                        {startSpace, startSpace, startSpace, _____Space, waterSpace, waterSpace, _____Space, startSpace, startSpace, startSpace},
                        {startSpace, startSpace, startSpace, _____Space, _____Space, _____Space, _____Space, startSpace, startSpace, startSpace},
                        {startSpace, startSpace, startSpace, _____Space, _____Space, _____Space, _____Space, startSpace, startSpace, startSpace},
                        {startSpace, startSpace, startSpace, _____Space, waterSpace, waterSpace, _____Space, startSpace, startSpace, startSpace},
                        {startSpace, startSpace, startSpace, _____Space, waterSpace, waterSpace, _____Space, startSpace, startSpace, startSpace},
                        {startSpace, startSpace, startSpace, _____Space, _____Space, _____Space, _____Space, startSpace, startSpace, startSpace},
                        {startSpace, startSpace, startSpace, _____Space, _____Space, _____Space, _____Space, startSpace, startSpace, startSpace},
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
                    new GameRules.Arsenal(0, 1, 1, playerOne, pieceType1),
                    new GameRules.Arsenal(0, 1, 1, playerOne, pieceType2),
                    new GameRules.Arsenal(0, 1, 1, playerOne, pieceTypeFlag),

                    new GameRules.Arsenal(0, 1, 1, PlayerTwo, pieceType1),
                    new GameRules.Arsenal(0, 1, 1, PlayerTwo, pieceType2),
                    new GameRules.Arsenal(0, 1, 1, PlayerTwo, pieceTypeFlag),
                },

                _players: new List<Player>()
                {
                    playerOne,
                    PlayerTwo
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
                        new Piece(1, 0, playerOne, pieceType2),
                        new Piece(0, 1, playerOne, pieceType1),

                        new Piece(2, 1, PlayerTwo, pieceType1),
                        new Piece(1, 2, PlayerTwo, pieceType2),
                        new Piece(2, 2, PlayerTwo, pieceTypeFlag)
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
        public static Player playerOne = new Player()
        {
            FriendlyName = "Player 1",
            FriendlySymbol = "+",
            Controller = new Controllers.RandomController(),
        };

        public static Player PlayerTwo = new Player()
        {
            FriendlyName = "Player 2",
            FriendlySymbol = "-",
            Controller = new Controllers.RandomController(),
        };

        public static PieceType pieceType1 = new PieceType("Pawn1", 1, "1");
        public static PieceType pieceType2 = new PieceType("Pawn2", 2, "2");
        public static PieceType pieceType3 = new PieceType("Pawn3", 3, "3");
        public static PieceType pieceType4 = new PieceType("Pawn4", 4, "4");
        public static PieceType pieceType5 = new PieceType("Pawn5", 5, "5");
        public static PieceType pieceType6 = new PieceType("Pawn6", 6, "6");
        public static PieceType pieceType7 = new PieceType("Pawn7", 7, "7");
        public static PieceType pieceType8 = new PieceType("Pawn8", 8, "8");
        public static PieceType pieceType9 = new PieceType("Pawn9", 9, "9");
        public static PieceType pieceTypeM = new PieceType("Marshall", 10, "M");

        public static PieceType pieceTypeFlag = new PieceType("Flag", 0, "F")
        {
            Movable = false,
        };

        #endregion
    }
}
