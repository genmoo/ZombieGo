using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Fusion;

public enum MoveDir2
{
    None,
    Up,
    Down,
    Left,
    Right
}

public enum PlayerState2
{
    Human,
    Zombie
}

[RequireComponent(typeof(NetworkObject), typeof(Rigidbody2D))]
public class Net_PlayerController : NetworkBehaviour
{
    public Grid grid;
    public Tilemap wallTilemap;
    public float speed = 5f;
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    private Tilemap cabinetTilemap;

    private Rigidbody2D rb;
    private Vector3Int cellPos = Vector3Int.zero;
    private Vector2 lastMoveInput = Vector2.down;
    private bool isMoving = false;
    private MoveDir2 dir = MoveDir2.None;

    public GameObject arrowPrefab;
    public float arrowSpeed = 10f;
    public float arrowLife = 2f;

    public Image arrowLoading;
    public Image arrowImage;
    private float timer = 0;
    private bool isCooldown = false;
    private float arrowCooldown = 1.5f;
    private float lastArrowTime = -999f;


    public Image dashLoading;
    public Image dashImage;
    private float timer2 = 0;
    private bool isCooldown2 = false;
    public float dashCooldown = 8f;
    private float lastDashTime = -999f;

    private bool isDashing = false;
    private Vector2 dashTarget;
    private float dashSpeed = 30f;
    private Vector2 dashDirection = Vector2.zero;
    private float dashLock = 0;

    private float ghostSpawnTimer = 0f;
    private float ghostSpawnInterval = 0.01f;

    public PlayerState2 PlayerState2 = PlayerState2.Human;
    public RuntimeAnimatorController zombieAnimator;
    private bool zombieSetupDone = false;

    // 네트워크 관련련
    [Networked, OnChangedRender(nameof(SetPlayerAlpha))]
    public float PlayerAlpha { get; set; }
    public Camera Camera;

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            Camera = Camera.main;
            Camera.GetComponent<FirstPersonCamera>().Target = transform;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (grid == null)
            grid = WaitingMapManager.Instance.grid;
        if (wallTilemap == null)
            wallTilemap = WaitingMapManager.Instance.wallTilemap;

             if (grid == null)
            grid = PlayMapManager.Instance.grid;
        if (wallTilemap == null)
            wallTilemap = PlayMapManager.Instance.wallTilemap;


