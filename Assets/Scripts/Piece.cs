using UnityEngine;

public class Piece : MonoBehaviour
{
    public Board board { get; private set; }
    public TetrominoData data { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public Vector3Int position { get; private set; }
    public int rotationIndex { get; private set; }

    public float stepDelay = 1f;
    public float lockDelay = 0.5f;
    private float stepTime;
    private float lockTime;

    public float dasDelay = 0.15f;
    public float dasRepeatRate = 0.05f;
    public float softDropMultiplier = 20f; 
    private float dasTime;
    private float softDropTime;
    private bool dasLeft;
    private bool dasRight;

    public void Initialize(Board board, Vector3Int position, TetrominoData data) {
        this.board = board;
        this.position = position;
        this.data = data;
        this.rotationIndex = 0;
        this.stepTime = Time.time + this.stepDelay;
        this.lockTime = 0f;
        this.dasTime = 0f;
        this.softDropTime = 0f;

        if (this.cells == null) {
            this.cells = new Vector3Int[data.cells.Length];
        }

        for (int i = 0; i < data.cells.Length; i++) {
            this.cells[i] = (Vector3Int)data.cells[i];
        }
    }

    private void Update() {
        this.board.Clear(this);

        this.lockTime += Time.deltaTime;

        // rotation
        if (Input.GetKeyDown(KeyCode.J)) Rotate(-1);
        else if (Input.GetKeyDown(KeyCode.L)) Rotate(1);

        // movement (left and right)
        if (Input.GetKeyDown(KeyCode.A))
        {
            Move(Vector2Int.left);
            dasLeft = true;
            dasTime = Time.time + dasDelay;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            Move(Vector2Int.right);
            dasRight = true;
            dasTime = Time.time + dasDelay;
        }

        if (Input.GetKeyUp(KeyCode.A)) dasLeft = false;
        if (Input.GetKeyUp(KeyCode.D)) dasRight = false;
    
        if (dasLeft && Time.time >= dasTime)
        {
            Move(Vector2Int.left);
            dasTime = Time.time + dasRepeatRate;
        }

        if (dasRight && Time.time >= dasTime)
        {
            Move(Vector2Int.right);
            dasTime = Time.time + dasRepeatRate;
        }


        // soft and hard drop
        if (Input.GetKey(KeyCode.S)) {
            softDropTime += Time.deltaTime;
            if (softDropTime >= stepDelay / softDropMultiplier) {
                Move(Vector2Int.down);
                softDropTime = 0f;
            }
        }
        else softDropTime = 0;
        
        if (Input.GetKeyDown(KeyCode.K)) HardDrop();

        if (Time.time >= this.stepTime) {
            Step();
        }

        this.board.Set(this);
    }

    private void Step() {
        this.stepTime = Time.time + this.stepDelay;
        Move(Vector2Int.down);

        if (this.lockTime >= this.lockDelay) {
            Lock();
        }
    }

    private void Lock() {
        this.board.Set(this);
        this.board.ClearFullLines();
        this.board.SpawnPiece();

        this.board.ToggleHoldState(false);
        if (this.board.HoldTetromino.HasValue) {
            this.board.queueManager.SetHoldPiece(this.board.HoldTetromino.Value);
        }
    }

    private void HardDrop() {
        while(Move(Vector2Int.down)) {
            continue;
        }

        Lock();
    }

    private bool Move(Vector2Int translation) {
        Vector3Int newPosition = this.position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool valid = this.board.IsValidPosition(this, newPosition);
        
        if (valid) {
            this.position = newPosition;
            this.lockTime = 0f;
        }

        return valid;
    }

    private void Rotate(int direction) {
        int originalRotationIndex = this.rotationIndex;
        this.rotationIndex = (((this.rotationIndex + direction) % 4) + 4) % 4;
        
        ApplyRotationMatrix(direction);

        if (!TestWallKicks(originalRotationIndex, direction)) {
            this.rotationIndex = originalRotationIndex;
            this.lockTime = 0f;
            ApplyRotationMatrix(-direction);
        }
    }

    private void ApplyRotationMatrix(int direction) {
        for (int i = 0; i < this.data.cells.Length; i++) {
            Vector3 cell = this.cells[i];

            // rotation logic is different for I and O tetrominoes
            int x, y;

            switch (this.data.tetromino) {
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

            this.cells[i] = new Vector3Int(x,y);
        }   
    }


    private bool TestWallKicks(int rotationIndex, int rotationDirection) {
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);

        for (int i = 0; i < this.data.wallKicks.GetLength(1); i++) {
            Vector2Int translation = this.data.wallKicks[wallKickIndex, i];

            if (Move(translation)) {
                return true;
            }
        }

        return false;
    }

    private int GetWallKickIndex(int rotationIndex, int rotationDirection) {
        int wallKickIndex = rotationIndex * 2;
        if (rotationDirection < 0) {
            wallKickIndex--;
        }

        int mod = this.data.wallKicks.GetLength(0);
        return ((wallKickIndex % mod) + mod) % mod;
    }
}