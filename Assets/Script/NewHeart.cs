using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewHeart : MonoBehaviour
{
    public Image InsideImage;
    public Image OutsideImage;
    private Coroutine NewHeartCoroutine;
    public AnimationCurve ChangeValueCurve;
    public float ChangeValueTime;
    void Start()
    {

    }
    void Update()
    {

    }
    public void SetValue(float value)
    {
        if (NewHeartCoroutine != null)
        {
            StopCoroutine(NewHeartCoroutine);
        }
        NewHeartCoroutine = StartCoroutine(ChangeValue(value));
    }
    IEnumerator ChangeValue(float value)
    {
        float timer = 0f;
        float OriginAmount = 0f;
        if (OutsideImage.fillAmount > value&&InsideImage.fillAmount>value) //值要变小则先外后内
        {
            OriginAmount = OutsideImage.fillAmount;
            timer = 0f;
            while (timer < ChangeValueTime)
            {
                OutsideImage.fillAmount = Mathf.Lerp(OriginAmount, value, ChangeValueCurve.Evaluate(timer / ChangeValueTime));
                timer += Time.deltaTime;
                yield return null;
            }
            OutsideImage.fillAmount = value;

            OriginAmount=InsideImage.fillAmount;
            timer = 0f;
            while (timer < ChangeValueTime)
            {
                InsideImage.fillAmount = Mathf.Lerp(OriginAmount, value, ChangeValueCurve.Evaluate(timer / ChangeValueTime));
                timer += Time.deltaTime;
                yield return null;
            }
            InsideImage.fillAmount = value;
        }
        else if(OutsideImage.fillAmount < value && InsideImage.fillAmount < value)  //值要变大则先内后外
        {
            OriginAmount = InsideImage.fillAmount;
            timer = 0f;
            while (timer < ChangeValueTime)
            {
                InsideImage.fillAmount = Mathf.Lerp(OriginAmount, value, ChangeValueCurve.Evaluate(timer / ChangeValueTime));
                timer += Time.deltaTime;
                yield return null;
            }
            InsideImage.fillAmount = value;

            OriginAmount = OutsideImage.fillAmount;
            timer = 0f;
            while (timer < ChangeValueTime)
            {
                OutsideImage.fillAmount = Mathf.Lerp(OriginAmount, value, ChangeValueCurve.Evaluate(timer / ChangeValueTime));
                timer += Time.deltaTime;
                yield return null;
            }
            OutsideImage.fillAmount = value;
        }
        else //否则一起动
        {
            OriginAmount = OutsideImage.fillAmount;
            float OriginAmount2= InsideImage.fillAmount;
            timer = 0f;
            while (timer < ChangeValueTime)
            {
                OutsideImage.fillAmount = Mathf.Lerp(OriginAmount, value, ChangeValueCurve.Evaluate(timer / ChangeValueTime));
                InsideImage.fillAmount = Mathf.Lerp(OriginAmount2, value, ChangeValueCurve.Evaluate(timer / ChangeValueTime));
                timer += Time.deltaTime;
                yield return null;
            }
            OutsideImage.fillAmount= value;
            InsideImage.fillAmount = value;
        }
    }
}
