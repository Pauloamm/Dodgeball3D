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
    
    
    private Rigidbody playerRb;
    private void Awake()
    {
        playerRb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        if (x != 0 || z != 0)
        {
            playerRb.position += Vector3.forward * (z * Time.deltaTime * movementSpeed);
            playerRb.position += Vector3.right * (x * Time.deltaTime * movementSpeed);
        }
        else playerRb.velocity = Vector3.zero;
    }
    
    // Update is called once per frame
    void Update()
    {
        MovementAndOrientationUpdate();
        
        if(Input.GetMouseButtonUp(0)) TryThrow();
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


    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == ball.transform) canCatch = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform == ball.transform) canCatch = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!Input.GetMouseButtonDown(0)) return; 
        
        
        float angleToBall = Vector3.Angle(transform.forward, ball.transform.position-transform.position);

        if (canCatch && angleToBall >= -angle / 2 && angleToBall <= angle / 2)
        {
            Debug.Log("CATCHED" + canCatch);
            ball.GrabBall(this.transform);
            canCatch = false;
        }
          
    }
}
