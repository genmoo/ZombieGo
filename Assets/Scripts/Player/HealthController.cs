using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Fusion;

public class HealthController : NetworkBehaviour
{
    [Networked]
    public int maxHealth { get; set; }
    
    [Networked]
    public int currentHealth { get; set; }
    
    public PlayerController playerController;
    
    public Slider healthSlider;
    public GameObject healthUI;

    public override void Spawned()
    {
        InitHealth();
    }

    public void InitHealth()
    {
        if (playerController.playerState == PlayerState.Zombie)
            maxHealth = 10; 
        currentHealth = maxHealth;
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
    public void RpcTakeDamage(int damage)
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
            Runner.Despawn(Object);
        }
    }
    
    IEnumerator ShowHealthUIForOneSecond()
    {
        healthUI.SetActive(true);
        yield return new WaitForSeconds(1f);
        healthUI.SetActive(false);
    }
}
