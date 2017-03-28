using MsgPack.Serialization;
using reseauxServeur;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReseauxServeur
{
    internal sealed class AIEngine
    {
        readonly int maximumDepth;
        readonly Random random;
        public readonly CellStates[,] cells;

        public AIEngine(DifficultyLevel difficultyLevel)
        {
            this.maximumDepth = (int)difficultyLevel;

            if (maximumDepth < (int)DifficultyLevel.Easy ||
                maximumDepth > (int)DifficultyLevel.Hard)
                throw new ArgumentOutOfRangeException("difficultyLevel");

            this.random = new Random(DateTime.Now.Millisecond);
        }

        public int GetBestMove(Board board, ActivePlayer player)
        {
            if (board == null)
                throw new ArgumentNullException("board");

            var node = new Node(board);
            var possibleMoves = getPossibleMoves(node);
            var scores = new double[possibleMoves.Count];
            Board updatedBoard;

            for (int i = 0; i < possibleMoves.Count; i++)
            {
                board.MakePlay(player, possibleMoves[i], out updatedBoard);
                var variant = new Node(updatedBoard);
                createTree(getOpponent(player), variant, 0);
                scores[i] = scoreNode(variant, player, 0);
            }

            double maximumScore = double.MinValue;
            var goodMoves = new List<int>();

            for (int i = 0; i < scores.Length; i++)
            {
                if (scores[i] > maximumScore)
                {
                    goodMoves.Clear();
                    goodMoves.Add(i);
                    maximumScore = scores[i];
                }
                else if (scores[i] == maximumScore)
                {
                    goodMoves.Add(i);
                }
            }

            return possibleMoves[goodMoves[random.Next(0, goodMoves.Count)]];
        }

        private List<int> getPossibleMoves(Node node)
        {
            var moves = new List<int>();

            for (int i = 0; i < Board.BoardColumns; i++)
            {
                if (node.Board.GetCellState(0, i) == CellStates.Empty)
                {
                    moves.Add(i);
                }
            }

            return moves;
        }

        private void createTree(ActivePlayer player, Node rootNode, int depth)
        {
            if (depth >= maximumDepth)
                return;

            var moves = getPossibleMoves(rootNode);

            foreach (var move in moves)
            {
                Board updatedBoard;
                rootNode.Board.MakePlay(player, move, out updatedBoard);
                var variantNode = new Node(updatedBoard);
                createTree(getOpponent(player), variantNode, depth + 1);
                rootNode.Variants.Add(variantNode);
            }
        }

        private double scoreNode(Node nodo, ActivePlayer player, int depth)
        {
            double score = 0;

            if (Judge.CheckForVictory(player, nodo.Board))
            {
                if (depth == 0)
                {
                    score = double.PositiveInfinity;
                }
                else
                {
                    score += Math.Pow(10.0, maximumDepth - depth);
                }
            }
            else if (Judge.CheckForVictory(getOpponent(player), nodo.Board))
            {
                score += -Math.Pow(100
                    , maximumDepth - depth);
            }
            else
            {
                foreach (var varianteContrincante in nodo.Variants)
                {
                    score += scoreNode(varianteContrincante, player, depth + 1);
                }
            }

            return score;
        }

        private static ActivePlayer getOpponent(ActivePlayer jugador)
        {
            return jugador == ActivePlayer.Player ? ActivePlayer.User : ActivePlayer.Player;
        }

        private class Node
        {
            readonly Board board;
            readonly List<Node> variants;

            public Board Board { get { return board; } }
            public List<Node> Variants { get { return variants; } }

            public Node(Board tablero)
            {
                this.board = tablero;
                this.variants = new List<Node>();
            }
        }
    }
    public enum DifficultyLevel
    {
        Easy = 1,
        Medium = 3,
        Hard = 4
    }
    class Program
    {
       

        public static void Fill(int col, int row)
        {

        }
        public T Deserialize<T>(byte[] bytes)
        {
            var serializer = MessagePackSerializer.Get<T>();
            using (var byteStream = new MemoryStream(bytes))
            {
                return serializer.Unpack(byteStream);
            }
        }
        public static void RecevingData()
        {
            IPAddress ipServeur;
            IPEndPoint ipeServeur;
            TcpClient tc = null;
           
            
            NetworkStream ns = null;

            try
            {

                ipServeur = IPAddress.Parse("127.0.0.1");
                ipeServeur = new IPEndPoint(ipServeur, 8888);
                tc = new TcpClient();
                tc.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                tc.Client.Bind(ipeServeur);
                tc.Connect(ipeServeur);
                ns = tc.GetStream();



                string s = null;
                string s1 = "ping";
                string reponse = null;
                StreamWriter write = new StreamWriter(ns);
                StreamReader reader = new StreamReader(ns);
               
                write.Flush();
                write.WriteLine(s1);
                write.Flush();
                do
                {
                    reponse = reader.ReadLine();
                    Console.WriteLine(reponse);
                } while (true);

            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }


        }
        private static void ProcessClientRequests(object argument)
        {
            TcpClient client = (TcpClient)argument;
            try
            {
                StreamWriter write = new StreamWriter(client.GetStream());
                StreamReader reader = new StreamReader(client.GetStream());
                string s = String.Empty;
                while (!(s = reader.ReadLine()).Equals("Exit") || (s == null))
                {
                   
                    Console.WriteLine("from client: " + s);
                    write.WriteLine("from server :" + s);
                    write.Flush();
                }
                reader.Close();
                write.Close();
                client.Close();
                Console.WriteLine("closing client connection");

            }
            catch (Exception)
            {

                Console.WriteLine("Problem ...");
            }
            finally
            {
                if (client != null)
                {
                    client.Close();
                }
            }

        }
        static void Main(string[] args)
        {
            TcpListener listener = null;
            try
            {
                listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8888);
                listener.Start();
                Console.WriteLine("multi...");
                while (true)
                {
                    Console.WriteLine("client are comming..");
                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine("connected");
                    //this allow several client to connect
                    Thread t = new Thread(ProcessClientRequests);
                    t.Start(client);

                }

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
            //do
            //{

            //    var game = Game.CreateConsoleGame(DifficultyLevel.Hard, ActivePlayer.Player, false);
            //    game.Play(); 
            //    Console.ReadKey(); 
            //}
            //while (true);

            finally
            {
                if (listener != null)
                {
                    listener.Stop();
                }
            }

        }

       

    }
}
