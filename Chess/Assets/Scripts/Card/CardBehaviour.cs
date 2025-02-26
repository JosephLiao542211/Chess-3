using System;
using UnityEngine;

public class CardBehaviour : MonoBehaviour
{
    public Action onPlayAction; // Function to run when played
    private bool isDragging = false;
    private Vector3 offset;
    private Transform currentSlot; // The slot the card started in
   

    [SerializeField] private GameObject playZone; // Assign the special play zone in Inspector

    void Start()
    {
        if (playZone == null)
        {
            playZone = GameObject.FindWithTag("PlayZone"); // Make sure PlayZone has a tag set in the Inspector
        }

        // Find the card’s original slot by checking DeckManager's dictionary
        foreach (var slot in DeckManager.Instance.slotCards)
        {
            if (slot.Value == this.gameObject)
            {
                currentSlot = slot.Key;
                break;
            }
        }
    }

    void OnMouseDown()
    {
        isDragging = true;
        offset = transform.position - GetMouseWorldPosition();
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
        isDragging = false;

        if (IsInPlayZone())
        {
            // Execute the special action when played
            onPlayAction?.Invoke();

            // Update DeckManager’s dictionary (set previous slot to null)
            if (currentSlot != null)
            {
                DeckManager.Instance.slotCards[currentSlot] = null;
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

        return playZoneCollider.bounds.Intersects(cardCollider.bounds);
    }
}