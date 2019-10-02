using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    private Stack<GameObject> projectiles;

    private void Start() {
        this.projectiles = new Stack<GameObject>();
    }

    public void Spawn(GameObject proj) {
        projectiles.Push(proj);
    }

    public GameObject GetLastSpawned() {
        return projectiles.Peek();
    }

    public void DestroyLastSpawned() {
        projectiles.Pop();
    }
}