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

    [Networked] private bool IsDashing { get; set; }
    [Networked] private Vector2 DashTarget { get; set; }
    [Networked] private float LastDashTime { get; set; }
    [Networked] private bool IsCooldown { get; set; }

    private Vector2 dashDirection = Vector2.zero;
    private float dashLock = 0f;
    private float ghostSpawnTimer = 0f;
    private float timer = 0f;
    private Vector2 lastMoveInput = Vector2.down;

    [Networked] public float ImmuneUntil { get; private set; }

    [SerializeField] private NetworkPrefabRef ghostPrefab;

    public void Init(Rigidbody2D rigidbody, Vector2 moveInput, Grid grid, Tilemap wallTilemap)
    {
        rb = rigidbody;
        lastMoveInput = moveInput;
        this.grid = grid;
        this.wallTilemap = wallTilemap;
    }

    private void Update()
    {
        UpdateZombieAlpha();
    }

    public void BecomeZombie()
    {
        animator.runtimeAnimatorController = zombieAnimator;
    }

    public void HandleDashInput()
    {
        if (IsDashing) return;

        if (HasInputAuthority && Keyboard.current.rightShiftKey.wasPressedThisFrame &&
            (Runner.SimulationTime - LastDashTime > dashCooldown))
        {
            if (lastMoveInput == Vector2.up) dashDirection = Vector2.up;
            else if (lastMoveInput == Vector2.down) dashDirection = Vector2.down;
            else if (lastMoveInput == Vector2.left) dashDirection = Vector2.left;
            else if (lastMoveInput == Vector2.right) dashDirection = Vector2.right;
            else return;

            RpcStartDash(dashDirection);
        }

        if (HasInputAuthority && IsCooldown)
        {
            timer += Time.deltaTime;
            float ratio = Mathf.Clamp01(timer / dashCooldown);
            dashLoading.fillAmount = ratio;

            if (timer >= dashCooldown)
            {
                IsCooldown = false;
                timer = 0;
                dashLoading.fillAmount = 1;
            }
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RpcStartDash(Vector2 dashDir)
    {
        Vector3Int startCell = grid.WorldToCell(rb.position);
        Vector3Int dashCell = startCell;

        for (int i = 0; i < 6; i++)
        {
            Vector3Int nextCell = dashCell + new Vector3Int((int)dashDir.x, (int)dashDir.y, 0);
            if (wallTilemap.HasTile(nextCell))
                break;
            dashCell = nextCell;
        }

        DashTarget = grid.CellToWorld(dashCell) + new Vector3(0.5f, 0f);
        IsDashing = true;
        LastDashTime = Runner.SimulationTime;
        IsCooldown = true;
    }

    public bool HandleDashMovement()
    {
        if (!IsDashing) return false;

        Vector2 nextPos = Vector2.MoveTowards(rb.position, DashTarget, dashSpeed * Time.fixedDeltaTime);
        Vector3Int nextCell = grid.WorldToCell(nextPos);

        if (wallTilemap.HasTile(nextCell))
        {
            IsDashing = false;
            dashLock = Runner.SimulationTime + 0.5f;
            return false;
        }

        rb.position = nextPos;

        ghostSpawnTimer += Time.fixedDeltaTime;
        if (ghostSpawnTimer >= ghostSpawnInterval)
        {
            CreateDashGhost();
            ghostSpawnTimer = 0f;
        }

        if (Vector2.Distance(rb.position, DashTarget) < 0.05f)
        {
            rb.position = DashTarget;
            IsDashing = false;
            dashLock = Runner.SimulationTime + 0.5f;
        }

        return true;
    }

    private void CreateDashGhost()
    {
        int spriteType = 0;

        if (Mathf.Abs(lastMoveInput.x) > Mathf.Abs(lastMoveInput.y))
        {
            spriteType = 0; // side
        }
        else
        {
            spriteType = (lastMoveInput.y > 0) ? 1 : 2; // up or down
        }

        bool flipX = (spriteType == 0) ? (lastMoveInput.x > 0) : false;

        Runner.Spawn(
            ghostPrefab,
            transform.position,
            Quaternion.identity,
            Object.InputAuthority, // 중요!
            (runner, ghostObj) =>
            {
                var dashGhost = ghostObj.GetComponent<DashGhost>();
                dashGhost.SpriteType = spriteType;
                dashGhost.IsFlipX = flipX;
            }
        );
    }

    public bool IsCurrentlyDashing => IsDashing;
    public float DashLock => dashLock;

    public void SetMoveInput(Vector2 moveInput)
    {
        if (moveInput != Vector2.zero)
            lastMoveInput = moveInput;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcSetImmune(float duration)
    {
        ImmuneUntil = Runner.SimulationTime + duration;
    }

    public void UpdateZombieAlpha()
    {
        if (Runner.SimulationTime < ImmuneUntil)
        {
            PlayerController pc = GetComponent<PlayerController>();
            if (!pc.isInvincible)
            {
                pc.isInvincible = true;
            }
        }
        else
        {
            PlayerController pc = GetComponent<PlayerController>();
            if (pc.isInvincible)
            {
                pc.isInvincible = false;
            }
        }
    }
    public bool IsImmune => Runner.SimulationTime < ImmuneUntil;
}
