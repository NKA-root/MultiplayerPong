using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPlayerControlller : MonoBehaviour
{
    public static Dictionary<ushort, NetworkPlayerControlller> list { get; set; } = new();

    [SerializeField] bool isLocal;

    public Rigidbody2D Rigidbody;

    public ushort playerId;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 100;

        Rigidbody = this.GetComponent<Rigidbody2D>();
        list = new();
    }
    private void Start()
    {
        list.Add(playerId, this);
    }

    bool[] Inputs = new bool[2];
    public void SetInput(bool inputW, bool inputS)
    {
        Inputs[0] = inputW;
        Inputs[1] = inputS;
    }
    private void FixedUpdate()
    {
        if (NetworkManager.isServer) MovePlayer(CalculateMovement(Inputs));

        if (NetworkManager.isClient) Client.SendInputs(Inputs[0], Inputs[1]);

        Inputs[0] = false; Inputs[1] = false;
    }
    float CalculateMovement(bool[] Inputs)
    {
        int moveY = 0;
        float playerSpeed = 20f;

        if (Inputs[0]) moveY += 1;
        else if (Inputs[1]) moveY -= 1;

        float MovementValue = 0f;

        MovementValue = (float)moveY * Time.deltaTime * playerSpeed;

        if (Rigidbody.position.y > 0)
        {
            if (Rigidbody.position.y + MovementValue > 10f)
            {
                MovementValue = 10f - Rigidbody.position.y;
            }
        }
        else
        {
            if (Rigidbody.position.y + MovementValue < -10f)
            {
                MovementValue = -10f + Rigidbody.position.y;
            }
        }

        return MovementValue;
    }
    public void MovePlayer(float positionY)
    {
        if (isLocal)
        {

        }

        Rigidbody.position = new Vector2(Rigidbody.position.x, Rigidbody.position.y + positionY);

        if (NetworkManager.isServer) Server.SendPlayerPosition(Rigidbody.position.x > 0, Rigidbody.position.y);
    }
    public void SetPlayerColorAlpha(float alpha = 1f)
    {
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, alpha);
    }
}