using System;
using System.Diagnostics;
using System.Globalization;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class CardBehaviour : MonoBehaviour
{
    public Action onPlayAction; // Function to run when played
    private bool isDragging = false;
    private Vector3 offset;
    private Transform currentSlot; // The slot the card started in
    private bool isWhitePlayerCard; // Whether this card belongs to the white player

    [SerializeField] private GameObject playZone; // Assign the special play zone in Inspector

    void Start()
    {
        if (playZone == null)
        {
            playZone = GameObject.FindWithTag("PlayZone"); // Make sure PlayZone has a tag set in the Inspector
        }

        // Find the card's original slot by checking DeckManager's dictionaries
        FindOriginalSlot();
    }

    private void FindOriginalSlot()
    {
        // First check white player slots
        foreach (var slot in DeckManager.Instance.whiteSlotCards)
        {
            if (slot.Value == this.gameObject)
            {
                currentSlot = slot.Key;
                isWhitePlayerCard = true;
                return;
            }
        }

        // If not found, check black player slots
        foreach (var slot in DeckManager.Instance.blackSlotCards)
        {
            if (slot.Value == this.gameObject)
            {
                currentSlot = slot.Key;
                isWhitePlayerCard = false;
                return;
            }
        }

        Debug.LogWarning("Card not found in any slot dictionary.");
    }

    void OnMouseDown()
    {
        GameController gameController = FindFirstObjectByType<GameController>();

        // Only allow dragging if it's the correct player's turn
        if ((isWhitePlayerCard && gameController.WhiteTurn) || (!isWhitePlayerCard && !gameController.WhiteTurn))
        {
            isDragging = true;
            offset = transform.position - GetMouseWorldPosition();
        }
    }

    void OnMouseDrag()
    {
        if (isDragging)
        {
            transform.position = GetMouseWorldPosition() + offset;
        }
    }

    void OnMouseUp()
    {
        if (!isDragging)
            return;

        isDragging = false;

        if (IsInPlayZone())
        {
            // Execute the special action when played
            onPlayAction?.Invoke();

            // Update appropriate DeckManager's dictionary (set previous slot to null)
            if (currentSlot != null)
            {
                if (isWhitePlayerCard)
                {
                    DeckManager.Instance.whiteSlotCards[currentSlot] = null;
                }
                else
                {
                    DeckManager.Instance.blackSlotCards[currentSlot] = null;
                }
            }

            // Destroy the card (or do something else like moving it to a discard pile)
            Destroy(gameObject);
        }
        else
        {
            // Return the card to its original slot
            transform.position = currentSlot.position;
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }

    bool IsInPlayZone()
    {
        Collider2D playZoneCollider = playZone.GetComponent<Collider2D>();
        Collider2D cardCollider = GetComponent<Collider2D>();

        // Get the play zone and card collider bounds
        Bounds playZoneBounds = playZoneCollider.bounds;
        Bounds cardBounds = cardCollider.bounds;

        // Create new bounds that ignore the z-axis (set z extents to 0)
        playZoneBounds.min = new Vector3(playZoneBounds.min.x, playZoneBounds.min.y, 0f);
        playZoneBounds.max = new Vector3(playZoneBounds.max.x, playZoneBounds.max.y, 0f);
        cardBounds.min = new Vector3(cardBounds.min.x, cardBounds.min.y, 0f);
        cardBounds.max = new Vector3(cardBounds.max.x, cardBounds.max.y, 0f);

        // Log the updated bounds
        Debug.Log(playZoneBounds);
        Debug.Log(cardBounds);

        // Check if the bounds intersect without considering the z-axis
        return playZoneBounds.Intersects(cardBounds);
    }
}