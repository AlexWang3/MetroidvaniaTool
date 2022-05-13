using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MetroidvaniaTools
{
    public class Jump : Abilities
    {
        [SerializeField]
        protected bool limitAirJumps; // wheater limiting the Air Jump velocity or not
        [SerializeField]
        protected int maxJumps; // max number of multiple jump allowed
        [SerializeField]
        protected float jumpForce; // the force of jumping
        [SerializeField]
        protected float holdForce; // Additional force when the button is held down
        [SerializeField]
        protected float buttonHoldTime; // Maxmum time for holding button until reach the max total jumping force
        [SerializeField]
        protected float distanceToCollider; // distance used to check ground collision
        [SerializeField]
        protected float horizontalWallJumpForce; // horizontal Wall jump force
        [SerializeField]
        protected float verticalWallJumpForce; // vertical wall jump force
        [SerializeField]
        protected float maxJumpSpeed; // limit the max jump speed for user with fast multiple jump input
        [SerializeField]
        protected float maxFallSpeed; // limit the max falling speed when the character is falling
        [SerializeField]
        protected float acceptedFallSpeed; // if limitAirJumps is turned on, this will disable jump input when fall speed is too fast
        [SerializeField]
        protected float glideTime; // Time allow to glide
        [SerializeField]
        [Range(-2, 2)]
        protected float gravity; // -2=extreme antigravity, 2=extreme grative, 0=no gravity ...
        [SerializeField]
        protected float wallJumpTime; // Time for Wall Jump

        public LayerMask collisionLayer; // the LayerMask that the player collide with

        private bool isWallJumping;
        private bool justWallJumped;
        private bool flipped;
        private float jumpCountDown;
        private float fallCountDown;
        private float wallJumpCountDown;
        private int numberOfJumpsLeft;

        [HideInInspector]
        public bool downwardJump;

        protected override void Initialization()
        {
            base.Initialization();
            numberOfJumpsLeft = maxJumps;
            jumpCountDown = buttonHoldTime;
            fallCountDown = glideTime;
            wallJumpCountDown = wallJumpTime;
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            CheckForJump();
        }

        //Check if there is a legal jump input
        protected virtual bool CheckForJump()
        {
            if (input.JumpHeld() && input.DownHeld())
            {
                downwardJump = true;
                return false;
            }
            else
            {
                downwardJump = false;
            }
            
            if (input.JumpPressed())
            {
                //disable input when the character fall off from a platform
                if (!character.isGrounded && numberOfJumpsLeft == maxJumps)
                {
                    character.isJumping = false;
                    return false;
                }
                //disable input when the character is falling too fast
                if(limitAirJumps && character.Falling(acceptedFallSpeed))
                {
                    character.isJumping = false;
                    return false;
                }
                if (character.isWallSliding)
                {
                    wallJumpTime = wallJumpCountDown;
                    isWallJumping = true;
                    return false;
                }
                numberOfJumpsLeft--;
                if (numberOfJumpsLeft >= 0)
                {
                    rb.velocity = new Vector2(rb.velocity.x, 0);
                    jumpCountDown = buttonHoldTime;
                    character.isJumping = true;
                    fallCountDown = glideTime;
                }
                return true;
            }
            else
                return false;
        }

        protected virtual void FixedUpdate()
        {
            IsJumping();
            Gliding();
            GroundCheck();
            WallSliding();
            WallJump();
        }

        // isJumping bool variable is updated in JumpPressed, true if there is a legal jump input
        // Assgin proper velocity to rb when jumping
        protected virtual void IsJumping()
        {
            if (character.isJumping)
            {
                rb.AddForce(Vector2.up * jumpForce); // Vector2.up is equavlent to Vector2(0,1)
                AdditionalAir();
            }
            // limit max jump speed
            if (rb.velocity.y > maxJumpSpeed)
            {
                rb.velocity = new Vector2(rb.velocity.x, maxJumpSpeed);
            }
            if (downwardJump)
            {
                DownwardJump();
            }
        }

        protected virtual void DownwardJump()
        {
            rb.AddForce(Vector2.down * jumpForce);
        }

        protected virtual void Gliding()
        {
            if (character.Falling(0) && input.JumpHeld() && !downwardJump)
            {
                fallCountDown -= Time.deltaTime;
                if(fallCountDown > 0 && rb.velocity.y > acceptedFallSpeed)
                {
                    anim.SetBool("Gliding", true);
                    FallSpeed(gravity);
                    return;
                }
            }
            anim.SetBool("Gliding", false);
        }

        // Add additional force while hoding on jump input
        protected virtual void AdditionalAir()
        {
            if (input.JumpHeld())
            {
                jumpCountDown -= Time.deltaTime;
                if (jumpCountDown <= 0)
                {
                    jumpCountDown = 0;
                    character.isJumping = false;
                }
                else
                {
                    rb.AddForce(Vector2.up * holdForce);
                }
            }
            else
            {
                character.isJumping = false;
            }
        }

        // To check if there is a collide with the ground.
        // Do proper routine for colliding/not colliding with the ground
        protected virtual void GroundCheck()
        {
            if (CollisionCheck(Vector2.down, distanceToCollider, collisionLayer) && !isJumping)
            {
                if (currentPlatform.GetComponent<MoveablePlatform>())
                {
                    transform.parent = currentPlatform.transform;
                }
                anim.SetBool("Grounded", true);
                character.isGrounded = true;
                numberOfJumpsLeft = maxJumps;
                fallCountDown = glideTime;
                justWallJumped = false;
            }
            else
            {
                transform.parent = null;
                anim.SetBool("Grounded", false);
                character.isGrounded = false;
                //limit the max fall speed when falling
                if(character.Falling(0) && rb.velocity.y < maxFallSpeed)
                {
                    rb.velocity = new Vector2(rb.velocity.x, maxFallSpeed);
                }
            }
            anim.SetFloat("VerticalSpeed", rb.velocity.y);
        }

        protected virtual bool WallCheck()
        {
            if ((!character.isFacingRight && CollisionCheck(Vector2.right, distanceToCollider, collisionLayer) || character.isFacingRight && CollisionCheck(Vector2.left, distanceToCollider, collisionLayer)) && movement.MovementPressed() && !character.isGrounded)
            {
                if (currentPlatform.GetComponent<OneWayPlatform>() || currentPlatform.GetComponent<Ladder>())
                {
                    return false;
                } 

                if (justWallJumped)
                {
                    wallJumpTime = 0;
                    justWallJumped = false;
                    isWallJumping = false;
                    movement.enabled = true;
                }
                return true;
            }
            return false;
        }
        
        protected virtual bool WallSliding()
        {
            if (WallCheck())
            {
                if (!flipped)
                {
                    Flip();
                    flipped = true;
                }
                FallSpeed(gravity); // sliding with speed same to giliding here
                character.isWallSliding = true;
                anim.SetBool("WallSliding", true);
                return true;
            }
            else
            {
                character.isWallSliding = false;
                anim.SetBool("WallSliding", false);
                if (flipped && !isWallJumping)
                {
                    Flip();
                    flipped = false;
                }
                return false;
            }
        }

        protected virtual void WallJump()
        {
            if (isWallJumping)
            {
                rb.AddForce(Vector2.up * verticalWallJumpForce);
                if (!character.isFacingRight)
                {
                    rb.AddForce(Vector2.left * horizontalWallJumpForce);
                }
                if (character.isFacingRight)
                {
                    rb.AddForce(Vector2.right * horizontalWallJumpForce);
                }
                movement.enabled = false;
                Invoke("JustWallJumped", .05f);
            }
            if (wallJumpTime > 0)
            {
                wallJumpTime -= Time.deltaTime;
                if (wallJumpTime <= 0)
                {
                    movement.enabled = true;
                    isWallJumping = false;
                    wallJumpTime = 0;
                }
            }
        }

        protected virtual void JustWallJumped()
        {
            justWallJumped = true;
        }
    }
}
