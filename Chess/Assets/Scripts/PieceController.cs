﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class PieceController : MonoBehaviour
{
    public GameController GameController;
    public GameObject WhitePieces;
    public GameObject BlackPieces;
    public Sprite QueenSprite;

    public float MoveSpeed = 20;
    public int numLives = 1;


    public float HighestRankY = 3.5f;
    public float LowestRankY = -3.5f;

    [HideInInspector]
    public bool DoubleStep = false;
    [HideInInspector]
    public bool DoubleMoveEnabled = false; // New field for double move enhancement
    public bool CanMoveDiagonally = false; // For StackPawn
    public bool MovingY = false;
    [HideInInspector]
    public bool MovingX = false;

    private Vector3 oldPosition;
    private Vector3 newPositionY;
    private Vector3 newPositionX;

    private bool moved = false;
    
    // Use this for initialization
    void Start()
    {
        numLives = 1;
        if (GameController == null) GameController = FindFirstObjectByType<GameController>();
        if (this.name.Contains("Knight")) MoveSpeed *= 2;
    }

    // Update is called once per frame
    void Update()
    {
        if (MovingY || MovingX)
        {
            if (Mathf.Abs(oldPosition.x - newPositionX.x) == Mathf.Abs(oldPosition.y - newPositionX.y))
            {
                MoveDiagonally();
            }
            else
            {
                MoveSideBySide();
            }
        }
    }

    public void LoseLife()
    {
        numLives--;

        if (numLives <= 0)
        {
            // Capture the piece
            if (this.tag == "White")
            {
                GameController.WhitePieces.transform.SetParent(null);
            }
            else if (this.tag == "Black")
            {
                GameController.BlackPieces.transform.SetParent(null);
            }
            Destroy(this.gameObject);
        }
        else
        {
            Debug.Log($"{this.name} lost a life! Remaining lives: {numLives}");
        }
    }


    void OnMouseDown()
    {
        if (GameController.SelectedPiece != null && GameController.SelectedPiece.GetComponent<PieceController>().IsMoving() == true)
        {
            // Prevent clicks during movement
            return;
        }

        if (GameController.SelectedPiece == this.gameObject)
        {
            GameController.DeselectPiece();
        }
        else
        {
            if (GameController.SelectedPiece == null)
            {
                GameController.SelectPiece(this.gameObject);
            }
            else
            {
                if (this.tag == GameController.SelectedPiece.tag)
                {
                    GameController.SelectPiece(this.gameObject);
                }
                else if ((this.tag == "White" && GameController.SelectedPiece.tag == "Black") || (this.tag == "Black" && GameController.SelectedPiece.tag == "White"))
                {
                    GameController.SelectedPiece.GetComponent<PieceController>().MovePiece(this.transform.position);
                }
            }
        }
    }

    public bool MovePiece(Vector3 newPosition, bool castling = false)
    {
        GameObject encounteredEnemy = null;

        newPosition.z = this.transform.position.z;
        this.oldPosition = this.transform.position;
        Debug.Log($"Attempting to move {this.name} to {newPosition}. CanMoveDiagonally={this.CanMoveDiagonally}");
        if (castling || ValidateMovement(oldPosition, newPosition, out encounteredEnemy))
        {
            if (encounteredEnemy != null)
            {
                PieceController enemyPiece = encounteredEnemy.GetComponent<PieceController>();

                // If the enemy piece has more than 1 life, reduce its life and stop the move
                if (enemyPiece.numLives > 1)
                {
                    enemyPiece.LoseLife();
                    // Switch turns after the attack
                    GameController.DeselectPiece();
                    //GameController.EndTurn();
                    return false; // Stop the move
                }
                else
                {
                    // If the enemy piece has 1 life, capture it
                    enemyPiece.LoseLife();
                }
            }

            if (!GameController.SpendMana(-1)) {//on a -1 it will use half the mana cost)
                return false; //stop the move if you can't afford
            }
            // Double-step

            if (this.name.Contains("Pawn") && Mathf.Abs(oldPosition.y - newPosition.y) == 2)
            {
                this.DoubleStep = true;
            }


            // Promotion
            else if (this.name.Contains("Pawn") && (newPosition.y == HighestRankY || newPosition.y == LowestRankY))
            {
                this.Promote();
            }


            // Castling
            else if (this.name.Contains("King") && Mathf.Abs(oldPosition.x - newPosition.x) == 2)
            {
                if (oldPosition.x - newPosition.x == 2) // queenside castling
                {
                    GameObject rook = GetPieceOnPosition(oldPosition.x - 4, oldPosition.y, this.tag);
                    Vector3 newRookPosition = oldPosition;
                    newRookPosition.x -= 1;
                    rook.GetComponent<PieceController>().MovePiece(newRookPosition, true);
                }
                else if (oldPosition.x - newPosition.x == -2) // kingside castling
                {
                    GameObject rook = GetPieceOnPosition(oldPosition.x + 3, oldPosition.y, this.tag);
                    Vector3 newRookPosition = oldPosition;
                    newRookPosition.x += 1;
                    rook.GetComponent<PieceController>().MovePiece(newRookPosition, true);
                }
            }

            

            this.moved = true;
            this.newPositionY = newPosition;
            this.newPositionY.x = this.transform.position.x;
            this.newPositionX = newPosition;
            MovingY = true; // Start movement

            DeathTileCard deathTileCard = FindFirstObjectByType<DeathTileCard>();
            if (deathTileCard != null)
            {
                deathTileCard.OnPieceMoved(this.gameObject);
            }
            return true;
        }
        else
        {
            GameController.GetComponent<AudioSource>().Play();
            return false;
        }
    }

    public bool ValidateMovement(Vector3 oldPosition, Vector3 newPosition, out GameObject encounteredEnemy, bool excludeCheck = false)
    {
        bool isValid = false;
        encounteredEnemy = GetPieceOnPosition(newPosition.x, newPosition.y);

        // Declare otherPiece here so it's accessible in all blocks
        GameObject otherPiece = null;

        if ((oldPosition.x == newPosition.x && oldPosition.y == newPosition.y) || encounteredEnemy != null && encounteredEnemy.tag == this.tag)
        {
            Debug.Log("Invalid move: Same position or friendly piece encountered.");
            return false;
        }

        if (this.name.Contains("King"))
        {
            // If the path is 1 square away in any direction
            if (Mathf.Abs(oldPosition.x - newPosition.x) <= 1 && Mathf.Abs(oldPosition.y - newPosition.y) <= 1)
            {
                if (excludeCheck == true || (excludeCheck == false && IsInCheck(newPosition) == false))
                {
                    isValid = true;
                }
            }
            // Check for castling
            else if (Mathf.Abs(oldPosition.x - newPosition.x) == 2 && oldPosition.y == newPosition.y && this.moved == false)
            {
                if (oldPosition.x - newPosition.x == 2) // queenside castling
                {
                    GameObject rook = GetPieceOnPosition(oldPosition.x - 4, oldPosition.y, this.tag);
                    if (rook.name.Contains("Rook") && rook.GetComponent<PieceController>().moved == false &&
                        CountPiecesBetweenPoints(oldPosition, rook.transform.position, Direction.Horizontal) == 0)
                    {
                        if (excludeCheck == true ||
                            (excludeCheck == false &&
                             IsInCheck(new Vector3(oldPosition.x - 0, oldPosition.y)) == false &&
                             IsInCheck(new Vector3(oldPosition.x - 1, oldPosition.y)) == false &&
                             IsInCheck(new Vector3(oldPosition.x - 2, oldPosition.y)) == false))
                        {
                            isValid = true;
                        }
                    }
                }
                else if (oldPosition.x - newPosition.x == -2) // kingside castling
                {
                    GameObject rook = GetPieceOnPosition(oldPosition.x + 3, oldPosition.y, this.tag);
                    if (rook.name.Contains("Rook") && rook.GetComponent<PieceController>().moved == false &&
                        CountPiecesBetweenPoints(oldPosition, rook.transform.position, Direction.Horizontal) == 0)
                    {
                        if (excludeCheck == true ||
                            (excludeCheck == false &&
                             IsInCheck(new Vector3(oldPosition.x + 0, oldPosition.y)) == false &&
                             IsInCheck(new Vector3(oldPosition.x + 1, oldPosition.y)) == false &&
                             IsInCheck(new Vector3(oldPosition.x + 2, oldPosition.y)) == false))
                        {
                            isValid = true;
                        }
                    }
                }
            }
        }

        if (this.name.Contains("Rook") || this.name.Contains("Queen"))
        {
            // If the path is a straight horizontal or vertical line
            if ((oldPosition.x == newPosition.x && CountPiecesBetweenPoints(oldPosition, newPosition, Direction.Vertical) == 0) ||
                (oldPosition.y == newPosition.y && CountPiecesBetweenPoints(oldPosition, newPosition, Direction.Horizontal) == 0))
            {
                if (excludeCheck == true || (excludeCheck == false && IsInCheck(newPosition) == false))
                {
                    isValid = true;
                }
            }
        }

        if (this.name.Contains("Bishop") || this.name.Contains("Queen"))
        {
            // If the path is a straight diagonal line
            if (Mathf.Abs(oldPosition.x - newPosition.x) == Mathf.Abs(oldPosition.y - newPosition.y) &&
                CountPiecesBetweenPoints(oldPosition, newPosition, Direction.Diagonal) == 0)
            {
                if (excludeCheck == true || (excludeCheck == false && IsInCheck(newPosition) == false))
                {
                    isValid = true;
                }
            }
        }

        if (this.name.Contains("Knight"))
        {
            // If the path is an 'L' shape
            if ((Mathf.Abs(oldPosition.x - newPosition.x) == 1 && Mathf.Abs(oldPosition.y - newPosition.y) == 2) ^
                (Mathf.Abs(oldPosition.x - newPosition.x) == 2 && Mathf.Abs(oldPosition.y - newPosition.y) == 1))
            {
                if (excludeCheck == true || (excludeCheck == false && IsInCheck(newPosition) == false))
                {
                    isValid = true;
                }
            }
        }

       if (this.name.Contains("Pawn"))
    {
        //Debug.Log($"Validating movement for {this.name}. DoubleMoveEnabled={this.DoubleMoveEnabled}, moved={this.moved}, CanMoveDiagonally={this.CanMoveDiagonally}");

        // Special diagonal movement for merged pawns (only allowed if CanMoveDiagonally is true)
        if (this.CanMoveDiagonally && (oldPosition.x == newPosition.x - 1 || oldPosition.x == newPosition.x + 1))
        {
            Debug.Log($"Attempting diagonal move for {this.name}. CanMoveDiagonally={this.CanMoveDiagonally}");
            otherPiece = GetPieceOnPosition(newPosition.x, newPosition.y);

            // Check if an enemy piece is encountered
            if (otherPiece != null && otherPiece.tag != this.tag)
            {
                Debug.Log("Valid move: Diagonal capture (stacked pawn).");
                if (excludeCheck == true || (excludeCheck == false && IsInCheck(newPosition) == false))
                {
                    isValid = true;
                }
            }
            else if (otherPiece == null)
            {
                Debug.Log("Valid move: Diagonal movement (stacked pawn).");
                if (excludeCheck == true || (excludeCheck == false && IsInCheck(newPosition) == false))
                {
                    isValid = true;
                }
            }
        }
        // Normal pawn movement (1 square forward)
        else if ((this.tag == "White" && oldPosition.y + 1 == newPosition.y) || (this.tag == "Black" && oldPosition.y - 1 == newPosition.y))
        {
            otherPiece = GetPieceOnPosition(newPosition.x, newPosition.y);

            // If moving forward
            if (oldPosition.x == newPosition.x && otherPiece == null)
            {
                Debug.Log("Valid move: Forward movement.");
                if (excludeCheck == true || (excludeCheck == false && IsInCheck(newPosition) == false))
                {
                    isValid = true;
                }
            }
            // If moving diagonally (normal pawn rules for capturing)
            else if (oldPosition.x == newPosition.x - 1 || oldPosition.x == newPosition.x + 1)
            {
                Debug.Log("Attempting diagonal move (normal pawn rules).");
                // Check if en passant is available
                if (otherPiece == null)
                {
                    otherPiece = GetPieceOnPosition(newPosition.x, oldPosition.y);
                    if (otherPiece != null && otherPiece.GetComponent<PieceController>().DoubleStep == false)
                    {
                        otherPiece = null;
                    }
                }
                // If an enemy piece is encountered
                if (otherPiece != null && otherPiece.tag != this.tag)
                {
                    Debug.Log("Valid move: Diagonal capture.");
                    if (excludeCheck == true || (excludeCheck == false && IsInCheck(newPosition) == false))
                    {
                        isValid = true;
                    }
                }
            }

            encounteredEnemy = otherPiece;
        }
        // Double-step (initial double move for pawns)
        else if ((this.tag == "White" && oldPosition.x == newPosition.x && oldPosition.y + 2 == newPosition.y) ||
                 (this.tag == "Black" && oldPosition.x == newPosition.x && oldPosition.y - 2 == newPosition.y))
        {
            Debug.Log($"Checking double move for {this.name}. DoubleMoveEnabled={this.DoubleMoveEnabled}, moved={this.moved}");

            // Allow double move if DoubleMoveEnabled is true OR if it's the pawn's first move
            if ((this.DoubleMoveEnabled || this.moved == false) && GetPieceOnPosition(newPosition.x, newPosition.y) == null)
            {
                Debug.Log($"{this.name} can move two squares!");
                if (excludeCheck == true || (excludeCheck == false && IsInCheck(newPosition) == false))
                {
                    isValid = true;
                }
            }
            else
            {
                Debug.Log($"{this.name} double move failed! Conditions not met.");
            }
        }
    }

    return isValid;
}
    /// <summary>
    /// 
    /// </summary>
    /// <param name="position"></param>
    /// <param name="color">"White" or "Black" for specific color, null for any color</param>
    /// <returns>Returns the piece on a given position on the board, null if the square is empty</returns>
    GameObject GetPieceOnPosition(float positionX, float positionY, string color = null)
    {
        if (color == null || color.ToLower() == "white")
        {
            foreach (Transform piece in WhitePieces.transform)
            {
                if (piece.position.x == positionX && piece.position.y == positionY)
                {
                    return piece.gameObject;
                }
            }
        }
        if (color == null || color.ToLower() == "black")
        {
            foreach (Transform piece in BlackPieces.transform)
            {
                if (piece.position.x == positionX && piece.position.y == positionY)
                {
                    return piece.gameObject;
                }
            }
        }

        return null;
    }

    int CountPiecesBetweenPoints(Vector3 pointA, Vector3 pointB, Direction direction)
    {
        int count = 0;

        foreach (Transform piece in WhitePieces.transform)
        {
            if ((direction == Direction.Horizontal && piece.position.x > Mathf.Min(pointA.x, pointB.x) && piece.position.x < Mathf.Max(pointA.x, pointB.x) && piece.position.y == pointA.y) ||
                (direction == Direction.Vertical && piece.position.y > Mathf.Min(pointA.y, pointB.y) && piece.position.y < Mathf.Max(pointA.y, pointB.y) && piece.position.x == pointA.x))
            {
                count++;
            }
            else if (direction == Direction.Diagonal && piece.position.x > Mathf.Min(pointA.x, pointB.x) && piece.position.x < Mathf.Max(pointA.x, pointB.x) &&
                     ((pointA.y - pointA.x == pointB.y - pointB.x && piece.position.y - piece.position.x == pointA.y - pointA.x) ||
                      (pointA.y + pointA.x == pointB.y + pointB.x && piece.position.y + piece.position.x == pointA.y + pointA.x)))
            {
                count++;
            }
        }
        foreach (Transform piece in BlackPieces.transform)
        {
            if ((direction == Direction.Horizontal && piece.position.x > Mathf.Min(pointA.x, pointB.x) && piece.position.x < Mathf.Max(pointA.x, pointB.x) && piece.position.y == pointA.y) ||
                (direction == Direction.Vertical && piece.position.y > Mathf.Min(pointA.y, pointB.y) && piece.position.y < Mathf.Max(pointA.y, pointB.y) && piece.position.x == pointA.x))
            {
                count++;
            }
            else if (direction == Direction.Diagonal && piece.position.x > Mathf.Min(pointA.x, pointB.x) && piece.position.x < Mathf.Max(pointA.x, pointB.x) &&
                     ((pointA.y - pointA.x == pointB.y - pointB.x && piece.position.y - piece.position.x == pointA.y - pointA.x) ||
                      (pointA.y + pointA.x == pointB.y + pointB.x && piece.position.y + piece.position.x == pointA.y + pointA.x)))
            {
                count++;
            }
        }

        return count;
    }

    public bool IsInCheck(Vector3 potentialPosition)
    {
        bool isInCheck = false;

        // Temporarily move the piece to the potential position
        Vector3 currentPosition = this.transform.position;
        this.transform.position = potentialPosition;

        // Check if the king is in check
        if (this.tag == "Black")
        {
            Vector3 kingPosition = BlackPieces.transform.Find("Black King").position;
            foreach (Transform piece in WhitePieces.transform)
            {
                // Skip the piece being moved
                if (piece.position == potentialPosition)
                    continue;

                GameObject encounteredEnemy;
                if (piece.GetComponent<PieceController>().ValidateMovement(piece.position, kingPosition, out encounteredEnemy, true))
                {
                    Debug.Log($"Black King is in check by: {piece.name}");
                    isInCheck = true;
                    break;
                }
            }
        }
        else if (this.tag == "White")
        {
            Vector3 kingPosition = WhitePieces.transform.Find("White King").position;
            foreach (Transform piece in BlackPieces.transform)
            {
                // Skip the piece being moved
                if (piece.position == potentialPosition)
                    continue;

                GameObject encounteredEnemy;
                if (piece.GetComponent<PieceController>().ValidateMovement(piece.position, kingPosition, out encounteredEnemy, true))
                {
                    Debug.Log($"White King is in check by: {piece.name}");
                    isInCheck = true;
                    break;
                }
            }
        }

        // Move the piece back to its original position
        this.transform.position = currentPosition;
        return isInCheck;
    }
    void MoveSideBySide()
    {
        if (MovingY == true)
        {
            this.transform.SetPositionAndRotation(Vector3.Lerp(this.transform.position, newPositionY, Time.deltaTime * MoveSpeed), this.transform.rotation);
            if (this.transform.position == newPositionY)
            {
                MovingY = false;
                MovingX = true;
            }
        }
        if (MovingX == true)
        {
            this.transform.SetPositionAndRotation(Vector3.Lerp(this.transform.position, newPositionX, Time.deltaTime * MoveSpeed), this.transform.rotation);
            if (this.transform.position == newPositionX)
            {
                this.transform.SetPositionAndRotation(newPositionX, this.transform.rotation);
                MovingX = false;
                if (GameController.SelectedPiece != null)
                {
                    GameController.DeselectPiece();
                    GameController.movesRemaining -=1;
                    //GameController.EndTurn();
                }
            }
        }
    }

    void MoveDiagonally()
    {
        if (MovingY == true)
        {
            this.transform.SetPositionAndRotation(Vector3.Lerp(this.transform.position, newPositionX, Time.deltaTime * MoveSpeed), this.transform.rotation);
            if (this.transform.position == newPositionX)
            {
                this.transform.SetPositionAndRotation(newPositionX, this.transform.rotation);
                MovingY = false;
                MovingX = false;
                if (GameController.SelectedPiece != null)
                {
                    GameController.DeselectPiece();
                    //MAYEBE END TURN TBD
                    GameController.movesRemaining -= 1;
                    //GameController.EndTurn();
                }
            }
        }
    }

    void Promote()
    {
        this.name = this.name.Replace("Pawn", "Queen");
        this.GetComponent<SpriteRenderer>().sprite = QueenSprite;
    }

    public bool IsMoving()
    {
        return MovingX || MovingY;
    }

    enum Direction
    {
        Horizontal,
        Vertical,
        Diagonal
    }
}
