using Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class TcpClientController : MonoBehaviour
{

    [HideInInspector]
    public Player Player;
    private Dictionary<Guid, GameObject> _playerGameObjectDict;

    public GameObject SpawPoint;
    public GameObject PlayerPrefab;
    public GameObject ConnectionUI;
    public Text PlayerNameInputText;
    public string IpAddress;
    public int Port;

    private void Awake()
    {
        Player = new Player();
        _playerGameObjectDict = new Dictionary<Guid, GameObject>();
        Player.GameState = GameState.Disconnected;
        Player.TcpClient = new TcpClient();
    }
    void Start()
    {

    }

    void Update()
    {
        if (Player.TcpClient.Connected)
        {
            switch (Player.GameState)
            {
                case GameState.Connecting:
                    Debug.Log("Connecting");
                    Connecting();
                    break;
                case GameState.Connected:
                    Debug.Log("Connected");
                    Connected();
                    break;
                case GameState.Sync:
                    Debug.Log("Syncing");
                    Sync();
                    break;
                case GameState.GameStarted:
                    Debug.Log("GameStarted");
                    GameStarted();
                    break;
            }
        }
        else
        {
            Debug.Log("Disconnected");
        }
    }

    private void GameStarted()
    {
        if (Player.DataAvailable())
        {
            Message message = ReceiveMessage();
            if (message.MessageType == MessageType.NewPlayer)
            {
                GameObject playerGameObject = Instantiate(PlayerPrefab,
                    new Vector3(message.PlayerInfo.X, message.PlayerInfo.Y, message.PlayerInfo.Z),
                    Quaternion.identity);

                //playerGameObject.SetActive(true);

                playerGameObject.GetComponent<PlayerUiController>().PlayerName.text =
                    message.PlayerInfo.Name;
                _playerGameObjectDict.Add(message.PlayerInfo.Id, playerGameObject);
            }
            else if (message.MessageType == MessageType.PlayerMovement)
            {
                if (_playerGameObjectDict.ContainsKey(message.PlayerInfo.Id) &&
                    message.PlayerInfo.Id != Player.Id)
                {
                    _playerGameObjectDict[message.PlayerInfo.Id].transform.position =
                        new Vector3(message.PlayerInfo.X, message.PlayerInfo.Y, message.PlayerInfo.Z);

                    _playerGameObjectDict[message.PlayerInfo.Id].transform.rotation = Quaternion.Euler(
                        message.PlayerInfo.directionX, message.PlayerInfo.directionY, message.PlayerInfo.directionZ);
                        //new Vector3(message.PlayerInfo.directionX, message.PlayerInfo.directionY, message.PlayerInfo.directionZ);
                }
            }
        }
    }

    private void Sync()
    {
        if (Player.DataAvailable())
        {
            Message message = ReceiveMessage();

            // processar messages NewPlayer
            if (message.MessageType == MessageType.NewPlayer)
            {
                GameObject playerGameObject = Instantiate(PlayerPrefab,
                    new Vector3(message.PlayerInfo.X, message.PlayerInfo.Y, message.PlayerInfo.Z),
                    Quaternion.identity);

                //playerGameObject.SetActive(true);

                playerGameObject.GetComponent<PlayerUiController>().PlayerName.text
                    = message.PlayerInfo.Name;
                _playerGameObjectDict.Add(message.PlayerInfo.Id, playerGameObject);
            }
            // processar messages PlayerMovement
            else if (message.MessageType == MessageType.PlayerMovement)
            {
                if (_playerGameObjectDict.ContainsKey(message.PlayerInfo.Id))
                {
                    _playerGameObjectDict[message.PlayerInfo.Id].transform.position =
                        new Vector3(message.PlayerInfo.X, message.PlayerInfo.Y, message.PlayerInfo.Z);

                    //_playerGameObjectDict[message.PlayerInfo.Id].transform.eulerAngles =
                    //    new Vector3(message.PlayerInfo.directionX, message.PlayerInfo.directionY, message.PlayerInfo.directionZ);

                    _playerGameObjectDict[message.PlayerInfo.Id].transform.rotation = Quaternion.Euler(
                       message.PlayerInfo.directionX, message.PlayerInfo.directionY, message.PlayerInfo.directionZ);
                }
            }
            else if (message.MessageType == MessageType.FinishedSync)
            {

                ConnectionUI.SetActive(false);
                GameObject playerGameObject =
                    Instantiate(PlayerPrefab, SpawPoint.transform.position, Quaternion.identity);
                playerGameObject.GetComponent<PlayerMovement>().TcpClientController = this;
                playerGameObject.GetComponent<PlayerMovement>().Playable = true;
                playerGameObject.GetComponent<PlayerMovement>().enabled = true;
                playerGameObject.GetComponent<PlayerUiController>().PlayerName.text = Player.Name;

                //----
                //playerGameObject.SetActive(true);
                //----
                _playerGameObjectDict.Add(Player.Id, playerGameObject);

                Player.GameState = GameState.GameStarted;
            }
        }
    }

    private void Connected()
    {
        if (Player.DataAvailable())
        {
            Message message = ReceiveMessage();
            Debug.Log(message.Description);
            Player.GameState = GameState.Sync;
        }
    }

    private void Connecting()
    {
        if (Player.TcpClient.GetStream().DataAvailable)
        {
            string playerJsonString = Player.BinaryReader.ReadString();
            Player player = JsonConvert.DeserializeObject<Player>(playerJsonString);
            Player.Id = player.Id;
            Player.MessageList.Add(player.MessageList.FirstOrDefault());
            Player.Name = PlayerNameInputText.text;

            Message message = new Message();
            message.MessageType = MessageType.PlayerName;
            Player.MessageList.Add(message);

            string newPlayerJsonString = JsonConvert.SerializeObject(Player);
            Player.BinaryWriter.Write(newPlayerJsonString);
            Player.GameState = GameState.Connected;
        }
    }

    private Message ReceiveMessage()
    {
        string msgJson = Player.BinaryReader.ReadString();
        Message msg = JsonConvert.DeserializeObject<Message>(msgJson);
        Player.MessageList.Add(msg);
        return msg;
    }

    public void SendMessage(Message message)
    {
        Debug.Log(Player + " " + Player.BinaryWriter);
        string messageJson = JsonConvert.SerializeObject(message);
        Player.BinaryWriter.Write(messageJson);
        Player.MessageList.Add(message);
    }

    public void StartTcpClient()
    {
        Player.TcpClient.BeginConnect(IPAddress.Parse(IpAddress), Port,
            AcceptConnection, Player.TcpClient);
        Player.GameState = GameState.Connecting;
    }

    private void AcceptConnection(IAsyncResult ar)
    {
        TcpClient tcpClient = (TcpClient)ar.AsyncState;
        tcpClient.EndConnect(ar);

        if (tcpClient.Connected)
        {
            Debug.Log("Client connected");
            Player.BinaryReader = new System.IO.BinaryReader(tcpClient.GetStream());
            Player.BinaryWriter = new System.IO.BinaryWriter(tcpClient.GetStream());
            Player.MessageList = new List<Message>();
        }
        else
        {
            Debug.Log("Client connection refused");
        }
    }
}
