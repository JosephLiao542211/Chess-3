using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class HealthAdder : MonoBehaviour
{
    public GameController gameController; // Reference to the GameController
    private bool isActive = false; // Tracks whether the listener is active

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
            // Raycast to detect which piece was clicked
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null)
            {
                PieceController piece = hit.collider.GetComponent<PieceController>();
                // Check if the clicked piece is a knight
                if (piece != null && piece.name.Contains("Knight"))
                {
                    // Add +1 health to the knight
                    piece.numLives++;
                    Debug.Log($"{piece.name} now has {piece.numLives} lives.");

                    // Auto-deactivate after successfully adding health
                    Deactivate();
                }
                else
                {
                    Debug.Log("You must select a knight.");
                }
            }
        }
    }
}