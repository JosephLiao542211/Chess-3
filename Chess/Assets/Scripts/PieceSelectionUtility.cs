using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceSelectionUtility : MonoBehaviour
{
    public static PieceSelectionUtility Instance { get; private set; }

    [SerializeField] private Camera boardCamera;
    [SerializeField] private GameController gameController;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (boardCamera == null)
        {
            boardCamera = GameObject.FindGameObjectWithTag("BoardCamera").GetComponent<Camera>();
        }

        if (gameController == null)
        {
            gameController = FindFirstObjectByType<GameController>();
        }
    }

    /// <summary>
    /// Handles piece selection regardless of camera rotation
    /// </summary>
    /// <param name="worldPosition">World position to raycast from</param>
    /// <returns>Selected piece GameObject or null if nothing was hit</returns>
    public GameObject SelectPieceAtPosition(Vector3 worldPosition)
    {
        Ray ray = boardCamera.ScreenPointToRay(boardCamera.WorldToScreenPoint(worldPosition));
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null)
        {
            GameObject hitObject = hit.collider.gameObject;
            // Check if we hit a piece
            if (hitObject.CompareTag("White") || hitObject.CompareTag("Black"))
            {
                return hitObject;
            }
        }

        return null;
    }

    /// <summary>
    /// Selects a piece by tag and name regardless of camera rotation
    /// </summary>
    /// <param name="pieceTag">Tag of the piece ("White" or "Black")</param>
    /// <param name="pieceName">Name of the piece (e.g., "Pawn", "King")</param>
    /// <returns>Selected piece GameObject or null if not found</returns>
    public GameObject SelectPieceByTagAndName(string pieceTag, string pieceName)
    {
        Transform parentContainer = pieceTag == "White" ?
            gameController.WhitePieces.transform :
            gameController.BlackPieces.transform;

        foreach (Transform child in parentContainer)
        {
            if (child.name.Contains(pieceName))
            {
                return child.gameObject;
            }
        }

        return null;
    }

    /// <summary>
    /// Selects a piece for card effects and handles proper GameController selection
    /// </summary>
    /// <param name="piece">Piece to select</param>
    /// <returns>True if selection was successful</returns>
    public bool SelectPieceForCardEffect(GameObject piece)
    {
        if (piece == null) return false;

        // Check if it's a valid selection based on turn
        bool isWhitePiece = piece.CompareTag("White");
        bool validSelection = (isWhitePiece && gameController.WhiteTurn) ||
                             (!isWhitePiece && !gameController.WhiteTurn);

        if (validSelection)
        {
            gameController.SelectPiece(piece);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Get all pieces of a specific type (e.g., all pawns)
    /// </summary>
    /// <param name="pieceType">Type of piece to find (e.g., "Pawn", "Knight")</param>
    /// <returns>List of all matching pieces</returns>
    public List<GameObject> GetAllPiecesOfType(string pieceType, string pieceTag = null)
    {
        List<GameObject> result = new List<GameObject>();

        // If no tag specified, search both white and black pieces
        Transform[] containersToSearch = pieceTag == null ?
            new Transform[] { gameController.WhitePieces.transform, gameController.BlackPieces.transform } :
            new Transform[] { pieceTag == "White" ? gameController.WhitePieces.transform : gameController.BlackPieces.transform };

        foreach (Transform container in containersToSearch)
        {
            foreach (Transform child in container)
            {
                if (child.name.Contains(pieceType))
                {
                    result.Add(child.gameObject);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Converts a screen position to board position, accounting for camera rotation
    /// </summary>
    /// <param name="screenPosition">Screen space position (from Input.mousePosition)</param>
    /// <returns>World position on the board</returns>
    public Vector3 ScreenToWorldPointOnBoard(Vector3 screenPosition)
    {
        Vector3 worldPos = boardCamera.ScreenToWorldPoint(screenPosition);
        worldPos.z = 0; // Set z to 0 to match the board's z position
        return worldPos;
    }
}