using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MetroidvaniaTools
{
    public class HorizontalMovement : Abilities
    {
        [SerializeField] //Allow the variables to be set in inspector but still not public
        protected float timeTillMaxSpeed; // time until the character reach the max speed when the user hold on moving input
        [SerializeField]
        protected float maxSpeed; // the limit of maxspeed
        [SerializeField]
        protected float sprintMultiplier; // the amount of speed multiplied when sprint input is holded
        [SerializeField]
        protected float crouchSpeedMultiplier; // the amount of speed multiplied when crouching
        [SerializeField]
        protected float hookSpeedMultiplier;
        [SerializeField]
        protected float ladderSpeed;
        [HideInInspector]
        public GameObject currentLadder;

        protected bool above;
        // below are the private fields required to calcualte the velocity and assign to rb
        private float acceleration;
        private float currentSpeed;
        private float horizontalInput;
        private float runTime;

        protected override void Initialization()
        {
            base.Initialization();
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            MovementPressed();
        }

        // Check if there is a legal move input
        public virtual bool MovementPressed()
        {
            if (Input.GetAxis("Horizontal") != 0)
            {
                horizontalInput = Input.GetAxis("Horizontal");
                return true;
            }
            else
                return false;
        }

        protected virtual void FixedUpdate()
        {
            Movement();
            RemoveFromGrapple();
            LadderMovement();
        }

        // Calculate velocity and assign to rb when there is a move input
        protected virtual void Movement()
        {
            if (MovementPressed())
            {
                anim.SetBool("Moving", true);
                acceleration = maxSpeed / timeTillMaxSpeed;
                runTime += Time.deltaTime;
                currentSpeed = horizontalInput * acceleration * runTime;
                CheckDirection();
            }
            else
            {
                anim.SetBool("Moving", false);
                acceleration = 0;
                runTime = 0;
                currentSpeed = 0;
            }
            SpeedMultiplier();
            anim.SetFloat("CurrentSpeed", currentSpeed);
            rb.velocity = new Vector2(currentSpeed, rb.velocity.y);
        }

        protected virtual void RemoveFromGrapple()
        {
            if (grapplingHook.removed)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.identity, Time.deltaTime * 500);
                if (transform.rotation == Quaternion.identity)
                {
                    grapplingHook.removed = false;
                    rb.freezeRotation = true;
                }
            }
        }

        protected virtual void LadderMovement()
        {
            if (character.isOnLadder && character != null)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.velocity = new Vector2(rb.velocity.x, 0);
                if (col.bounds.min.y >= (currentLadder.GetComponent<Ladder>().topOfLadder.y - col.bounds.extents.y))
                {
                    anim.SetBool("OnLadder", false);
                    above = true;
                }
                else
                {
                    anim.SetBool("OnLadder", true);
                    above = false;
                }
                if (input.UpHeld())
                {
                    anim.SetBool("ClimbingLadder", true);
                    transform.position = Vector2.MoveTowards(transform.position, currentLadder.GetComponent<Ladder>().topOfLadder, ladderSpeed * Time.deltaTime);
                    if (above)
                    {
                        anim.SetBool("ClimbingLadder", false);
                    }
                    return;
                }
                else
                {
                    anim.SetBool("ClimbingLadder", false);
                }

                if (input.DownHeld())
                {
                    anim.SetBool("ClimbingLadder", true);
                    transform.position = Vector2.MoveTowards(transform.position, currentLadder.GetComponent<Ladder>().bottomOfLadder, ladderSpeed * Time.deltaTime);
                    return;
                }
            }
            else
            {
                anim.SetBool("OnLadder", false);
                rb.bodyType = RigidbodyType2D.Dynamic;
            }
        }

        // Check the direction of the character and Flip when there is a need
        protected virtual void CheckDirection()
        {
            if (currentSpeed > 0)
            {
                if (character.isFacingRight)
                {
                    character.isFacingRight = false;
                    //transform.position = new Vector2(transform.position.x + 1.45f, transform.position.y);
                    Flip();
                }
                if (currentSpeed > maxSpeed)
                {
                    currentSpeed = maxSpeed;
                }
            }
            if (currentSpeed < 0)
            {
                if (!character.isFacingRight)
                {
                    character.isFacingRight = true;
                    //transform.position = new Vector2(transform.position.x - 1.45f, transform.position.y);
                    Flip();
                }
                if (currentSpeed < -maxSpeed)
                {
                    currentSpeed = -maxSpeed;
                }
            }
        }

        // muliply the current speed when sprinting, i.e. hold on left shift will make the charater go faster
        protected virtual void SpeedMultiplier()
        {
            if (input.SprintingHeld())
            {
                currentSpeed *= sprintMultiplier;
            }
            if (character.isCrouching)
            {
                currentSpeed *= crouchSpeedMultiplier;
            }
            if (grapplingHook.connected)
            {
                if (input.UpHeld() || input.DownHeld() || CollisionCheck(Vector2.right, .1f, jump.collisionLayer) || CollisionCheck(Vector2.left, .1f, jump.collisionLayer) 
                    || CollisionCheck(Vector2.down, .1f, jump.collisionLayer) || CollisionCheck(Vector2.up, .1f, jump.collisionLayer) || character.isGrounded)
                {
                    return;
                }
                currentSpeed *= hookSpeedMultiplier;
                if (grapplingHook.hookTrail.transform.position.y > grapplingHook.objectConnectedTo.transform.position.y)
                {
                    currentSpeed *= -hookSpeedMultiplier;
                }
                rb.rotation -= currentSpeed;
            }

            if (character.isWallSliding)
            {
                currentSpeed = 0;
            }
            if (!character.isFacingRight && CollisionCheck(Vector2.right, .05f, jump.collisionLayer) || character.isFacingRight && CollisionCheck(Vector2.left, .05f, jump.collisionLayer))
            {
                if (currentPlatform != null && (currentPlatform.GetComponent<OneWayPlatform>() || currentPlatform.GetComponent<Ladder>()))
                {
                    return;
                }
                currentSpeed = .01f;
            }
        }
    }
}

