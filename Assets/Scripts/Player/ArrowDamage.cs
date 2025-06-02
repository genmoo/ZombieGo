using UnityEngine;
using Fusion;

public class ArrowDamage : NetworkBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        HealthController health = collision.GetComponent<HealthController>();
        if (health == null) return;

        if (health.playerController.playerState == PlayerState.Human)
            return;
        
        ZombieHandler zombie = collision.GetComponent<ZombieHandler>();
        if (zombie != null)
        {
            zombie.SetImmune(0.5f);
        }

        health.TakeDamage(1);

        if (health.playerController.playerState == PlayerState.Zombie)
        {
            Destroy(gameObject);
        }
    }
}
