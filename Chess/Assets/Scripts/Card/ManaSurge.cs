using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaSurge : MonoBehaviour
{
    public GameController gameController; // Reference to the GameController
    private bool isActive = false; // Tracks whether the listener is active


    public void Activate(){
        isActive = true;
        gameController.SpendMana(-8);
    }

    public void Deactivate(){
        isActive = false;
    }

    // Toggle the active state
    public void ToggleActive()
    {
        if (isActive)
        {
            Deactivate();
        }
        else
        {
            Activate();
        }
    }

    void Update(){
    }
}