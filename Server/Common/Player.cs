﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using Newtonsoft.Json;

namespace Common
{
    public class Player
    {

        public Player() { }

        public Guid Id { get; set; }
        public Guid BallId { get; set; }
        public int Score { get; set; }
        public string Name { get; set; }
        public List<Message> MessageList { get; set; }
        [JsonIgnore]
        public TcpClient TcpClient { get; set; }
        [JsonIgnore]
        public BinaryReader BinaryReader { get; set; }
        [JsonIgnore]
        public BinaryWriter BinaryWriter { get; set; }
        public GameState GameState { get; set; }

        public bool DataAvailable()
        {
            return TcpClient.GetStream().DataAvailable;
        }
    }
}
