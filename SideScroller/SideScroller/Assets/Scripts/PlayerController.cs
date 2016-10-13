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
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        int wallDirX = (playerPhysics.collisionInfo.left) ? -1 : 1;

     
        // Implement Acceleration
        float targetVelocityX = input.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (playerPhysics.collisionInfo.below) ? accelerationTimeGrounded : accelerationTimeAirbone);


        bool wallSliding = false;
        if ((playerPhysics.collisionInfo.left || playerPhysics.collisionInfo.right) && !playerPhysics.collisionInfo.below && velocity.y < 0)
        {
            wallSliding = true;
            
            if (velocity.y < -wallSlideSpeedMax*Time.deltaTime)
            {
                velocity.y = -wallSlideSpeedMax* Time.deltaTime;
            }

            if (timeUnstick > 0)
            {
                velocity.x = 0;
                velocityXSmoothing = 0;
                if (input.x != wallDirX && input.x != 0)
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

        // Stop and reboot velocity in Y axis when have a collision above or below
        if (playerPhysics.collisionInfo.above || playerPhysics.collisionInfo.below)
        {
            velocity.y = 0;
        }
        //Get Inputs
     
        // Jump action when it's on a floor
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (wallSliding)
            {
                if (wallDirX == input.x)
                {
                 
                    velocity.x = -wallDirX * wallJumpClimp.x*Time.deltaTime;
                    velocity.y = wallJumpClimp.y * Time.deltaTime;
                }
                else if (input.x == 0)
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
            else if(playerPhysics.collisionInfo.below)
            { 
             velocity.y = maxJumpVelocity;
            }
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (velocity.y > minJumpVelocity)
            {
                velocity.y = minJumpVelocity;
            }
        }




        //Alway aply Gravity Force
        velocity.y += gravity * Time.deltaTime;
        // Apply Velocity
        playerPhysics.Move(velocity,input);
    }
}
