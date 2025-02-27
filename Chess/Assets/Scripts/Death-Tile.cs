using UnityEngine;

public class DeathTile : MonoBehaviour
{
    // Called when a 2D collider enters the trigger area
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the colliding object has the "Piece" or "Player" tag
        if (collision.CompareTag("Piece") || collision.CompareTag("Player"))
        {
            // Destroy the colliding object
            Destroy(collision.gameObject);
            Debug.Log("Object collided with Death Tile and was destroyed.");
        }
    }
}
