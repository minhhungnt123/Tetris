using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public Board board { get; private set; }
    public TetrominoData data { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public Vector3Int position { get; private set; }
    public int rotationIndex {  get; private set; }

    public float stepDelay = 1.0f;
    public float lockDelay = 0.5f;

    private float stepTime;
    private float lockTime;

    //Khởi tạo
    public void Initialized(Board board, Vector3Int position, TetrominoData data)
    {
        this.board = board;
        this.position = position;
        this.data = data;
        this.rotationIndex = 0;
        this.stepTime = Time.time + stepDelay;
        this.lockTime = 0f;

        if(this.cells == null)
        {
            this.cells = new Vector3Int[data.cells.Length];
        }

        for(int i = 0; i < this.cells.Length; i++)
            this.cells[i] = (Vector3Int)data.cells[i];
    }

    //Xử lý phím tắt
    private void Update()
    {
        if (board != null && (board.IsGameOver || board.IsPaused))
            return;

        this.board.Clear(this);

        this.lockTime += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            Rotate(-1);
        }else if (Input.GetKeyDown(KeyCode.E))
        {
            Rotate(1);
        }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Move(Vector2Int.left);
        }else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            Move(Vector2Int.right);
        }
        if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            Move(Vector2Int.down);
        }
        if (Input.GetKeyDown(KeyCode.Space)){
            HardDrop();
        }
        if(Time.time > this.stepTime)
        {
            Step();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            board.HoldPiece();
        }
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            UpdateStepDelay(0.1f);
        }
        else
        {
            UpdateStepDelay(1.0f);
        }
        this.board.Set(this);
    }
    //Xử lý khối rơi theo thời gian nhất định
    private void Step()
    {
        this.stepTime = Time.time + this.stepDelay;

        Move(Vector2Int.down);

        if(this.lockTime > this.lockDelay)
        {
            Lock() ;
        }
    }
    //Khóa khôi
    private void Lock()
    {
        this.board.Set(this);
        this.board.ClearLines();
        this.board.SpawnPiece();
    }
    //Thả khối
    private void HardDrop()
    {
        while (Move(Vector2Int.down))
        {
            continue;
        }
        Lock();
    }
    //Di chuyển
    private bool Move(Vector2Int transition)
    {
        Vector3Int newPosition = this.position;
        newPosition.x += transition.x;
        newPosition.y += transition.y;

        bool valid = this.board.IsValidPosition(this, newPosition);

        if (valid)
        {
            this.position = newPosition;
            this.lockTime = 0f;
        }

        return valid;
    }
    //Xử lý xoay khối
    private void ApplyRotationMatrix(int direction)
    {
        for (int i = 0; i < this.cells.Length; i++)
        {
            Vector3 cell = this.cells[i];

            int x, y;

            switch (this.data.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt((cell.x * Data.RotationMatrix[0] * direction) + (cell.y * Data.RotationMatrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * Data.RotationMatrix[2] * direction) + (cell.y * Data.RotationMatrix[3] * direction));
                    break;

                default:
                    x = Mathf.RoundToInt((cell.x * Data.RotationMatrix[0] * direction) + (cell.y * Data.RotationMatrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * Data.RotationMatrix[2] * direction) + (cell.y * Data.RotationMatrix[3] * direction));
                    break;
            }

            this.cells[i] = new Vector3Int(x, y, 0);
        }
    }

    private void Rotate(int direction)
    {
        int originalRotation = this.rotationIndex;
        this.rotationIndex = Wrap(this.rotationIndex + direction, 0, 4);

        ApplyRotationMatrix(direction);
        if(!TestWallKicks(this.rotationIndex, direction))
        {
            this.rotationIndex = originalRotation;
            ApplyRotationMatrix(-direction);
        }
    }
    //Xử lý wall kicks
    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = GetWallKicksIndex(rotationIndex, rotationDirection);

        for(int i = 0; i < this.data.wallKicks.GetLength(1); i++)
        {
            Vector2Int translation = this.data.wallKicks[wallKickIndex, i];

            if (Move(translation))
            {
                return true;
            }
        }
        return false;
    }
    
    private int GetWallKicksIndex(int rotationIndex, int rotationDirection)
    {
        int wallKicksIndex = rotationIndex * 2;

        if (rotationDirection < 0)
        {
            wallKicksIndex--;
        }
        return Wrap(wallKicksIndex, 0, this.data.wallKicks.GetLength(0));
    }

    private int Wrap(int input, int min, int max)
    {
        if(input < min)
        {
            return max - (min - input) % (max - min);
        }
        else
        {
            return min + (input - min) % (max - min);
        }
    }
    //Cập nhật lại khoản trễ độ rơi khi tăng cấp
    public void UpdateStepDelay(float newStepDelay)
    {
        this.stepDelay = newStepDelay;
    }
}
