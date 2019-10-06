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
    /// <param name="proj">The projectile to add</param>
    public void Spawn(GameObject proj) {
        projectiles.Push(proj);
    }

    /// <returns>The first projectile in the stack.</returns>
    public GameObject GetLastSpawned() {
        return projectiles.Peek();
    }

    /// <summary>
    /// Destroy the first projectile in the stack.
    /// </summary>
    public void DestroyLastSpawned() {
        if (projectiles.Count > 0) projectiles.Pop();
    }
}