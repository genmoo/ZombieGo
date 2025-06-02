using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Fusion;

public class HealthController : NetworkBehaviour
{
    public int maxHealth;
    private int currentHealth;
    public PlayerController playerController;
    
    public Slider healthSlider;
    public GameObject healthUI;

    public override void Spawned()
    {
        InitHealth();
    }

    void InitHealth()
    {
        if (playerController.playerState == PlayerState.Zombie)
            maxHealth = 10; 
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (playerController.playerState == PlayerState.Human)
            return;
        
        currentHealth -= damage;
        healthSlider.value = currentHealth;
        
        StartCoroutine(ShowHealthUIForOneSecond());

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
    }
    
    IEnumerator ShowHealthUIForOneSecond()
    {
        healthUI.SetActive(true);
        yield return new WaitForSeconds(1f);
        healthUI.SetActive(false);
    }
}
