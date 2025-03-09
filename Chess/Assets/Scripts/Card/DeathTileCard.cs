using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathTileCard : MonoBehaviour
{
    private bool isActive = false; // Tracks if the player is selecting a tile to mark as a death tile
    private GameController gameController; // Reference to the GameController
    private GameObject deathTile; // Reference to the death tile object
    private int turnsRemaining = 3; // Number of turns before the death tile disappears

    void Start()
    {
        gameController = FindFirstObjectByType<GameController>();
    }

    public void Activate()
    {
        isActive = true;
        Debug.Log("Death Tile card activated! Click on a tile to mark it as a death tile.");
    }

    void Update()
    {
        if (isActive && Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));

            if (hit.collider != null && hit.collider.CompareTag("Tile"))
            {
                // Mark the tile as a death tile
                Debug.Log("Tile selected: " + hit.collider.gameObject.name);
                deathTile = hit.collider.gameObject;
                deathTile.GetComponent<SpriteRenderer>().color = Color.red; // Change color to indicate death tile
                Debug.Log("Death Tile placed on " + deathTile.name);

                // Deactivate the card after placing the death tile
                isActive = false;

                // Subscribe to the end turn event to track turns
                gameController.OnTurnEnd += HandleTurnEnd;
            }
            else
            {
                Debug.Log("Invalid selection. Please click on a tile.");
            }
        }
    }

    private void HandleTurnEnd()
    {
        if (deathTile != null)
        {
            turnsRemaining--;

            if (turnsRemaining <= 0)
            {
                // Remove the death tile after 3 turns
                RemoveDeathTile();
            }
        }
    }

    private void RemoveDeathTile()
    {
        if (deathTile != null)
        {
            deathTile.GetComponent<SpriteRenderer>().color = Color.white; // Reset tile color
            gameController.OnTurnEnd -= HandleTurnEnd; // Unsubscribe from the event
            deathTile = null;
            Debug.Log("Death Tile removed after 3 turns.");
        }
    }

    public void OnPieceMoved(GameObject piece)
    {
        if (deathTile != null)
        {
            Debug.Log($"Checking if {piece.name} stepped on the Death Tile at {deathTile.name}");
            Debug.Log($"Piece position: {piece.transform.position}, Death Tile position: {deathTile.transform.position}");

            float distance = Vector3.Distance(piece.transform.position, deathTile.transform.position);
            float tolerance = 0.1f; // Adjust this value as needed

            Debug.Log($"Distance between piece and death tile: {distance}");

            if (distance <= tolerance)
            {
                Debug.Log($"{piece.name} stepped on the Death Tile at {deathTile.name}");
                PieceController pieceController = piece.GetComponent<PieceController>();
                if (pieceController != null)
                {
                    pieceController.LoseLife();
                    Debug.Log($"{piece.name} stepped on the Death Tile and lost a life!");

                    // Remove the death tile after a piece steps on it
                    RemoveDeathTile();
                }
            }
            else
            {
                Debug.Log($"{piece.name} did not step on the Death Tile. Distance: {distance}");
            }
        }
        else
        {
            Debug.Log("No Death Tile active.");
        }
    }
}