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
    // Start is called before the first frame update


    private bool canCatch;
    [SerializeField] Ball ball;
    [SerializeField] private float angle;

    //------------
    public TcpClientController tcpClientController;

    public bool Playable = false;

    private Vector3 _oldPosition;

    internal void PlayerHit()
    {
        throw new NotImplementedException();
    }

    private Quaternion _oldRotation;
    //------------


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


        if (x != 0 || z != 0)
        {
            playerRb.position += Vector3.forward * (z * Time.deltaTime * movementSpeed);
            playerRb.position += Vector3.right * (x * Time.deltaTime * movementSpeed);
        }
        else playerRb.velocity = Vector3.zero;

        //-----------------
        if (transform.position != _oldPosition || transform.rotation != _oldRotation)
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

        _oldPosition = transform.position;
        _oldRotation = transform.rotation;
        //-----------------

    }

    // Update is called once per frame
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



    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.transform == ball.transform) canCatch = true;
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.transform == ball.transform) canCatch = false;
    //}

    //private void OnTriggerStay(Collider other)
    //{
    //    if (!Input.GetMouseButtonDown(0)) return; 
        
        
    //    float angleToBall = Vector3.Angle(transform.forward, ball.transform.position-transform.position);

    //    if (canCatch && angleToBall >= -angle / 2 && angleToBall <= angle / 2)
    //    {
    //        Debug.Log("CATCHED" + canCatch);
    //        ball.GrabBall(this.transform);
    //        canCatch = false;
    //    }
          
    //}


    //-------------------------------------------------------------------

    //public TcpClientController TcpClientController;
    //public bool Playable;

    //private Vector3 _oldPosition;
    //private float _horizontal;
    //private float _vertical;

    ////void Update()
    ////{
    ////    if (!Playable) return;

    ////    _horizontal = Input.GetAxis("Horizontal") * Time.deltaTime * 3.0f;
    ////    _vertical = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;
    ////}

    //void FixedUpdate()
    //{
    //    if (!Playable) return;

    //    transform.Translate(_horizontal, 0, _vertical);

    //    if (transform.position != _oldPosition)
    //    {
    //        Message msg = new Message();
    //        msg.MessageType = MessageType.PlayerMovement;
    //        PlayerInfo info = new PlayerInfo();
    //        info.Id = TcpClientController.Player.Id;
    //        info.Name = TcpClientController.Player.Name;
    //        info.X = transform.position.x;
    //        info.Y = transform.position.y;
    //        info.Z = transform.position.z;
    //        msg.PlayerInfo = info;
    //        TcpClientController.SendMessage(msg);
    //    }

    //    _oldPosition = transform.position;
    //}
}
