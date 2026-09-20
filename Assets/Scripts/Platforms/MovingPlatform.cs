using System.Collections.Generic;
using UnityEngine;

/// Ping-pongs a kinematic platform between two offsets from its start position,
/// and carries along any rigidbody resting on top of it.
[RequireComponent(typeof(Rigidbody))]
public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Vector3 pointA = Vector3.zero;
    [SerializeField] private Vector3 pointB = new Vector3(3f, 0f, 0f);
    [SerializeField] private float speed = 2f;

    private Rigidbody rb;
    private Vector3 worldA;
    private Vector3 worldB;
    private Vector3 targetPoint;
    private Vector3 previousPosition;
    private readonly HashSet<Rigidbody> riders = new HashSet<Rigidbody>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;

        worldA = transform.position + pointA;
        worldB = transform.position + pointB;
        targetPoint = worldB;
        previousPosition = rb.position;
    }

    private void FixedUpdate()
    {
        Vector3 newPosition = Vector3.MoveTowards(rb.position, targetPoint, speed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);

        Vector3 delta = newPosition - previousPosition;
        previousPosition = newPosition;

        if (delta != Vector3.zero)
        {
            CarryRiders(delta);
        }

        if (Vector3.Distance(newPosition, targetPoint) < 0.05f)
        {
            targetPoint = targetPoint == worldA ? worldB : worldA;
        }
    }

    private void CarryRiders(Vector3 delta)
    {
        foreach (Rigidbody rider in riders)
        {
            if (rider != null)
            {
                rider.MovePosition(rider.position + delta);
            }
        }
    }

    private void OnCollisionEnter(Collision collision) => TryTrackRider(collision);

    private void OnCollisionStay(Collision collision) => TryTrackRider(collision);

    private void OnCollisionExit(Collision collision)
    {
        if (collision.rigidbody != null)
        {
            riders.Remove(collision.rigidbody);
        }
    }

    private void TryTrackRider(Collision collision)
    {
        if (collision.rigidbody == null)
        {
            return;
        }

        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y > 0.5f)
            {
                riders.Add(collision.rigidbody);
                return;
            }
        }
    }
}
