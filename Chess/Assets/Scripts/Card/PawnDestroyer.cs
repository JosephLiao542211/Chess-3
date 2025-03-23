using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnDestroyer : MonoBehaviour
{
    private bool isActive = false; // Tracks if the player is selecting a pawn to destroy
    private string opponentTag; // Stores the opponent's piece tag ("White" or "Black")
    public Camera boardCamera; // A reference to the camera that views the chessboard

    public void Activate()
    {
        isActive = true;
        opponentTag = GameObject.FindFirstObjectByType<GameController>().WhiteTurn ? "Black" : "White";
        Debug.Log("Pawn Destroyer card activated! Click on an opponent's pawn to remove it.");
    }

    void Update()
    {
        if (isActive && Input.GetMouseButtonDown(0))
        {
            // Use the board camera for raycasting, rather than Camera.main
            RaycastHit2D hit = Physics2D.GetRayIntersection(boardCamera.ScreenPointToRay(Input.mousePosition));

            if (hit.collider != null)
            {
                GameObject selectedPiece = hit.collider.gameObject;

                if (selectedPiece.CompareTag(opponentTag) && selectedPiece.name.Contains("Pawn"))
                {
                    Destroy(selectedPiece);
                    Debug.Log("Pawn Destroyer used! Removed " + selectedPiece.name);

                    isActive = false;
                }
                else
                {
                    Debug.Log("Invalid selection. Please click on an opponent's pawn.");
                }
            }
        }
    }
}
