using UnityEngine;

/// Attach to a solid obstacle cube. Costs the player a life on contact.
public class Obstacle : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<PlayerController>() != null && GameManager.Instance != null)
        {
            GameManager.Instance.RegisterHit();
        }
    }
}
