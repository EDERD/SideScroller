using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlataformController : RayCastManager {
    public LayerMask passengerMask;
    public Vector3[] LocalWaypoints;
    public float speed;
    public float waitTime;
    [Range(0,2)]
    public float easeAmount;
    public bool cyclic;
    int fromWayPointIndex=0;
    float percentBetweenWaypoints;
    float nextMoveTime;

  Vector3[] globalWayPoints;

    List<PassengerMovement> passengerMovement;
    Dictionary<Transform, PlayerPhysics> passengerDictionary = new Dictionary<Transform,PlayerPhysics>();

    public override void Start () {
        base.Start();
        // Set the Path Platform
        globalWayPoints = new Vector3 [LocalWaypoints.Length];
        for (int i=0;i < LocalWaypoints.Length;i++)
        {
            globalWayPoints[i] = LocalWaypoints[i] + transform.position;
        }


	}
	
	
	void Update () {

        UpdateRaycastOrigins();  // refresh bounds
        Vector3 velocity =CalculatePlatformMovement();
        CalculatePassengerMovement(velocity);

        MovePassengers(true);
        transform.Translate(velocity);
        MovePassengers(false); 
	}

    // smooth Movement
    float Ease(float x)
    {
        float a = easeAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
    }

    // Plataform Movement Path
    Vector3 CalculatePlatformMovement()
    {
        // Time wait 
        if (Time.time < nextMoveTime)
        {
            return Vector3.zero;
        }

       
        fromWayPointIndex %= globalWayPoints.Length;
        int toWayPointIndex = (fromWayPointIndex + 1) %globalWayPoints.Length;
      
        float distanceBetweenPoints = Vector3.Distance(globalWayPoints[fromWayPointIndex],globalWayPoints[toWayPointIndex]);
        percentBetweenWaypoints += Time.deltaTime * speed / distanceBetweenPoints;
        percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
        float easedPercentBetweenWayPoints = Ease(percentBetweenWaypoints);

        Vector3 newpos = Vector3.Lerp(globalWayPoints[fromWayPointIndex],globalWayPoints[toWayPointIndex],easedPercentBetweenWayPoints);
        if (percentBetweenWaypoints >= 1)
        {
            percentBetweenWaypoints = 0;
            fromWayPointIndex++;
            if (!cyclic)
            {
                if (fromWayPointIndex >= globalWayPoints.Length - 1)
                {
                    fromWayPointIndex = 0;
                    System.Array.Reverse(globalWayPoints);
                }
            }
            nextMoveTime = Time.time + waitTime;
        }

        return newpos-transform.position;
    }


    void MovePassengers(bool beforeMovePlatform)
    {
        foreach (PassengerMovement passenger in passengerMovement)
        {
            if (!passengerDictionary.ContainsKey(passenger.transform))
            {
                passengerDictionary.Add(passenger.transform, passenger.transform.GetComponent<PlayerPhysics>());
            }

            if (passenger.moveBeforePlatform == beforeMovePlatform)
            {
                passengerDictionary[passenger.transform].Move(passenger.velocity, passenger.standingOnPlatform);
             
            }

        }
    }

    void CalculatePassengerMovement(Vector3 Velocity)
    {
        HashSet<Transform> movedPassengers = new HashSet<Transform>();
       
        passengerMovement = new List<PassengerMovement>();

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
                   
                    if(!movedPassengers.Contains(hit.transform))
                    {
                       
                        movedPassengers.Add(hit.transform);
                        float pushX = (directionY == 1) ? Velocity.x : 0;
                        float pushY = Velocity.y - (hit.distance - skinWidth) * directionY;

                        passengerMovement.Add(new PassengerMovement(hit.transform,new Vector3(pushX,pushY),directionY==1,true));
                    }
                   
                }
            }
        }

        // Horizontal moving platform
        if (Velocity.x != 0)
        {
            float raylenght = Mathf.Abs(Velocity.x) + skinWidth;

            for (int i = 0; i < horizontalRayCount; i++)
            {

                Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottonLeft : raycastOrigins.bottonRight;
                rayOrigin += Vector2.up * (horizontalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, raylenght, passengerMask);
                //Debug.DrawRay(rayOrigin, Vector2.right * directionX, Color.red);

                if (hit) // found a passenger
                {
                    

                    if (!movedPassengers.Contains(hit.transform))
                    {

                        movedPassengers.Add(hit.transform);
                        float pushX = Velocity.x - (hit.distance - skinWidth) * directionX;
                   
                        float pushY = -skinWidth;
                       
                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), false, true));
                    }

                }
            }
        }




        //Passenger on top of Horizontally or downward moving platform
        if (directionY == -1 || Velocity.y == 0 && Velocity.x != 0)
        {
            float raylenght = skinWidth * 2;

            for (int i = 0; i < verticalRayCount; i++)
            {

                Vector2 rayOrigin = raycastOrigins.topLeft + Vector2.right * (verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, raylenght, passengerMask);
                // Debug.DrawRay(rayOrigin, Vector2.up * directionY, Color.red); ,raylenght,passengerMask

                if (hit) // found a passenger
                {

                    if (!movedPassengers.Contains(hit.transform))
                    {

                        movedPassengers.Add(hit.transform);
                        float pushX = Velocity.x;
                        float pushY = Velocity.y;

                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
                    }

                }
            }
        }
    }

    struct PassengerMovement {
        public Transform transform;
        public Vector3 velocity;
        public bool standingOnPlatform;
        public bool moveBeforePlatform;

        public PassengerMovement(Transform _transform , Vector3 _velocity,bool _standingOnPlatform,bool _moveBeforePlatform) {
            transform = _transform ;
            velocity = _velocity;
            standingOnPlatform = _standingOnPlatform;
            moveBeforePlatform = _moveBeforePlatform;
        }

    }

    void OnDrawGizmos()
    {
        if (LocalWaypoints != null)
        {
            Gizmos.color = Color.red;
            float size = .3f;
            for (int i =0; i<LocalWaypoints.Length;i++)
            {

                Vector3 GlobalWaypoint = (Application.isPlaying)? globalWayPoints[i] : LocalWaypoints[i] + transform.position;
                Gizmos.DrawLine(GlobalWaypoint - Vector3.up * size,GlobalWaypoint+Vector3.up*size);
                Gizmos.DrawLine(GlobalWaypoint - Vector3.left * size, GlobalWaypoint + Vector3.left * size);
            }

        }
    }
}
