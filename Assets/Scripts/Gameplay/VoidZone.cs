using UnityEngine;

/// Place a trigger volume below the floor row (spanning the gaps).
/// Costs the player a life if they fall into it.
[RequireComponent(typeof(Collider))]
public class VoidZone : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null && GameManager.Instance != null)
        {
            GameManager.Instance.RegisterHit();
        }
    }
}
