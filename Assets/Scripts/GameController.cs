using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions.Must;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
    public static GameController Singleton { get; private set; }
    public bool isGameActive { get; set; } = true;

    [SerializeField] GameObject LeaveGamePrompt;

    [SerializeField] Rigidbody2D Ball;

    public Rigidbody2D GameBall => Ball;

    [SerializeField] TextMeshProUGUI WaitingText;
    [SerializeField] TextMeshProUGUI LeftScoreText;
    [SerializeField] TextMeshProUGUI RightScoreText;

    public bool isGamePlaying = false;

    Vector2 BallVelocity => Ball.velocity;
    float BallSpeeed => BaseBallSpeed * (1 + ((float)BallTouches / 10));
    float BaseBallSpeed = 8f;

    public ushort BallTouches { get; set; } = 0;

    ushort _scoreLeft;
    public ushort ScoreLeft
    {
        get => _scoreLeft;
        set
        {
            _scoreLeft = value;

            if (LeftScoreText) LeftScoreText.text = value.ToString();
        }
    }

    ushort _scoreRight;
    public ushort ScoreRight
    {
        get => _scoreRight;
        set
        {
            _scoreRight = value;

            if (RightScoreText) RightScoreText.text = value.ToString();
        }
    }


    private void Awake()
    {
        Singleton = this;

        if (!Ball) Ball = GameObject.FindWithTag("Ball").GetComponent<Rigidbody2D>();
        if (NetworkManager.isClient) Client.SendGameConnectedData();
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
    private void FixedUpdate()
    {
        if (NetworkManager.isClient)
        {
            StartCoroutine(PingedFixedUpdate());
            return;
        }

        if (isGamePlaying)
        {
            UpdateBallMovement();
            CheckForPoint();
            CheckBallVelocity();

            if (NetworkManager.isServer && NetworkManager.Singleton.CurrentTick % 5 == 0) Server.SendBallPosition(Ball.position);
        }
    }
    IEnumerator PingedFixedUpdate()
    {
        for (int i = 0; i < Client.Singleton.TickPing; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        if (isGamePlaying)
        {
            UpdateBallMovement();
            CheckForPoint();
            CheckBallVelocity();
        }

        yield return null;
    }
    public void StartNewGame()
    {
        if (WaitingText) WaitingText.gameObject.SetActive(false);
        isGamePlaying = true;
        ScoreLeft = 0;
        ScoreRight = 0;
        FindObjectOfType<NetworkPlayerControlller>().SetPlayerColorAlpha();
        NewPoint();
    }
    public void PlayerLeft()
    {
        isGamePlaying = false;
        FindObjectOfType<NetworkPlayerControlller>().SetPlayerColorAlpha(.2f);
    }

    public void NewPoint()
    {
        BallTouches = 0;
        Ball.position = Vector2.zero;
        SetBallRandomSpeed();
    }
    public void UpdateBallMovement()
    {
        if (BallVelocity == Vector2.zero) SetBallRandomSpeed();

        if (Mathf.Abs(Ball.position.y) >= 11.5f) Ball.velocity = new Vector2(Ball.velocity.x, Ball.velocity.y * -1f);

        Vector2 BallMovement = BallVelocity * BallSpeeed * Time.deltaTime;

        if (Ball.position.y > 0)
        {
            if (Ball.position.y + BallMovement.y > 11.5f)
            {
                BallMovement = new Vector2(BallMovement.x, 11.5f - Ball.position.y);
                RandomizeBallVelocity();
            }
        }
        else
        {
            if (Ball.position.y + BallMovement.y < -11.5f)
            {
                BallMovement = new Vector2(BallMovement.x, -11.5f - Ball.position.y);
                RandomizeBallVelocity();
            }
        }

        //Ball.position += BallMovement;
    }
    void SetBallRandomSpeed()
    {
        Ball.velocity = GetRandomVector() * BallSpeeed;
        if (NetworkManager.isServer) Server.SendBallData(BallTouches, Ball.velocity);
    }
    Vector2 GetRandomVector()
    {
        Vector2 value = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;

        while (Mathf.Abs(value.x) < .3f || Mathf.Abs(value.y) < .3f)
        {
            value = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        }

        return value;
    }
    void CheckForPoint()
    {
        if (Mathf.Abs(Ball.position.x) > 22)
        {
            if (Ball.position.x > 0)
            {
                ScoreLeft++;
            }
            else
            {
                ScoreRight++;
            }
            NewPoint();
        }
    }
    public void RandomizeBallVelocity()
    {
        Ball.velocity = new Vector2(BallVelocity.x * Random.Range(.7f, 1.3f), BallVelocity.y * Random.Range(.7f, 1.3f)).normalized * BallSpeeed;
        if (NetworkManager.isServer) Server.SendBallData(BallTouches, Ball.velocity);
    }

    void CheckBallVelocity()
    {
        Vector2 value = BallVelocity;

        if (!(Mathf.Abs(value.x) < .3f || Mathf.Abs(value.y) < .3f)) return;

        while ((Mathf.Abs(value.x) < .3f || Mathf.Abs(value.y) < .3f) && BallVelocity.x * value.x < 0)
        {
            value = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        }

        Ball.velocity = value.normalized * BallSpeeed;
        if (NetworkManager.isServer) Server.SendBallData(BallTouches, Ball.velocity);
    }
}