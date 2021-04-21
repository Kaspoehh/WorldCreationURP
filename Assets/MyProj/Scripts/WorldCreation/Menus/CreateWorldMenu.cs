using System.IO;
using ScriptableObjectArchitecture;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreateWorldMenu : MonoBehaviour
{
    [Header("Values")]
    [SerializeField] private IntReference renderDistance = default(IntReference);
    [SerializeField] private Vector2Reference seed = default(Vector2Reference);
    [SerializeField] private StringReference saveName = default(StringReference);
    [SerializeField] private StringReference worldName = default(StringReference);

    [Header("UI")]
    [SerializeField] private TMP_InputField renderDistanceText; 
    [SerializeField] private TMP_InputField seedText;
    [SerializeField] private TMP_InputField saveNameText;
    [SerializeField] private TextMeshProUGUI warningText;
    
    public void CreateWorld()
    {
        int rendDistance = 5;
        
        int.TryParse(renderDistanceText.text, out rendDistance);
        Debug.Log(rendDistance);
        
        if (rendDistance == 0)
            rendDistance = 3;
        
        renderDistance.Value = rendDistance;

        float x = 0;
        float y = 0;
        
        float.TryParse(seedText.text, out x);
        float.TryParse(seedText.text, out y);
        
        seed.Value = new Vector2(x, y);
        
        //check if there is already an world with this name if so show warning do not proceed
        
        saveName.Value = Application.dataPath + "/Resources/" + saveNameText.text + ".json";
        
        if (File.Exists(string.Concat(saveName.Value)))
        {
            warningText.gameObject.SetActive(true);
            return;
        }

        worldName.Value = saveNameText.text;
        OpenNewWorld();
    }

    private void OpenNewWorld()
    {
        SceneManager.LoadScene("NewWorld", LoadSceneMode.Single);
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("StartMenu", LoadSceneMode.Single);
    }
}
