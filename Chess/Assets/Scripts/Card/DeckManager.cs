using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;
using System.Linq;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }
    public List<CardData> deck = new List<CardData>();
    public int maxDraw = 4;
    public GameObject cardPrefab;
    private GameController gameController;

    // Separate dictionaries for white and black player slots
    public Dictionary<Transform, GameObject> whiteSlotCards = new Dictionary<Transform, GameObject>();
    public Dictionary<Transform, GameObject> blackSlotCards = new Dictionary<Transform, GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Determine which slots should be active based on the turn
        Dictionary<Transform, GameObject> activeSlots = gameController.WhiteTurn ? whiteSlotCards : blackSlotCards;
        Dictionary<Transform, GameObject> inactiveSlots = gameController.WhiteTurn ? blackSlotCards : whiteSlotCards;

        // Enable cards for the active player
        foreach (var slot in activeSlots)
        {
            if (slot.Value != null)
            {
                slot.Value.SetActive(true);
            }
        }

        // Disable cards for the inactive player
        foreach (var slot in inactiveSlots)
        {
            if (slot.Value != null)
            {
                slot.Value.SetActive(false);
            }
        }
    }


    private void Start()
    {
        // Get reference to the GameController
        gameController = FindFirstObjectByType<GameController>();

        if (gameController == null)
        {
            Debug.LogError("GameController not found!");
        }

        deck = GenerateDeck();
        ShuffleDeck();

        // Initialize slots for white player (Pos1 and Pos2)
        for (int i = 1; i <= 4; i++)
        {
            Transform slot = GameObject.Find("Pos" + i).transform;
            whiteSlotCards.Add(slot, null);
        }

        // Initialize slots for black player (Pos3 and Pos4)
        for (int i = 1; i <= 4; i++)
        {
            Transform slot = GameObject.Find("Pos" + i).transform;
            blackSlotCards.Add(slot, null);
        }
    }

    public List<CardData> GenerateDeck()
    {
        List<CardData> newDeck = new List<CardData>();
        for (int i = 0; i < 7; i++) // Adjust based on deck size
        {
            CardData card = new CardData();
            card.cardName = "Card " + (i + 1);
            card.cardSprite = Resources.Load<Sprite>("CardSprites/Card" + (i + 1)); // Ensure sprites are in "Resources/CardSprites/"
            newDeck.Add(card);
        }
        return newDeck;
    }

    public void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            CardData temp = deck[i];
            int randomIndex = Random.Range(i, deck.Count);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }
    public void DrawCard()
    {
        
        if (gameController == null) return;

        // Regenerate deck if empty
        if (deck.Count <= 0)
        {
            Debug.Log("Deck is empty. Generating new deck.");
            deck = GenerateDeck();
            ShuffleDeck();
        }

        // Get current player's slots
        Dictionary<Transform, GameObject> currentSlots = gameController.WhiteTurn ? whiteSlotCards : blackSlotCards;

        

        // Find an available slot
        Transform availableSlot = FindFirstAvailableSlot(currentSlots);

        // If there's an available slot and cards in the deck, draw a card
        if (availableSlot != null && deck.Count > 0 && maxDraw > 0)
        {
            // Draw and display card
            --maxDraw;
            CardData card = deck[0];
            deck.RemoveAt(0);
            currentSlots[availableSlot] = DisplayCard(card, availableSlot);
        }
        else if (availableSlot == null)
        {
            Debug.Log("No available slots for the current player. Current slots: " +
     string.Join(", ", currentSlots.Select(kv => $"{kv.Key}: {kv.Value}")));
        }
        else
        {
            Debug.Log("No cards left in the deck.");
        }
    }

    private Transform FindFirstAvailableSlot(Dictionary<Transform, GameObject> slots)
    {
        foreach (var slot in slots.Keys)
        {
            if (slots[slot] == null)
            {
                return slot;
            }
        }
        return null; // No available slots
    }

    public int CountAvailableSlots(Dictionary<Transform, GameObject> slots)
    {
        int count = 0;
        foreach (var slot in slots.Values)
        {
            if (slot == null)
            {
                count++;
            }
        }
        return count;
    }



    GameObject DisplayCard(CardData cardData, Transform slotPosition)
    {
        GameObject newCard = Instantiate(cardPrefab, slotPosition.position, Quaternion.identity);
        newCard.GetComponent<SpriteRenderer>().sprite = cardData.cardSprite;
        newCard.GetComponent<CardBehaviour>().cardName = cardData.cardName;
        return newCard;
    }

    // Method to clear a specific slot
    public void ClearSlot(Transform slot)
    {
        // Check which dictionary contains the slot
        if (whiteSlotCards.ContainsKey(slot) && whiteSlotCards[slot] != null)
        {
            Destroy(whiteSlotCards[slot]);
            whiteSlotCards[slot] = null;
        }
        else if (blackSlotCards.ContainsKey(slot) && blackSlotCards[slot] != null)
        {
            Destroy(blackSlotCards[slot]);
            blackSlotCards[slot] = null;
        }
    }

    // Method to clear all slots for a specific player
    public void ClearPlayerSlots(bool isWhitePlayer)
    {
        Dictionary<Transform, GameObject> slotsToCheck = isWhitePlayer ? whiteSlotCards : blackSlotCards;

        foreach (var slot in slotsToCheck.Keys)
        {
            if (slotsToCheck[slot] != null)
            {
                Destroy(slotsToCheck[slot]);
                slotsToCheck[slot] = null;
            }
        }
    }

    // Method to clear all slots
    public void ClearAllSlots()
    {
        ClearPlayerSlots(true);  // Clear white player slots
        ClearPlayerSlots(false); // Clear black player slots
    }

    private void OnMouseDown()
    {
        DrawCard();
    }
}
