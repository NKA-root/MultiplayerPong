using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PromptManager : MonoBehaviour
{
    [SerializeField] GameObject[] Arrows = new GameObject[2];
    [SerializeField] PromptType type;

    public PromptType promptType => type;

    bool isSelectedLeft = false;

    public enum PromptType : ushort
    {
        CloseServer = 0,
        LeaveGame
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameController.Singleton.isGameActive = true;
            this.gameObject.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            isSelectedLeft = true;
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            isSelectedLeft = false;
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            switch (type)
            {
                case PromptType.CloseServer: case PromptType.LeaveGame: 
                    if (isSelectedLeft)
                        LeaveScene();
                    else
                        CancelPrompt();
                    break;
            }
        }


        Arrows[0].SetActive(isSelectedLeft);
        Arrows[1].SetActive(!isSelectedLeft);
    }  
    public void LeaveScene()
    {
        if(GameObject.FindObjectOfType<NetworkManager>())
            Destroy(GameObject.FindObjectOfType<NetworkManager>().gameObject);

        SceneManager.LoadScene(0);
    }
    public void CancelPrompt()
    {
        GameController.Singleton.isGameActive = true;
        this.gameObject.SetActive(false);
    }
}
