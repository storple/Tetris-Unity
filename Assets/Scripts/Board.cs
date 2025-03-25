using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using Unity.VisualScripting;

public class Board : MonoBehaviour
{
    public Tilemap tilemap { get; private set; }
    public Piece activePiece { get; private set; }
    public TetrominoData[] tetrominoes;
    public Vector3Int spawnPosition;
    public Vector2Int boardSize = new Vector2Int(10, 21);
    public QueueManager queueManager { get; private set; }

    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-this.boardSize.x / 2, -this.boardSize.y / 2);
            return new RectInt(position, this.boardSize);
        }
    }

    public List<TetrominoData> bag {get; private set;}
    private TetrominoData? holdTetromino = null;
    private bool holdUsed = false;

    public TetrominoData? HoldTetromino
    {
        get => holdTetromino;
        private set => holdTetromino = value;
    }

    private void Awake()
    {
        this.tilemap = GetComponentInChildren<Tilemap>();
        this.activePiece = GetComponentInChildren<Piece>();
        this.queueManager = FindObjectOfType<QueueManager>();
        this.holdUsed = false;

        for (int i = 0; i < this.tetrominoes.Length; i++)
        {
            this.tetrominoes[i].Initialize();
        }
    }

    private void Start()
    {
        SpawnPiece();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W)) {
            HoldPiece();
        }    
    }

    public void SpawnPiece()
    {
        TetrominoData data = this.queueManager.GetNextPiece();
        this.activePiece.Initialize(this, spawnPosition, data);
        Set(this.activePiece);
    }

    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, null);
        }
    }

    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = this.Bounds;
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;

            if (!bounds.Contains((Vector2Int)tilePosition)) return false;
            if (this.tilemap.HasTile(tilePosition)) return false;

        }

        return true;
    }

    public void HoldPiece() 
    {
        if (holdUsed) return;
        holdUsed = true;

        Clear(activePiece);

        if (HoldTetromino == null) 
        {
            HoldTetromino = activePiece.data;
            SpawnPiece();
            queueManager.SetHoldPiece(HoldTetromino.Value);
        }
        else 
        {
            TetrominoData tmp = activePiece.data;
            activePiece.Initialize(this, spawnPosition, HoldTetromino.Value);
            HoldTetromino = tmp;
            queueManager.SetHoldPiece(HoldTetromino.Value);
        }
    }

    public void ToggleHoldState(bool value) 
    {
        holdUsed = value;
    }

    public void ClearFullLines() 
    {
        for (int y = this.Bounds.y + this.Bounds.height - 1; y >= this.Bounds.y; y--) 
        { 
            bool isFullLine = true;
            for (int x = this.Bounds.x; x < this.Bounds.x + this.Bounds.width; x++) 
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);

                if (!this.tilemap.HasTile(tilePosition)) 
                {
                    isFullLine = false;
                    break;
                }
            }

            if (isFullLine) 
            {
                ClearLine(y);
                ShiftLinesDown(y);
                y++;
            }
        }
    }

    private void ClearLine(int row) 
    {
        for (int x = this.Bounds.x; x < this.Bounds.x + this.Bounds.width; x++) 
        {
            Vector3Int tilePosition = new Vector3Int(x, row, 0);
            this.tilemap.SetTile(tilePosition, null);
        }
    }

    private void ShiftLinesDown(int clearedRow)
    {
        for (int y = clearedRow + 1; y <= this.Bounds.y + this.Bounds.height - 1; y++)
        {
            for (int x = this.Bounds.x; x < this.Bounds.x + this.Bounds.width; x++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                Vector3Int newTilePosition = new Vector3Int(x, y - 1, 0);
                TileBase tile = this.tilemap.GetTile(tilePosition);

                this.tilemap.SetTile(newTilePosition, tile);
                this.tilemap.SetTile(tilePosition, null);
            }
        }
    }
}