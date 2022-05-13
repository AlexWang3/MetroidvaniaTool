using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MetroidvaniaTools
{
    [RequireComponent (typeof (CapsuleCollider2D))]

    public class Crouch : Abilities
    {
        [SerializeField]
        [Range(0, 1)]
        protected float colliderMultiplier; // The factor multiples to the height of collider, defines how much the collider will shrink for crouching
        [SerializeField]
        protected LayerMask layers; // The layermask that the collider will collide with, used to check above
        private CapsuleCollider2D playerCollider; // the Object of Collider itself
        private Vector2 originalCollider; // the size of original collider
        private Vector2 crouchingColliderSize; // the size of crouched collider when crouching
        private Vector2 originalOffset; // the offset of original collider
        private Vector2 crouchingOffset; // the offset of crouched collider when crouching

        protected override void Initialization()
        {
            base.Initialization();
            playerCollider = GetComponent<CapsuleCollider2D>();
            originalCollider = playerCollider.size;
            crouchingColliderSize = new Vector2(playerCollider.size.x, (playerCollider.size.y * colliderMultiplier));
            originalOffset = playerCollider.offset;
            crouchingOffset = new Vector2(playerCollider.offset.x, (playerCollider.offset.y * colliderMultiplier));
        }

        protected virtual void FixedUpdate()
        {
            Crouching();
        }

        // Crouching when appropriate
        protected virtual void Crouching()
        {
            if (input.CrouchHeld() && character.isGrounded)
            {
                character.isCrouching = true;
                anim.SetBool("Crouching", true);
                playerCollider.size = crouchingColliderSize;
                playerCollider.offset = crouchingOffset;
            }
            else
            {
                if (character.isCrouching)
                {
                    if (CollisionCheck(Vector2.up, playerCollider.size.y * .25f, layers))
                    {
                        return;
                    }
                    // when the collider shrinks (i.e. crouching), there will be an tiny time where the character is not grounded
                    // if the character is not grounded, it will play the jump animation, to prevent that:
                    // start an Coroutine to set the animation after collider shrink is finished
                    StartCoroutine(CrouchDisabled());
                }
            }
        }

        // the method did inside the coroutine
        protected virtual IEnumerator CrouchDisabled()
        {
            playerCollider.offset = originalOffset;
            yield return new WaitForSeconds(.01f);
            playerCollider.size = originalCollider;
            yield return new WaitForSeconds(.15f);
            character.isCrouching = false;
            anim.SetBool("Crouching", false);
        }
    }
}
