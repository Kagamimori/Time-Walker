using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Juke : MonoBehaviour
{
    [Header("Y谐振运动")]
    public float YAmplitude;
    public float YPeriod;
    public Vector3 Offset;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        float Yoffset = YAmplitude * Mathf.Sin((2f * Mathf.PI / YPeriod) * Time.time);
        transform.localPosition=new Vector3 (0, Yoffset, 0)+Offset;
    }
}
