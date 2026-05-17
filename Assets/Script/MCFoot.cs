using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MCFoot : MonoBehaviour
{
    public bool IsGround;
    public Collider2D PlatForm;
    [SerializeField]private float isGroundCheck;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        IsGround=Physics2D.Raycast(transform.position,Vector2.down,isGroundCheck,LayerMask.GetMask("Ground"))||Physics2D.Raycast(transform.position,Vector2.down,isGroundCheck,LayerMask.GetMask("EmptyGround"));
        PlatForm = Physics2D.Raycast(transform.position, Vector2.down, isGroundCheck, LayerMask.GetMask("TimeInteractable")).collider;
    }
    private void OnDrawGizmos(){
        Gizmos.DrawLine(transform.position,new Vector2(transform.position.x,transform.position.y-isGroundCheck));
    }
}
