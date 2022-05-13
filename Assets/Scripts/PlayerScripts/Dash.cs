using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MetroidvaniaTools
{
    [RequireComponent(typeof(CapsuleCollider2D))]

    public class Dash : Abilities
    {
        [SerializeField]
        protected float dashForce; // The froce applied to rb when dash
        [SerializeField]
        protected float dashCoolDownTime; // The time need to wait until next dash
        [SerializeField]
        protected float dashAmountTime; // The duration of dash
        [SerializeField]
        protected LayerMask dashingLayers; // The layer than dashing can pass through

        private bool canDash;
        private float dashCountDown;
        private CapsuleCollider2D capsuleCollider2D;
        private Vector2 deltaPosition;

        protected override void Initialization()
        {
            base.Initialization();
            capsuleCollider2D = GetComponent<CapsuleCollider2D>();
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            Dashing();
        }

        // Dashing initialization and call Finish Dashing coroutine
        protected virtual void Dashing()
        {
            if (input.DashPressed() && canDash)
            {
                deltaPosition = transform.position;
                dashCountDown = dashCoolDownTime;
                character.isDashing = true;

                // Config the shape of collider when dashing
                capsuleCollider2D.direction = CapsuleDirection2D.Horizontal;
                capsuleCollider2D.size = new Vector2(capsuleCollider2D.size.y, capsuleCollider2D.size.x);

                anim.SetBool("Dashing", true);
                StartCoroutine(FinishedDashing());
            }
        }

        protected virtual void FixedUpdate()
        {
            DashMode();
            ResetDashCounter();
        }

        // Actual Dash logic, add force, disable collided colliders
        protected virtual void DashMode()
        {
            if (character.isDashing)
            {
                FallSpeed(0); // Disable the gravity influence while dashing
                movement.enabled = false; // Disable the Horizontal Movement sript, do not taken moving input
                if (!character.isFacingRight)
                {
                    DashCollision(Vector2.right, .5f, dashingLayers);
                    rb.AddForce(Vector2.right * dashForce);
                }
                else
                {
                    DashCollision(Vector2.left, .5f, dashingLayers);
                    rb.AddForce(Vector2.left * dashForce);
                }
            }
        }

        //Check if the Character collides with another collider that has certain laymask and disable that collider
        protected virtual void DashCollision(Vector2 direction, float distance, LayerMask collision)
        {
            RaycastHit2D[] hits = new RaycastHit2D[10];
            int numHits = col.Cast(direction, hits, distance);
            for (int i = 0; i < numHits; i++)
            {
                if ((1 << hits[i].collider.gameObject.layer & collision) != 0)
                {
                    hits[i].collider.enabled = false;
                    StartCoroutine(TrunColliderBackOn(hits[i].collider.gameObject));
                }
            }
        }

        // Reset Dash Counter for dash count down logic
        protected virtual void ResetDashCounter()
        {
            if (dashCountDown > 0)
            {
                canDash = false;
                dashCountDown -= Time.deltaTime;
            }
            else
            {
                canDash = true;
            }
        }

        // Coroutine for Dashing, wait dashAmountTime, then set everthing back
        protected virtual IEnumerator FinishedDashing()
        {
            yield return new WaitForSeconds(dashAmountTime);
            capsuleCollider2D.direction = CapsuleDirection2D.Vertical;
            capsuleCollider2D.size = new Vector2(capsuleCollider2D.size.y, capsuleCollider2D.size.x);
            anim.SetBool("Dashing", false);
            character.isDashing = false;
            FallSpeed(1); // Enable gravity influence
            movement.enabled = true;
            rb.velocity = new Vector2(0, rb.velocity.y);
            RaycastHit2D[] hits = new RaycastHit2D[10];
            yield return new WaitForSeconds(.1f);
            hits = Physics2D.CapsuleCastAll(new Vector2(col.bounds.center.x, col.bounds.center.y + .05f), new Vector2(col.bounds.size.x, col.bounds.size.y - .1f), CapsuleDirection2D.Vertical, 0, Vector2.zero, 0, jump.collisionLayer);
            if (hits.Length > 0)
            {
                transform.position = deltaPosition;
            }
        }

        // Coroutine to turn the collided collider back on
        protected virtual IEnumerator TrunColliderBackOn(GameObject obj)
        {
            yield return new WaitForSeconds(dashAmountTime);
            obj.GetComponent<Collider2D>().enabled = true;
        }
    }
}
