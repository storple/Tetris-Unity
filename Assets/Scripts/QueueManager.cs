using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class QueueManager : MonoBehaviour
{
    public Tilemap previewTilemap;
    public Vector3Int queueStartPosition;
    public Vector3Int holdPosition;

    private Queue<TetrominoData> nextQueue = new Queue<TetrominoData>();
    private TetrominoData? holdPiece = null;

    private Board board;

    public int queueSize = 5; // Number of upcoming pieces shown

    private List<TetrominoData> bag = new List<TetrominoData>();

    private void Start()
    {
        board = FindObjectOfType<Board>();
        if (board == null)
        {
            Debug.LogError("QueueManager: Board component not found!");
            enabled = false;
            return;
        }
        InitializeQueue();
    }

    private void InitializeQueue()
    {
        if (board == null)
        {
            Debug.LogError("QueueManager: Board component not found!");
            enabled = false;
            return;
        }

        RefillBag();
        for (int i = 0; i < queueSize; i++)
        {
            EnqueueNewPiece();
        }
    }

    private void RefillBag()
    {
        bag.Clear();
        bag.AddRange(board.tetrominoes);
        ShuffleBag();
    }

    private void ShuffleBag()
    {
        for (int i = 0; i < bag.Count; i++)
        {
            int randomIndex = Random.Range(i, bag.Count);
            TetrominoData temp = bag[i];
            bag[i] = bag[randomIndex];
            bag[randomIndex] = temp;
        }
    }

    public TetrominoData GetNextPiece()
    {
        if (nextQueue.Count == 0)
        {
            Debug.LogError("QueueManager: Attempted to get next piece from an empty queue.");
            return default(TetrominoData);
        }
        TetrominoData next = nextQueue.Dequeue();
        EnqueueNewPiece();
        UpdatePreview();
        return next;
    }

    private void EnqueueNewPiece()
    {
        if (bag.Count == 0)
        {
            RefillBag();
        }

        TetrominoData newPiece = bag[0];
        bag.RemoveAt(0);
        nextQueue.Enqueue(newPiece);
    }

    private void UpdatePreview()
    {
        previewTilemap.ClearAllTiles();
        int offset = 0;
        foreach (TetrominoData data in nextQueue)
        {
            DrawPreviewPiece(data, queueStartPosition + new Vector3Int(0, -offset * 3, 0));
            offset++;
        }
    }

    private void DrawPreviewPiece(TetrominoData data, Vector3Int position)
    {
        foreach (Vector3Int cell in data.cells)
        {
            previewTilemap.SetTile(position + cell, data.tile);
        }
    }

    public void SetHoldPiece(TetrominoData data)
    {
        holdPiece = data;
        UpdateHoldPreview();
    }

    private void UpdateHoldPreview()
    {
        // Clear only the hold preview area
        ClearHoldPreview();

        if (holdPiece.HasValue)
        {
            Debug.Log($"Drawing hold piece: {holdPiece.Value.tetromino}");
            DrawPreviewPiece(holdPiece.Value, holdPosition);
        }
        else
        {
            Debug.Log("No piece in hold.");
        }
    }

    private void ClearHoldPreview()
    {
        // Assuming the largest Tetromino size is 4x4, clear a 4x4 area around the hold position
        for (int x = -2; x <= 2; x++)
        {
            for (int y = -2; y <= 2; y++)
            {
                Vector3Int tilePosition = holdPosition + new Vector3Int(x, y, 0);
                previewTilemap.SetTile(tilePosition, null);
            }
        }
    }
}