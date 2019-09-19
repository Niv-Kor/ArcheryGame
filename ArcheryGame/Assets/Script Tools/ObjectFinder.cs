using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Script_Tools {
    public class ObjectFinder
    {
        public static Transform GetChild(Transform transform, String childName) {
            for (int i = 0; i < transform.childCount; i++) {
                Transform obj = transform.GetChild(i);
                if (obj.name.Equals(childName)) return obj;
            }

            return null;
        }
    }
}
