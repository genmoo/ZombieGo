using UnityEngine;
using UnityEngine.InputSystem;

public enum MoveDir
{
    None,
    Up,
    Down,
    Left,
    Right
}

public class PlayerController : MonoBehaviour
{
    public Grid grid;
    public float speed;
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    
    Vector3Int cellPos = Vector3Int.zero;
    Vector2 lastMoveInput = Vector2.down;
    bool isMoving = false;
    MoveDir dir = MoveDir.None;

    void Start()
    {
        Vector3 pos = grid.CellToWorld(cellPos) + new Vector3(0.5f, 0.5f);
        transform.position = pos;
    }

    void Update()
    {
        DirInput();
        UpdatePosition();
        UpdateMoving();
        UpdateAnimator();
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

    void UpdatePosition()
    {
        if (!isMoving)
        return;

        Vector3 destPos = grid.CellToWorld(cellPos) + new Vector3(0.5f, 0.5f);
        Vector3 moveDir = destPos - transform.position;

        float dist = moveDir.magnitude;
        if (dist < speed * Time.deltaTime)
        {
            transform.position = destPos;
            isMoving = false;
        }

        else
        {
            transform.position += moveDir.normalized * speed * Time.deltaTime;
            isMoving = true;
        }
    }

    void UpdateMoving()
    {
        if (!isMoving)
        {
            switch (dir)
            {
                case MoveDir.Up:
                    cellPos += Vector3Int.up;
                    isMoving = true;
                    break;
                
                case MoveDir.Down:
                    cellPos += Vector3Int.down;
                    isMoving = true;
                    break;
                case MoveDir.Left:
                    cellPos += Vector3Int.left;
                    isMoving = true;
                    break;
                case MoveDir.Right:
                    cellPos += Vector3Int.right;
                    isMoving = true;
                    break;
            }
        }
    }

    void UpdateAnimator()
    {
        Vector2 moveInput = Vector2.zero; 
        
        switch (dir)
        {
            case MoveDir.Up:
                moveInput = Vector2.up;
                break;
            case MoveDir.Down:
                moveInput = Vector2.down;
                break;
            case MoveDir.Left:
                moveInput = Vector2.left;
                break;
            case MoveDir.Right:
                moveInput = Vector2.right;
                break;
            case MoveDir.None:
                moveInput = lastMoveInput;
                break;
        }
        
        if (dir != MoveDir.None)
        {
            lastMoveInput = moveInput;
        }
        
        if (lastMoveInput.x > 0)
            spriteRenderer.flipX = true;
        else if (lastMoveInput.x < 0)
            spriteRenderer.flipX = false;
        
        animator.SetFloat ("XInput", moveInput.x);
        animator.SetFloat ("YInput", moveInput.y);
        animator.SetBool("isWalk", isMoving);
    }

}