        GameObject cabinetObj = GameObject.Find("Cabinet");
        cabinetTilemap = cabinetObj.GetComponent<Tilemap>();
    }

    private void Start()
    {
        cellPos = grid.WorldToCell(transform.position);
        rb.position = grid.CellToWorld(cellPos) + new Vector3(0.5f, 0f);

        if (PlayerState2 == PlayerState2.Zombie)
        {
            BecomeZombie();
        }
    }

    private void Update()
    {
        DirInput();
        if (PlayerState2 == PlayerState2.Human)
        {
            ArrowInput();
        }
        else if (PlayerState2 == PlayerState2.Zombie)
        {
            DashInput();
        }

        else if (PlayerState2 == PlayerState2.Zombie && !zombieSetupDone)
        {
            BecomeZombie();
        }

        if (HasStateAuthority)
            CabinetAlpha(); // 값 변경은 내 권한에서만
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority == false)
        {
            return;
        }
        UpdateAnimator();
        UpdateMoving();

        if (isDashing)
        {
            Vector2 nextPos = Vector2.MoveTowards(rb.position, dashTarget, dashSpeed * Runner.DeltaTime);
            Vector3Int nextCell = grid.WorldToCell(nextPos);

            if (wallTilemap.HasTile(nextCell))
            {
                isDashing = false;
                dashLock = Time.time + 0.5f;
                cellPos = grid.WorldToCell(rb.position);
                return;
            }

            rb.position = nextPos;

            ghostSpawnTimer += Runner.DeltaTime;
            if (ghostSpawnTimer >= ghostSpawnInterval)
            {
                CreateDashGhost();
                ghostSpawnTimer = 0f;
            }

            if (Vector2.Distance(rb.position, dashTarget) < 0.05f)
            {
                rb.position = dashTarget;
                isDashing = false;
                cellPos = grid.WorldToCell(rb.position);
                dashLock = Time.time + 0.5f;
            }
            return; // 대시 중엔 일반 이동 금지
        }

        UpdatePosition();
    }

    void DirInput()
    {
        if (Time.time < dashLock)
        {
            dir = MoveDir2.None;
            return;
        }

        if (Input.GetKey(KeyCode.W))
            dir = MoveDir2.Up;
        else if (Input.GetKey(KeyCode.S))
            dir = MoveDir2.Down;
        else if (Input.GetKey(KeyCode.A))
            dir = MoveDir2.Left;
        else if (Input.GetKey(KeyCode.D))
            dir = MoveDir2.Right;
        else
            dir = MoveDir2.None;
    }

    void UpdateMoving()
    {
        if (isMoving || dir == MoveDir2.None)
            return;

        Vector3Int nextCell = cellPos;
        switch (dir)
        {
            case MoveDir2.Up: nextCell += Vector3Int.up; break;
            case MoveDir2.Down: nextCell += Vector3Int.down; break;
            case MoveDir2.Left: nextCell += Vector3Int.left; break;
            case MoveDir2.Right: nextCell += Vector3Int.right; break;
        }

        Vector3Int topCell = nextCell + Vector3Int.up;

        if (!wallTilemap.HasTile(nextCell))
        {
            cellPos = nextCell;
            isMoving = true;
        }
    }

    void UpdatePosition()
    {
        if (!isMoving)
            return;

        Vector2 destPos = grid.CellToWorld(cellPos) + new Vector3(0.5f, 0f);
        Vector2 MoveDir2 = destPos - rb.position;

        float dist = MoveDir2.magnitude;
        if (dist < speed * Runner.DeltaTime)
        {
            rb.MovePosition(destPos);
            isMoving = false;
        }
        else
        {
            rb.MovePosition(rb.position + MoveDir2.normalized * speed * Runner.DeltaTime);
        }
    }

    void UpdateAnimator()
    {
        Vector2 moveInput = Vector2.zero;

        switch (dir)
        {
            case MoveDir2.Up: moveInput = Vector2.up; break;
            case MoveDir2.Down: moveInput = Vector2.down; break;
            case MoveDir2.Left: moveInput = Vector2.left; break;
            case MoveDir2.Right: moveInput = Vector2.right; break;
            case MoveDir2.None: moveInput = lastMoveInput; break;
        }

        if (dir != MoveDir2.None)
            lastMoveInput = moveInput;

        if (dir == MoveDir2.Right)
            spriteRenderer.flipX = true;
        else if (dir == MoveDir2.Left)
            spriteRenderer.flipX = false;

        if (dir == MoveDir2.Up || dir == MoveDir2.Down)
            spriteRenderer.flipX = false;

        animator.SetFloat("XInput", moveInput.x);
        animator.SetFloat("YInput", moveInput.y);
        animator.SetBool("isWalk", isMoving && dir != MoveDir2.None);
    }

    void ShootArrow()
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

        GameObject arrow = Instantiate(arrowPrefab, spawnPos, Quaternion.identity);

        Rigidbody2D arrowRb = arrow.GetComponent<Rigidbody2D>();
        arrowRb.linearVelocity = shootDir * arrowSpeed;

        float zRot = 0f;
        if (shootDir == Vector2.up)
            zRot = 180f;
        else if (shootDir == Vector2.down)
            zRot = 0f;
        else if (shootDir == Vector2.left)
            zRot = -90f;
        else if (shootDir == Vector2.right)
            zRot = 90f;

        arrow.transform.rotation = Quaternion.Euler(0f, 0f, zRot);

        Destroy(arrow, arrowLife);
    }

    void DashInput()
    {
        if (isDashing) return;

        if (Keyboard.current.rightShiftKey.wasPressedThisFrame &&
            Time.time - lastDashTime > dashCooldown)
        {
            if (lastMoveInput == Vector2.up) dashDirection = Vector2Int.up;
            else if (lastMoveInput == Vector2.down) dashDirection = Vector2Int.down;
            else if (lastMoveInput == Vector2.left) dashDirection = Vector2Int.left;
            else if (lastMoveInput == Vector2.right) dashDirection = Vector2Int.right;
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
            isCooldown2 = true;
            dashLoading.fillAmount = 0;
        }

        if (isCooldown2)
        {
            timer2 += Runner.DeltaTime;
            float ratio = Mathf.Clamp01(timer2 / dashCooldown);
            dashLoading.fillAmount = ratio;

            if (timer2 >= dashCooldown)
            {
                isCooldown2 = false;
                timer2 = 0;
                dashLoading.fillAmount = 1;
            }
        }
    }

    void CreateDashGhost()
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
    void SetPlayerAlpha()
    {
        Color c = spriteRenderer.color;
        c.a = PlayerAlpha;
        spriteRenderer.color = c;
    }

    void ArrowInput()
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
            timer += Runner.DeltaTime;
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

    void BecomeZombie()
    {
        zombieSetupDone = true;

        arrowLoading.gameObject.SetActive(false);
        arrowImage.gameObject.SetActive(false);

        dashLoading.gameObject.SetActive(true);
        dashImage.gameObject.SetActive(true);

        animator.runtimeAnimatorController = zombieAnimator;
    }

    void CabinetAlpha()
    {
        Vector3Int currentCell = grid.WorldToCell(transform.position);
        if (cabinetTilemap.HasTile(currentCell))
            PlayerAlpha = 0.5f;
        else
            PlayerAlpha = 1f;
    }
}
