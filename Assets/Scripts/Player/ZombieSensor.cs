using UnityEngine;

public class ZombieSensor : MonoBehaviour
{
    public ZombieHandler zombieHandler;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Time.time < zombieHandler.ImmuneUntil)
        {
            Debug.Log("감염 무시");
            return;
        }

        var health = collision.GetComponent<HealthController>();
        if (health == null) return;

        var otherPlayer = health.playerController;
        if (otherPlayer == zombieHandler.GetComponent<PlayerController>()) return;

        if (otherPlayer.playerState == PlayerState.Human)
        {
            Debug.Log("감염 발생!");
            otherPlayer.playerState = PlayerState.Zombie;
            otherPlayer.BecomeZombie();
        }
    }
}
