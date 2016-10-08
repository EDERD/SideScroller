using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlataformController : RayCastManager {
    public LayerMask passengerMask;
    public Vector3 move;
    public override void Start () {
        base.Start();
	}
	
	
	void Update () {

        UpdateRaycastOrigins();
        Vector3 velocity = move * Time.deltaTime;
        MovePassenger(velocity);
        transform.Translate(velocity); 
	}

    void MovePassenger(Vector3 Velocity)
    {
        HashSet<Transform> movedPassengers = new HashSet<Transform>();

        float directionX = Mathf.Sign(Velocity.x);
        float directionY = Mathf.Sign(Velocity.y);
       
        // vertical moving platform
        if(Velocity.y != 0)
        {
            float raylenght = Mathf.Abs(Velocity.y) + skinWidth;

            for(int i=0;i< verticalRayCount;i++)
            {
               
                Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottonLeft : raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing*i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin,Vector2.up*directionY,raylenght,passengerMask);
                // Debug.DrawRay(rayOrigin, Vector2.up * directionY, Color.red); ,raylenght,passengerMask

                if (hit) // found a passenger
                {
                    print("hit");
                    if(!movedPassengers.Contains(hit.transform))
                    {
                       
                        movedPassengers.Add(hit.transform);
                        float pushX = (directionY == 1) ? Velocity.x : 0;
                        float pushY = Velocity.y - (hit.distance - skinWidth) * directionY;
                       
                        hit.transform.Translate(new Vector3(pushX, pushY));
                    }
                   
                }
            }
        }
    }
}
