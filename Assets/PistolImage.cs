using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TofAr.V0.Hand;

public class PistolImage : MonoBehaviour
{
    [SerializeField]
    private Image pistol;
    [SerializeField]
    private Image ok;
    [SerializeField]
    private bool mirror = false;

    public PoseIndex currentPose
    {
        set
        {
            pistol.gameObject.SetActive(value == PoseIndex.Pistol);
            ok.gameObject.SetActive(value == PoseIndex.OK);
        }
    }

    private void Start() 
    {
        var y = mirror ? 180 : 0;
        pistol.rectTransform.rotation = Quaternion.Euler(0, y, 0);
        ok.rectTransform.rotation = Quaternion.Euler(0, y, 0);   
    }


    public void Show(bool show)
    {
        gameObject.SetActive(show);
    }
}
