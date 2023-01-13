using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TofAr.V0.Hand;

public class ResultPanel : MonoBehaviour
{
    public enum Action: int
    {
        Replay, Close 
    }
    
    public float inputTime = 1;

    [SerializeField]
    private Text scoreText;
    [SerializeField]
    private GameObject actionsArea;
    [SerializeField]
    private Image paperImage;
    [SerializeField]
    private Image rockImage;

    public delegate void ResultPanelActionDelegate(Action action);
    public ResultPanelActionDelegate actionDelegate;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (actionsArea.gameObject.activeSelf)
        {
            if (currentPose == PoseIndex.OpenPalm)
            {
                if (lastPose == PoseIndex.OpenPalm)
                {
                    timer += Time.deltaTime;
                }
                else
                {
                    timer = 0;
                }

                paperImage.fillAmount = timer / inputTime;
                rockImage.fillAmount = 0;
                if (paperImage.fillAmount >= 1)
                {
                    actionDelegate?.Invoke(Action.Replay);
                }
            }
            else if (currentPose == PoseIndex.Fist)
            {
                if (lastPose == PoseIndex.Fist)
                {
                    timer += Time.deltaTime;
                }
                else
                {
                    timer = 0;
                }
                if (paperImage.fillAmount >= 1)
                {
                    actionDelegate?.Invoke(Action.Close);
                }

                rockImage.fillAmount = timer / inputTime;
                paperImage.fillAmount = 0;
                if (rockImage.fillAmount >= 1)
                {
                    actionDelegate?.Invoke(Action.Close);
                }
            }
            else
            {
                ResetTimer();
            }

            lastPose = currentPose;
        }
    }

    public void SetScore(int score)
    {
        scoreText.text = score + "回成功！！";
    }

    public void Show(bool show)
    {
        gameObject.SetActive(show);
        ResetTimer();
        actionsArea.gameObject.SetActive(false);
        if (show)
        {
            StartCoroutine(ShowActions());
        }
    }

    private IEnumerator ShowActions()
    {
        yield return new WaitForSeconds(2);
        actionsArea.gameObject.SetActive(true);
    }

    private void ResetTimer()
    {
        timer = 0;
        paperImage.fillAmount = 0;
        rockImage.fillAmount = 0;
    }

    private PoseIndex lastPose;
    private PoseIndex currentPose;
    private float timer = 0;
    public void SetCurrentPose(PoseIndex pose)
    {
        currentPose = pose;
    }
}
