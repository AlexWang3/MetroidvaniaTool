using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.IK;

namespace MetroidvaniaTools
{
    public class AimManager : Abilities
    {
        public Solver2D aimingGun;  // the IK at Gun Tip with aiming direction as target
        public Solver2D aimingLeftHand; // the IK at left hand with somewhere on the gun as target to 'hold gun'
        public Solver2D notAimingGun; // the normal IK at Gun Tip without pointing aiming direction
        public Solver2D notAimingLeftHand; // the normal IK at left hand
        public Transform whereToAim; // the target aiming direction, will be updated in the PointGun method in Weapon script
        public Transform whereToPlaceHand; // The target on the gun where the left hand should hold
        public Transform origin; // origin of bounds, Assign the parent bone of the gun
        public Bounds bounds; // The bounds need to determin forward direction

        [SerializeField]
        protected float autoTargetRadius; // The radius of the circle to detect auto aiming target

        private bool lockedOn; // wheather or not a target is locked
        [HideInInspector]
        public bool aiming;

        protected override void Initialization()
        {
            base.Initialization();
            aimingGun.enabled = false;
            aimingLeftHand.enabled = false;
            bounds.center = origin.position;
        }

        protected virtual void FixedUpdate()
        {
            Aiming();
            DirectionalAim();
            bounds.center = origin.position;
        }

        protected virtual void Aiming()
        {
            if (input.AimingHeld() || DirectionalAim())
            {
                if (input.AimingHeld())
                {
                    CheckForTargets();
                    if (!lockedOn && !DirectionalAim())
                    {
                        NotAiming();
                        return;
                    }
                }
                ChangeArms();
                aiming = true;
                return;
            }
            NotAiming();
        }

        // Get all the collider of targets inside a circle
        protected virtual void CheckForTargets()
        {
            GameObject[] targets;
            // The circle is centerer at gunBarrel position with radius of autoTargetRadius
            Collider2D[] colliders = Physics2D.OverlapCircleAll(weapon.gunBarrel.position, autoTargetRadius);
            if (colliders.Length > 0)
            {
                targets = new GameObject[colliders.Length];
                for (int i = 0; i < colliders.Length; i ++)
                {
                    targets[i] = colliders[i].gameObject;
                }
                LockOnTarget(targets);
            }
        }

        // Lock on the closed target with in all the target get in CheckForTargets.
        protected virtual GameObject LockOnTarget(GameObject[] targets)
        {
            Transform closestTarget = null;
            float closestDistanceSqr = Mathf.Infinity;
            Vector3 currentPosition = transform.position;
            foreach (GameObject potentialTarget in targets)
            {
                if (potentialTarget.tag == "Target")
                {
                    Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
                    float dSqrToTarget = directionToTarget.sqrMagnitude;
                    if (dSqrToTarget < closestDistanceSqr)
                    {
                        closestDistanceSqr = dSqrToTarget;
                        closestTarget = potentialTarget.transform;
                    }
                }
            }
            if (closestTarget != null)
            {
                lockedOn = true;
                whereToAim.transform.position = closestTarget.position;
                aimingGun.transform.GetChild(0).position = whereToAim.transform.position;
                aimingLeftHand.transform.GetChild(0).position = whereToPlaceHand.transform.position;
                return closestTarget.gameObject;
            }
            lockedOn = false;
            return null;
        }

        // set variables back when there is no autoaiming
        protected virtual void NotAiming()
        {
            lockedOn = false;
            aiming = false;
            ChangeArms();
        }

        // 8 direction aiming
        public virtual bool DirectionalAim()
        {
            if (character.isOnLadder)
            {
                return false;
            }
            
            if (input.UpHeld())
            {
                whereToAim.transform.position = new Vector2(bounds.center.x, bounds.max.y);
                return true;
            }
            if (input.DownHeld())
            {
                whereToAim.transform.position = new Vector2(bounds.center.x, bounds.min.y);
                return true;
            }
            if (input.TiltedUpHeld())
            {
                if (!character.isFacingRight)
                {
                    whereToAim.transform.position = new Vector2(bounds.max.x, bounds.max.y);
                }
                else
                {
                    whereToAim.transform.position = new Vector2(bounds.min.x, bounds.max.y);
                }
                return true;
            }
            if (input.TiltedDownHeld())
            {
                if (!character.isFacingRight)
                {
                    whereToAim.transform.position = new Vector2(bounds.max.x, bounds.min.y);
                }
                else
                {
                    whereToAim.transform.position = new Vector2(bounds.min.x, bounds.min.y);
                }
                return true;
            }
            return false;
        }

        // Enable or disable IK solve script to switch between normal and point gun
        public virtual void ChangeArms()
        {
            if (weapon.currentTimeTillChangeArms > 0 || aiming)
            {
                notAimingGun.enabled = false;
                notAimingLeftHand.enabled = false;
                aimingGun.enabled = true;
                aimingLeftHand.enabled = true;
            }
            if (weapon.currentTimeTillChangeArms < 0 && !aiming)
            {
                notAimingGun.enabled = true;
                notAimingLeftHand.enabled = true;
                aimingGun.enabled = false;
                aimingLeftHand.enabled = false;
            }
        }

        // Draw in Unity Scene:
        // 1. bounds for 8 direction aiming
        // 2. circle for the range of detecting autoaiming
        private void OnDrawGizmos()
        {
            weapon = GetComponent<Weapon>();
            Gizmos.DrawWireSphere(weapon.gunBarrel.position, autoTargetRadius);
            Gizmos.DrawWireCube(origin.position, bounds.size);
        }
    }

}