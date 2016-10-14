using UnityEngine;
using System.Collections;
[RequireComponent(typeof(PlayerPhysics))]
public class PlayerController : MonoBehaviour {


    // Jump Variables
    public float maxJumpHeight = 5;
    public float minJumHeight = 1;
    public float timeToJumpApex = .5f;
    float accelerationTimeAirbone = .5f;  // Air Friction
    float accelerationTimeGrounded = .1f; // Ground Friction
    float maxJumpVelocity;
    float minJumpVelocity;
    // Movement variables
    float velocityXSmoothing;
    float moveSpeed=0.2f;
    float gravity ;
    Vector3 velocity;
    PlayerPhysics playerPhysics;
    // wall movement
    public float wallSlideSpeedMax =3;
    public Vector2 wallJumpClimp;
    public Vector2 wallJumpOff;
    public Vector2 wallLeap;
    public float wallStickTime = .25f;
    float timeUnstick;
    bool wallSliding;
    int wallDirX;

    Vector2 directionalInput;
    

    void Start()
    {
        playerPhysics = GetComponent<PlayerPhysics>();
        // Calculate velocity and gravity "ecuation"
        gravity = (-(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex,2))*Time.deltaTime;
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex ;
        minJumpVelocity = Mathf.Sqrt(2* Mathf.Abs(gravity)*minJumHeight*Time.deltaTime);
    }
    void Update()
    {
    
        // Implement Acceleration
        CalculateVelocity();
        HandleWallSliding();

        playerPhysics.Move(velocity, directionalInput);
        // Stop and reboot velocity in Y axis when have a collision above or below
        if (playerPhysics.collisionInfo.above || playerPhysics.collisionInfo.below)
        {
            if(playerPhysics.collisionInfo.slidingDownMaxSlope)
            { 
                velocity.y += playerPhysics.collisionInfo.slopeNormal.y * -gravity*Time.deltaTime;
            }
            else
            {
                velocity.y = 0;
            }
        }
    }

    public void SetDirectionalInput(Vector2 Input)
    {
        directionalInput = Input;
    }

    public void JumpInputDown()
    {
        
        if (wallSliding)
        {
            if (wallDirX == directionalInput.x)
            {

                velocity.x = -wallDirX * wallJumpClimp.x * Time.deltaTime;
                velocity.y = wallJumpClimp.y * Time.deltaTime;
            }
            else if (directionalInput.x == 0)
            {
                velocity.x = -wallDirX * wallJumpOff.x * Time.deltaTime;
                velocity.y = wallJumpOff.y * Time.deltaTime;
            }
            else
            {

                velocity.x = -wallDirX * wallLeap.x * Time.deltaTime;
                velocity.y = wallLeap.y * Time.deltaTime;
            }
        }
        else if (playerPhysics.collisionInfo.below)
        {
            if (playerPhysics.collisionInfo.slidingDownMaxSlope)
            {
              
                if (directionalInput.x != -Mathf.Sign(playerPhysics.collisionInfo.slopeNormal.x))
                {
                    
                    velocity.y = maxJumpVelocity * playerPhysics.collisionInfo.slopeNormal.y;
                    velocity.x = maxJumpVelocity * playerPhysics.collisionInfo.slopeNormal.x;
                }
            }
            else
            {
                velocity.y = maxJumpVelocity;
            }
                
        }
    }

    public void JumpInputUp()
    {
        if (velocity.y > minJumpVelocity)
        {
            velocity.y = minJumpVelocity;
        }
    }

   

    void CalculateVelocity()
    {
        float targetVelocityX = directionalInput.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (playerPhysics.collisionInfo.below) ? accelerationTimeGrounded : accelerationTimeAirbone);
        velocity.y += gravity * Time.deltaTime;
    }
    void HandleWallSliding()
    {
        wallDirX = (playerPhysics.collisionInfo.left) ? -1 : 1;
        wallSliding = false;
        if ((playerPhysics.collisionInfo.left || playerPhysics.collisionInfo.right) && !playerPhysics.collisionInfo.below && velocity.y < 0)
        {
            wallSliding = true;

            if (velocity.y < -wallSlideSpeedMax * Time.deltaTime)
            {
                velocity.y = -wallSlideSpeedMax * Time.deltaTime;
            }

            if (timeUnstick > 0)
            {
                velocity.x = 0;
                velocityXSmoothing = 0;
                if (directionalInput.x != wallDirX && directionalInput.x != 0)
                {
                    timeUnstick -= Time.deltaTime;
                }
                else
                {
                    timeUnstick = wallStickTime;
                }
            }
            else
            {
                timeUnstick = wallStickTime;
            }
        }
    }
}
