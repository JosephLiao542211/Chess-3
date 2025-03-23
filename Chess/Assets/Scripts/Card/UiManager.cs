using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Diagnostics;
using Debug = UnityEngine.Debug;


public class UIManager : MonoBehaviour
{
    // UI Text References
    public TextMeshProUGUI whiteManaText;
    public TextMeshProUGUI blackManaText;
    public TextMeshProUGUI phaseText;
    public TextMeshProUGUI turnIndicatorText;

    // UI Button References
    public Button endTurnWhiteButton;
    public Button endTurnBlackButton;
    public Button endPhaseButton;

    // References to other managers
    private GameController gameController;
    private ManaManager manaManager;

    void Awake()
    {
        // Find references if not assigned in inspector
        gameController = GetComponentInParent<GameController>() ?? FindFirstObjectByType<GameController>();
        manaManager = GetComponentInParent<ManaManager>() ?? FindFirstObjectByType<ManaManager>();
    }

    public void Initialize()
    {
        // Set up button listeners
        if (endTurnWhiteButton != null)
            endTurnWhiteButton.onClick.AddListener(() => ButtonEndTurn(true));

        if (endTurnBlackButton != null)
            endTurnBlackButton.onClick.AddListener(() => ButtonEndTurn(false));

        if (endPhaseButton != null)
            endPhaseButton.onClick.AddListener(EndPhaseButtonClicked);

        // Initial UI update
        UpdatePhaseUI(GameController.GamePhase.CardPhase);
        UpdateTurnIndicator(true); // Default to white's turn
    }

    private void ButtonEndTurn(bool isWhite)
    {
        if (gameController != null)
        {
            if ((isWhite && gameController.WhiteTurn) || (!isWhite && !gameController.WhiteTurn))
            {
                gameController.EndTurn();
            }
        }
    }

    private void EndPhaseButtonClicked()
    {
        if (gameController != null)
        {
            gameController.EndCardPhase();
        }
    }

    /// <summary>
    /// Updates the mana display in the UI
    /// </summary>
    public void UpdateManaUI(int whiteAvailableMana, int blackAvailableMana)
    {
        if (whiteManaText != null)
        {
            whiteManaText.text = "White Mana: " + whiteAvailableMana;
        }

        if (blackManaText != null)
        {
            blackManaText.text = "Black Mana: " + blackAvailableMana;
        }
    }

    /// <summary>
    /// Updates the phase display in the UI
    /// </summary>
    public void UpdatePhaseUI(GameController.GamePhase currentPhase)
    {
        string phaseString = currentPhase == GameController.GamePhase.CardPhase ? "Card Phase" : "Move Phase";

        if (phaseText != null)
        {
            phaseText.text = "Current Phase: " + phaseString;
        }

        if (endPhaseButton != null)
        {
            // Enable/disable the end phase button based on the current phase
            endPhaseButton.interactable = currentPhase == GameController.GamePhase.CardPhase;
        }
    }

    /// <summary>
    /// Updates the turn indicator display in the UI
    /// </summary>
    public void UpdateTurnIndicator(bool isWhiteTurn)
    {
        if (turnIndicatorText != null)
        {
            turnIndicatorText.text = "Current Turn: " + (isWhiteTurn ? "White" : "Black");
        }

        // Update button visibility or interactability based on whose turn it is
        if (endTurnWhiteButton != null)
        {
            endTurnWhiteButton.gameObject.SetActive(isWhiteTurn);
        }

        if (endTurnBlackButton != null)
        {
            endTurnBlackButton.gameObject.SetActive(!isWhiteTurn);
        }
    }

    /// <summary>
    /// Updates the moves remaining display in the UI
    /// </summary>
    public void UpdateMovesRemainingUI(int movesRemaining)
    {
        // If you want to add a moves remaining text display
        // For now we'll just log it
        Debug.Log("Moves Remaining: " + movesRemaining);
    }

    /// <summary>
    /// Highlights or shows a message for checkmate or stalemate
    /// </summary>
    public void ShowGameOverState(bool isCheckmate)
    {
        string message = isCheckmate ? "Checkmate!" : "Stalemate!";
        Debug.Log(message);

        // Here you could create and display a game over panel with the message
        // For example:
        // gameOverPanel.SetActive(true);
        // gameOverText.text = message;
    }
}