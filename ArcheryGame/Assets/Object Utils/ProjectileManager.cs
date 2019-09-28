using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    private Stack<GameObject> projectiles;

    private void Start() {
        this.projectiles = new Stack<GameObject>();
    }

    /// <summary>
    /// Push a new projectile to the stack.
    /// </summary>
    /// <param name="proj">The arrow to push</param>
    public void Spawn(GameObject proj) {
        projectiles.Push(proj);
    }

    /// <returns>The last spawned projectile.</returns>
    public GameObject GetLastSpawned() {
        return projectiles.Peek();
    }

    /// <summary>
    /// Remove the last spawnd arrow from the stack.
    /// </summary>
    public void DestroyLastSpawned() {
        if (projectiles.Count > 0) projectiles.Pop();
    }
}