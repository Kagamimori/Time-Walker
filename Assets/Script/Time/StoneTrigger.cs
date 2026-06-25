using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneTrigger : MonoBehaviour
{
    public List<Stone1> Left;
    public List<Stone1> Right;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            foreach(Stone1 stone in Left)
            {
                stone.DieOut(4);
            }
            foreach (Stone1 stone in Right)
            {
                stone.DieOut(2);
            }
            this.enabled = false;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
