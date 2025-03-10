using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackPawn : MonoBehaviour
{
    private bool isActive = false; // Tracks if the player is selecting pawns to stack
    private string playerTag; // Stores the player's piece tag ("White" or "Black")
    private GameObject firstPawn; // Stores the first pawn selected for stacking

    public void Activate()
    {
        isActive = true;
        playerTag = GameObject.FindFirstObjectByType<GameController>().WhiteTurn ? "White" : "Black";
        Debug.Log("Pawn Stack card activated! Click on two adjacent pawns to stack them.");
    }

    void Update()
    {
        if (isActive && Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));

            if (hit.collider != null)
            {
                GameObject selectedPiece = hit.collider.gameObject;

                if (selectedPiece.CompareTag(playerTag) && selectedPiece.name.Contains("Pawn"))
                {
                    if (firstPawn == null)
                    {
                        // Select the first pawn
                        firstPawn = selectedPiece;
                        Debug.Log($"First pawn selected: {firstPawn.name}");
                    }
                    else
                    {
                        // Check if the second pawn is adjacent to the first pawn
                        if (IsAdjacent(firstPawn, selectedPiece))
                        {
                            // Stack the pawns
                            StackPawns(firstPawn, selectedPiece);
                            isActive = false; // Deactivate the card after stacking
                        }
                        else
                        {
                            Debug.Log("Selected pawns are not adjacent. Please select two adjacent pawns.");
                            firstPawn = null; // Reset the first pawn selection
                        }
                    }
                }
                else
                {
                    Debug.Log("Invalid selection. Please click on your pawns.");
                }
            }
        }
    }

    private bool IsAdjacent(GameObject pawn1, GameObject pawn2)
    {
        Vector3 pos1 = pawn1.transform.position;
        Vector3 pos2 = pawn2.transform.position;

        // Check if the pawns are adjacent horizontally or vertically
        return (Mathf.Abs(pos1.x - pos2.x) == 1 && pos1.y == pos2.y) || // Adjacent horizontally
               (Mathf.Abs(pos1.y - pos2.y) == 1 && pos1.x == pos2.x);   // Adjacent vertically
    }
    private void StackPawns(GameObject pawn1, GameObject pawn2)
{
    // Choose one pawn to be the "stacked" pawn
    GameObject stackedPawn = pawn1;
    Destroy(pawn2); // Remove the second pawn

    // Modify the stacked pawn's movement capabilities
    PieceController pawnController = stackedPawn.GetComponent<PieceController>();
    if (pawnController != null)
    {
        pawnController.CanMoveDiagonally = true; // Allow diagonal movement
        Debug.Log($"Pawns stacked! {stackedPawn.name} can now move diagonally. CanMoveDiagonally: {pawnController.CanMoveDiagonally}");
    }
    else
    {
        Debug.LogError("Selected pawn does not have a PieceController component.");
    }
}
}

