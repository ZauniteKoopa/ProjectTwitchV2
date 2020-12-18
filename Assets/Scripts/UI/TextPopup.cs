using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextPopup : MonoBehaviour
{
    //Serialized variables
    [SerializeField]
    private TMP_Text popupInfo = null;
    [SerializeField]
    private Vector3 initialPos = Vector3.zero;
    [SerializeField]
    private Vector3 endPos = Vector3.zero;
    [SerializeField]
    private Color startColor = Color.white;
    [SerializeField]
    private Color endColor = Color.clear;
    [SerializeField]
    private float fadeDuration = 0.75f;

    //Timer variables
    private float fadeTimer;


    // Start is called before the first frame update
    void Start()
    {
        fadeTimer = 0.0f;
    }

    //Method to set text up
    public void SetUpPopup(string textInfo)
    {
        popupInfo.text = textInfo;
    }

    // Update is called once per frame
    void Update()
    {
        fadeTimer += Time.deltaTime;
        float percent = fadeTimer / fadeDuration;

        if (percent < 1.0f)
        {
            transform.localPosition = Vector3.Lerp(initialPos, endPos, percent);
            popupInfo.color = Color.Lerp(startColor, endColor, percent);
        }
        else
        {
            Destroy(gameObject);
        }

    }
}
