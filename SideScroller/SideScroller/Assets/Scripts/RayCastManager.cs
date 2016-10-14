using UnityEngine;
using System.Collections;
[RequireComponent(typeof(BoxCollider2D))]

public class RayCastManager : MonoBehaviour {
    [HideInInspector]
    public int horizontalRayCount ;
    [HideInInspector]
    public int verticalRayCount ;
    const float distanceBetweenRays = 0.25f;
    public LayerMask collisionMask;
    public const float skinWidth = 0.015f;
    [HideInInspector]
    public float horizontalRaySpacing;
    [HideInInspector]
    public float verticalRaySpacing;
    [HideInInspector]
    public BoxCollider2D collider;
    [HideInInspector]
    public RaycastOrigins raycastOrigins;

    public virtual void Awake()
    {
        collider = GetComponent<BoxCollider2D>();      
    }

    public virtual void Start()
    {
         // Calculate Space between Rays
        CalculateRaySpacing();
    }


    public void UpdateRaycastOrigins()
    {
        Bounds bounds = collider.bounds; // bounds assign
        bounds.Expand(skinWidth * -2f);

        // Define the corners
        raycastOrigins.bottonLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottonRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    public void CalculateRaySpacing()
    {
        // Reduce a little bit the bounds for collision Detection Later
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        float boundWidth = bounds.size.x;
        float boundHeight = bounds.size.y;
        // the Rays couldn't be less than 2

        horizontalRayCount = Mathf.RoundToInt(boundHeight/distanceBetweenRays);
        verticalRayCount = Mathf.RoundToInt(boundWidth / distanceBetweenRays);
        // Space between Rays
        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }
    //--------------------------------------------------Structs Definition---------------------------------

    public struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottonLeft, bottonRight;

    }
}
