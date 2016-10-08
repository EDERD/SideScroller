using UnityEngine;
using System.Collections;


public class PlayerPhysics : RayCastManager
{

    float maxClimbAngle = 70f;
    float maxDescendAngle = 70f;
    public CollisionInfo collisionInfo;


    public override void Start()
    {
        base.Start();
    }

 //-----------------------------------------Move Functions------------------------------------- 
    public void Move(Vector3 velocity)
    {
        // calculate Corners
        UpdateRaycastOrigins();
        // Reset Collisions Info
        collisionInfo.Reset();

        if(velocity.y< 0)
        {
            DescendSlope(ref velocity);
        }
        if (velocity.x!=0)
        { 
            HorizontalCollisions(ref velocity);
        }
        if(velocity.y != 0)
        { 
            VerticalCollisions(ref velocity);
        }
        // movement
        transform.Translate(velocity);

    }
    // ******************* Simple Move Functions*****************
    void HorizontalCollisions(ref Vector3 velocity)
    {
        float directionX = Mathf.Sign(velocity.x);
        // ray for Horizontal Collision detection (ray length is the movement distance per frame)
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;
        // Ray Collision Detection
        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottonLeft : raycastOrigins.bottonRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i );
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
           
            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal,Vector2.up);
                if(i==0 && slopeAngle <= maxClimbAngle)
                {
                    ClimpSlope(ref velocity,slopeAngle);
                }
                if(!collisionInfo.climbingSlope || slopeAngle > maxClimbAngle) 
                { 
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    rayLength = hit.distance;

                    if(collisionInfo.climbingSlope)
                    {
                        velocity.y = Mathf.Tan(collisionInfo.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
                    }

                    collisionInfo.right = (directionX==1);
                    collisionInfo.left = (directionX ==-1);
                }
            }
        }

    }

    void VerticalCollisions(ref Vector3 velocity)
    {
        float directionY = Mathf.Sign(velocity.y);
        //Ray for vertical Collision Detection
        float rayLength = Mathf.Abs(velocity.y)+ skinWidth;

        for (int i = 0; i < verticalRayCount; i++)
        {
            // depends on Velocity assign the first ray to bottonleft corner or topleft corner
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottonLeft : raycastOrigins.topLeft;
            //Origin of ray
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            RaycastHit2D hit =Physics2D.Raycast(rayOrigin,Vector2.up*directionY,rayLength,collisionMask);
          
            if (hit)
            {
                velocity.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance;

                if (collisionInfo.climbingSlope)
                {
                    velocity.x = velocity.y / Mathf.Tan(collisionInfo.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x );
                }
                
                collisionInfo.above = (directionY == 1);
                collisionInfo.below = (directionY == -1);
            }
        }

    }
    // ******************* Climb Move Functions*****************
    void ClimpSlope(ref Vector3 velocity, float slopeAngle)
    {
        float moveDistance = Mathf.Abs(velocity.x);
        float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        if (velocity.y <= climbVelocityY)
        {
            velocity.y = climbVelocityY;
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
            collisionInfo.below = true;
            collisionInfo.climbingSlope = true;
            collisionInfo.slopeAngle = slopeAngle;
        }

    }

    void DescendSlope(ref Vector3 velocity)
    {
        float directionX = Mathf.Sign(velocity.x);
        Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottonRight : raycastOrigins.bottonLeft;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle != 0 && slopeAngle <= maxDescendAngle)
            {
                if (Mathf.Sign(hit.normal.x) == directionX)
                {
                    if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))
                    {
                        float moveDistance = Mathf.Abs(velocity.x);
                        float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                        velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
                        velocity.y -= descendVelocityY;

                        collisionInfo.slopeAngle = slopeAngle;
                        collisionInfo.descendingSlopes = true;
                        collisionInfo.below = true;
                    }

                }
            }

        }
    }
    //------------------------------------------------------ Ray Implement Function------------------------

    

    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;
        public bool climbingSlope, descendingSlopes;
        public float slopeAngle, oldAngle;
        
        public void Reset()
        {
            climbingSlope = descendingSlopes=false;
            above = below = false;
            left = right = false;
            slopeAngle =oldAngle= 0;

        }    

    }

}
