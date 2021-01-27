using Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{

    [SerializeField] private float movementSpeed;
    [SerializeField] private Camera sceneCamera;


    public bool Playable = false;
    public TcpClientController tcpClientController;
    private Vector3 _oldPosition;
    private Quaternion _oldRotation;
    private Rigidbody playerRb;



    private void Awake()
    {
        playerRb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (!Playable) return;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        // Change Player position
        if (x != 0 || z != 0)
        {
            playerRb.position += Vector3.forward * (z * Time.deltaTime * movementSpeed);
            playerRb.position += Vector3.right * (x * Time.deltaTime * movementSpeed);
        }
        else playerRb.velocity = Vector3.zero;

        
        if (transform.position != _oldPosition || transform.rotation != _oldRotation) SendPlayerMovementMessage();


        _oldPosition = transform.position;
        _oldRotation = transform.rotation;

    }


    private void SendPlayerMovementMessage()
    {
        Message msg = new Message();
        msg.MessageType = MessageType.PlayerMovement;
        PlayerInfo info = new PlayerInfo();
        info.Id = tcpClientController.Player.Id;
        info.Name = tcpClientController.Player.Name;


        info.directionX = transform.rotation.eulerAngles.x;
        info.directionY = transform.rotation.eulerAngles.y;
        info.directionZ = transform.rotation.eulerAngles.z;

        info.X = transform.position.x;
        info.Y = transform.position.y;
        info.Z = transform.position.z;
        msg.PlayerInfo = info;
        tcpClientController.SendMessage(msg);
    }


    void Update()
    {
        if (!Playable) return;

        MovementAndOrientationUpdate();
        
        if(Input.GetMouseButtonDown(0)) TryThrow();
    }

    void MovementAndOrientationUpdate()
    {
        Vector3 mousePoint = sceneCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y,sceneCamera.transform.position.y));
        mousePoint.y *= 0f;
        
        transform.LookAt(mousePoint);
    }
    
    void TryThrow()
    {
            Ball ball = GetComponentInChildren<Ball>();
            ball?.ThrowBall();
    }
}
