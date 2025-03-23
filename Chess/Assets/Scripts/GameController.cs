using System.Collections;
using System.Collections.Generic;
using static System.Math;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class GameController : MonoBehaviour
{
    public GameObject Board;
    public GameObject WhitePieces;
    public GameObject BlackPieces;
    public int TurnCount = 1;
    public int initMana;
    public GameObject SelectedPiece;
    public bool WhiteTurn = true;
    public int MaxMana = 10;
    public DeckManager deckManager; // Assign it in the Unity Inspector
    public Camera mainCamera;

    // Manager references
    public ManaManager manaManager; // Reference to the ManaManager
    public UIManager uiManager;    // Reference to the UIManager

    // Phase management
    public enum GamePhase { CardPhase, MovePhase }
    public GamePhase currentPhase = GamePhase.CardPhase;
    public int maxMovesPerTurn = 1; // Variable to control max moves per turn
    public int movesRemaining = 0;

    // Use this for initialization
    void Start()
    {
        mainCamera = Camera.main; // Find the main camera

        // Initialize ManaManager if not assigned
        if (manaManager == null)
        {
            manaManager = GetComponent<ManaManager>();
            if (manaManager == null)
            {
                manaManager = gameObject.AddComponent<ManaManager>();
            }
        }

        // Initialize UIManager if not assigned
        if (uiManager == null)
        {
            uiManager = GetComponent<UIManager>();
            if (uiManager == null)
            {
                uiManager = gameObject.AddComponent<UIManager>();
            }
        }

        // Initialize mana through the manaManager
        manaManager.Initialize(initMana, MaxMana, uiManager);

        // Initialize UI
        uiManager.Initialize();

        movesRemaining = maxMovesPerTurn;
        uiManager.UpdatePhaseUI(currentPhase);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdatePieceColors()
    {
        // Check all white pieces
        foreach (Transform piece in WhitePieces.transform)
        {
            PieceController pieceController = piece.GetComponent<PieceController>();
            if (pieceController != null)
            {
                // If piece has more than 1 life, color it green
                if (pieceController.numLives > 1)
                {
                    piece.GetComponent<SpriteRenderer>().color = Color.green;
                }
                // If it's selected, keep it yellow
                else if (SelectedPiece == piece.gameObject)
                {
                    piece.GetComponent<SpriteRenderer>().color = Color.yellow;
                }
                // Otherwise, normal color
                else
                {
                    piece.GetComponent<SpriteRenderer>().color = Color.white;
                }
            }
        }

        // Check all black pieces
        foreach (Transform piece in BlackPieces.transform)
        {
            PieceController pieceController = piece.GetComponent<PieceController>();
            if (pieceController != null)
            {
                // If piece has more than 1 life, color it green
                if (pieceController.numLives > 1)
                {
                    piece.GetComponent<SpriteRenderer>().color = Color.green;
                }
                // If it's selected, keep it yellow
                else if (SelectedPiece == piece.gameObject)
                {
                    piece.GetComponent<SpriteRenderer>().color = Color.yellow;
                }
                // Otherwise, normal color
                else
                {
                    piece.GetComponent<SpriteRenderer>().color = Color.white;
                }
            }
        }
    }

    // Public method to be called from UIManager
    public void EndCardPhase()
    {
        // Only allow ending card phase for the current player's turn
        if (currentPhase == GamePhase.CardPhase)
        {
            SwitchToMovePhase();
        }
    }

    void SwitchToMovePhase()
    {
        currentPhase = GamePhase.MovePhase;
        movesRemaining = maxMovesPerTurn;
        uiManager.UpdatePhaseUI(currentPhase);
        uiManager.UpdateMovesRemainingUI(movesRemaining);
    }

    public void SelectPiece(GameObject piece)
    {
        // Only allow piece selection during Move Phase
        if (currentPhase != GamePhase.MovePhase)
        {
            Debug.Log("Cannot select pieces during Card Phase");
            return;
        }

        if (movesRemaining <= 0)
        {
            Debug.Log("No moves remaining");
            return;
        }

        if (piece.tag == "White" && WhiteTurn == true || piece.tag == "Black" && WhiteTurn == false)
        {
            DeselectPiece();
            SelectedPiece = piece;

            // Highlight
            SelectedPiece.GetComponent<SpriteRenderer>().color = Color.yellow;

            // Put above other pieces
            Vector3 newPosition = SelectedPiece.transform.position;
            newPosition.z = -1;
            SelectedPiece.transform.SetPositionAndRotation(newPosition, SelectedPiece.transform.rotation);
        }
    }

    public void DeselectPiece()
    {
        UpdatePieceColors();
        if (SelectedPiece != null)
        {
            // Remove highlight
            SelectedPiece.GetComponent<SpriteRenderer>().color = Color.white;

            // Put back on the same level as other pieces
            Vector3 newPosition = SelectedPiece.transform.position;
            newPosition.z = 0;
            SelectedPiece.transform.SetPositionAndRotation(newPosition, SelectedPiece.transform.rotation);

            SelectedPiece = null;
        }
    }

    public void ActivatePawnDoubleMove(GameObject pawn)
    {
        if (pawn != null && pawn.name.Contains("Pawn"))
        {
            PieceController pawnController = pawn.GetComponent<PieceController>();
            if (pawnController != null)
            {
                pawnController.DoubleMoveEnabled = true;
                Debug.Log($"PawnDoubleMove card activated! {pawn.name} can now move double the distance.");
            }
        }
        else
        {
            Debug.Log("Invalid selection. Please select a pawn to enhance.");
        }
    }

    // Call this method when a move is successfully completed
    public void MoveMade()
    {
        UpdatePieceColors();
        if (currentPhase == GamePhase.MovePhase)
        {
            movesRemaining--;
            uiManager.UpdateMovesRemainingUI(movesRemaining);

            if (movesRemaining <= 0)
            {
                EndTurn();
            }
        }
    }

    public bool SpendMana(int cost = 1)
    {
        // Delegate to ManaManager
        return manaManager.SpendMana(WhiteTurn, cost);
    }

    public void EndTurn()
    {
        UpdatePieceColors();
        bool kingIsInCheck = false;
        bool hasValidMoves = false;

        // Update mana values using ManaManager
        TurnCount = WhiteTurn ? TurnCount : TurnCount + 1;
        manaManager.UpdateManaForNewTurn(WhiteTurn, TurnCount, initMana);

        WhiteTurn = !WhiteTurn;
        uiManager.UpdateTurnIndicator(WhiteTurn);

        // Reset to Card Phase at the start of each turn
        currentPhase = GamePhase.CardPhase;
        uiManager.UpdatePhaseUI(currentPhase);

        deckManager.maxDraw = 4;
        // AL Edit: Added the logic to turn the board 
        // Rotate the camera 180 degrees to have the current player at the bottom
        mainCamera.transform.Rotate(0, 0, 180);

        // Rotate each piece to keep them facing the right way
        foreach (Transform piece in WhitePieces.transform)
        {
            piece.Rotate(0, 0, 180);
        }
        foreach (Transform piece in BlackPieces.transform)
        {
            piece.Rotate(0, 0, 180);
        }

        if (WhiteTurn)
        {
            deckManager.maxDraw = deckManager.CountAvailableSlots(deckManager.whiteSlotCards);
            foreach (Transform piece in WhitePieces.transform)
            {
                if (hasValidMoves == false && HasValidMoves(piece.gameObject))
                {
                    hasValidMoves = true;
                }

                if (piece.name.Contains("Pawn"))
                {
                    PieceController pawn = piece.GetComponent<PieceController>();
                    if (WhiteTurn && piece.tag == "Black") // Reset only black pawns when white's turn starts
                    {
                        pawn.DoubleStep = false;
                    }
                    else if (!WhiteTurn && piece.tag == "White") // Reset only white pawns when black's turn starts
                    {
                        pawn.DoubleStep = false;
                    }
                }

                else if (piece.name.Contains("King"))
                {
                    kingIsInCheck = piece.GetComponent<PieceController>().IsInCheck(piece.position);
                }
            }
        }
        else
        {
            deckManager.maxDraw = deckManager.CountAvailableSlots(deckManager.blackSlotCards);
            foreach (Transform piece in BlackPieces.transform)
            {
                if (hasValidMoves == false && HasValidMoves(piece.gameObject))
                {
                    hasValidMoves = true;
                }

                if (piece.name.Contains("Pawn"))
                {
                    PieceController pawn = piece.GetComponent<PieceController>();
                    if (WhiteTurn && piece.tag == "Black") // Reset only black pawns when white's turn starts
                    {
                        pawn.DoubleStep = false;
                    }
                    else if (!WhiteTurn && piece.tag == "White") // Reset only white pawns when black's turn starts
                    {
                        pawn.DoubleStep = false;
                    }
                }
                else if (piece.name.Contains("King"))
                {
                    kingIsInCheck = piece.GetComponent<PieceController>().IsInCheck(piece.position);
                }
            }
        }

        if (hasValidMoves == false)
        {
            if (kingIsInCheck == false)
            {
                Stalemate();
            }
            else
            {
                Checkmate();
            }
        }
    }

    bool HasValidMoves(GameObject piece)
    {
        PieceController pieceController = piece.GetComponent<PieceController>();
        GameObject encounteredEnemy;

        foreach (Transform square in Board.transform)
        {
            if (pieceController.ValidateMovement(piece.transform.position, new Vector3(square.position.x, square.position.y, piece.transform.position.z), out encounteredEnemy))
            {
                Debug.Log(piece + " on " + square);
                return true;
            }
        }
        return false;
    }

    void Stalemate()
    {
        Debug.Log("Stalemate!");
        uiManager.ShowGameOverState(false);
    }

    void Checkmate()
    {
        Debug.Log("Checkmate!");
        uiManager.ShowGameOverState(true);
    }
}