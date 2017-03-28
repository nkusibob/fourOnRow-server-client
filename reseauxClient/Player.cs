using MsgPack.Serialization;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace reseauxClient
{
    internal class Player
    {
        public byte[] myMove;
        public static Player CreateHumanPlayer(ActivePlayer color, IODevice iODevice)
        {
            return new HumanConsolePlayer(color, iODevice);
        }

        public static Player CreateComputerPlayer(ActivePlayer color, DifficultyLevel difficultyLevel, IODevice iODevice)
        {
            return new ComputerConsolePlayer(color, difficultyLevel, iODevice);
        }

        private readonly ActivePlayer playerStatus;
        private readonly IODevice iODevice;
        public static CellStates[,] cells;
        private Player(ActivePlayer colorJugador, IODevice iODevice)
        {
            if (colorJugador != ActivePlayer.Player && colorJugador != ActivePlayer.User)
                throw new ArgumentOutOfRangeException("playerColor");

            this.playerStatus = colorJugador;
            this.iODevice = iODevice;
        }

        public ActivePlayer Color { get { return playerStatus; } }

        public virtual int RequestMove(Board tablero)
        {
            return -1;
        }

        private class ComputerConsolePlayer : Player
        {
            private readonly AIEngine engine;

            public ComputerConsolePlayer(ActivePlayer color, DifficultyLevel difficulty, IODevice iODevice)
                : base(color, iODevice)
            {
                engine = new AIEngine(difficulty);
            }
           
            public override int RequestMove(Board board)
            {
                Debug.Assert(board != null);

                var move = engine.GetBestMove(board, playerStatus);
                iODevice.Output("player {0}'s turn . Hmmm...I'll play: {1}", playerStatus, move);
                myMove = Serialize(move);
                iODevice.Output("");
                return move;
            }
        }
        public byte[] Serialize<T>(T thisObj)
        {
            var serializer = MessagePackSerializer.Get<T>();

            using (var byteStream = new MemoryStream())
            {
                serializer.Pack(byteStream, thisObj);
                return byteStream.ToArray();
            }
        }

        [SerializableAttribute]
        public class ChosenColorRandomly
        {
            int myMove;
        }
        private class HumanConsolePlayer : Player
        {
            public HumanConsolePlayer(ActivePlayer color, IODevice iOdevice)
                : base(color, iOdevice)
            { }
           
            public override int RequestMove(Board tablero)
            {
                Debug.Assert(tablero != null);

                while (true)
                {
                    var input = (string)iODevice.Request("Player {0} 's turn: ", playerStatus);
                    iODevice.Output("");
                    int jugada = -1;

                    if (int.TryParse(input, out jugada))
                    {
                        if (jugada < 0 || jugada >= Board.BoardColumns)
                        {
                            iODevice.Output("Column number must be within 0 and 6. Try again.");
                            continue;
                        }
                        else
                        {
                            
                            return jugada;
                        }
                    }
                    else
                    {
                        iODevice.Output("'{0}' is not a column number. Try again.", input);
                    }
                }
            }
        }
    }
}