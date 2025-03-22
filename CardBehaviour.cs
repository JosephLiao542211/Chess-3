using System;
using System.Diagnostics;
using System.Globalization;
using UnityEngine;
using Debug = UnityEngine.Debug;
using TMPro; // For TextMeshPro, if used for UI text

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
    public string cardName; // Name of the card
    public int cardCost; // Mana cost
    [SerializeField] private GameObject playZone; // Assign the special play zone in Inspector
    [SerializeField] private TextMeshProUGUI descriptionText; // Text component for description (assign in Inspector)
    [SerializeField] private string cardDescription = "No description available."; // Default description

    private bool isInspecting = false;
    private float lastClickTime; // To detect double-click
    private const float doubleClickThreshold = 0.2f; // Time window for double-click (0.2 seconds)

    void Start()
    {
        if (playZone == null)
        {
            playZone = GameObject.FindWithTag("PlayZone"); // Ensure PlayZone has the proper tag set in the Inspector
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
        if (descriptionText == null)
        {
            Debug.LogError("Description Text not assigned in Inspector for " + cardName + ". Please assign a TextMeshProUGUI.");
            return; // Exit to avoid null reference issues
        }
        // Hide the description text by default
        descriptionText.gameObject.SetActive(false);

        // Find the card's original slot by checking DeckManager's dictionaries
        FindOriginalSlot();
        SetCardDescription();
    }

    private void SetCardDescription()
    {
        switch (cardName)
        {
            case "Card 1":
                cardDescription = "Destroy an enemy pawn on the board.";
                break;
            case "Card 2":
                cardDescription = "Add health to a selected knight.";
                break;
            case "Card 3":
                cardDescription = "Grant a pawn double movement for this turn.";
                break;
            case "Card 4":
                cardDescription = "A basic card with no special ability.";
                break;
            case "Card 5":
                cardDescription = "A basic card with no special ability.";
                break;
            case "Card 6":
                cardDescription = "A basic card with no special ability.";
                break;
            case "Card 7":
                cardDescription = "A basic card with no special ability.";
                break;
            case "Unleash the Hounds": // Example from your earlier request
                cardDescription = "For each enemy minion, summon a 1/1 Hound with Charge.";
                break;
            // Add more card descriptions here as needed
            case "Healing Potion":
                cardDescription = "Restore 5 health to a selected unit.";
                break;
            case "Fireball":
                cardDescription = "Deal 3 damage to a target enemy.";
                break;
            default:
                cardDescription = "No description available.";
                Debug.LogWarning($"No description set for card: {cardName}");
                break;
        }
    }

    // Ability execution
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

        // Then check black player slots
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

        if (Time.time - lastClickTime < doubleClickThreshold)
        {
            ToggleInspectMode();
            lastClickTime = 0f; // Reset after handling double-click
            return; // Prevent dragging during double-click
        }
        lastClickTime = Time.time; // Record the click time

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

        GameController gameController = FindFirstObjectByType<GameController>();
        if (IsInPlayZone() && gameController.SpendMana(cardCost))
        {
            // Execute the special action when played
            ExecuteAbility(cardName);

            // Update the appropriate slot dictionary (set previous slot to null)
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

            // Destroy the card (or move it to a discard pile)
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

        Bounds playZoneBounds = playZoneCollider.bounds;
        Bounds cardBounds = cardCollider.bounds;

        // Ignore z-axis: set z extents to 0
        playZoneBounds.min = new Vector3(playZoneBounds.min.x, playZoneBounds.min.y, 0f);
        playZoneBounds.max = new Vector3(playZoneBounds.max.x, playZoneBounds.max.y, 0f);
        cardBounds.min = new Vector3(cardBounds.min.x, cardBounds.min.y, 0f);
        cardBounds.max = new Vector3(cardBounds.max.x, cardBounds.max.y, 0f);

        Debug.Log(playZoneBounds);
        Debug.Log(cardBounds);

        return playZoneBounds.Intersects(cardBounds);
    }

    // Toggle the display of the card's description directly on the card
    private void ToggleInspectMode()
    {
        isInspecting = !isInspecting;
        descriptionText.gameObject.SetActive(isInspecting);
        if (isInspecting)
        {
            UpdateInspectPanel();
        }
    }

    private void UpdateInspectPanel()
    {
        if (descriptionText != null)
        {
            descriptionText.text = $"{cardName}\n{cardDescription}";
        }
        else
        {
            Debug.LogWarning("descriptionText is null, cannot update inspect panel.");
        }
    }

    // Modular method to set card data
    public void SetCardData(string name, string description, int cost)
    {
        cardName = name;
        cardDescription = description;
        cardCost = cost;
        if (isInspecting)
            UpdateInspectPanel(); // Update if already inspecting
    }
}