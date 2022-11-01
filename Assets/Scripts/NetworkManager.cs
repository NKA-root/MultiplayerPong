using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Singleton => GetNetworkManager();
    public ushort CurrentTick { get; protected set; }

    public enum ServerToClientId : ushort
    {
        gameStart,
        ballData,
        ballPosition,
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
    public static bool isServer => GetNetworkManager() is Server;
    public static NetworkManager GetNetworkManager()
    {
        return FindObjectOfType<NetworkManager>();
    }
    public static void SendMessages(Message message, ushort toId = 0)
    {
        Debug.Log("Trying to Send...");

        Debug.Log("Sending");

        if (isClient)
        {
            Client client = Singleton as Client;

            client.client.Send(message);
        }
        else if (isServer)
        {
            Server server = Singleton as Server;

            Debug.Log("Sending as Server");

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