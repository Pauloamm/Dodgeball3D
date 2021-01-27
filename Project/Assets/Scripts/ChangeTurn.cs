using Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ChangeTurn : MonoBehaviour
{
    [SerializeField] Ball ball;

    public TcpClientController tcpClientController;
    public Text myScore;
    public int score;
    public static bool isCurrentTurn;

    // Update is called once per frame
    void Update()
    {
        if (score != 3) return;

        // Sends victory message to Server
        Message msg = new Message();
        msg.MessageType = MessageType.EndGame;
        PlayerInfo info = new PlayerInfo();
        info.Id = tcpClientController.Player.Id;      // WINNER
        info.Name = tcpClientController.Player.Name;  // WINNER

        msg.PlayerInfo = info;
        tcpClientController.SendMessage(msg);

        score = -1;
    }

    // Checks when ball hits
    void OnCollisionEnter(Collision Other)
    {
        // Verifies if player is Playable and if the game isn't over
        if (!ball.parentPlayer.GetComponent<PlayerMovement>().Playable || score == 3) return;

        // Checks if hits Wall or another Ball
        if (Other.gameObject.CompareTag("Wall") || Other.gameObject.CompareTag("Ball"))
        {
            ball.isGrabbed = true;
            ball.ResetPosition(); // Resets Ball's position
        }
        // Checks if hits another Player
        else if (Other.gameObject.CompareTag("Player") && Other.gameObject.transform != ball.parentPlayer)
        {
            if (!ball.isGrabbed)
            {
                score++;
                ball.isGrabbed = true;
            }
            myScore.text = score.ToString();
            ball.ResetPosition(); // Resets Ball's position
        }
    }
}
