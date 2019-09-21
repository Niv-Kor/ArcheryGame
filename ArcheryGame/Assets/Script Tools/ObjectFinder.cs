using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Script_Tools {
    public class ObjectFinder
    {
        /// <summary>
        /// Get a child of a game object.
        /// </summary>
        /// <param name="obj">The parent game object</param>
        /// <param name="childName">The child's name'</param>
        /// <returns>The game object that is set as the parent's child.</returns>
        public static GameObject GetChild(GameObject obj, String childName) {
            Transform transform = obj.transform;

            for (int i = 0; i < transform.childCount; i++) {
                GameObject child = transform.GetChild(i).gameObject;
                if (child.name.Equals(childName)) return child;
            }

            return null;
        }
    }
}
