using UnityEngine;

public class ArrowDamage : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        HealthController health = collision.GetComponent<HealthController>();
        if (health == null) return;

        if (health.playerController.playerState == PlayerState.Human)
            return;
            
        health.TakeDamage(1);
        
        if (health.playerController.playerState == PlayerState.Zombie)
        {
            Destroy(gameObject);
        }
    }
}
