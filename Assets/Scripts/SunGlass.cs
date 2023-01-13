using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TofAr.V0.Face;

public class SunGlass : MonoBehaviour
{
    [SerializeField]
    private Camera tofCamera;
    [SerializeField]
    private RectTransform canvasRect;
    [SerializeField]
    private RectTransform colorImageRect;
    private FaceResult faceResult;
    private RectTransform rectTransform;

    // Start is called before the first frame update
    void Start()
    {
        TofArFaceManager.OnFrameArrived += FaceDetected;
        rectTransform = GetComponent<RectTransform>();
    }

    private void FaceDetected(object sender)
    {
        TofArFaceManager mgr = (TofArFaceManager)sender;

        foreach (FaceResult fr in mgr.FaceData.Data.results)
        {
            faceResult = fr;
            break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (faceResult == null)
        {
            return;
        }

        var facePose = faceResult.pose;
        Vector3 pos = (Vector3)facePose.position;
        Quaternion rot = (Quaternion)facePose.rotation;

        var vs = faceResult.vertices;

        if (gameObject.activeSelf == true)
        {
            // right eye
            Vector3 rightEye = rot * (Vector3)vs[1107] + pos;
            Vector3 leftEye = rot * (Vector3)vs[1163] + pos;
            var left = CalcCanvasPointFrom(rightEye);
            var right = CalcCanvasPointFrom(leftEye);

            rectTransform.anchoredPosition = (left + right) / 2;
            var distance = Vector2.Distance(left, right);
            rectTransform.localScale = distance / 140 * Vector3.one;

            float rad = Mathf.Atan2(right.y - left.y, right.x - left.x);
            rectTransform.rotation = Quaternion.Euler(0, 0, rad * Mathf.Rad2Deg);
        }
    }


    private Vector2 CalcCanvasPointFrom(Vector3 handPoint)
    {
        var viewPoint = tofCamera.WorldToViewportPoint(handPoint);
        //print("ViewPoint: " + viewPoint.x + "," + viewPoint.y);

        var handImagePoint = new Vector3(
            viewPoint.x * colorImageRect.sizeDelta.x,
            viewPoint.y * colorImageRect.sizeDelta.y,
            viewPoint.z
        ); 
        //print("HandImagePoint: " + handImagePoint.x + "," + handImagePoint.y);
        var marginX = (colorImageRect.sizeDelta.x - canvasRect.sizeDelta.x) / 2;
        var marginY = (colorImageRect.sizeDelta.y - canvasRect.sizeDelta.y) / 2;
        //print("Margin: " + marginX + "," + marginY);
        var x = handImagePoint.x - marginX;
        var y = handImagePoint.y - marginY;
        //print("Point: " + x + "," + y);
        return new Vector2(x, y);
    }
}
