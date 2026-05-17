using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class Stone : MonoBehaviour, IDamagableE, ITimeControlable
{
    public bool CanReserveTime { get; set; }
    private MCscript MC;
    private Rigidbody2D rb;
    public float BiteOutSpeed = 1.0f;
    private Coroutine BiteCorotine;

    public bool IsTimeReserving = false;
    private bool IsTimeReserving2 = false;
    private bool IsRecording = false;

    private float CurrentTime;

    private Vector2[] PosList = new Vector2[10000];
    private Vector3 LastPos;
    private Vector3 TargetPos;
    private int ListTotalNum;
    private int CurrentNum;
    public float RecordInteval;

    [Header("变亮")]
    [SerializeField] private AnimationCurve LightCurve;
    [SerializeField] private float LightTime;
    [SerializeField] private float Darkrgb;
    [SerializeField] private float Lightrbg;
    private SpriteRenderer SR;
    private Coroutine LightCorotine;
    private bool IsLighten;
    private Vector3 velocity;
    // Start is called before the first frame update
    void Start()
    {
        MC = PlayerManager.Instance.MC;
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        SR = GetComponent<SpriteRenderer>();
        CanReserveTime = true;
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        ListTotalNum = 0;
        CurrentNum= 0;
        PosList[0] = transform.position;
        LastPos = transform.position;
        TargetPos = transform.position;
        StartCoroutine(Record());
    }
    // Update is called once per frame
    void Update()
    {
        if (IsTimeReserving)
        {
            transform.position = transform.position = Vector3.SmoothDamp(transform.position,TargetPos,ref velocity,0.01f,100);
        }
    }
    void LateUpdate()
    {
        if (IsTimeReserving2)
        {
            IsTimeReserving2 = false;
        }
        else
        {
            IsTimeReserving = false;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    public void DieOut()
    {
        if (IsTimeReserving)
        {
            return;
        }
        if (BiteCorotine != null)
        {
            StopCoroutine(BiteCorotine);
        }
        BiteCorotine = StartCoroutine(Bite());
    }
    IEnumerator Bite()
    {
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        IsTimeReserving = false;
        rb.bodyType = RigidbodyType2D.Kinematic;
        float BiteDirection = MC.BiteDirection;
        if (BiteDirection == 1)
        {
            rb.velocity = new Vector2(0, BiteOutSpeed);
        }
        else if (BiteDirection == 2)
        {
            rb.velocity = new Vector2(BiteOutSpeed, 0);
        }
        else if (BiteDirection == 3)
        {
            //rb.velocity = new Vector2 (0, -BiteOutSpeed);
        }
        else if (BiteDirection == 4)
        {
            rb.velocity = new Vector2(-BiteOutSpeed, 0);
        }
        IsRecording = true;
        yield return new WaitForSeconds(1f);
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        rb.bodyType = RigidbodyType2D.Dynamic;
        IsRecording = false;
    }

    public void ChangeCurrentTime(float deltaTime)
    {
        rb.bodyType = RigidbodyType2D.Kinematic;
        IsTimeReserving = true;
        IsTimeReserving2 = true;
        CurrentTime += deltaTime;
        if (CurrentTime < 0)
        {
            CurrentTime = 0;
        }
        if(CurrentTime > ListTotalNum * RecordInteval)
        {
            CurrentTime = ListTotalNum * RecordInteval;
        }
        CurrentNum = (int)(CurrentTime / RecordInteval);
        TargetPos = PosList[CurrentNum];
        LastPos = transform.position;
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
    IEnumerator Record()
    {
        while (true)
        {
            if (!IsTimeReserving&&(transform.position - LastPos).magnitude>0.5f)
            {
                ListTotalNum = CurrentNum + 1;
                CurrentNum = ListTotalNum;
                PosList[ListTotalNum] = transform.position;
                LastPos = transform.position;
                CurrentTime += RecordInteval;
            }
            yield return new WaitForSeconds(RecordInteval);
        }
    }
}
