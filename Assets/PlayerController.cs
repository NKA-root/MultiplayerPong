using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Riptide;
using static NetworkManager;
using System.Linq;

public class PlayerController : MonoBehaviour
{
    bool[] Inputs = new bool[2];
    bool[] LastInputs = new bool[2];
    ushort Tick { get; set; } = 0;

    float playerSpeed = 20f;

    Rigidbody2D rigidbody;
    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            Inputs[0] = true;
        }
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            Inputs[1] = true;
        }
    }
    private void FixedUpdate()
    {
        Tick++;

        if (Inputs[0] && Inputs[1]) Inputs.ToList().ForEach(x => x = false);

        short moveY = 0;

        if (Inputs[0]) moveY += 1;
        else if (Inputs[1]) moveY -= 1;

        rigidbody.position += new Vector2(0, (float)moveY * Time.deltaTime * playerSpeed);

        if (Mathf.Abs(rigidbody.position.y) > 10f)
        {
            rigidbody.position = new Vector2(rigidbody.position.x, rigidbody.position.y > 0 ? 10 : -10);
        }

        if ((Inputs == LastInputs && Tick % 5 == 0) || Inputs != LastInputs)
        {
            SendInputs(Inputs);
        }

        LastInputs = Inputs;

        for (int i = 0; i < Inputs.Length; i++)
        {
            Inputs[i] = false;
        }

        if (NetworkManager.isServer) Server.SendPlayerPosition(rigidbody.position.x > 0, rigidbody.position.y);
    }
    void SendInputs(bool[] inputs)
    {
        if (!NetworkManager.isClient) return;

        Message message = Message.Create(MessageSendMode.Unreliable, ClientToServerId.input);

        message.AddBool(inputs[0]);
        message.AddBool(inputs[1]);

        SendMessages(message);
    }
}
