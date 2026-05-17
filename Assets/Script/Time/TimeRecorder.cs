using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeRecorder : MonoBehaviour
{
    struct TimeRecord
    {
        public Vector3 position;
        public float time;
    }
    private Stack<TimeRecord> timeRecords=new Stack<TimeRecord>();
    private TimeManager timeManager;
    private void Start()
    {
        timeManager = TimeManager.Instance;
    }
    void Update()
    {
        //if (timeManager.TimeRate > 0)
        //{
        //    timeRecords.Push(new TimeRecord { position = transform.position, time = timeManager.CurrentTime });
        //}
        //else
        //{
        //    while (timeManager.CurrentTime < timeRecords.Peek().time)
        //    {
        //        transform.position = timeRecords.Pop().position;
        //    }
        //}
    }
}
