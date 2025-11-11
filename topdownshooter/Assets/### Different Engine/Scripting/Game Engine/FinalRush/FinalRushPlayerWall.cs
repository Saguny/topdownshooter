using UnityEngine;

public class FinalRushPlayerClamp : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    private Rigidbody2D playerRb;

    private void FixedUpdate()
    {
        // no arena active → do nothing
        if (FinalRushArenaController.Instance == null ||
            !FinalRushArenaController.Instance.HasArena)
            return;

        // find player rb once
        if (playerRb == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null)
                playerRb = playerObj.GetComponent<Rigidbody2D>();
        }

        if (playerRb == null)
            return;

        Vector2 center = FinalRushArenaController.Instance.Center;
        float radius = FinalRushArenaController.Instance.Radius;

        if (radius <= 0f)
            return;

        Vector2 pos = playerRb.position;
        Vector2 delta = pos - center;
        float dist = delta.magnitude;

        // inside the arena → free movement
        if (dist <= radius || dist <= 0.0001f)
            return;

        // player tried to go past the border:
        Vector2 dir = delta / dist;
        Vector2 clampedPos = center + dir * radius;

        // move player back onto the circle boundary
        playerRb.position = clampedPos;

        // remove outward velocity (so you can't push through the wall)
        Vector2 vel = playerRb.linearVelocity;
        float outwardSpeed = Vector2.Dot(vel, dir);
        if (outwardSpeed > 0f)
        {
            playerRb.linearVelocity = vel - dir * outwardSpeed;
        }
    }
}
