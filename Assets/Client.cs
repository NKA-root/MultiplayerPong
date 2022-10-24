using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Riptide;
using Riptide.Transports;
using Riptide.Utils;
using System;
using UnityEngine.SceneManagement;
using TMPro;

public class Client : NetworkManager
{
    new public static Client Singleton { get; private set; }

    public Riptide.Client client { get; private set; }

    ushort port => 7777;
    string ip => PlayerPrefs.GetString("IP");
    public ushort CurrentTick { get; private set; }
    public ushort TickDivergenceTolerance { get; private set; } = 1;
    public bool isTryingToConnect { get; private set; } = false;
    public ushort TickPing { get; private set; } = 0;
    public float Ping { get; private set; } = 0;
    public ushort lagCall { get; set; } = 1;

    private void Awake()
    {
        Singleton = this;
        DontDestroyOnLoad(this);
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        client = new Riptide.Client();
        CurrentTick = 0;

        client.Connected += DidConnect;
        client.ConnectionFailed += FailedToConnect;
        client.Disconnected += DidDisconnect;
    }
    private void FixedUpdate()
    {
        client.Update();

        CurrentTick++;
        //if (CurrentTick % 250 == 0 || lagCall % 25 == 0 || (CurrentTick % 50 == 0 && SceneManager.GetActiveScene().name.Contains("Lobby"))) PingServer();
        Debug.Log("TickPing: " + TickPing);
    }
    public void Connect()
    {
        Debug.Log("Trying to connect at: " + $"{ip}:{port}"); isTryingToConnect = true;

        if (client == null) client = new Riptide.Client();

        client.Connect($"{ip}:{port}");

        GameObject.Find("Connect").GetComponent<TextMeshProUGUI>().text = "Connecting...";
    }
    private void OnApplicationQuit()
    {
        client.Disconnect();
    }
    private void OnDestroy()
    {
        client.Disconnect();
    }
    void DidConnect(object sender, EventArgs e)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
    void FailedToConnect(object sender, EventArgs e)
    {
        Debug.Log("Failed to connect!");
        GameObject.Find("Connect").GetComponent<TextMeshProUGUI>().text = "Failed to connect!";

        StartCoroutine(SetConnectTextBackAndDestroy());
    }
    IEnumerator SetConnectTextBackAndDestroy()
    {
        yield return new WaitForSecondsRealtime(3f);

        GameObject.Find("Connect").GetComponent<TextMeshProUGUI>().text = "Connect";
        Destroy(this.gameObject);
    }
    void DidDisconnect(object sender, EventArgs e)
    {
        SceneManager.LoadScene(0);
        Destroy(this.gameObject);
    }
}
