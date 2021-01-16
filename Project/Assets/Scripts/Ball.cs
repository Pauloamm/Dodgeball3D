using Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class Ball : MonoBehaviour
{
    private readonly Vector3 offset = new Vector3(1f, 0f, 0f);

    public TcpClientController TcpClientController;

    public Guid BallId;

    public bool isGrabbed;

    public Transform parentPlayer;


    [SerializeField] private LayerMask wallLayer;
    // Start is called before the first frame update
    void Start()
    {
        isGrabbed = true;
        //parentPlayer = transform.parent;
    }

    //void OnEnable() { parentPlayer = transform.parent; }

    private Vector3 _oldPosition;
    private Quaternion _oldRotation;

    //public void SetNewParent(Transform newParent)
    //{
    //    transform.parent = newParent;
    //    transform.localPosition = offset;
    //    transform.localRotation = Quaternion.identity;
    //}



    public void ResetPosition()
    {
        transform.parent = parentPlayer;
        transform.localPosition = offset;
        transform.localRotation = Quaternion.identity;
    }

    void Update()
    {

        if (!parentPlayer.GetComponent<PlayerMovement>().Playable) return;

        //if (!transform.parent.GetComponent<PlayerMovement>().Playable) return;

        if (transform.position != _oldPosition || transform.rotation != _oldRotation)
        {
            Debug.Log("entrou para mandar " + BallId);
            Message msg = new Message();
            msg.MessageType = MessageType.BallMovement;
            PlayerInfo info = new PlayerInfo();
            info.Id = TcpClientController.Player.Id;
            info.BallId = BallId;

            if (isGrabbed)
            {
                Debug.Log("bola para mandar mensagem");
                info.ParentId = TcpClientController.Player.Id;
            }
            else
            {
                info.ParentId = null;
            }
            info.Name = TcpClientController.Player.Name;

            info.directionX = transform.rotation.eulerAngles.x;
            info.directionY = transform.rotation.eulerAngles.y;
            info.directionZ = transform.rotation.eulerAngles.z;

            info.X = transform.position.x;
            info.Y = transform.position.y;
            info.Z = transform.position.z;
            msg.PlayerInfo = info;
            TcpClientController.SendMessage(msg);
        }

        _oldPosition = transform.position;
        _oldRotation = transform.rotation;

    }



    public void GrabBall(Transform playerThatGrabs)
    {
        if (!isGrabbed)
        {
            isGrabbed = true;
            transform.SetParent(playerThatGrabs.transform);
            transform.localPosition = offset;
            transform.localRotation = Quaternion.identity;
        }
    }

    public void ThrowBall()
    {
        if (!isGrabbed) return;
        transform.SetParent(null);
        StartCoroutine(BallThrow());

    }


    IEnumerator BallThrow()
    {
        isGrabbed = false;


        while (!isGrabbed)
        {
            transform.position += transform.forward * (20f * Time.deltaTime);
            yield return null;
        }
    }
}
