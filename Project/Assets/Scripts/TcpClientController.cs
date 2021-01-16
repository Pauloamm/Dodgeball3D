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
    public static Guid PlayerID;
    public Dictionary<Guid, GameObject> _balls;
    public Dictionary<Guid?, GameObject> _playerGameObjectDict;


    //[SerializeField] GameObject ball;

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
                playerGameObject.GetComponentInChildren<Ball>().parentPlayer = playerGameObject.transform;


                _playerGameObjectDict.Add(message.PlayerInfo.Id, playerGameObject);

                playerGameObject.GetComponentInChildren<Ball>().BallId = message.PlayerInfo.BallId; // ball
                _balls.Add(message.PlayerInfo.BallId, playerGameObject.GetComponentInChildren<Ball>().gameObject);

                playerGameObject.name = message.PlayerInfo.Id.ToString();
                playerGameObject.GetComponentInChildren<Ball>().gameObject.name = message.PlayerInfo.BallId.ToString();
                //changeTurn.AddNewPlayer();
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
            else if (message.MessageType == MessageType.BallMovement)
            {
                if (_playerGameObjectDict.ContainsKey(message.PlayerInfo.Id) &&
                    message.PlayerInfo.Id != Player.Id)
                {

                    GameObject ballToMove = _balls[message.PlayerInfo.BallId];
                    Debug.Log("nome da bola " + ballToMove.name + "ball id " + message.PlayerInfo.BallId);


                    ballToMove.transform.position = new Vector3(message.PlayerInfo.X, message.PlayerInfo.Y, message.PlayerInfo.Z);

                    ballToMove.transform.rotation = Quaternion.Euler(
                    message.PlayerInfo.directionX, message.PlayerInfo.directionY, message.PlayerInfo.directionZ);


                    //if (message.PlayerInfo.ParentId == null) ballToMove.transform.parent = null;
                    //else
                    //{
                    //    ballToMove.transform.parent = _playerGameObjectDict[message.PlayerInfo.Id].transform;
                    //}

                }
            }
            //else if (message.MessageType == MessageType.ChangeTurn)
            //{

            //    if (_playerGameObjectDict.ContainsKey(message.PlayerInfo.Id) &&
            //        message.PlayerInfo.Id != Player.Id)
            //    {
            //        Debug.Log("vaitefoderpaulo");
            //        ChangeTurn.isCurrentTurn = true;
            //        ball.GetComponent<Ball>().SetNewParent(_playerGameObjectDict[Player.Id].transform);

            //    }
            //    else
            //    {
            //        ChangeTurn.isCurrentTurn = false;
            //        ball.GetComponent<Ball>().SetNewParent(_playerGameObjectDict[message.PlayerInfo.Id].transform);

            //    }
            //}
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
                playerGameObject.GetComponentInChildren<Ball>().parentPlayer = playerGameObject.transform;

                _playerGameObjectDict.Add(message.PlayerInfo.Id, playerGameObject);

                playerGameObject.GetComponentInChildren<Ball>().BallId = message.PlayerInfo.BallId; // ball
                _balls.Add(message.PlayerInfo.BallId, playerGameObject.GetComponentInChildren<Ball>().gameObject);

                playerGameObject.name = message.PlayerInfo.Id.ToString();
                playerGameObject.GetComponentInChildren<Ball>().gameObject.name = message.PlayerInfo.BallId.ToString();

                //changeTurn.AddNewPlayer();
            }
            // processar messages PlayerMovement
            else if (message.MessageType == MessageType.PlayerMovement)
            {
                if (_playerGameObjectDict.ContainsKey(message.PlayerInfo.Id))
                {
                    Debug.Log("num" + _playerGameObjectDict.Count);

                    _playerGameObjectDict[message.PlayerInfo.Id].transform.position =
                        new Vector3(message.PlayerInfo.X, message.PlayerInfo.Y, message.PlayerInfo.Z);

                    //_playerGameObjectDict[message.PlayerInfo.Id].transform.eulerAngles =
                    //    new Vector3(message.PlayerInfo.directionX, message.PlayerInfo.directionY, message.PlayerInfo.directionZ);

                    _playerGameObjectDict[message.PlayerInfo.Id].transform.rotation = Quaternion.Euler(
                       message.PlayerInfo.directionX, message.PlayerInfo.directionY, message.PlayerInfo.directionZ);
                }
            }
            //
            else if (message.MessageType == MessageType.BallMovement)
            {
                if (_playerGameObjectDict.ContainsKey(message.PlayerInfo.Id) &&
                    message.PlayerInfo.Id != Player.Id)
                {

                    GameObject ballToMove = _balls[message.PlayerInfo.BallId];


                    ballToMove.transform.position = new Vector3(message.PlayerInfo.X, message.PlayerInfo.Y, message.PlayerInfo.Z);

                    ballToMove.transform.rotation = Quaternion.Euler(
                    message.PlayerInfo.directionX, message.PlayerInfo.directionY, message.PlayerInfo.directionZ);

                    //if (message.PlayerInfo.ParentId == null) ballToMove.transform.parent = null;
                    //else
                    //{
                    //    //Guid temp = (Guid)message.PlayerInfo.ParentId;
                    //    ballToMove.transform.parent = _playerGameObjectDict[message.PlayerInfo.Id].transform;

                    //}

                }
            }
            //else if (message.MessageType == MessageType.ChangeTurn)
            //{
            //    if (_playerGameObjectDict.ContainsKey(message.PlayerInfo.Id) &&
            //        message.PlayerInfo.Id != Player.Id)
            //    {
            //        ChangeTurn.isCurrentTurn = true;
            //        //ball.transform.parent = _playerGameObjectDict[message.PlayerInfo.Id].transform;
            //        ball.GetComponent<Ball>().SetNewParent(_playerGameObjectDict[Player.Id].transform);

            //    }else
            //    {
            //        ChangeTurn.isCurrentTurn = false;
            //        ball.GetComponent<Ball>().SetNewParent(_playerGameObjectDict[message.PlayerInfo.Id].transform);

            //    }
            //}


            else if (message.MessageType == MessageType.FinishedSync)
            {

                ConnectionUI.SetActive(false);
                GameObject playerGameObject =
                    Instantiate(PlayerPrefab, SpawPoint.transform.position, Quaternion.identity);
                playerGameObject.GetComponent<PlayerMovement>().TcpClientController = this;
                playerGameObject.GetComponent<PlayerMovement>().Playable = true;
                playerGameObject.GetComponent<PlayerMovement>().enabled = true;
                playerGameObject.GetComponentInChildren<Ball>().parentPlayer = playerGameObject.transform;
                playerGameObject.GetComponent<PlayerUiController>().PlayerName.text = Player.Name;

                playerGameObject.GetComponentInChildren<Ball>().TcpClientController = this;
                playerGameObject.GetComponentInChildren<ChangeTurn>().tcpClientController = this;



                Debug.Log(message.PlayerInfo);

                _playerGameObjectDict.Add(Player.Id, playerGameObject);

                playerGameObject.GetComponentInChildren<Ball>().BallId = message.PlayerInfo.BallId; // ball
                _balls.Add(message.PlayerInfo.BallId, playerGameObject.GetComponentInChildren<Ball>().gameObject);

                //
                playerGameObject.name = message.PlayerInfo.Id.ToString();
                playerGameObject.GetComponentInChildren<Ball>().gameObject.name = message.PlayerInfo.BallId.ToString();
                //

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
}
