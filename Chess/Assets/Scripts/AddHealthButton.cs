using UnityEngine;
using UnityEngine.UI;

public class AddHealthButton : MonoBehaviour
{
    public GameController gameController; // Reference to the GameController
    private bool isSelectingKnight = false; // Tracks whether we're in "select knight" mode

    void Start()
    {
        // Get the Button component and add a listener for the click event
        Button button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
    }

    void OnButtonClick()
    {
        // Toggle "select knight" mode
        isSelectingKnight = !isSelectingKnight;

        if (isSelectingKnight)
        {
            Debug.Log("Select a knight to add +1 health.");
        }
        else
        {
            Debug.Log("Knight selection canceled.");
        }
    }

    void Update()
    {
        // If we're in "select knight" mode, check for mouse clicks
        if (isSelectingKnight && Input.GetMouseButtonDown(0))
        {
            // Raycast to detect which piece was clicked
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null)
            {
                PieceController piece = hit.collider.GetComponent<PieceController>();

                // Check if the clicked piece is a knight
                if (piece != null && piece.name.Contains("Knight"))
                {
                    // Add +1 health to the knight
                    piece.numLives++;
                    Debug.Log($"{piece.name} now has {piece.numLives} lives.");

                    // Exit "select knight" mode
                    isSelectingKnight = false;
                }
                else
                {
                    Debug.Log("You must select a knight.");
                }
            }
        }
    }
}