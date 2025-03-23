using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using static System.Math;
using Debug = UnityEngine.Debug;

public class ManaManager : MonoBehaviour
{
    private int whiteMana;        // Current white player mana
    private int blackMana;        // Current black player mana
    private int whiteManaSpent;   // Mana spent by white player
    private int blackManaSpent;   // Mana spent by black player
    private int maxMana;          // Maximum mana allowed

    private UIManager uiManager;  // Reference to the UI manager

    /// <summary>
    /// Initializes the ManaManager with starting values
    /// </summary>
    public void Initialize(int initialMana, int maxManaValue, UIManager uiManagerRef)
    {
        whiteMana = initialMana;
        blackMana = initialMana;
        whiteManaSpent = 0;
        blackManaSpent = 0;
        maxMana = maxManaValue;

        uiManager = uiManagerRef;

        UpdateUI();
    }

    /// <summary>
    /// Spends mana for the current player if they have enough
    /// </summary>
    /// <param name="isWhiteTurn">Whether it's white's turn</param>
    /// <param name="cost">Mana cost to spend (-1 for half)</param>
    /// <returns>Whether spending mana was successful</returns>
    public bool SpendMana(bool isWhiteTurn, int cost = 1)
    {
        if (isWhiteTurn)
        {
            if (cost == -1)
            {
                cost = (int)Floor((decimal)whiteMana / 2);
            }

            if ((whiteMana - whiteManaSpent - cost) < 0)
            {
                return false;
            }
            whiteManaSpent += cost;
        }
        else
        {
            if (cost == -1)
            {
                cost = (int)Floor((decimal)blackMana / 2);
            }

            if ((blackMana - blackManaSpent - cost) < 0)
            {
                return false;
            }

            blackManaSpent += cost;
        }

        UpdateUI();
        return true;
    }

    /// <summary>
    /// Updates mana values at the beginning of a new turn
    /// </summary>
    /// <param name="isCurrentlyWhiteTurn">Whether it's currently white's turn (before switch)</param>
    /// <param name="turnCount">Current turn count</param>
    /// <param name="initialMana">Initial mana value</param>
    public void UpdateManaForNewTurn(bool isCurrentlyWhiteTurn, int turnCount, int initialMana)
    {
        if (isCurrentlyWhiteTurn)
        {
            // Black will get mana when White's turn ends
            blackMana = Min(turnCount + initialMana, maxMana);
            blackManaSpent = 0;
            Debug.Log("Black mana: " + blackMana);
        }
        else
        {
            // White will get mana when Black's turn ends
            whiteMana = Min(turnCount + initialMana, maxMana);
            whiteManaSpent = 0;
            Debug.Log("White mana: " + whiteMana);
        }

        UpdateUI();
    }

    /// <summary>
    /// Gets the available mana for a player
    /// </summary>
    /// <param name="isWhitePlayer">Whether to get white player's mana</param>
    /// <returns>Available mana</returns>
    public int GetAvailableMana(bool isWhitePlayer)
    {
        return isWhitePlayer ? (whiteMana - whiteManaSpent) : (blackMana - blackManaSpent);
    }

    /// <summary>
    /// Updates the UI through the UIManager
    /// </summary>
    private void UpdateUI()
    {
        if (uiManager != null)
        {
            uiManager.UpdateManaUI(whiteMana - whiteManaSpent, blackMana - blackManaSpent);
        }
    }
}