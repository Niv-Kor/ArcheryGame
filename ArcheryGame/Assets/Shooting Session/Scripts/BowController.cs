using UnityEngine;

public class BowController : MonoBehaviour
{
    [Tooltip("The player's bow object")]
    [SerializeField] private GameObject bow;

    private Animator bowAnimator;

    private void Start() {
        this.bowAnimator = bow.GetComponent<Animator>();
    }

    /// <summary>
    /// Activate draw animation.
    /// </summary>
    public void Draw() {
        bowAnimator.SetTrigger("draw");
    }

    /// <summary>
    /// Activate withdraw animation.
    /// </summary>
    public void Withdraw() {
        bowAnimator.SetTrigger("withdraw");
    }

    /// <summary>
    /// Activate fire animation.
    /// </summary>
    public void Fire() {
        bowAnimator.SetTrigger("fire");
    }
}