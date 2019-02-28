using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardDriver : MonoBehaviour
{
    public float drivingVelocity = 5.0f;
    private bool isDriving = false;
    public Rigidbody2D rb;

    public void StartDriving()
    {
        isDriving = true;
    }

    void Update()
    {
        if (isDriving && GameManager.Instance.ActorsMovable){
            rb.velocity = new Vector2(drivingVelocity, 0f);
        }
        else{
            rb.velocity = Vector2.zero;
        }
    }

}
