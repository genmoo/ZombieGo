using UnityEngine;

public class HealthController : MonoBehaviour
{
    public int maxHealth;
    private int currentHealth;
    public PlayerController playerController;

    private void Awake()
    {
        InitHealth();
    }

    void InitHealth()
    {
        if (playerController.playerState == PlayerState.Zombie)
            maxHealth = 10;
        else if (playerController.playerState == PlayerState.Human)
            maxHealth = 1;

        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (playerController.playerState == PlayerState.Zombie)
        {
            Destroy(gameObject);
        }
        else if (playerController.playerState == PlayerState.Human)
        {
            
            playerController.playerState = PlayerState.Zombie;
            InitHealth();
            playerController.BecomeZombie(); 
        }
    }
}
