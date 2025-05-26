using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public enum MoveDir
{
    None,
    Up,
    Down,
    Left,
    Right
}

public enum PlayerState
{
    Human,
    Zombie
}

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
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
    private MoveDir dir = MoveDir.None;

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

    public PlayerState playerState = PlayerState.Human;
    public RuntimeAnimatorController zombieAnimator;
    private bool zombieSetupDone = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (grid == null)
            grid = WaitingMapManager.Instance.grid;


        GameObject cabinetObj = GameObject.Find("Cabinet");
        cabinetTilemap = cabinetObj.GetComponent<Tilemap>();
    }

    private void Start()
    {
        cellPos = grid.WorldToCell(transform.position);
        rb.position = grid.CellToWorld(cellPos) + new Vector3(0.5f, 0f);

        if (playerState == PlayerState.Zombie)
        {
            BecomeZombie();
        }
    }

    private void Update()
    {
        DirInput();
        UpdateMoving();
        UpdateAnimator();

        if (playerState == PlayerState.Human)
        {
            ArrowInput();
        } 
        else if (playerState == PlayerState.Zombie)
        {
            DashInput();
        }

        else if (playerState == PlayerState.Zombie && !zombieSetupDone)
        {
            BecomeZombie();
        }

        CabinetAlpha();
        
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            Vector2 nextPos = Vector2.MoveTowards(rb.position, dashTarget, dashSpeed * Time.fixedDeltaTime);
            Vector3Int nextCell = grid.WorldToCell(nextPos);
            
            if (wallTilemap.HasTile(nextCell))
            {
                isDashing = false;
                dashLock = Time.time + 0.5f;
                cellPos = grid.WorldToCell(rb.position);
                return;
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
            dir = MoveDir.None;
            return;
        }
        
        var keyboard = Keyboard.current;

        if (keyboard.wKey.isPressed)
            dir = MoveDir.Up;
        else if (keyboard.sKey.isPressed)
            dir = MoveDir.Down;
        else if (keyboard.aKey.isPressed)
            dir = MoveDir.Left;
        else if (keyboard.dKey.isPressed)
            dir = MoveDir.Right;
        else
            dir = MoveDir.None;
    }

    void UpdateMoving()
    {
        if (isMoving || dir == MoveDir.None)
            return;

        Vector3Int nextCell = cellPos;
        switch (dir)
        {
            case MoveDir.Up: nextCell += Vector3Int.up; break;
            case MoveDir.Down: nextCell += Vector3Int.down; break;
            case MoveDir.Left: nextCell += Vector3Int.left; break;
            case MoveDir.Right: nextCell += Vector3Int.right; break;
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
        Vector2 moveDir = destPos - rb.position;

        float dist = moveDir.magnitude;
        if (dist < speed * Time.fixedDeltaTime)
        {
            rb.MovePosition(destPos);
            isMoving = false;
        }
        else
        {
            rb.MovePosition(rb.position + moveDir.normalized * speed * Time.fixedDeltaTime);
        }
    }

    void UpdateAnimator()
    {
        Vector2 moveInput = Vector2.zero;

        switch (dir)
        {
            case MoveDir.Up: moveInput = Vector2.up; break;
            case MoveDir.Down: moveInput = Vector2.down; break;
            case MoveDir.Left: moveInput = Vector2.left; break;
            case MoveDir.Right: moveInput = Vector2.right; break;
            case MoveDir.None: moveInput = lastMoveInput; break;
        }

        if (dir != MoveDir.None)
            lastMoveInput = moveInput;

        if (dir == MoveDir.Right)
            spriteRenderer.flipX = true;
        else if (dir == MoveDir.Left)
            spriteRenderer.flipX = false;

        if (dir == MoveDir.Up || dir == MoveDir.Down)
            spriteRenderer.flipX = false;

        animator.SetFloat("XInput", moveInput.x);
        animator.SetFloat("YInput", moveInput.y);
        animator.SetBool("isWalk", isMoving && dir != MoveDir.None);
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
                Vector3Int nextCell = dashCell + new Vector3Int((int)dashDirection.x , (int)dashDirection.y, 0);
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
            timer2 += Time.deltaTime;
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

    void SetPlayerAlpha(float alpha)
    {
        Color c = spriteRenderer.color;
        c.a = alpha;
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
        {
            SetPlayerAlpha(0.5f);
        }
        else
        {
            SetPlayerAlpha(1f);
        }
    }
}
