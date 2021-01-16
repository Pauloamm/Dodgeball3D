using Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ChangeTurn : MonoBehaviour
{

    //[SerializeField] PlayerMovement player_01;
    //[SerializeField] PlayerMovement player_02;
    [SerializeField] Ball ball;

    Dictionary<Guid, PlayerMovement> players;
    public TcpClientController tcpClientController;



    Guid currentPlayer;
    private readonly Vector3 ballOffset = new Vector3(1f, 0f, 0f);

    public static bool isCurrentTurn;

    void Awake()
    {

        players = new Dictionary<Guid, PlayerMovement>();

        
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnCollisionEnter(Collision Other)
    {

        if (!ball.parentPlayer.GetComponent<PlayerMovement>().Playable) return;

        if (Other.gameObject.CompareTag("Wall"))
        {
            Debug.Log("bateu na puta da parede");
            ball.isGrabbed = true;
            GiveBallToNextPlayer();
        }
        else if (Other.gameObject.CompareTag("Player"))
        {
            Other.gameObject.GetComponent<PlayerMovement>().PlayerHit();
            GiveBallToNextPlayer();
        }
    }

    private void GiveBallToNextPlayer()
    {
        
        ball.ResetPosition();


        Debug.Log("é a pouta da minha vez");

    }
}
