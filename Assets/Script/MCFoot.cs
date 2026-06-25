using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class MCFoot : MonoBehaviour
{
    public bool IsGround;
    public Collider2D PlatForm;
    [SerializeField]private float isGroundCheck;
    public LayerMask layerMask;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        IsGround = Physics2D.Raycast(transform.position, Vector2.down, isGroundCheck, layerMask);
        RaycastHit2D hit = Physics2D.Raycast(transform.position,Vector2.down,isGroundCheck,layerMask);
        if (hit.collider == null)
        {
            PlatForm = null;
        }
        else if(hit.collider.gameObject.tag=="Platform")
        {
            PlatForm = hit.collider;
        }
    }
    private void OnDrawGizmos(){
        Gizmos.DrawLine(transform.position,new Vector2(transform.position.x,transform.position.y-isGroundCheck));
    }
}
