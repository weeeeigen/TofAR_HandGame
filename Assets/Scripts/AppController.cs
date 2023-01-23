using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TofAr.V0.Hand;

enum GameMode
{
    PistolBangBang = 0,
    JankenPonPon
}

enum GameStatus
{
    waitingForInit = 0,
    waitingForCountdown,
    onCountdown,
    waitingForStart,
    waitingForPose,
    waitingForNext,
    finish
}

enum Janken
{
    Rock = 0,
    Paper,
    Scissors
}

public class AppController : MonoBehaviour
{
    [Header("Game Option")]
    [SerializeField]
    private float interval = 1f;
    [SerializeField]
    private float decrease = 0.1f;
    [SerializeField]
    private float minInterval = 0.4f;

    [SerializeField]
    private HandImage jankenImage;
    [SerializeField]
    private Text introText;
    [SerializeField]
    private GameObject introPanel;
    [SerializeField]
    private Text countdown;
    [SerializeField]
    private Text scoreText;
    [SerializeField]
    private Text successText;
    [SerializeField]
    private Slider intervalSlider;
    [SerializeField]
    private ResultPanel resultPanel;
    [SerializeField]
    private Sprite[] jankenImages = new Sprite[Enum.GetNames(typeof(Janken)).Length];

    [SerializeField]
    private PistolImage[] pistolImages;

    [SerializeField]
    private GameObject sunglassImage;

    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip pistolAudio;
    [SerializeField]
    private AudioClip jankenAudio;
    [SerializeField]
    private AudioClip ponAudio;
    [SerializeField]
    private AudioClip successAudio;
    [SerializeField]
    private AudioClip failAudio;

    private GameMode gameMode;
    private GameStatus gameStatus = GameStatus.waitingForInit;
    private PoseIndex[] targetPoses = new PoseIndex[] { PoseIndex.Pistol, PoseIndex.OK };
    private bool playinJankenAudio = false;
    private Janken currentJanken;

    private PoseIndex leftPose;
    private PoseIndex rightPose;

    private int score = 0;
    private int maxScore = 0;

    private float timer = 0;
    private float pitch = 1.0f;
    
    private const string initIntro = "スマホから70cmほど離れた状態で\n右手をピストル、左手をOKにしてください。\nもしくは両手をチョキにしてください。\nサングラスは指パッチンで非表示にできます。";
    private const string pistolIntro = "弾を打つ音に合わせて\n右手と左手を入れ替えてください";
    private const string jankenIntro = "画面に表示されるじゃんけんに合わせて\n負けるように手を出してください";



    // Start is called before the first frame update
    void Start()
    {
        resultPanel.actionDelegate += ResultPanelAction;

        introText.text = initIntro;

        UpdatePistol(false);

        TofArHandManager.OnGestureEstimated += OnGestureEstimated;
    }

    private void ResultPanelAction(ResultPanel.Action action)
    {
        resultPanel.Show(false);
        
        // Reset
        pitch = 1f;
        ApplyPitch(1);
        score = 0;
        maxScore = 0;
        scoreText.text = score.ToString();
        intervalSlider.gameObject.SetActive(false);
        targetPoses[0] = PoseIndex.OK;
        targetPoses[1] = PoseIndex.Pistol;
        UpdatePistol(false);

        if (action == ResultPanel.Action.Replay)
        {
            StartCoroutine(StartCountdown(3));
        }
        else if (action == ResultPanel.Action.Close)
        {
            introText.text = initIntro;
            introPanel.gameObject.SetActive(true);
            gameStatus = GameStatus.waitingForInit;
        }
    }

