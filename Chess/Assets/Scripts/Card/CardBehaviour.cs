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
    public PawnDestroyer pawnDestroyer;
    public HealthAdder addHealth;
    public PawnDoubleMove pawnDoubleMove;
    private Transform currentSlot; // The slot the card started in
    private bool isWhitePlayerCard; // Whether this card belongs to the white player
    public string cardName;// Name of the card
    [SerializeField] private GameObject playZone; // Assign the special play zone in Inspector

    void Start()
    {
        if (playZone == null)
        {
            playZone = GameObject.FindWithTag("PlayZone"); // Make sure PlayZone has a tag set in the Inspector
        }
        if (pawnDestroyer == null)
        {
            pawnDestroyer = FindFirstObjectByType<PawnDestroyer>(); // Try to find it if not assigned
        }
        if (addHealth == null)
        {
            addHealth = FindFirstObjectByType<HealthAdder>(); // Try to find it if not assigned
        }
        if (pawnDoubleMove == null)
        {
            pawnDoubleMove = FindFirstObjectByType<PawnDoubleMove>();
        }

        // Find the card's original slot by checking DeckManager's dictionaries
        FindOriginalSlot();
    }






    //Ability type:


    private void ExecuteAbility(string cardName)
    {
        Debug.Log($"Executing ability for {cardName}");

        switch (cardName)
        {
            case "Card 1":
                Debug.Log("Card1: kill a pawn");
                pawnDestroyer.Activate();
                break;

            case "Card 2":
                addHealth.Activate();
                Debug.Log("Card2: Add Health to a Knight");

                break;

            case "Card 3":
                pawnDoubleMove.Activate();
                Debug.Log("Card3: Select a Pawn to have double the movement");
                break;

            case "Card 4":
                Debug.Log("Card4: No special ability.");
                break;

            case "Card 5":
                Debug.Log("Card5: No special ability.");
                break;

            case "Card 6":
                Debug.Log("Card6: No special ability.");
                break;

            case "Card 7":
                Debug.Log("Card7: No special ability.");
                break;

            default:
                Debug.LogWarning($"Unknown card: {cardName}");
                break;
        }
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
            ExecuteAbility(cardName);

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