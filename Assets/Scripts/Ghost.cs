using UnityEngine;
using UnityEngine.Tilemaps;

public class Ghost : MonoBehaviour
{
    public Tile tile; // Tile used for the ghost block
    public Board mainBoard; // Reference to the main Board
    public Piece trackingPiece; // Reference to the active piece being tracked

    public Tilemap tilemap { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public Vector3Int position { get; private set; }

    
    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
        cells = new Vector3Int[4]; // Initialize the cells array
        trackingPiece = mainBoard.activePiece;
    }

    public void UpdateGhost()
    {
        Clear();
        Copy();
        Drop();
        Set();
    }

    private void Clear()
    {
        // Clear the ghost tiles from the tilemap
        for (int i = 0; i < cells.Length; i++)
        {
            Vector3Int tilePosition = cells[i] + position;
            tilemap.SetTile(tilePosition, null);
        }
    }

    private void Copy()
    {
        // Copy the active piece's cells to the ghost
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i] = trackingPiece.cells[i];
        }

        // Copy the active piece's position to the ghost
        position = trackingPiece.position;
    }

    private void Drop()
    {
        // Calculate the lowest valid position for the ghost
        Vector3Int ghostPosition = trackingPiece.position;
        int bottom = -mainBoard.boardSize.y / 2;

        // Iterate downward to find the lowest valid position
        while (mainBoard.IsValidPosition(trackingPiece, ghostPosition + Vector3Int.down))
        {
            ghostPosition += Vector3Int.down;
        }

        this.position = ghostPosition;
    }

    private void Set()
    {
        // Set the ghost tiles on the tilemap
        for (int i = 0; i < cells.Length; i++)
        {
            Vector3Int tilePosition = cells[i] + position;
            tilemap.SetTile(tilePosition, tile);
        }
    }
}
