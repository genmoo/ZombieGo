using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using Fusion;
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
public class PlayerController : NetworkBehaviour
{ 
    public Grid grid;
    public Tilemap wallTilemap;
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    
    public float speed = 5f;

    public ArrowHandler arrowHandler;
    public ZombieHandler zombieHandler;

    private Tilemap cabinetTilemap;
    private Rigidbody2D rb;
    private Vector3Int cellPos = Vector3Int.zero;
    private Vector2 lastMoveInput = Vector2.down;
    private bool isMoving = false;
    private MoveDir dir = MoveDir.None;
    private bool zombieSetupDone = false;

    public Camera Camera;
    public HealthController healthController;
    public Image circle;
    
    [Networked, OnChangedRender(nameof(SetPlayerAlpha))]
    public float playerAlpha { get; set; }
    
    [Networked] 
    public PlayerState playerState { get; set; }
    
    [Networked]
    public bool isInvincible { get; set; }
    
    [Networked, OnChangedRender(nameof(OnFlipChanged))]
    public bool isFlipped { get; set; }
   
    
    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            Camera = Camera.main;
            Camera.GetComponent<FirstPersonCamera>().Target = transform;
            
            arrowHandler.arrowLoading.gameObject.SetActive(true);
            arrowHandler.arrowImage.gameObject.SetActive(true);

            zombieHandler.dashLoading.gameObject.SetActive(false);
            zombieHandler.dashImage.gameObject.SetActive(false);
        }
        else
        {
            circle.enabled = false;
            arrowHandler.arrowLoading.gameObject.SetActive(false);
            arrowHandler.arrowImage.gameObject.SetActive(false);

            zombieHandler.dashLoading.gameObject.SetActive(false);
            zombieHandler.dashImage.gameObject.SetActive(false);
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

        arrowHandler.Init(rb, lastMoveInput);
        zombieHandler.Init(rb, lastMoveInput, grid, wallTilemap);
        
        healthController = GetComponent<HealthController>();
        healthController.playerController = this;
    }

    private void Start()
    {
        cellPos = grid.WorldToCell(transform.position);
        rb.position = grid.CellToWorld(cellPos) + new Vector3(0.5f, 0f);
    }

    public override void FixedUpdateNetwork()
    {
        CabinetAlpha();
        
        if (HasStateAuthority)
        {
            DirInput();


            if (playerState == PlayerState.Human)
            {
                arrowHandler.SetMoveInput(lastMoveInput);
                arrowHandler.HandleArrowInput();
            }
            else if (playerState == PlayerState.Zombie)
            {
                zombieHandler.SetMoveInput(lastMoveInput);
                zombieHandler.HandleDashInput();
            }

            //디버깅용
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                playerState = PlayerState.Zombie;
            }
        }

        if (playerState == PlayerState.Zombie && !zombieSetupDone)
        {
            BecomeZombie();
        }

        if (playerState == PlayerState.Zombie && zombieHandler.HandleDashMovement())
        {
            cellPos = grid.WorldToCell(rb.position);
            return;
        }

        UpdateMoving();
        UpdateAnimator();
        UpdatePosition();
    }

    void DirInput()
    {
        if (Time.time < zombieHandler.DashLock)
        {
            dir = MoveDir.None;
            return;
        }

        var keyboard = Keyboard.current;
        if (keyboard.wKey.isPressed) dir = MoveDir.Up;
        else if (keyboard.sKey.isPressed) dir = MoveDir.Down;
        else if (keyboard.aKey.isPressed) dir = MoveDir.Left;
        else if (keyboard.dKey.isPressed) dir = MoveDir.Right;
        else dir = MoveDir.None;
    }

    void UpdateMoving()
    {
        if (isMoving || dir == MoveDir.None) return;

        Vector3Int nextCell = cellPos;
        switch (dir)
        {
            case MoveDir.Up: nextCell += Vector3Int.up; break;
            case MoveDir.Down: nextCell += Vector3Int.down; break;
            case MoveDir.Left: nextCell += Vector3Int.left; break;
            case MoveDir.Right: nextCell += Vector3Int.right; break;
        }

        if (!wallTilemap.HasTile(nextCell))
        {
            cellPos = nextCell;
            isMoving = true;
        }
    }

    void UpdatePosition()
    {
        if (!isMoving) return;

        Vector2 destPos = grid.CellToWorld(cellPos) + new Vector3(0.5f, 0f);
        Vector2 moveDir = destPos - rb.position;

        if (moveDir.magnitude < speed * Time.fixedDeltaTime)
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
            isFlipped = spriteRenderer.flipX = true;
        else if (dir == MoveDir.Left)
            isFlipped = spriteRenderer.flipX = false;

        if (dir == MoveDir.Up || dir == MoveDir.Down)
            isFlipped = spriteRenderer.flipX = false;

        animator.SetFloat("XInput", moveInput.x);
        animator.SetFloat("YInput", moveInput.y);
        animator.SetBool("isWalk", isMoving && dir != MoveDir.None);
    }

    public void BecomeZombie()
    {
        if (HasStateAuthority)
        {
            zombieHandler.dashLoading.gameObject.SetActive(true);
            zombieHandler.dashImage.gameObject.SetActive(true);
        }

        zombieSetupDone = true;
        zombieHandler.BecomeZombie();
        arrowHandler.HideArrowUI();
        healthController.InitHealth();
    }

    void CabinetAlpha()
    {
        Vector3 positionAbove = transform.position + new Vector3(0, 0.5f);
        Vector3Int currentCell = grid.WorldToCell(positionAbove);

        float targetAlpha = 1f;
        
        if (HasStateAuthority)
        {
            if (cabinetTilemap.HasTile(currentCell))
            {
                targetAlpha = 0.5f;
            }
            else
            {
                targetAlpha = 1f;
            }
            if (Mathf.Abs(playerAlpha - targetAlpha) > 0.01f)
            {
                playerAlpha = targetAlpha;
            }
        }
    }
    
    void SetPlayerAlpha()
    {
        float alphaToSet = playerAlpha;

        if (cabinetTilemap.HasTile(grid.WorldToCell(transform.position + new Vector3(0, 0.5f))))
        {
            if (Runner.LocalPlayer == Object.InputAuthority)
            {
                alphaToSet = 0.5f;
            }
            else
            {
                alphaToSet = 0f;
            }
        }

        Color c = spriteRenderer.color;
        c.a = alphaToSet;
        spriteRenderer.color = c;
    }

    
    private void OnFlipChanged()
    {
        spriteRenderer.flipX = isFlipped;
    }
}
