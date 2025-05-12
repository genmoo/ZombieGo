using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public enum MoveDir
{
    None,
    Up,
    Down,
    Left,
    Right
}

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public Grid grid;
    public Tilemap wallTilemap;
    public float speed = 5f;
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    private Rigidbody2D rb;
    private Vector3Int cellPos = Vector3Int.zero;
    private Vector2 lastMoveInput = Vector2.down;
    private bool isMoving = false;
    private MoveDir dir = MoveDir.None;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (grid == null)
            grid = MapManager.Instance.grid;
    }

    private void Start()
    {
        cellPos = grid.WorldToCell(transform.position);
        rb.position = grid.CellToWorld(cellPos) + new Vector3(0.5f, 0f); 
    }

    private void Update()
    {
        DirInput();
        UpdateMoving();
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        UpdatePosition();
    }

    void DirInput()
    {
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
            case MoveDir.Up:    nextCell += Vector3Int.up; break;
            case MoveDir.Down:  nextCell += Vector3Int.down; break;
            case MoveDir.Left:  nextCell += Vector3Int.left; break;
            case MoveDir.Right: nextCell += Vector3Int.right; break;
        }
        
        Vector3Int topCell = nextCell + Vector3Int.up;

        if (!wallTilemap.HasTile(nextCell) && !wallTilemap.HasTile(topCell))
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

        if (lastMoveInput.x > 0)
            spriteRenderer.flipX = true;
        else if (lastMoveInput.x < 0)
            spriteRenderer.flipX = false;

        animator.SetFloat("XInput", moveInput.x);
        animator.SetFloat("YInput", moveInput.y);
        animator.SetBool("isWalk", isMoving && dir != MoveDir.None);
    }
}
