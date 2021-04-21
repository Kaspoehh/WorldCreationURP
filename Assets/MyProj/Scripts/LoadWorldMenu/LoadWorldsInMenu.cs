using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadWorldsInMenu : MonoBehaviour
{
    [SerializeField] private GameObject worldDataTextPrefab;
    [SerializeField] private GameObject contentField;
    
    private void Awake()
    {
        LoadWorldList();
    }

    void LoadWorldList()
    {
        if (File.Exists(string.Concat(Application.dataPath, "/Resources/", "existingWorlds", ".json")))
        {
            string saveStringWorlds = File.ReadAllText(Application.dataPath + "/Resources/" + "existingWorlds" + ".json");

            List<WorldData> existingWorldCollection = new List<WorldData>();
            
            Worlds worldsSaveCollection = JsonUtility.FromJson<Worlds>(saveStringWorlds);

            existingWorldCollection = worldsSaveCollection.WorldDatas;

            for (int i = 0; i < existingWorldCollection.Count; i++)
            {
                var textInstance = Instantiate(worldDataTextPrefab);
                textInstance.transform.SetParent(contentField.transform);

                textInstance.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = existingWorldCollection[i].WorldName;
                textInstance.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = existingWorldCollection[i].SavePath;

                textInstance.GetComponent<ButtonLoadWorld>().SavePath = existingWorldCollection[i].SavePath;
            }            
        }
    }
    
    public void BackToMainMenu()
    {
        SceneManager.LoadScene("StartMenu", LoadSceneMode.Single);
    }
}
