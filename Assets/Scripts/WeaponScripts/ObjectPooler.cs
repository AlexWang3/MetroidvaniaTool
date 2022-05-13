using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MetroidvaniaTools
{
    public class ObjectPooler : MonoBehaviour
    {
        private static ObjectPooler instance;
        public static ObjectPooler Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject obj = new GameObject("ObjectPoller");
                    obj.AddComponent<ObjectPooler>();
                }
                return instance;
            }
        }

        // Awake runs before Start()
        // This will create an Object Pooling System and make sure only one exist per scene 
        // Will see the ObjectPooler inside the DontDestroyOnLoad in the Scene
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        private GameObject currentItem;
        public void CreatePool(WeaponTypes weapon, List<GameObject> currentPool, GameObject projectileParentFolder, Weapon weaponScript)
        {
            weaponScript.totalPools.Add(projectileParentFolder);
            for (int i = 0; i < weapon.amountToPool; i++)
            {
                currentItem = Instantiate(weapon.projectile); // creating the weapon projectile
                currentItem.SetActive(false);
                currentPool.Add(currentItem);
                currentItem.transform.SetParent(projectileParentFolder.transform); // move the projectile instance to the projectile parent folder
            }
            projectileParentFolder.name = weapon.name; // give the folder name as the weapon type name given in Unity (for example: Base Projectile)
            projectileParentFolder.tag = weapon.projectile.tag;
        }

        //Find the first non actived projectile in currentPool
        public virtual GameObject GetObject(List<GameObject> currentPool, WeaponTypes weapon, Weapon weaponScript, GameObject projectileParentFolder, string tag)
        {
            for (int i = 0; i < currentPool.Count; i ++)
            {
                if (!currentPool[i].activeInHierarchy && currentPool[i].tag == tag)
                {
                    if (weapon.canResetPool && weaponScript.bulletsToReset.Count < weapon.amountToPool)
                    {
                        weaponScript.bulletsToReset.Add(currentPool[i]);
                    }
                    return currentPool[i];
                }
            }
            foreach (GameObject item in currentPool)
            {
                if (weapon.canExpandPool && item.tag == tag)
                {
                    currentItem = Instantiate(weapon.projectile); // creating the weapon projectile
                    currentItem.SetActive(false);
                    currentPool.Add(currentItem);
                    currentItem.transform.SetParent(projectileParentFolder.transform); // move the projectile instance to the projectile parent folder
                    return currentItem;
                }
                if (weapon.canResetPool && item.tag == tag)
                {
                    currentItem = weaponScript.bulletsToReset[0];
                    weaponScript.bulletsToReset.RemoveAt(0);
                    currentItem.SetActive(false);
                    weaponScript.bulletsToReset.Add(currentItem);
                    currentItem.GetComponent<Projectile>().DestroyProjectile();
                    return currentItem;
                }
            }
            return null;
        }
    }
}

