using ReseauxServeur;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace reseauxServeur
{
    public class Game
    {
        private Board board;
        private Player humanPlayer, computerPlayer, activePlayer;
        private IODevice iODevice;

        public static Game CreateConsoleGame(DifficultyLevel difficultyLevel, ActivePlayer colorOrdenador, bool computerHasFirstTurn)
        {
            return new ConsoleGame(difficultyLevel, colorOrdenador, computerHasFirstTurn, IODevice.CreateConsoleDevice());
        }

        private Game(DifficultyLevel difficultyLevel, ActivePlayer computerColor, bool computerHasFirstTurn, IODevice iODevice)
        {
            board = Board.EmptyBoard;
            this.iODevice = iODevice;
        }

        //public ActivePlayer ActivePlayerColor { get { return this.activePlayer.Color; } }
        public Board Board { get { return board; } }
        public IODevice UserInterface { get { return iODevice; } }

        private void changeActivePlayer()
        {
            if (activePlayer == humanPlayer)
            {
                activePlayer = computerPlayer;
            }
            else
            {
                activePlayer = humanPlayer;
            }
        }

        public virtual void Play()
        {
            activePlayer.RequestMove(board);
        }

        private class ConsoleGame : Game
        {
            public ConsoleGame(DifficultyLevel difficultyLevel, ActivePlayer computerColor, bool computerPlaysFirst, IODevice iODevice)
                : base(difficultyLevel, computerColor, computerPlaysFirst, iODevice)
            {
                computerPlayer = Player.CreateComputerPlayer(computerColor, difficultyLevel, iODevice);
                humanPlayer = Player.CreateHumanPlayer(computerColor == ActivePlayer.Player ? ActivePlayer.User : ActivePlayer.Player, iODevice);

                if (computerPlaysFirst)
                {
                    activePlayer = computerPlayer;
                }
                else
                {
                    activePlayer = humanPlayer;
                }

                this.iODevice = iODevice;
            }

            public override void Play()
            {
                while (true)
                {
                    iODevice.Output("");
                    iODevice.Output(board.ToString());
                    iODevice.Output("");

                    int move = activePlayer.RequestMove(board);

                    if (!board.MakePlay(activePlayer.Color, move, out board))
                    {
                        iODevice.Output("Row is full. Try again.");
                        continue;
                    }

                    if (Judge.CheckForVictory(activePlayer.Color, board))
                    {
                        iODevice.Output(board.DrawStringBuilder(move));
                        iODevice.Output("");

                        if (activePlayer == computerPlayer)
                        {
                            iODevice.Accept("I'm sorry player {0}. I won again...", humanPlayer.Color);
                        }
                        else
                        {
                            iODevice.Accept("Congratulations player {0}! ¡You won!", humanPlayer.Color);
                        }
                        break;
                    }

                    if (board.NumberOfEmptyCells == 0)
                    {
                        iODevice.Output(board.DrawStringBuilder(move));
                        iODevice.Output("");
                        iODevice.Accept("¡Draw! I didnt loose...again");
                        break;
                    }

                    changeActivePlayer();
                }
            }
        }
    }

    }