    public  void OnGestureEstimated(object sender, GestureResultProperty result)
    {
        if (result.gestureIndex == GestureIndex.SnapFinger)
        {
            if (gameStatus != GameStatus.waitingForStart && gameStatus != GameStatus.waitingForPose && gameStatus != GameStatus.waitingForNext)
            {
                sunglassImage.SetActive(!sunglassImage.activeSelf);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (successText.color.a > 0)
        {
            var c = successText.color;
            c.a -= Time.deltaTime * 2;
            successText.color = c;
        }

        switch (gameStatus)
        {
            case GameStatus.waitingForInit:
                if (EvaluatePistol())
                {
                    print("Game mode: PistolBangBang");
                    gameMode = GameMode.PistolBangBang;
                    Array.Reverse(targetPoses);
                    StartCoroutine(StartIntroduction(5));
                }
                else if (leftPose == PoseIndex.Peace && rightPose == PoseIndex.Peace)
                {
                    print("Game mode: JankenPonPon");
                    gameMode = GameMode.JankenPonPon;
                    StartCoroutine(StartIntroduction(3));
                }
                break;
            case GameStatus.waitingForCountdown:
                if (endIntro)
                {
                    endIntro = false;
                    StartCoroutine(StartCountdown(3));
                }
                break;
            case GameStatus.waitingForStart:
                if (gameMode == GameMode.PistolBangBang)
                {
                    audioSource.PlayOneShot(pistolAudio);
                    gameStatus = GameStatus.waitingForPose;
                    UpdatePistol(true);
                }
                else if (!playinJankenAudio)
                {
                    playinJankenAudio = true;
                    StartCoroutine(PlayJankenSounds());
                }
                intervalSlider.gameObject.SetActive(true);
                intervalSlider.minValue = 0;
                intervalSlider.maxValue = interval;
                intervalSlider.value = interval;
                
                break;
            case GameStatus.waitingForPose:
                timer += Time.deltaTime;
                intervalSlider.value = interval - timer;
                
                if (gameMode == GameMode.PistolBangBang)
                {
                    if (timer <= interval)
                    {
                        if (EvaluatePistol())
                        {
                            UpdateView(true);
                            Array.Reverse(targetPoses);
                            UpdatePistol(false);
                            timer = 0;
                            pitch = Mathf.Min(pitch + 0.1f, 2f);
                            ApplyPitch(pitch);
                        }
                    }
                    else
                    {
                        UpdateView(false);
                        Array.Reverse(targetPoses);
                        UpdatePistol(false);
                        timer = 0;
                        pitch = Mathf.Min(pitch + 0.1f, 2f);
                        ApplyPitch(pitch);
                    }
                }
                else if (gameMode == GameMode.JankenPonPon)
                {
                    if (EvaluateJanken())
                    {
                        jankenImage.gameObject.SetActive(false);
                        UpdateView(true);
                        timer = 0;
                    }
                    else
                    {
                        if (timer > interval)
                        {
                            jankenImage.gameObject.SetActive(false);

                            UpdateView(false);
                            timer = 0;
                        }
                    }
                }
                break;
            case GameStatus.waitingForNext:
                intervalSlider.gameObject.SetActive(false);
                timer += Time.deltaTime;
                if (timer > 0.5f)
                {
                    gameStatus = GameStatus.waitingForStart;
                    timer = 0;
                    interval = Mathf.Max(interval - decrease, minInterval);
                }
                break;
            case GameStatus.finish:
                resultPanel.SetScore(score);
                resultPanel.SetCurrentPose(leftPose);
                break;
            default: break;
        }

        if (TofArHandManager.Instance.HandData?.Data == null) { 
            leftPose = PoseIndex.None;
            rightPose = PoseIndex.None;
            return; 
        }
        TofArHandManager.Instance.HandData.GetPoseIndex(out leftPose, out rightPose);
    }

    private bool endIntro = false;
    private IEnumerator StartIntroduction(int count)
    {
        gameStatus = GameStatus.waitingForCountdown;
        introText.text = gameMode == GameMode.PistolBangBang ? pistolIntro : jankenIntro;
        
        yield return new WaitForSeconds(count);
        introPanel.gameObject.SetActive(false);
        endIntro = true;
    }

    private IEnumerator StartCountdown(int count)
    {        
        gameStatus = GameStatus.onCountdown;

        countdown.gameObject.SetActive(true);
        for (int i = count; i > 0; i--)
        {
            countdown.text = i.ToString();
            yield return new WaitForSeconds(1);
        }
        countdown.gameObject.SetActive(false);
        gameStatus = GameStatus.waitingForStart;
        interval = 1f;
        ApplyPitch(1);
    }

    private IEnumerator PlayJankenSounds()
    {
        audioSource.clip = jankenAudio;
        audioSource.Play();
        yield return new WaitForSeconds(jankenAudio.length / audioSource.pitch);

        audioSource.clip = ponAudio;
        audioSource.Play();
        currentJanken = (Janken)UnityEngine.Random.Range(0, Enum.GetNames(typeof(Janken)).Length);
        jankenImage.SetSprite(jankenImages[(int)currentJanken]);
        jankenImage.gameObject.SetActive(true);
        yield return new WaitForSeconds(ponAudio.length / audioSource.pitch);
        gameStatus = GameStatus.waitingForPose;
        playinJankenAudio = false;

        audioSource.Play();
    }

    private bool EvaluatePistol()
    {
        return leftPose == targetPoses[0] && rightPose == targetPoses[1];
    }

    private void UpdatePistol(bool show)
    {
        for (int i = 0; i < pistolImages.Length; i++)
        {
            pistolImages[i].Show(show);
            pistolImages[i].currentPose = targetPoses[i];
        }
    }

    private bool EvaluateJanken()
    {
        var result = false;
        switch (currentJanken)
        {
            case Janken.Rock:
                result = leftPose == PoseIndex.Peace;
                break;
            case Janken.Paper:
                result = leftPose == PoseIndex.Fist;
                break;
            case Janken.Scissors:
                result = leftPose == PoseIndex.OpenPalm;
                break;
        }
        return result;
    }

    private void UpdateView(bool success)
    {
        score = success ? score + 1 : score;
        maxScore++;

        gameStatus = maxScore >= 10 ? GameStatus.finish : GameStatus.waitingForNext;
        if (gameStatus == GameStatus.finish)
        {
            resultPanel.Show(true);
        }
        
        // Update text
        //scoreText.color = success ? Color.red : Color.black;
        scoreText.text = score.ToString();
        successText.text = success ? "Success" : "Fail";
        successText.color = success ? Color.red : Color.black;

        // Play sound
        //audioSource.clip = success ? successAudio : failAudio;
        ApplyPitch(1);
        audioSource.PlayOneShot(success ? successAudio : failAudio);
        ApplyPitch(pitch);
    }

    private void ApplyPitch(float pitch)
    {
        audioSource.pitch = pitch;
        audioSource.outputAudioMixerGroup.audioMixer.SetFloat("Pitch", 1 / pitch);
    }
}
