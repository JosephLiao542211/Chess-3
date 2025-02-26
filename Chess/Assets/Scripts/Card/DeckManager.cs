using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }

    public List<CardData> deck = new List<CardData>();
    public GameObject cardPrefab;

    // Dictionary to track which cards are in which slots
    public Dictionary<Transform, GameObject> slotCards = new Dictionary<Transform, GameObject>();

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

    private void Start()
    {
        deck = GenerateDeck();
        ShuffleDeck();

        // Automatically find Pos1 to Pos4 and add them to the dictionary
        for (int i = 1; i <= 4; i++)
        {
            Transform slot = GameObject.Find("Pos" + i).transform;
            slotCards.Add(slot, null);
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
        if (deck.Count <= 0)
        {
            deck = GenerateDeck();
            return;
        }

        // Find first available slot
        Transform availableSlot = FindFirstAvailableSlot();

        if (availableSlot == null)
        {
            Debug.LogWarning("No available slots to place card.");
            return;
        }

        // Draw card and place in available slot
        CardData drawnCard = deck[0];
        deck.RemoveAt(0);
        GameObject cardObject = DisplayCard(drawnCard, availableSlot);

        // Update dictionary
        slotCards[availableSlot] = cardObject;
    }

    private Transform FindFirstAvailableSlot()
    {
        foreach (var slot in slotCards.Keys)
        {
            if (slotCards[slot] == null)
            {
                return slot;
            }
        }
        return null; // No available slots
    }

    GameObject DisplayCard(CardData cardData, Transform slotPosition)
    {
        GameObject newCard = Instantiate(cardPrefab, slotPosition.position, Quaternion.identity);
        newCard.GetComponent<SpriteRenderer>().sprite = cardData.cardSprite;
        return newCard;
    }

    // Method to clear a specific slot
    public void ClearSlot(Transform slot)
    {
        if (slotCards.ContainsKey(slot) && slotCards[slot] != null)
        {
            Destroy(slotCards[slot]);
            slotCards[slot] = null;
        }
    }

    // Method to clear all slots
    public void ClearAllSlots()
    {
        foreach (var slot in slotCards.Keys)
        {
            ClearSlot(slot);
        }
    }

    private void OnMouseDown()
    {
        DrawCard();
    }
}