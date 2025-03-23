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
    public int whiteMana;
    public int blackMana;
    public int whiteManaSpent;
    public int blackManaSpent;
    public GameObject SelectedPiece;
    public bool WhiteTurn = true;
    public int MaxMana = 10;
    public DeckManager deckManager; // Assign it in the Unity Inspector
    public Camera mainCamera;
    public Camera boardCamera;
    public Button endTurnWhite;
    public Button endTurnBlack;

    public delegate void TurnEndHandler();
    public event TurnEndHandler OnTurnEnd;
    public Button endPhaseButton;    // Single button for ending the phase
    public TextMeshProUGUI WhiteManaText;
    public TextMeshProUGUI BlackManaText;
    public TextMeshProUGUI PhaseText;
    

    // Phase management
    public enum GamePhase { CardPhase, MovePhase }
    public GamePhase currentPhase = GamePhase.CardPhase;
    public int maxMovesPerTurn = 1; // Variable to control max moves per turn
    public int movesRemaining = 0;

    // Use this for initialization
    void Start()
    {
        mainCamera = Camera.main; // Find the main camera
        //Set initial mana values
        whiteMana = initMana;
        blackMana = initMana;
        whiteManaSpent = 0;
        blackManaSpent = 0;
        movesRemaining = maxMovesPerTurn;
        UpdateManaUI();
        UpdatePhaseUI();
        //Buttons
        endTurnWhite.onClick.AddListener(() => ButtonEndTurn(true));
        endTurnBlack.onClick.AddListener(() => ButtonEndTurn(false));
        endPhaseButton.onClick.AddListener(EndCardPhase);
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

    void ButtonEndTurn(bool White)
    {
        if ((White && WhiteTurn) || (!White && !WhiteTurn))
        {
            EndTurn();
        }
    }

    void EndCardPhase()
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
        UpdatePhaseUI();
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
            UpdatePhaseUI();

            if (movesRemaining <= 0)
            {
                EndTurn();
            }
        }
    }

    public bool SpendMana(int cost = 1)
    {
        if (WhiteTurn)
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
        else if (!WhiteTurn)
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

        UpdateManaUI();
        return true;
    }
    
    public void EndTurn()
    {
        UpdatePieceColors();
        bool kingIsInCheck = false;
        bool hasValidMoves = false;

        //Update mana values
        if (!WhiteTurn)
        {
            TurnCount++;
            whiteMana = Min(TurnCount+initMana, MaxMana);
            whiteManaSpent = 0;
            Debug.Log(whiteMana);
        }
        else
        {
            blackMana = Min(TurnCount+initMana, MaxMana);
            blackManaSpent = 0;
            Debug.Log(blackMana);
        }

        UpdateManaUI();

        WhiteTurn = !WhiteTurn;

        // Reset to Card Phase at the start of each turn
        currentPhase = GamePhase.CardPhase;
        UpdatePhaseUI();

        deckManager.maxDraw = 4;
        // AL Edit: Added the logic to turn the board 
        // Rotate the camera 180 degrees to have the current player at the bottom
        boardCamera.transform.Rotate(0, 0, 180);

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

        OnTurnEnd?.Invoke();

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
    }

    void Checkmate()
    {
        Debug.Log("Checkmate!");
    }

    void UpdateManaUI()
    {
        WhiteManaText.text = "White Mana: " + (whiteMana - whiteManaSpent);
        BlackManaText.text = "Black Mana: " + (blackMana - blackManaSpent);
    }

    void UpdatePhaseUI()
    {
        string phaseString = currentPhase == GamePhase.CardPhase ? "Card Phase" : "Move Phase";
        PhaseText.text = "Current Phase: " + phaseString;

        if (currentPhase == GamePhase.MovePhase)
        {
            //MovesRemainingText.text = "Moves Remaining: " + movesRemaining;
            // You might want to disable the phase end button during Move Phase
            endPhaseButton.interactable = false;
        }
        else
        {
            //MovesRemainingText.text = "";
            endPhaseButton.interactable = true;
        }
    }
}