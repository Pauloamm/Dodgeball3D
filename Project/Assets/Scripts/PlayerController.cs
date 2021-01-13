using Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public TcpClientController TcpClientController;
    public bool Playable;

    private Vector3 _oldPosition;
    private float _horizontal;
    private float _vertical;

	void Update () {
        if (!Playable) return;

        _horizontal = Input.GetAxis("Horizontal") * Time.deltaTime * 3.0f;
        _vertical = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;
	}

    void FixedUpdate()
    {
        if (!Playable) return;

        transform.Translate(_horizontal, 0, _vertical);

        if (transform.position != _oldPosition)
        {
            Message msg = new Message();
            msg.MessageType = MessageType.PlayerMovement;
            PlayerInfo info = new PlayerInfo();
            info.Id = TcpClientController.Player.Id;
            info.Name = TcpClientController.Player.Name;
            info.X = transform.position.x;
            info.Y = transform.position.y;
            info.Z = transform.position.z;
            msg.PlayerInfo = info;
            TcpClientController.SendMessage(msg);
        }

        _oldPosition = transform.position;
    }
}
