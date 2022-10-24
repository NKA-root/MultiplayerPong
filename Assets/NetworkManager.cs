using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    static NetworkManager _singleton;
    public static NetworkManager Singleton
    {
        get => _singleton;
        set
        {
            if (Singleton != null)
            {
                Debug.LogError("Singleton of Network Manager already exist");
                Destroy(value.gameObject);
            }
            _singleton = value;
        }
    }

    public ushort CurrentTick { get; protected set; }


    private void Awake()
    {
        Singleton = this;
    }
    public enum ServerToClientId : ushort
    {
        gameStart,
        ballData,
        playerPositions,
        pingBack
    }
    public enum ClientToServerId : ushort
    {
        connectData,
        input,
        pingData
    }
    public static bool isClient => GetNetworkManager() is Client;
    public static NetworkManager GetNetworkManager()
    {
        return FindObjectOfType<NetworkManager>();
    }
    public static void SendMessages(Message message, ushort toId = 0)
    {
        if (Singleton == null) return;

        if (Singleton is Client)
        {
            Client client = Singleton as Client;

            client.client.Send(message);
        }
        else
        {
            Server server = Singleton as Server;

            server.server.Send(message, toId);
        }
    }
    public static void SendMessageToAll(Message message)
    {
        if (!(Singleton is Server)) return; if (Singleton == null) return;

        Server server = Singleton as Server;

        server.server.SendToAll(message);
    }

}
