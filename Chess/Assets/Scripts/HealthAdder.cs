using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class HealthAdder : MonoBehaviour
{
    public GameController gameController; // Reference to the GameController
    private PieceSelectionUtility selectionUtility; // Reference to the selection utility
    private bool isActive = false; // Tracks whether the listener is active

    private void Start()
    {
        // Get references if not assigned
        if (gameController == null)
        {
            gameController = FindFirstObjectByType<GameController>();
        }

        // Find the selection utility
        selectionUtility = PieceSelectionUtility.Instance;
        if (selectionUtility == null)
        {
            selectionUtility = FindFirstObjectByType<PieceSelectionUtility>();
            Debug.LogWarning("PieceSelectionUtility not found as a singleton. Using FindObjectOfType instead.");
        }
    }

    // Public method to activate the listener
    public void Activate()
    {
        isActive = true;
        Debug.Log("Knight health listener activated. Select a knight to add +1 health.");
    }

    // Public method to deactivate the listener
    public void Deactivate()
    {
        isActive = false;
        Debug.Log("Knight health listener deactivated.");
    }

    // Toggle the active state
    public void ToggleActive()
    {
        if (isActive)
        {
            Deactivate();
        }
        else
        {
            Activate();
        }
    }

    void Update()
    {
        // Only check for mouse clicks if the listener is active
        if (isActive && Input.GetMouseButtonDown(0))
        {
            // Use the utility to handle selection regardless of camera rotation
            Vector3 mousePos = Input.mousePosition;
            Vector3 worldPos = selectionUtility.ScreenToWorldPointOnBoard(mousePos);

            GameObject selectedPiece = selectionUtility.SelectPieceAtPosition(worldPos);

            if (selectedPiece != null)
            {
                PieceController piece = selectedPiece.GetComponent<PieceController>();

                // Check if the clicked piece is a knight
                if (piece != null && selectedPiece.name.Contains("Knight"))
                {
                    // Validate this is a piece that belongs to the current player
                    bool isWhitePiece = selectedPiece.CompareTag("White");
                    if ((isWhitePiece && gameController.WhiteTurn) || (!isWhitePiece && !gameController.WhiteTurn))
                    {
                        // Add +1 health to the knight
                        piece.numLives++;
                        Debug.Log($"{piece.name} now has {piece.numLives} lives.");

                        // Update the colors of all pieces to ensure consistent appearance
                        gameController.UpdatePieceColors();

                        // Auto-deactivate after successfully adding health
                        Deactivate();
                    }
                    else
                    {
                        Debug.Log("You can only select your own knights.");
                    }
                }
                else
                {
                    Debug.Log("You must select a knight.");
                }
            }
        }
    }
}