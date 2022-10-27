using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public static Menu Singleton { get; private set; }



    private void Awake()
    {
        Singleton = this;

        EnterMainMenu();
        ExitClientMode();
        ExitHostMode();
    }

    private void Update()
    {
        ClientUpdate();
        MainMenuUpdate(); 
        UpdateHost();
        EnterOrSpace();
        Escape();
    }
    
    

    #region Main Menu

    [SerializeField] GameObject MainMenuCanvas;
    [SerializeField] GameObject[] MainMenuArrows = new GameObject[2];
    int x { set; get; } = 0;

    public bool isInMainMenu { get; set; } = true;

    void MainMenuUpdate()
    {
        if (!isInMainMenu) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            x = -1;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            x = 1;
        }

        MainMenuArrows[0].SetActive(x == -1);
        MainMenuArrows[1].SetActive(x == 1);
    }

    void EnterMainMenu()
    {
        x = 0;
        isInMainMenu = true;
        MainMenuCanvas.SetActive(isInMainMenu);
    }

    void ExitMainMenu()
    {
        isInMainMenu = false;
        MainMenuCanvas.SetActive(isInMainMenu);
    }

    #endregion

    #region Client Mode

    [SerializeField] GameObject ClientModeCanvas;
    [SerializeField] GameObject[] ClientArrows;
    [SerializeField] TMP_InputField ipField;

    bool isIpEnabled => ipField.isFocused;

    int _yClient = -1;
    int yClient
    {
        get => _yClient;
        set
        {
            if (value * -1 >= ClientArrows.Length || value > 0) return;
            _yClient = value;
        }
    }

    bool isInClientMode { get; set; } = false;

    void ClientUpdate()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow) && ipField.isFocused)
        {
            ipField.DeactivateInputField();
            yClient--;
            return;
        }
        if (Input.GetKeyDown(KeyCode.UpArrow) && ipField.isFocused)
        {
            ipField.DeactivateInputField();
            yClient++;
            return;
        }

        if (!isInClientMode || ipField.isFocused) return;

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            yClient--;
        }
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            yClient++;
        }

        for (int i = 0; i < ClientArrows.Length; i++)
        {
            ClientArrows[i].SetActive(i == -yClient);
        }
    }

    void EnterClientMode()
    {
        yClient = -1;
        ipField.ActivateInputField();
        isInClientMode = true;
        ClientModeCanvas.SetActive(isInClientMode);
    }
    void ExitClientMode()
    {
        isInClientMode = false;
        ClientModeCanvas.SetActive(isInClientMode);

        if(GameObject.Find("Connect"))
            GameObject.Find("Connect").GetComponent<TextMeshProUGUI>().text = "Connect!";

        if (!FindObjectOfType<NetworkManager>()) return;

        NetworkManager[] list = FindObjectsOfType<NetworkManager>();

        for (int i = 0; i < list.Length; i++)
        {
            Destroy(list[i].gameObject);
        }
    }

    #endregion

    #region Host Mode

    [SerializeField] GameObject HostModeCanvas;
    [SerializeField] GameObject[] HostArrows;
    bool isInHostMode { get; set; } = false;

    int _yHost = -1;
    int yHost
    {
        get => _yHost;
        set
        {
            if (value * -1 >= HostArrows.Length || value > 0) return;
            _yHost = value;
        }
    }

    void UpdateHost()
    {
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            yHost--;
        }
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            yHost++;
        }

        for (int i = 0; i < HostArrows.Length; i++)
        {
            HostArrows[i].SetActive(i == -yHost);
        }
    }

    void EnterHostMode()
    {
        _yHost = -1;
        isInHostMode = true;
        HostModeCanvas.SetActive(isInHostMode);
    }
    void ExitHostMode()
    {
        isInHostMode = false;
        HostModeCanvas.SetActive(isInHostMode);

        if (!FindObjectOfType<NetworkManager>()) return;

        NetworkManager[] list = FindObjectsOfType<NetworkManager>();

        for (int i = 0; i < list.Length; i++)
        {
            Destroy(list[i].gameObject);
        }
    }

    #endregion

    #region Functions

    void EnterOrSpace()
    {
        if (!(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))) return;

        if (isInMainMenu)
        {
            switch (x)
            {
                case -1: ExitMainMenu(); EnterClientMode();  break;
                case 1: ExitMainMenu(); EnterHostMode(); break;
                default: break;
            }
        }
        else if (isInClientMode)
        {
            switch (yClient)
            {
                case 0: Escape(false); break;
                case -1: ipField.ActivateInputField(); break;
                case -2:
                    ConnectClient();
                    break;
            }
        }
        else if (isInHostMode)
        {
            switch (yHost)
            {
                case 0: Escape(false); break;
                case -1:
                    TryToRunHost();
                    break;
            }
        }
    }
    public void MenuToHost()
    {
        ExitMainMenu(); EnterHostMode();
    }
    public void MenuToClient()
    {
        ExitMainMenu(); EnterClientMode();
    }
    public void Escape(bool isFromKeyboard = true)
    {
        if (!Input.GetKeyDown(KeyCode.Escape) && isFromKeyboard) return;

        if (isInClientMode)
        {
            ExitClientMode();
            EnterMainMenu();
        }
        else if(isInHostMode)
        {
            ExitHostMode();
            EnterMainMenu();
        }
    }
    public void ConnectClient()
    {
        if (!FindObjectOfType<NetworkManager>().IsUnityNull()) return;

        Client client = Instantiate(new GameObject(), Vector3.zero, Quaternion.identity).AddComponent<Client>();
        client.name = "Client";
        client.Connect();
    }
    public void TryToRunHost()
    {
        if (!FindObjectOfType<NetworkManager>().IsUnityNull()) return;

        Server server = Instantiate(new GameObject(), Vector2.zero, Quaternion.identity).AddComponent<Server>();
        server.name = "Server";
        DontDestroyOnLoad(server);
        SceneManager.LoadScene(2);
    }

    #endregion
}
