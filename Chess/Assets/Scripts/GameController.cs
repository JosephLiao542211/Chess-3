using System.Collections;
using System.Collections.Generic;
using static System.Math;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameController : MonoBehaviour
{
    public GameObject Board;
    public GameObject WhitePieces;
    public GameObject BlackPieces;
    public int TurnCount = 1;
    public int whiteMana;
    public int blackMana;
    public int whiteManaSpent;
    public int blackManaSpent;
    public GameObject SelectedPiece;
    public bool WhiteTurn = true;
    public int MaxMana = 10;
    public DeckManager deckManager; // Assign it in the Unity Inspector
    public Camera mainCamera;
    public Button endTurnWhite;
    public Button endTurnBlack;
    public delegate void TurnEndHandler();
    public event TurnEndHandler OnTurnEnd;
  

    public TextMeshProUGUI WhiteManaText;
    public TextMeshProUGUI BlackManaText;

    // Use this for initialization
    void Start()
    {
        mainCamera = Camera.main; // Find the main camera
        //Set initial mana values
        whiteMana = 1;
        blackMana = 1;
        whiteManaSpent = 0;
        blackManaSpent = 0;
        UpdateManaUI();
        //Buttons
        endTurnWhite.onClick.AddListener(() => ButtonEndTurn(true));
        endTurnBlack.onClick.AddListener(() => ButtonEndTurn(false));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ButtonEndTurn(bool White){
        if ((White && WhiteTurn) || (!White && !WhiteTurn)){
            EndTurn();
        }
    }

    public void SelectPiece(GameObject piece)
    {
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

    public bool SpendMana(int cost = 1)
    {   
        if (WhiteTurn){
            if (cost == -1) {
                cost = (int)Floor((decimal)whiteMana/2);
            }

            if ((whiteMana - whiteManaSpent - cost) < 0){
                return false;
            }
            whiteManaSpent += cost;
        }
        else if (!WhiteTurn){
            if (cost == -1) {
                cost = (int)Floor((decimal)blackMana/2);
            }

            if ((blackMana - blackManaSpent - cost) < 0){
                return false;
            }
        
            blackManaSpent += cost;
        }

        UpdateManaUI();
        return true;
    }
    
    public void EndTurn()
    {
        bool kingIsInCheck = false;
        bool hasValidMoves = false;
        

        //Update mana values
        if (!WhiteTurn)
        {
            TurnCount++;
            whiteMana = Min(TurnCount, MaxMana);
            whiteManaSpent = 0;
            Debug.Log(whiteMana);
        }
        else
        {
            blackMana = Min(TurnCount, MaxMana);
            blackManaSpent = 0;
            Debug.Log(blackMana);
        }

        UpdateManaUI();

        WhiteTurn = !WhiteTurn;
        
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
                    piece.GetComponent<PieceController>().DoubleStep = false;
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
                    piece.GetComponent<PieceController>().DoubleStep = false;
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
        BlackManaText.text = "Black Mana " + (blackMana - blackManaSpent);
    }
}
