using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MetroidvaniaTools
{
    // Below allows us to create this scriptable object by right click in Unity
    [CreateAssetMenu(fileName = "WeaponType", menuName = "Metroidvania/Weapons", order = 1)]
    public class WeaponTypes : ScriptableObject
    {
        public GameObject projectile;
        public float projectileSpeed;
        public int amountToPool;
        public float lifeTime;
        public bool automatic;
        public float timeBetweenShoots;
        public bool canExpandPool;
        public bool canResetPool;

        protected virtual void OnEnable()
        {
            if (canExpandPool && canResetPool)
            {
                canResetPool = false;
            }
        }
    }
}