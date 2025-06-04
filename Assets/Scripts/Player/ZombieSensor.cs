using UnityEngine;
using Fusion;

public class ZombieSensor : NetworkBehaviour
{
    public ZombieHandler zombieHandler;

    private PlayerController ownerPlayer;

    private void Awake()
    {
        ownerPlayer = zombieHandler.GetComponent<PlayerController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsImmune()) return;

        if (!collision.TryGetComponent(out HealthController health)) return;

        var targetPlayer = health.playerController;
        
        if (targetPlayer == ownerPlayer) return;
        
        if (ownerPlayer.playerState != PlayerState.Zombie) return;
        if (targetPlayer.playerState != PlayerState.Human) return;

        Infect(targetPlayer);
    }

    private bool IsImmune()
    {
        return Runner.SimulationTime < zombieHandler.ImmuneUntil;
    }

    private void Infect(PlayerController target)
    {
        target.playerState = PlayerState.Zombie;
        target.BecomeZombie();
    }
}