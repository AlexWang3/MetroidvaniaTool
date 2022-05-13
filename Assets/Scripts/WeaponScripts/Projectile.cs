using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MetroidvaniaTools
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField]
        protected WeaponTypes weapon;

        public bool fired;
        public bool left;
        public float projectileLifeTime;

        private bool flipped;

        // We don't want to reload the scene when we fire, so we can't use Start()
        // This will run when the object is set to activate
        protected virtual void OnEnable()
        {
            projectileLifeTime = weapon.lifeTime;
        }

        protected virtual void FixedUpdate()
        {
           Movement();
        }

        public virtual void Movement()
        {
            if (fired)
            {
                projectileLifeTime -= Time.deltaTime;
                if (projectileLifeTime > 0)
                {
                    if (gameObject.tag == "GrapplingHook")
                    {
                        transform.parent = GameObject.FindWithTag("Player").transform;
                        transform.Translate(Vector2.right * weapon.projectileSpeed * Time.deltaTime);
                        return;
                    }
                    
                    if (!left)
                    {
                        if (flipped)
                        {
                            flipped = false;
                            transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
                        }
                        transform.Translate(Vector2.right * weapon.projectileSpeed * Time.deltaTime);
                    }
                    else
                    {
                        if (!flipped)
                        {
                            flipped = true;
                            transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
                        }
                        transform.Translate(Vector2.left * weapon.projectileSpeed * Time.deltaTime);
                    }
                }
                else
                {
                    DestroyProjectile();
                }
            }
        }

        public virtual void DestroyProjectile()
        {
            projectileLifeTime = weapon.lifeTime;
            gameObject.SetActive(false);
        }
    }
}
