using Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class ServerController
    {
        private List<Player> _playerList;

        public ServerController()
        {
            _playerList = new List<Player>();
        }

        public async void StartServer()
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 7777);
            tcpListener.Start();
            Console.WriteLine("Server started");


            //Server Running
            while (true)
            {
                // If new connection pending accepts it
                if (tcpListener.Pending())
                {
                    Console.WriteLine("New pending connection");
                    await  AsyncAcceptNewTCPClient(tcpListener);
                    //tcpListener.BeginAcceptTcpClient(AcceptTcpClient, tcpListener);
                }

                bool canRestart = false;
                // Else checks for messages
                foreach (Player PlayerToReadMessages in _playerList)
                {

                    if (canRestart) break;
                    switch (PlayerToReadMessages.GameState)
                    {
                        case GameState.Connecting:
                            if (PlayerToReadMessages.DataAvailable())
                            {
                                Console.WriteLine("New player registering");
                                string playerJson = PlayerToReadMessages.BinaryReader.ReadString();
                                Player playerMsg = JsonConvert.DeserializeObject<Player>(playerJson);
                                PlayerToReadMessages.Name = playerMsg.Name;

                                foreach (Player notifyPlayer in _playerList)
                                {
                                    Message msg = new Message();
                                    msg.MessageType = MessageType.NewPlayer;
                                    msg.Description = (notifyPlayer == PlayerToReadMessages) ?
                                        "Successfully joined" :
                                        "Player " + PlayerToReadMessages.Name + " has joined";
                                    PlayerInfo playerInfo = new PlayerInfo();
                                    playerInfo.Id = PlayerToReadMessages.Id;
                                    playerInfo.Name = PlayerToReadMessages.Name;
                                    playerInfo.X = 0;
                                    playerInfo.Y = 0;
                                    playerInfo.Z = 0;
                                    playerInfo.directionX = 0;
                                    playerInfo.directionY = 0;
                                    playerInfo.directionZ = 0;
                                    playerInfo.Score = 0;
                                    playerInfo.BallId = PlayerToReadMessages.BallId;
                                    msg.PlayerInfo = playerInfo;

                                    string msgJson = JsonConvert.SerializeObject(msg);
                                    notifyPlayer.BinaryWriter.Write(msgJson);
                                    notifyPlayer.MessageList.Add(msg);
                                    Console.WriteLine(msgJson);
                                }
                                PlayerToReadMessages.GameState = GameState.Sync;
                            }
                            break;



                        case GameState.Sync:
                            Console.WriteLine("New player sync");
                            // processar todos os NewPlayer
                            SyncNewPlayers(PlayerToReadMessages);
                            // processar todos os PlayerMovement
                            SyncPlayerMovement(PlayerToReadMessages);

                            Message messagePlayer = new Message();
                            messagePlayer.MessageType = MessageType.FinishedSync;

                            PlayerInfo pi = new PlayerInfo();
                            pi.Id = PlayerToReadMessages.Id;
                            pi.BallId = PlayerToReadMessages.BallId;
                            pi.Score = PlayerToReadMessages.Score;

                            messagePlayer.PlayerInfo = pi;

                            string msgPlayerJson = JsonConvert.SerializeObject(messagePlayer);
                            PlayerToReadMessages.BinaryWriter.Write(msgPlayerJson);
                            PlayerToReadMessages.GameState = GameState.GameStarted;
                            break;


                        // Executes during game execution
                        case GameState.GameStarted:

                            if (PlayerToReadMessages.DataAvailable())
                            {

                                // Reading message from player to re send to all others if is for that
                                string msgJson = PlayerToReadMessages.BinaryReader.ReadString();
                                Console.WriteLine(msgJson);


                                Message message = JsonConvert.DeserializeObject<Message>(msgJson);

                                // MANDAR MENSAGEM DE PLAYER INFO MAS COM ENUM DA MESSAGETYPE COM BALLMOVEMENT

                                if (message.MessageType == MessageType.PlayerMovement ||
                                    message.MessageType == MessageType.BallMovement ||
                                    message.MessageType == MessageType.ChangeTurn)

                                {
                                    foreach (Player playerToSend in _playerList)
                                    {
                                        if (playerToSend.GameState == GameState.GameStarted) WriteMessageToPlayer(playerToSend, msgJson);
                                    }
                                }
                                else if (message.MessageType == MessageType.EndGame)
                                {
                                    foreach (Player playerToSend in _playerList)
                                    {
                                        if (playerToSend.GameState == GameState.GameStarted) WriteMessageToPlayer(playerToSend, msgJson);
                                        playerToSend.GameState = GameState.Restarting;
                                    }
                                    canRestart = true;
                                }
                            }
                            break;



                    }
                }

                if (canRestart) _playerList.Clear();
            }
        }

        private void SyncPlayerMovement(Player player)
        {
            foreach (Player p in _playerList)
            {
                if (p.GameState == GameState.GameStarted)
                {
                    var last = p.MessageList.LastOrDefault(
                            m => m.MessageType == MessageType.PlayerMovement);
                    if (last != null)
                    {
                        Message msg = new Message();
                        msg.PlayerInfo = last.PlayerInfo;
                        string jsonMsg = JsonConvert.SerializeObject(msg);
                        player.BinaryWriter.Write(jsonMsg);
                    }
                }
            }


        }

        private void SyncNewPlayers(Player player)
        {
            foreach (Player p in _playerList)
            {
                if (p.GameState == GameState.GameStarted)
                {
                    Message msg = new Message();
                    msg.MessageType = MessageType.NewPlayer;
                    PlayerInfo info = p.MessageList.FirstOrDefault(m =>
                                                m.MessageType == MessageType.NewPlayer).PlayerInfo;
                    msg.PlayerInfo = info;

                    string jsonMsg = JsonConvert.SerializeObject(msg);
                    player.BinaryWriter.Write(jsonMsg);
                }
            }
        }


        private async Task AsyncAcceptNewTCPClient(TcpListener tcpListener)
        {

            TcpClient tcpClient =  await tcpListener.AcceptTcpClientAsync();

            if (tcpClient.Connected)
            {
                Console.WriteLine("Accepted new connection");

                Player player = new Player();
                Message message = new Message();
                message.Description = "Hello new player";
                message.MessageType = MessageType.Information;

                player.MessageList = new List<Message>();
                player.MessageList.Add(message);
                player.TcpClient = tcpClient;
                player.BinaryReader = new System.IO.BinaryReader(tcpClient.GetStream());
                player.BinaryWriter = new System.IO.BinaryWriter(tcpClient.GetStream());
                player.Id = Guid.NewGuid();
                player.BallId = Guid.NewGuid(); //
                player.Score = 0;


                player.GameState = GameState.Connecting;

                _playerList.Add(player);

                string playerJson = JsonConvert.SerializeObject(player);
                Console.WriteLine(playerJson);
                player.BinaryWriter.Write(playerJson);
            }
            else
            {
                Console.WriteLine("Connection refused");
            }
        }

        //private void AcceptTcpClient(IAsyncResult ar)
        //{
        //    TcpListener tcpListener = (TcpListener)ar.AsyncState;
        //    TcpClient tcpClient = tcpListener.EndAcceptTcpClient(ar);
        //    if (tcpClient.Connected)
        //    {
        //        Console.WriteLine("Accepted new connection");

        //        Player player = new Player();
        //        Message message = new Message();
        //        message.Description = "Hello new player";
        //        message.MessageType = MessageType.Information;

        //        player.MessageList = new List<Message>();
        //        player.MessageList.Add(message);
        //        player.TcpClient = tcpClient;
        //        player.BinaryReader = new System.IO.BinaryReader(tcpClient.GetStream());
        //        player.BinaryWriter = new System.IO.BinaryWriter(tcpClient.GetStream());
        //        player.Id = Guid.NewGuid();
        //        player.BallId = Guid.NewGuid(); //
        //        player.Score = 0;


        //        player.GameState = GameState.Connecting;

        //        _playerList.Add(player);

        //        string playerJson = JsonConvert.SerializeObject(player);
        //        Console.WriteLine(playerJson);
        //        player.BinaryWriter.Write(playerJson);
        //    }
        //    else
        //    {
        //        Console.WriteLine("Connection refused");
        //    }
        //}





        private void WriteMessageToPlayer(Player playerToSend, string messageToSend) => playerToSend.BinaryWriter.Write(messageToSend);
    }
}
