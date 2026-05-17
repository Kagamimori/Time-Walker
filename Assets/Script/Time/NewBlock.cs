using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBlock : MonoBehaviour,ITimeControlable
{
    public bool CanReserveTime { get; set; }
    [SerializeField] private Vector3 StartPos;
    [SerializeField] private Vector3 EndPos;
    [SerializeField] public AnimationCurve Xcurve;
    [SerializeField] public AnimationCurve Ycurve;
    [SerializeField] private float CurrentTime;
    [SerializeField] private float EndTime;
    [Header("变亮")]
    [SerializeField] private AnimationCurve LightCurve;
    [SerializeField] private float LightTime;
    [SerializeField] private float Darkrgb;
    [SerializeField] private float Lightrbg;
    private SpriteRenderer SR;
    private Coroutine LightCorotine;
    private bool IsLighten;
    // Start is called before the first frame update
    void Start()
    {
        CanReserveTime = true;
        SR = GetComponent<SpriteRenderer>();
        transform.position = StartPos;
        SR.color = new Color(Darkrgb, Darkrgb, Darkrgb,1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ChangeCurrentTime(float deltaTime)
    {
        CurrentTime += deltaTime;
        if (CurrentTime > EndTime)
        {
            CurrentTime = EndTime;
        }
        else if (CurrentTime < 0)
        {
            CurrentTime = 0;
        }
        transform.position=new Vector3(Mathf.Lerp(StartPos.x, EndPos.x, Xcurve.Evaluate(CurrentTime / EndTime)), Mathf.Lerp(StartPos.y, EndPos.y, Ycurve.Evaluate(CurrentTime / EndTime)), transform.position.z);
    }
    public void Lighten(bool _isLighten)
    {
        if (IsLighten == _isLighten)
        {
            return;
        }

        if (LightCorotine != null)
        {
            StopCoroutine(LightCorotine);
        }
        LightCorotine = StartCoroutine(Light(_isLighten));

        IsLighten = _isLighten;
    }
    IEnumerator Light(bool _isLighten)
    {
        float orginrbg=SR.color.r;
        float targetrbg=_isLighten ? Lightrbg : Darkrgb;
        float timer=0;
        while(timer<LightTime)
        {
            timer += Time.deltaTime;
            float rbg = Mathf.Lerp(orginrbg, targetrbg, LightCurve.Evaluate(timer / LightTime));
            SR.color = new Color(rbg, rbg, rbg, 1);
            yield return null;
        }
        SR.color = new Color(targetrbg, targetrbg, targetrbg, 1);
    }
}
