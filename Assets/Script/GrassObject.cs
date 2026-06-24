using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassObject : MonoBehaviour
{
    private Animator anim;
    [Header("变亮")]
    [SerializeField] private AnimationCurve LightCurve;
    [SerializeField] private float LightTime;
    [SerializeField] private float Darkrgb;
    [SerializeField] private float Lightrbg;
    private SpriteRenderer SR;
    private Coroutine LightCorotine;
    // Start is called before the first frame update
    void Start()
    {
        SR=GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetAnimatiorPercent(float per)
    {
        anim.Play("Grass", 0, per);
    }
    public void Lighten(bool _isLighten)
    {
        if (LightCorotine != null)
        {
            StopCoroutine(LightCorotine);
        }
        LightCorotine = StartCoroutine(Light(_isLighten));
    }
    IEnumerator Light(bool _isLighten)
    {
        float orginrbg = SR.color.r;
        float targetrbg = _isLighten ? Lightrbg : Darkrgb;
        float timer = 0;
        while (timer < LightTime)
        {
            timer += Time.deltaTime;
            float rbg = Mathf.Lerp(orginrbg, targetrbg, LightCurve.Evaluate(timer / LightTime));
            SR.color = new Color(rbg, rbg, rbg, 1);
            yield return null;
        }
        SR.color = new Color(targetrbg, targetrbg, targetrbg, 1);
    }
}
