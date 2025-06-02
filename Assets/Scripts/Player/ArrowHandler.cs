using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class ArrowHandler : NetworkBehaviour
{
    public GameObject arrowPrefab;
    public float arrowSpeed = 10f;
    public float arrowLife = 2f;
    public float arrowCooldown = 1.5f;
    
    public Image arrowLoading;
    public Image arrowImage;

    private Rigidbody2D rb;
    private Vector2 lastMoveInput = Vector2.down;

    private float lastArrowTime = -999f;
    private float timer = 0f;
    private bool isCooldown = false;
    
    public void Init(Rigidbody2D rigidbody, Vector2 moveInput)
    {
        rb = rigidbody;
        lastMoveInput = moveInput;
    }

    public void HandleArrowInput()
    {
        if (Keyboard.current.rightShiftKey.wasPressedThisFrame && Time.time - lastArrowTime >= arrowCooldown)
        {
            ShootArrow();
            lastArrowTime = Time.time;
            isCooldown = true;
            arrowLoading.fillAmount = 0;
        }

        if (isCooldown)
        {
            timer += Time.deltaTime;
            float ratio = Mathf.Clamp01(timer / arrowCooldown);
            arrowLoading.fillAmount = ratio;

            if (timer >= arrowCooldown)
            {
                isCooldown = false;
                timer = 0;
                arrowLoading.fillAmount = 1;
            }
        }
    }

    private void ShootArrow()
    {
        Vector2 shootDir = lastMoveInput.normalized;
        Vector2 spawnOffset = Vector2.zero;

        if (shootDir == Vector2.up)
            spawnOffset = new Vector2(0f, 1f);
        else if (shootDir == Vector2.down)
            spawnOffset = new Vector2(0f, 0f);
        else if (shootDir == Vector2.left)
            spawnOffset = new Vector2(-0.5f, 0.7f);
        else if (shootDir == Vector2.right)
            spawnOffset = new Vector2(0.5f, 0.7f);

        Vector2 spawnPos = rb.position + spawnOffset;
        NetworkObject arrow = Runner.Spawn(arrowPrefab, spawnPos, Quaternion.identity, Runner.LocalPlayer);

        Rigidbody2D arrowRb = arrow.GetComponent<Rigidbody2D>();
        arrowRb.linearVelocity = shootDir * arrowSpeed;

        float zRot = shootDir == Vector2.up ? 180f :
                     shootDir == Vector2.down ? 0f :
                     shootDir == Vector2.left ? -90f :
                     shootDir == Vector2.right ? 90f : 0f;

        arrow.transform.rotation = Quaternion.Euler(0f, 0f, zRot);
        ArrowDespawn(arrow, arrowLife).Forget();
    }

    private async UniTaskVoid ArrowDespawn(NetworkObject netObj, float time)
    {
        int delayMs = Mathf.RoundToInt(time * 1000f);
        await UniTask.Delay(delayMs);
        Runner.Despawn(netObj);
    }

    public void SetMoveInput(Vector2 moveInput)
    {
        if (moveInput != Vector2.zero)
            lastMoveInput = moveInput;
    }
    
    public void HideArrowUI()
    {
        arrowLoading.gameObject.SetActive(false);
        arrowImage.gameObject.SetActive(false);
    }
}
