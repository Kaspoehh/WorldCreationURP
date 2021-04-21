using System.Collections;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonLoadWorld : MonoBehaviour
{
    public string SavePath;
    public StringReference SavePathReference = (default(StringReference));
    
    public void OpenWorld()
    {
        SavePathReference.Value = SavePath;
        SceneManager.LoadScene("LoadWorld", LoadSceneMode.Single);
    }
}
