using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MetroidvaniaTools
{
    public class OneWayPlatform : PlatformManager
    {
        [SerializeField]
        protected enum OneWayPlatforms { GoingUp, GoingDown, Both }
        [SerializeField]
        protected OneWayPlatforms type;
        [SerializeField]
        protected float delay = 0.5f;

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject == player)
            {
                if (!character.isGrounded && player.GetComponent<Collider2D>().bounds.min.y < platformCollider.bounds.center.y && (type == OneWayPlatforms.Both || type == OneWayPlatforms.GoingUp))
                {
                    Physics2D.IgnoreCollision(player.GetComponent<Collider2D>(), platformCollider, true);
                    StartCoroutine(StopIgnoring());
                }
            }
        }

        protected virtual void OnCollisionStay2D(Collision2D collision)
        {
            if (collision.gameObject == player)
            {
                if (player.GetComponent<Jump>().downwardJump && player.GetComponent<Collider2D>().bounds.min.y > platformCollider.bounds.center.y && (type == OneWayPlatforms.Both || type == OneWayPlatforms.GoingDown))
                {
                    Physics2D.IgnoreCollision(player.GetComponent<Collider2D>(), platformCollider, true);
                    StartCoroutine(StopIgnoring());
                }
            }
        }

        protected virtual IEnumerator StopIgnoring()
        {
            yield return new WaitForSeconds(delay);
            Physics2D.IgnoreCollision(player.GetComponent<Collider2D>(), platformCollider, false);
        }
    }

}
