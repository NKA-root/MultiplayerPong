using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Singleton { get; private set; }
    public bool isGameActive { get; set; } = true;

    [SerializeField] GameObject LeaveGamePrompt;


    private void Awake()
    {
        Singleton = this;
    }
    private void Update()
    {
        if (!isGameActive) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isGameActive = false;
            LeaveGamePrompt.SetActive(true);
        }
    }
}
