using UnityEngine;

public class QuitOnEscape : MonoBehaviour
{
    void Update()
    {
        // Check if the Escape key was pressed down in this frame
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}