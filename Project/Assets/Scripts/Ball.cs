using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public  class Ball : MonoBehaviour
{
    private readonly Vector3 offset = new Vector3(1f, 0f, 0f);
    
   
    
    private bool isGrabbed;

    [SerializeField] private LayerMask wallLayer;
    // Start is called before the first frame update
    void Start()
    {
        isGrabbed = false;
        

    }



    void Update()
    {

        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(ray,out hit, 20f * Time.deltaTime, wallLayer))
        {
            Vector3 newDirection = Vector3.Reflect(ray.direction, hit.normal);
            
            
            float rotation = 90f - Mathf.Atan2(newDirection.z, newDirection.x) * Mathf.Rad2Deg;
            transform.eulerAngles = new Vector3(0f, rotation, 0f);
            // transform.forward = newDirection;

        }
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
