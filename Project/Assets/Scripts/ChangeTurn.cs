using Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ChangeTurn : MonoBehaviour
{

    //[SerializeField] PlayerMovement player_01;
    //[SerializeField] PlayerMovement player_02;
    [SerializeField] Ball ball;

    Dictionary<Guid, PlayerMovement> players;
    public TcpClientController tcpClientController;


    public Text myScore;
    public int score;


    Guid currentPlayer;
    private readonly Vector3 ballOffset = new Vector3(1f, 0f, 0f);

    public static bool isCurrentTurn;

    void Awake()
    {
        //score = 0;
        //myScore.text = score.ToString();
        players = new Dictionary<Guid, PlayerMovement>();


    }

    // Update is called once per frame
    void Update()
    {
        if (score == 3)
        {
            Debug.Log("WIN CRL");

            Message msg = new Message();
            msg.MessageType = MessageType.EndGame;
            PlayerInfo info = new PlayerInfo();
            info.Id = tcpClientController.Player.Id;      // WINNER
            info.Name = tcpClientController.Player.Name;  // WINNER

            msg.PlayerInfo = info;
            tcpClientController.SendMessage(msg);

            score = -1;
        }


    }

    void OnCollisionEnter(Collision Other)
    {

        if (!ball.parentPlayer.GetComponent<PlayerMovement>().Playable || score == 3) return;

        if (Other.gameObject.CompareTag("Wall") || Other.gameObject.CompareTag("Ball"))
        {
            Debug.Log("bateu na puta da parede");
            ball.isGrabbed = true;
            GiveBallToNextPlayer();
        }
        else if (Other.gameObject.CompareTag("Player") && Other.gameObject.transform != ball.parentPlayer)
        {

            if (!ball.isGrabbed)
            {
                score++;
                ball.isGrabbed = true;
            }
            //Other.gameObject.GetComponent<PlayerMovement>().PlayerHit();
            myScore.text = score.ToString();
            GiveBallToNextPlayer();
        }
    }

    

    private void GiveBallToNextPlayer()
    {

        ball.ResetPosition();


        Debug.Log("é a pouta da minha vez");

    }
}
