using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsObject : MonoBehaviour {

    public float minGroundNormalY = 0.65f;
    protected bool grounded;
    protected Vector2 groundNormal;

    protected Vector2 velocity; //protected so that classes that extend can get it but not accessible outside
    public float gravityModifier = 1f; //so that we can scale gravity differently

    protected Vector2 targetVelocity; //comes from another script

    protected Rigidbody2D rb2d;

    protected const float minMoveDistance = 0.001f;
    protected const float shellRadius = 0.01f;

    protected ContactFilter2D contactFilter2D;
    protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];
    protected List<RaycastHit2D> hitBufferList = new List<RaycastHit2D>(16);

    void OnEnable()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Use this for initialization
    void Start() {
        contactFilter2D.useTriggers = false;
        contactFilter2D.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer)); //setting layer collision matrix
        contactFilter2D.useLayerMask = true;
    }

    // Update is called once per frame
    void Update() {
        targetVelocity = Vector2.zero;
        ComputeVelocity();
    }

    protected virtual void ComputeVelocity()
    {

    }

    void FixedUpdate()
    {
        velocity += gravityModifier * Physics2D.gravity * Time.deltaTime;
        velocity.x = targetVelocity.x;

        grounded = false;

        Vector2 deltaPosition = velocity * Time.deltaTime;

        //X STUFF
        Vector2 moveAlongGround = new Vector2(groundNormal.y, -groundNormal.x); //calculates vector normal to the groundNormal. this makes it parallel to the ground

        Vector2 move = moveAlongGround * deltaPosition.x;

        Movement(move, false);

        //Y STUFF
        move = Vector2.up * deltaPosition.y; //eventually will pass this to our movement function

        Movement(move, true);
    }

    void Movement(Vector2 move, bool yMovement)
    {
        float distance = move.magnitude;

        if(distance > minMoveDistance)
        {
            //check the frame ahead of us to see if this collider will overlap with any other colliders in the scene
           int count = rb2d.Cast(move, contactFilter2D, hitBuffer, minMoveDistance + shellRadius);

            //list of objects that overlap with us
            hitBufferList.Clear();
            for ( int i = 0; i < count; i++)
            {
                hitBufferList.Add(hitBuffer[i]);
            }

            //check the normal vector of each of the things we are colliding with
            for ( int i = 0; i < hitBufferList.Count; i++)
            {
                Vector2 currentNormal = hitBufferList[i].normal;
                //check if player is grounded to play idling animation or falling animation
                //NOTE : this doesnt allow player to slide unless horizontal movement is indicated

                if(currentNormal.y > minGroundNormalY)
                {
                    grounded = true;
                    if (yMovement)
                    {
                        groundNormal = currentNormal;
                        currentNormal.x = 0;
                    }
                }
                float projection = Vector2.Dot(velocity, currentNormal);
                if (projection < 0)
                {
                    velocity = velocity - projection * currentNormal;
                }

                float modifiedDistance = hitBufferList[i].distance - shellRadius;
                distance = modifiedDistance < distance ? modifiedDistance : distance;
            }

        }
        rb2d.position = rb2d.position + move.normalized * distance;
    }


}
