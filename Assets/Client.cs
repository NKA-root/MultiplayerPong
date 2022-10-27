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

        if (CurrentTick % 50 == 0) PingServer();
        //if (CurrentTick % 250 == 0 || lagCall % 25 == 0 || (CurrentTick % 50 == 0 && SceneManager.GetActiveScene().name.Contains("Lobby"))) PingServer();
        //Debug.Log("TickPing: " + TickPing);
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
        SceneManager.LoadScene(1); 
        PingServer();
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

    public static void SendInputs(bool inputW, bool inputS)
    {
        Message message = Message.Create(MessageSendMode.Unreliable, ClientToServerId.input);

        message.AddBool(inputW);
        message.AddBool(inputS);

        SendMessages(message);
    }

    [MessageHandler((ushort)ServerToClientId.ballData)]
    public static void SetBallData(Message message)
    {
        GameController.Singleton.BallTouches = message.GetUShort();
        GameController.Singleton.GameBall.velocity = message.GetVector2();
    }
    [MessageHandler((ushort)ServerToClientId.ballPosition)]
    public static void SetBallPosition(Message message)
    {
        GameController.Singleton.GameBall.position = message.GetVector2();
    }
    [MessageHandler((ushort)ServerToClientId.playerPositions)]
    public static void SetPlayerPosition(Message message)
    {
        if (NetworkPlayerControlller.list.TryGetValue((ushort)(message.GetBool() ? 1 : 0), out NetworkPlayerControlller player))
        {
            player.Rigidbody.position = new Vector2(player.Rigidbody.position.x, message.GetFloat());
        }
    }
    [MessageHandler((ushort)ServerToClientId.gameStart)]
    public static void SetGameStart(Message message)
    {
        GameController.Singleton.isGamePlaying = true;
    }

    public static void SendGameConnectedData() => SendMessages(Message.Create(MessageSendMode.Reliable, ClientToServerId.connectData));


    [MessageHandler((ushort)ServerToClientId.pingBack)]
    public static void CalculatePing(Message message)
    {
        ushort currentTick = message.GetUShort();

        ushort ping = (ushort)(Singleton.CurrentTick - currentTick);

        Singleton.CurrentTick = currentTick;

        Debug.Log("Ping: " + ping);

        Singleton.TickPing = ping;
    }

    public void PingServer()
    {
        Message message = Message.Create(MessageSendMode.Unreliable, ClientToServerId.pingData);

        SendMessages(message);
    }
}