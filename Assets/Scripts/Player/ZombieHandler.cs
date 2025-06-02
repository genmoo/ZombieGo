using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Fusion;

public class ZombieHandler : NetworkBehaviour
{
    public Grid grid;
    public Tilemap wallTilemap;
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public RuntimeAnimatorController zombieAnimator;
    public Image dashLoading;
    public Image dashImage;
    
    public float dashCooldown = 8f;
    public float dashSpeed = 30f;
    public float ghostSpawnInterval = 0.01f;

    private Rigidbody2D rb;
    private bool isDashing = false;
    private Vector2 dashTarget;
    private Vector2 dashDirection = Vector2.zero;
    private float dashLock = 0f;
    private float ghostSpawnTimer = 0f;
    private float lastDashTime = -999f;
    private float timer = 0f;
    private bool isCooldown = false;
    
    private Vector2 lastMoveInput = Vector2.down;
    
    private float immuneUntil = 0f;
    public float ImmuneUntil => immuneUntil;
    private bool isImmune = false;

    public void Init(Rigidbody2D rigidbody, Vector2 moveInput)
    {
        rb = rigidbody;
        lastMoveInput = moveInput;
    }

    private void Update()
    {
        if (isImmune && Time.time >= immuneUntil)
        {
            isImmune = false;
            SetZombieAlpha(1f);
        }
    }
    
    public void BecomeZombie()
    {
        dashLoading.gameObject.SetActive(true);
        dashImage.gameObject.SetActive(true);
        animator.runtimeAnimatorController = zombieAnimator;
    }

    public void HandleDashInput()
    {
        if (isDashing) return;

        if (Keyboard.current.rightShiftKey.wasPressedThisFrame && Time.time - lastDashTime > dashCooldown)
        {
            if (lastMoveInput == Vector2.up) dashDirection = Vector2.up;
            else if (lastMoveInput == Vector2.down) dashDirection = Vector2.down;
            else if (lastMoveInput == Vector2.left) dashDirection = Vector2.left;
            else if (lastMoveInput == Vector2.right) dashDirection = Vector2.right;
            else return;

            Vector3Int startCell = grid.WorldToCell(rb.position);
            Vector3Int dashCell = startCell;

            for (int i = 0; i < 6; i++)
            {
                Vector3Int nextCell = dashCell + new Vector3Int((int)dashDirection.x, (int)dashDirection.y, 0);
                if (wallTilemap.HasTile(nextCell))
                    break;
                dashCell = nextCell;
            }

            dashTarget = grid.CellToWorld(dashCell) + new Vector3(0.5f, 0f);
            isDashing = true;
            lastDashTime = Time.time;
            isCooldown = true;
            dashLoading.fillAmount = 0;
        }

        if (isCooldown)
        {
            timer += Time.deltaTime;
            float ratio = Mathf.Clamp01(timer / dashCooldown);
            dashLoading.fillAmount = ratio;

            if (timer >= dashCooldown)
            {
                isCooldown = false;
                timer = 0;
                dashLoading.fillAmount = 1;
            }
        }
    }

    public bool HandleDashMovement()
    {
        if (!isDashing) return false;

        Vector2 nextPos = Vector2.MoveTowards(rb.position, dashTarget, dashSpeed * Time.fixedDeltaTime);
        Vector3Int nextCell = grid.WorldToCell(nextPos);

        if (wallTilemap.HasTile(nextCell))
        {
            isDashing = false;
            dashLock = Time.time + 0.5f;
            return false;
        }

        rb.position = nextPos;

        ghostSpawnTimer += Time.fixedDeltaTime;
        if (ghostSpawnTimer >= ghostSpawnInterval)
        {
            CreateDashGhost();
            ghostSpawnTimer = 0f;
        }

        if (Vector2.Distance(rb.position, dashTarget) < 0.05f)
        {
            rb.position = dashTarget;
            isDashing = false;
            dashLock = Time.time + 0.5f;
        }

        return true;
    }

    private void CreateDashGhost()
    {
        GameObject ghost = new GameObject("DashGhost");
        SpriteRenderer sr = ghost.AddComponent<SpriteRenderer>();

        sr.sprite = spriteRenderer.sprite;
        sr.flipX = spriteRenderer.flipX;
        sr.sortingOrder = spriteRenderer.sortingOrder - 1;
        sr.color = new Color(1f, 1f, 1f, 0.5f);

        ghost.transform.position = transform.position;
        ghost.transform.localScale = transform.localScale;

        Destroy(ghost, 0.2f);
    }

    public bool IsDashing => isDashing;
    public float DashLock => dashLock;
    
    public void SetMoveInput(Vector2 moveInput)
    {
        if (moveInput != Vector2.zero)
            lastMoveInput = moveInput;
    }
    
    
    public void SetImmune(float duration)
    {
        immuneUntil = Time.time + duration;
        isImmune = true;
        SetZombieAlpha(0.5f);
    }
    
    private void SetZombieAlpha(float alpha)
    {
        Color c = spriteRenderer.color;
        c.a = alpha;
        spriteRenderer.color = c;
    }
}
