using Common;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TcpClientController : MonoBehaviour
{

    [HideInInspector]
    public Player Player;
    public static Guid PlayerID;
    public Dictionary<Guid, GameObject> _balls;
    public Dictionary<Guid?, GameObject> _playerGameObjectDict;

    [SerializeField] ChangeTurn changeTurn;

    public GameObject SpawPoint;
    public GameObject PlayerPrefab;
    public GameObject ConnectionUI;
    public Text PlayerNameInputText;
    public string IpAddress;
    public int Port;

    private void Awake()
    {
        Player = new Player();
        _balls = new Dictionary<Guid, GameObject>();
        _playerGameObjectDict = new Dictionary<Guid?, GameObject>();
        Player.GameState = GameState.Disconnected;
        Player.TcpClient = new TcpClient();
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

            switch (message.MessageType)
            {
                case MessageType.NewPlayer:
                    ReadNewPlayerMessage(message);
                    break;
                case MessageType.PlayerMovement:
                    if (_playerGameObjectDict.ContainsKey(message.PlayerInfo.Id) && message.PlayerInfo.Id != Player.Id)
                        ReadPlayerMovementMessage(message);
                    break;
                case MessageType.BallMovement:
                    if (_playerGameObjectDict.ContainsKey(message.PlayerInfo.Id) && message.PlayerInfo.Id != Player.Id)
                        ReadBallMovementMessage(message);
                    break;
                case MessageType.EndGame:
                    ReadEndGameMessage(message);
                    break;

                default:
                    break;
            }
        }
    }

    // Reads New Player Message
    private void ReadNewPlayerMessage(Message message)
    {
        GameObject playerGameObject = Instantiate(PlayerPrefab,
                    new Vector3(message.PlayerInfo.X, message.PlayerInfo.Y, message.PlayerInfo.Z),
                    Quaternion.identity);

        playerGameObject.GetComponent<PlayerUiController>().PlayerName.text = message.PlayerInfo.Name;
        playerGameObject.GetComponentInChildren<Ball>().parentPlayer = playerGameObject.transform;

        _playerGameObjectDict.Add(message.PlayerInfo.Id, playerGameObject);

        playerGameObject.GetComponentInChildren<Ball>().BallId = message.PlayerInfo.BallId; // ball
        _balls.Add(message.PlayerInfo.BallId, playerGameObject.GetComponentInChildren<Ball>().gameObject);

        playerGameObject.name = message.PlayerInfo.Id.ToString();
        playerGameObject.GetComponentInChildren<Ball>().gameObject.name = message.PlayerInfo.BallId.ToString();
    }

    // Reads Player Movement Message
    private void ReadPlayerMovementMessage(Message message)
    {
        _playerGameObjectDict[message.PlayerInfo.Id].transform.position =
                        new Vector3(message.PlayerInfo.X, message.PlayerInfo.Y, message.PlayerInfo.Z);

        _playerGameObjectDict[message.PlayerInfo.Id].transform.rotation = Quaternion.Euler(
           message.PlayerInfo.directionX, message.PlayerInfo.directionY, message.PlayerInfo.directionZ);
    }

    // Reads Ball Movement Message
    private void ReadBallMovementMessage(Message message)
    {
        GameObject ballToMove = _balls[message.PlayerInfo.BallId];

        ballToMove.transform.position = new Vector3(message.PlayerInfo.X, message.PlayerInfo.Y, message.PlayerInfo.Z);

        ballToMove.transform.rotation = Quaternion.Euler(
        message.PlayerInfo.directionX, message.PlayerInfo.directionY, message.PlayerInfo.directionZ);

        _playerGameObjectDict[message.PlayerInfo.Id].GetComponentInChildren<Text>().text = message.PlayerInfo.Score.ToString();
    }

    // Reads End Game Message 
    private void ReadEndGameMessage(Message message)
    {
        if (_playerGameObjectDict.ContainsKey(message.PlayerInfo.Id) &&
                    message.PlayerInfo.Id != Player.Id)
        {
            foreach (KeyValuePair<Guid?, GameObject> client in _playerGameObjectDict)
            {
                if (client.Key != message.PlayerInfo.Id) _playerGameObjectDict[client.Key].GetComponentInChildren<Text>().text = "You Lost";
            }

            _playerGameObjectDict[message.PlayerInfo.Id].GetComponentInChildren<Text>().text = "You WON";
            StartCoroutine(ReloadGame());
        }
        else
        {
            foreach (KeyValuePair<Guid?, GameObject> client in _playerGameObjectDict)
            {
                if (client.Key != Player.Id) _playerGameObjectDict[client.Key].GetComponentInChildren<Text>().text = "You Lost";
            }

            _playerGameObjectDict[Player.Id].GetComponentInChildren<Text>().text = "You WON";
            StartCoroutine(ReloadGame());
        }
    }


    private void ReadFinishedSyncMessage(Message message)
    {
        // Disables Connect UI
        ConnectionUI.SetActive(false);

        // Instantiates Playable Player and setups variable initialization
        GameObject playerGameObject =
            Instantiate(PlayerPrefab, SpawPoint.transform.position, Quaternion.identity);
        playerGameObject.GetComponent<PlayerMovement>().tcpClientController = this;
        playerGameObject.GetComponent<PlayerMovement>().Playable = true;
        playerGameObject.GetComponent<PlayerMovement>().enabled = true;
        playerGameObject.GetComponentInChildren<Ball>().parentPlayer = playerGameObject.transform;
        playerGameObject.GetComponent<PlayerUiController>().PlayerName.text = Player.Name;

        playerGameObject.GetComponentInChildren<Ball>().TcpClientController = this;
        playerGameObject.GetComponentInChildren<Ball>().changeTurn = playerGameObject.GetComponentInChildren<ChangeTurn>();

        playerGameObject.GetComponentInChildren<ChangeTurn>().tcpClientController = this;
        playerGameObject.GetComponentInChildren<ChangeTurn>().myScore = playerGameObject.GetComponentInChildren<Text>();


        playerGameObject.GetComponentInChildren<Text>().text = message.PlayerInfo.Score.ToString();


        _playerGameObjectDict.Add(Player.Id, playerGameObject);

        playerGameObject.GetComponentInChildren<Ball>().BallId = message.PlayerInfo.BallId; // ball
        _balls.Add(message.PlayerInfo.BallId, playerGameObject.GetComponentInChildren<Ball>().gameObject);

        Player.GameState = GameState.GameStarted;
    }

    private void Sync()
    {
        if (Player.DataAvailable())
        {
            Message message = ReceiveMessage();

            switch (message.MessageType)
            {
                case MessageType.NewPlayer:
                    ReadNewPlayerMessage(message);
                    break;
                case MessageType.PlayerMovement:
                    if (_playerGameObjectDict.ContainsKey(message.PlayerInfo.Id))
                        ReadPlayerMovementMessage(message);
                    break;
                case MessageType.BallMovement:
                    if (_playerGameObjectDict.ContainsKey(message.PlayerInfo.Id) && message.PlayerInfo.Id != Player.Id)
                        ReadBallMovementMessage(message);
                    break;
                case MessageType.FinishedSync:
                    ReadFinishedSyncMessage(message);
                    break;
                default:
                    break;
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
            //
            PlayerID = Player.Id;
            //
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

    IEnumerator ReloadGame()
    {
        yield return new WaitForSeconds(4f);

        foreach (KeyValuePair<Guid?, GameObject> client in _playerGameObjectDict)
        {
            Destroy(client.Value);
        }
        _playerGameObjectDict.Clear();
        _balls.Clear();
        Player.TcpClient.Close();
        Player.TcpClient = new TcpClient();


        Player.GameState = GameState.Disconnected;

        ConnectionUI.SetActive(true);
    }
}
