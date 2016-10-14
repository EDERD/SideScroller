using UnityEngine;
using System.Collections;


public class PlayerPhysics : RayCastManager
{

    public float maxSlopeAngle = 70f;
    public CollisionInfo collisionInfo;
    [HideInInspector]
    public Vector2 playerInput;

    public override void Start()
    {
        base.Start();
    }

    //-----------------------------------------Move Functions------------------------------------- 
    public void Move(Vector2 moveAmount, bool standingOnPlatform = false)
    {
        Move(moveAmount, Vector2.zero,  standingOnPlatform );
    }

    public void Move(Vector2 moveAmount,Vector2 input, bool standingOnPlatform=false)
    {
        playerInput = input;
        // calculate Corners
        UpdateRaycastOrigins();
        // Reset Collisions Info
        collisionInfo.Reset();   

        if(moveAmount.y< 0)
        {
            DescendSlope(ref moveAmount);
        }
        if (moveAmount.x != 0)
        {
            collisionInfo.faceDir = (int)Mathf.Sign(moveAmount.x);
        }
        HorizontalCollisions(ref moveAmount);
        
        if(moveAmount.y != 0)
        { 
            VerticalCollisions(ref moveAmount);
        }
        if (standingOnPlatform)
        {
            collisionInfo.below = true;
        }
        // movement
        transform.Translate(moveAmount);

    }
    // ******************* Simple Move Functions*****************
    void HorizontalCollisions(ref Vector2 moveAmount)
    {
        float directionX = collisionInfo.faceDir;
        // ray for Horizontal Collision detection (ray length is the movement distance per frame)
        float rayLength = Mathf.Abs(moveAmount.x) + skinWidth;
        // Ray Collision Detection

        if (Mathf.Abs(moveAmount.x)< skinWidth)
        {
            rayLength = 2 * skinWidth;
        }

        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottonLeft : raycastOrigins.bottonRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i );
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.right * directionX, Color.red);
            if (hit)
            {
                if (hit.distance==0)
                {
                    continue;
                }

                float slopeAngle = Vector2.Angle(hit.normal,Vector2.up);
           
                if(i==0 && slopeAngle <= maxSlopeAngle)
                {
                    ClimpSlope(ref moveAmount,slopeAngle,hit.normal);
                }
                if(!collisionInfo.climbingSlope || slopeAngle > maxSlopeAngle) 
                { 
                    moveAmount.x = (hit.distance - skinWidth) * directionX;
                    rayLength = hit.distance;

                    if(collisionInfo.climbingSlope)
                    {
                        moveAmount.y = Mathf.Tan(collisionInfo.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x);
                    }

                    collisionInfo.right = (directionX==1);
                    collisionInfo.left = (directionX ==-1);
                }
            }
        }

    }

    void VerticalCollisions(ref Vector2 moveAmount)
    {
        float directionY = Mathf.Sign(moveAmount.y);
        //Ray for vertical Collision Detection
        float rayLength = Mathf.Abs(moveAmount.y)+ skinWidth;

        for (int i = 0; i < verticalRayCount; i++)
        {
            // depends on moveAmount assign the first ray to bottonleft corner or topleft corner
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottonLeft : raycastOrigins.topLeft;
            //Origin of ray
            rayOrigin += Vector2.right * (verticalRaySpacing * i + moveAmount.x);
            RaycastHit2D hit =Physics2D.Raycast(rayOrigin,Vector2.up*directionY,rayLength,collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.up * directionY,Color.red);
            if (hit)
            {
                if (hit.collider.tag=="Through")
                {
                   
                    if(directionY==1)
                    {
                        continue;
                    }
                    if(playerInput.y==-1)
                    {
                        continue;
                    }
                }

                if (hit.distance == 0)
                {
                    continue;
                }

                moveAmount.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance;

                if (collisionInfo.climbingSlope)
                {
                    moveAmount.x = moveAmount.y / Mathf.Tan(collisionInfo.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(moveAmount.x );
                }
                
                collisionInfo.above = (directionY == 1);
                collisionInfo.below = (directionY == -1);
            }
        }

    }
    // ******************* Climb Move Functions*****************
    void ClimpSlope(ref Vector2 moveAmount, float slopeAngle , Vector2 slopeNormal)
    {
        float moveDistance = Mathf.Abs(moveAmount.x);
        float climbmoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        if (moveAmount.y <= climbmoveAmountY)
        {
            moveAmount.y = climbmoveAmountY;
            moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
            collisionInfo.below = true;
            collisionInfo.climbingSlope = true;
            collisionInfo.slopeAngle = slopeAngle;
            collisionInfo.slopeNormal = slopeNormal;
        }

    }

    void DescendSlope(ref Vector2 moveAmount)
    {
        RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast(raycastOrigins.bottonLeft,Vector2.down,Mathf.Abs(moveAmount.y)+skinWidth,collisionMask);
        RaycastHit2D maxSlopeHitRight = Physics2D.Raycast(raycastOrigins.bottonRight, Vector2.down, Mathf.Abs(moveAmount.y) + skinWidth, collisionMask);
        if(maxSlopeHitLeft ^ maxSlopeHitRight)
        {
           
            SlideDownMaxSlope(maxSlopeHitLeft,ref moveAmount);
            SlideDownMaxSlope(maxSlopeHitRight,ref moveAmount);
        }
        if (!collisionInfo.slidingDownMaxSlope)
        { 
            float directionX = Mathf.Sign(moveAmount.x);
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottonRight : raycastOrigins.bottonLeft;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);
   
            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != 0 && slopeAngle <= maxSlopeAngle)
                {
                    if (Mathf.Sign(hit.normal.x) == directionX)
                    {
                        if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x))
                        {
                            float moveDistance = Mathf.Abs(moveAmount.x);
                            float descendmoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                            moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
                            moveAmount.y -= descendmoveAmountY;

                            collisionInfo.slopeAngle = slopeAngle;
                            collisionInfo.descendingSlopes = true;
                            collisionInfo.below = true;
                            collisionInfo.slopeNormal = hit.normal;
                        }

                    }
                }

            }
        }
    }

    void SlideDownMaxSlope(RaycastHit2D hit, ref Vector2 moveAmount)
    {
        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal,Vector2.up);
            if(slopeAngle > maxSlopeAngle)
            {
                moveAmount.x =Mathf.Sign(hit.normal.x)* (Mathf.Abs(moveAmount.y) - hit.distance) / Mathf.Tan(slopeAngle* Mathf.Deg2Rad);
                collisionInfo.slopeAngle = slopeAngle;
                collisionInfo.slidingDownMaxSlope = true;
                collisionInfo.slopeNormal = hit.normal;
            }
        }
    }
    //------------------------------------------------------ Ray Implement Function------------------------

    

    public struct CollisionInfo
    {
        public int faceDir;
        public bool above, below;
        public bool left, right;
        public bool climbingSlope, descendingSlopes;
        public float slopeAngle, oldAngle;
        public bool slidingDownMaxSlope;
        public Vector2 slopeNormal;
        
        public void Reset()
        {
            climbingSlope = descendingSlopes=false;
            above = below = false;
            left = right = false;
            slopeAngle =oldAngle= 0;
            slidingDownMaxSlope = false;
            slopeNormal = Vector2.zero;
        }    

    }

}
