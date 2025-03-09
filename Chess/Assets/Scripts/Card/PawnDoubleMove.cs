using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnDoubleMove : MonoBehaviour
{
    private bool isActive = false; 
    private string playerTag; 

    public void Activate()
    {
        isActive = true;
        playerTag = GameObject.FindFirstObjectByType<GameController>().WhiteTurn ? "White" : "Black";
        Debug.Log("Pawn Double Move card activated! Click on your pawn to enhance its movement.");
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
                    // Activate the double move enhancement
                    GameController gameController = GameObject.FindFirstObjectByType<GameController>();
                    gameController.ActivatePawnDoubleMove(selectedPiece);

                    isActive = false;
                }
                else
                {
                    Debug.Log("Invalid selection. Please click on your pawn.");
                }
            }
        }
    }
}