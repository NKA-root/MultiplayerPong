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
    }
    [MessageHandler((ushort)ClientToServerId.input)]
    public static void ReciveInput(ushort fromPlayer, Message message)
    {
        if(NetworkPlayerControlller.list.TryGetValue(fromPlayer, out NetworkPlayerControlller player));
            player.SetInput(message.GetBool(),message.GetBool());
    }
    [MessageHandler((ushort)ClientToServerId.pingData)]
    public static void RecivePing(ushort fromPlayer, Message message)
    {
        Message backMessage = Message.Create(MessageSendMode.Unreliable, ServerToClientId.pingBack);

        backMessage.AddUShort(Singleton.CurrentTick);

        Singleton.server.Send(backMessage, fromPlayer);
    }
}
