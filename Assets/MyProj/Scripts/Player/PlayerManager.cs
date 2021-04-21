using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseScreen;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Pause();
        }
    }

    public void Pause()
    {
        pauseScreen.SetActive(!pauseScreen.activeSelf);
        
        if (pauseScreen.gameObject.activeInHierarchy)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void Quit()
    {
        GameObject.FindWithTag("ChunkManager").GetComponent<ChunkManager>().Save(true);
    }
    
}
