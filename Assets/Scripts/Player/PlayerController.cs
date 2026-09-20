using UnityEngine;
using UnityEngine.InputSystem;

/// Arcade-style ball controller: forward/left/right movement plus a jump, driven by the new Input System.
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private LayerMask groundLayers = ~0;

    private Rigidbody rb;
    private bool isGrounded;
    private bool controlsEnabled = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (!controlsEnabled)
        {
            return;
        }

        isGrounded = groundCheck != null &&
            Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayers, QueryTriggerInteraction.Ignore);

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && isGrounded && keyboard.spaceKey.wasPressedThisFrame)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }
    }

    private void FixedUpdate()
    {
        if (!controlsEnabled)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;
        float forward = 0f;
        float strafe = 0f;

        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) forward += 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) strafe += 1f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) strafe -= 1f;
        }

        Vector3 moveDirection = new Vector3(strafe, 0f, forward);
        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }

        Vector3 targetVelocity = moveDirection * moveSpeed;
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
    }

    /// Used by GameManager to send the player back to a safe point after losing a life.
    public void ResetToPosition(Vector3 position)
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = position;
    }

    /// Used by GameManager to freeze the player once the game is over.
    public void SetControlsEnabled(bool enabled)
    {
        controlsEnabled = enabled;
        if (!enabled)
        {
            rb.linearVelocity = Vector3.zero;
        }
    }
}
