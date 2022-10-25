using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Riptide;
using Riptide.Transports;
using Riptide.Utils;
using Unity.VisualScripting;

public class Server : NetworkManager
{
    new public static Server Singleton { get; private set; }
    public Riptide.Server server { get; private set; }

    public ushort port => 7777;
    public ushort maxClientsConnected => 1;
    public string ip { get; set; }

    public bool isGameInLobby { get; private set; } = true;

    Rigidbody2D Ball => GameObject.FindGameObjectWithTag("Ball").GetComponent<Rigidbody2D>();
    Rigidbody2D ServerPlayer => GameObject.Find("Player").GetComponent<Rigidbody2D>();
    Rigidbody2D ExternalPlayer => GameObject.Find("Opponent").GetComponent<Rigidbody2D>();


    private void Awake()
    {
        Singleton = this;

        Application.runInBackground = true;

        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        server = new();
        server.Start(port,maxClientsConnected);
        CurrentTick = 0;

        server.ClientDisconnected += PlayerLeft;
    }
    void PlayerLeft(object sender, ServerDisconnectedEventArgs e)
    {
        GameController.Singleton.PlayerLeft();
    }  
    private void FixedUpdate()
    {
        server.Update();

        CurrentTick++;

        //if (CurrentTick % 250 == 0) SendTickSyncMessage();
    }
    private void OnApplicationQuit()
    {
        server.Stop();
    }

    [MessageHandler((ushort)ClientToServerId.connectData)]
    public static void ReciveConnectData(ushort fromPlayer, Message message)
    {
        GameController.Singleton.StartNewGame();

        SendStartGameInfo();
    }
    [MessageHandler((ushort)ClientToServerId.input)]
    public static void ReciveInput(ushort fromPlayer, Message message)
    {
        if (NetworkPlayerControlller.list.TryGetValue(fromPlayer, out NetworkPlayerControlller player))
        {
            Debug.Log("Recived Input");

            player.SetInput(message.GetBool(),message.GetBool());
        }
    }
    [MessageHandler((ushort)ClientToServerId.pingData)]
    public static void RecivePing(ushort fromPlayer, Message message)
    {
        Message backMessage = Message.Create(MessageSendMode.Unreliable, ServerToClientId.pingBack);

        backMessage.AddUShort(Singleton.CurrentTick);

        Singleton.server.Send(backMessage, fromPlayer);
    }

    public static void SendBallPosition(Vector2 position)
    {
        Message message = Message.Create(MessageSendMode.Unreliable, ServerToClientId.ballPosition);

        message.AddVector2(position);

        Singleton.server.SendToAll(message);
    }
    public static void SendBallData(ushort touches, Vector2 velocity)
    {
        Message message = Message.Create(MessageSendMode.Unreliable, ServerToClientId.ballData);

        message.AddUShort(touches);
        message.AddVector2(velocity);

        Singleton.server.SendToAll(message);
    }
    public static void SendPlayerPosition(bool isPlayerOnRightSide, float posY)
    {
        Message message = Message.Create(MessageSendMode.Unreliable, ServerToClientId.playerPositions);

        message.AddBool(isPlayerOnRightSide);
        message.AddFloat(posY);

        Singleton.server.SendToAll(message);
    }
    public static void SendStartGameInfo() => Singleton.server.SendToAll(Message.Create(MessageSendMode.Reliable, ServerToClientId.gameStart));
}
