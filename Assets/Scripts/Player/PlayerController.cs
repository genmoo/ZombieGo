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
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    public PlayerState playerState = PlayerState.Human;
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

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (grid == null)
            grid = WaitingMapManager.Instance.grid;

        GameObject cabinetObj = GameObject.Find("Cabinet");
        cabinetTilemap = cabinetObj.GetComponent<Tilemap>();

        arrowHandler.Init(rb, lastMoveInput);
        zombieHandler.Init(rb, lastMoveInput);
        
        GetComponent<HealthController>().playerController = this;
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
/*#if UNITY_EDITOR
        if (playerState == PlayerState.Zombie)
            return;
#endif*/
        
        DirInput();
        UpdateMoving();
        UpdateAnimator();

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

        if (playerState == PlayerState.Zombie && !zombieSetupDone)
        {
            BecomeZombie();
        }

        CabinetAlpha();
    }

    private void FixedUpdate()
    {
        if (playerState == PlayerState.Zombie && zombieHandler.HandleDashMovement())
        {
            cellPos = grid.WorldToCell(rb.position);
            return;
        }
        
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
            spriteRenderer.flipX = true;
        else if (dir == MoveDir.Left)
            spriteRenderer.flipX = false;

        if (dir == MoveDir.Up || dir == MoveDir.Down)
            spriteRenderer.flipX = false;

        animator.SetFloat("XInput", moveInput.x);
        animator.SetFloat("YInput", moveInput.y);
        animator.SetBool("isWalk", isMoving && dir != MoveDir.None);
    }

    public void BecomeZombie()
    {
        zombieSetupDone = true;
        zombieHandler.BecomeZombie();
        arrowHandler.HideArrowUI();
    }

    void CabinetAlpha()
    {
        Vector3Int currentCell = grid.WorldToCell(transform.position);
        SetPlayerAlpha(cabinetTilemap.HasTile(currentCell) ? 0.5f : 1f);
    }

    void SetPlayerAlpha(float alpha)
    {
        Color c = spriteRenderer.color;
        c.a = alpha;
        spriteRenderer.color = c;
    }
}
