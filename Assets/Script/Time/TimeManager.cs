using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    private Camera mainCamera;
    public float DetectmaxDistance;
    public LayerMask TimeLayer;
    private RaycastHit2D MouseHit;
    private RaycastHit2D LastMouseHit;
    private RaycastHit2D PressedMouseHit;
    public bool IsTimeReversing;
    public static TimeManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

    }
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButton(0))
        {
            if (MouseHit.collider != null)
            {
                if (Input.GetKey(KeyCode.Q))
                {
                    ITimeControlable timeControlable = MouseHit.collider.GetComponent<ITimeControlable>();
                    if (timeControlable != null && timeControlable.CanReserveTime)
                    {
                        timeControlable.ChangeCurrentTime(-Time.deltaTime);
                        IsTimeReversing = true;
                    }
                }
                else if (Input.GetKey(KeyCode.E))
                {
                    ITimeControlable timeControlable = MouseHit.collider.GetComponent<ITimeControlable>();
                    if (timeControlable != null && timeControlable.CanReserveTime)
                    {
                        timeControlable.ChangeCurrentTime(Time.deltaTime);
                        IsTimeReversing = true;
                    }
                }
                else
                {
                    IsTimeReversing = false;
                }
            }
            else
            {
                IsTimeReversing = false;
            }
        }
        else
        {
            MouseHit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, DetectmaxDistance, TimeLayer);
            IsTimeReversing = false;
        }
        if (LastMouseHit.collider == null)
        {
            if(MouseHit.collider != null)
            {
                ITimeControlable timeControlable = MouseHit.collider.GetComponent<ITimeControlable>();
                if(timeControlable != null)
                {
                    timeControlable.Lighten(true);
                }
            }
        }
        else
        {
            if(MouseHit.collider == null)
            {
                ITimeControlable timeControlable = LastMouseHit.collider.GetComponent<ITimeControlable>();
                if (timeControlable != null)
                {
                    timeControlable.Lighten(false);
                }
            }
            else if (MouseHit.collider != LastMouseHit.collider)
            {
                ITimeControlable timeControlable = LastMouseHit.collider.GetComponent<ITimeControlable>();
                if (timeControlable != null)
                {
                    timeControlable.Lighten(false);
                }
                timeControlable = MouseHit.collider.GetComponent<ITimeControlable>();
                if (timeControlable != null)
                {
                    timeControlable.Lighten(true);
                }
            }
        }
        LastMouseHit = MouseHit;
    }
}
