using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gook : MonoBehaviour
{
    private Rigidbody2D rb;
    private HingeJoint2D hingeJoint2D;
    public Vector2 Offset;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        hingeJoint2D = GetComponent<HingeJoint2D>();
    }

    // Update is called once per frame
    void Update()
    {
        rb.centerOfMass = Offset;
    }
}
