using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BITE : MonoBehaviour
{
    public MCscript MC;
    public Animator anim;
    private bool B1;
    private bool B2;
    private PolygonCollider2D PC2D;
    public float BiteAngle;
    // public Dictionary<GameObject, int> collidedObjects = new Dictionary<GameObject, int>();
    // void OnTriggerEnter2D(Collider2D other){
    //     GameObject collidedObj=other.gameObject;
    //     if (collidedObjects.ContainsKey(collidedObj)){
    //         collidedObjects[collidedObj]++;
    //     }
    //     else{
    //         collidedObjects.Add(collidedObj,1);
    //     }
    // }
    // void OnTriggerExit2D(Collider2D other){
    //     GameObject OutObj=other.gameObject;
    //     if(collidedObjects.ContainsKey(OutObj)){
    //         collidedObjects[OutObj]--;
    //         if(collidedObjects[OutObj]==0){
    //             collidedObjects.Remove(OutObj);
    //         }
    //     }
    // }

    private SpriteRenderer SR;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        PC2D = GetComponent<PolygonCollider2D>();
        SR = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector2(MC.transform.position.x, MC.transform.position.y + 2f);

        MC.Closemouth = false;
    }
    public void Bite(float BiteDirecion) {
        SR.enabled = true;
        Invoke("SRDisappear", 0.5f);
        switch (BiteDirecion)
        {
            case 1:
                transform.localRotation = Quaternion.Euler(0, 0, 0);
                break;
            case 2:
                transform.localRotation = Quaternion.Euler(0, 0, 270);
                break;
            case 3:
                transform.localRotation = Quaternion.Euler(0, 0, 180);
                break;
            case 4:
                transform.localRotation = Quaternion.Euler(0, 0, 90);
                break;
        }
        List<Collider> results = new List<Collider>();
        Collider2D[] colliders = Physics2D.OverlapAreaAll(PC2D.bounds.min, PC2D.bounds.max);
        List<GameObject> result = new List<GameObject>();
        foreach (Collider2D col in colliders) {
            if (result.Contains(col.gameObject)) {
                return;
            }
            result.Add(col.gameObject);
            if (col != PC2D) {
                GameObject obj = col.gameObject;
                IDamagableE iDamagableE = obj.GetComponent<IDamagableE>();
                if (iDamagableE != null) {
                    iDamagableE.DieOut();
                }
                Enemy enemy = obj.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(1, MC.transform);
                }
            }
        }
    }
    public void SRDisappear()
    {
        SR.enabled = false;
    }
}


// public class CollisionSnapshot : MonoBehaviour
// {
//     public Collider targetCollider;
//     public KeyCode triggerKey = KeyCode.Space;
    
//     private void Update()
//     {
//         if (Input.GetKeyDown(triggerKey))
//         {
//             // 在按下空格键的时刻检测
//             TakeCollisionSnapshot();
//         }
//     }
    
//     public void TakeCollisionSnapshot()
//     {
//         Debug.Log($"--- 碰撞快照（时间: {Time.time}） ---");
        
//         List<Collider> collidingTriggers = GetCollidingTriggers();
        
//         if (collidingTriggers.Count > 0)
//         {
//             foreach (Collider trigger in collidingTriggers)
//             {
//                 Debug.Log($"检测到触发器: {trigger.name} | 位置: {trigger.transform.position}");
                
//                 // 获取额外信息
//                 TriggerInfo info = trigger.GetComponent<TriggerInfo>();
//                 if (info != null)
//                 {
//                     Debug.Log($"触发器类型: {info.triggerType}");
//                 }
//             }
//         }
//         else
//         {
//             Debug.Log("当前没有触发任何触发器");
//         }
//     }
    
//     private List<Collider> GetCollidingTriggers()
//     {
//         List<Collider> results = new List<Collider>();
//         Collider[] colliders = Physics.OverlapBox(
//             targetCollider.bounds.center,
//             targetCollider.bounds.extents,
//             Quaternion.identity
//         );
        
//         foreach (Collider col in colliders)
//         {
//             if (col.isTrigger && col != targetCollider)
//             {
//                 results.Add(col);
//             }
//         }
        
//         return results;
//     }
// }