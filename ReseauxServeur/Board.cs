using MsgPack.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace reseauxServeur

{

    public sealed class Board
    {
        public const int BoardRows = 6, BoardColumns = 7;
        public static readonly Board EmptyBoard = new Board();
        public static List<string> finalpick = new List<string>();

        private readonly CellStates[,] cells;
        private readonly int numberOfEmptyCells;

        public Board()
        {
            cells = new CellStates[BoardRows, BoardColumns];
            numberOfEmptyCells = BoardRows * BoardColumns;
        }
        public static void SendingPacket(StringBuilder packet)
        {
            IPAddress ipLocal;
            IPEndPoint ipeLocal;
            TcpListener List;
            TcpClient tc = null;
            NetworkStream ns = null;
            string s2;

            int n = 0;
            try
            {
                ipLocal = IPAddress.Parse("127.0.0.1");
                ipeLocal = new IPEndPoint(ipLocal, 8888);
                List = new TcpListener(ipeLocal);


                List.Start();
                tc = List.AcceptTcpClient();

                ns = tc.GetStream();
                StreamReader reader = new StreamReader(ns);
                StreamWriter write = new StreamWriter(ns);
                string response = null;
                //Console.WriteLine(response);


                response = reader.ReadLine();
                Console.WriteLine(response);
                Thread.Sleep(200);
                write.WriteLine(packet);
                write.Flush();
                n++;





                Console.WriteLine("j'envoie");
                Console.WriteLine("de player");
                Console.ReadKey();
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
            finally
            {
                if ((tc != null) && (ns != null))
                {
                    tc.Close();
                    ns.Close();

                }

            }
        }


        public string DrawStringBuilder(int nk)
        {
            var mybuilder = new StringBuilder();

            List<string> mypick = new List<string>();

            foreach (var item in chosingColorRandomly())
            {
                mypick.Add(item);
            }
            foreach (var item in mypick)
            {
                finalpick.Add(item);
            }


            CellStates[,] cells = new CellStates[6, 7];

            var builder = new StringBuilder();
            var header = "  0        1         2         3        4        5         6";
            var divisor = "-------------------------------------------------------------------";
            builder.AppendLine(header);
            builder.AppendLine(divisor);

            int p = 0;
            for (int i = 0; i < cells.GetLength(0); i++)
            {

                for (int j = 0; j < cells.GetLength(1); j++)
                {

                    int counter1 = 0;
                    int counter2 = 1;
                    cells[5, nk] = CellStates.Player;
                    //cells[5, 0] = CellStates.User;
                    var str = cells[i, j] == CellStates.Empty ? "| ······· " : (cells[i, j] == CellStates.User ? "|     0   " : "|   x     ");

                    builder.Append(str);
                    counter1 = counter1 + 2;
                    counter2++;

                }



                builder.Append('|');
                builder.AppendLine();
                builder.AppendLine(divisor);



            }
            SendingPacket(builder);
            return builder.ToString(0, builder.Length - 1);
        }

        public Board(Board board, int numberOfEmptyCells)
        {
            if (board == null)
                throw new ArgumentNullException("board");

            if (numberOfEmptyCells < 0 || numberOfEmptyCells > BoardRows * BoardColumns)
                throw new ArgumentOutOfRangeException("numberOfEmptyCells");

            cells = new CellStates[BoardRows, BoardColumns];

            if (board != null)
            {
                for (int i = 0; i < BoardRows; i++)
                {
                    for (int j = 0; j < BoardColumns; j++)
                    {
                        cells[i, j] = board.cells[i, j];
                    }
                }
            }

            this.numberOfEmptyCells = numberOfEmptyCells;
        }

        public int NumberOfEmptyCells
        {
            get
            {
                return numberOfEmptyCells;
            }
        }
        public T Deserialize<T>(byte[] bytes)
        {
            var serializer = MessagePackSerializer.Get<T>();
            using (var byteStream = new MemoryStream(bytes))
            {
                return serializer.Unpack(byteStream);
            }
        }
        public string Getpack()
        {
            IPAddress ipServeur;
            IPEndPoint ipeServeur;
            TcpClient tc = null;
            NetworkStream ns = null;
            ipServeur = IPAddress.Parse("127.0.0.1");
            ipeServeur = new IPEndPoint(ipServeur, 8888);
            tc = new TcpClient();

            tc.Connect(ipeServeur);
            ns = tc.GetStream();





            string reponse = null;
            StreamWriter write = new StreamWriter(ns);
            StreamReader reader = new StreamReader(ns);
            //Console.WriteLine("un string svp");
            //s = Console.ReadLine();
            write.Flush();
            // write.WriteLine(s1);
            write.Flush();



            do
            {
                reponse = reader.ReadLine();
                return reponse;
            } while (true);


        }


        public CellStates GetCellState(int row, int column)
        {
            if (row < 0 || row >= BoardRows)
                throw new ArgumentOutOfRangeException("row");

            if (column < 0 || column >= BoardColumns) throw new ArgumentOutOfRangeException("column");

            return cells[row, column];
        }

        public bool MakePlay(ActivePlayer player, int column, out Board board)
        {
            if (column < 0 || column >= BoardColumns) throw new ArgumentOutOfRangeException("column");

            if (cells[0, column] != CellStates.Empty)
            {
                board = this;
                return false;
            }

            board = new Board(this, numberOfEmptyCells - 1);

            int i;

            for (i = BoardRows - 1; i > -1; i--)
            {
                if (cells[i, column] == CellStates.Empty)
                    break;
            }

            board.cells[i, column] = (CellStates)player;
            return true;
        }
        public List<string> chosingColorRandomly()
        {
            List<string> chosencolors = new List<string>();
            Random rnd = new Random();
            List<string> mycolorsList = new List<string>();
            mycolorsList.Add("   red   ");
            mycolorsList.Add(" yellow  ");
            mycolorsList.Add("  blue   ");
            mycolorsList.Add("  green  ");
            mycolorsList.Add("  white  ");
            mycolorsList.Add("  orange ");
            mycolorsList.Add("  purple ");
            mycolorsList.Add("  grey   ");

            int firstchosendcolor = rnd.Next(mycolorsList.Count);
            int secondchosencolor = rnd.Next(mycolorsList.Count);
            if (firstchosendcolor == secondchosencolor)
            {
                secondchosencolor = rnd.Next(mycolorsList.Count);

            }
            chosencolors.Add((string)mycolorsList[firstchosendcolor]);
            chosencolors.Add((string)mycolorsList[secondchosencolor]);
            return chosencolors;
        }
        //[DebuggerStepThrough]

        public override string ToString()
        {
            var mybuilder = new StringBuilder();

            List<string> mypick = new List<string>();

            foreach (var item in chosingColorRandomly())
            {
                mypick.Add(item);
            }
            foreach (var item in mypick)
            {
                finalpick.Add(item);
            }


            var builder = new StringBuilder();
            var header = "  0        1         2         3        4        5         6";
            var divisor = "------------------------------------------------------------------------";
            builder.AppendLine(header);
            builder.AppendLine(divisor);

            int p = 0;
            for (int i = 0; i < cells.GetLength(0); i++)
            {

                for (int j = 0; j < cells.GetLength(1); j++)
                {

                    int counter1 = 0;
                    int counter2 = 1;

                    var str = cells[i, j] == CellStates.Empty ? "| ······· " : (cells[i, j] == CellStates.Player ? "| " + mypick[counter1] : "|" + mypick[counter2]);

                    builder.Append(str);
                    counter1 = counter1 + 2;
                    counter2++;

                }

                builder.Append('|');
                builder.AppendLine();
                builder.AppendLine(divisor);


            }
            Console.WriteLine(" it's user 's turn and his next color will be {0}", "" + mypick[1]);
            //if (Getpack() != null) builder.Append(Getpack());

            return builder.ToString(0, builder.Length - 1);
        }
    }

    public enum CellStates
    {
        Empty = 0,
        Player = ActivePlayer.Player,
        User = ActivePlayer.User
    }
    public enum ActivePlayer
    {
        User = 1,
        Player = 2
    }

}