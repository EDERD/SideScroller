using UnityEngine;
using System.Collections;
[RequireComponent(typeof(PlayerPhysics))]
public class PlayerController : MonoBehaviour {


    // Jump Variables
    public float jumpHeight = 5;
    public float timeToJumpApex = .5f;
    float accelerationTimeAirbone = .5f;  // Air Friction
    float accelerationTimeGrounded = .1f; // Ground Friction
    float jumpVelocity;
    // Movement variables
    float velocityXSmoothing;
    float moveSpeed=0.2f;
    float gravity ;
    Vector3 velocity;
    PlayerPhysics playerPhysics;

    void Start()
    {
        playerPhysics = GetComponent<PlayerPhysics>();
        // Calculate velocity and gravity "ecuation"
        gravity = (-(2 * jumpHeight) / Mathf.Pow(timeToJumpApex,2))*Time.deltaTime;
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex ;
       
    }

    void Update()
    {
        // Stop and reboot velocity in Y axis when have a collision above or below
        if (playerPhysics.collisionInfo.above || playerPhysics.collisionInfo.below)
        {
            velocity.y = 0;
        }
        //Get Inputs
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));
        // Jump action when it's on a floor
        if (Input.GetKeyDown(KeyCode.Space) && playerPhysics.collisionInfo.below)
        {
            velocity.y = jumpVelocity;
        }

        // Implement Acceleration
        float targetVelocityX= input.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x,targetVelocityX,ref velocityXSmoothing,(playerPhysics.collisionInfo.below)? accelerationTimeGrounded:accelerationTimeAirbone);
        //Alway aply Gravity Force
        velocity.y += gravity * Time.deltaTime;
        // Apply Velocity
        playerPhysics.Move(velocity);
    }
}
