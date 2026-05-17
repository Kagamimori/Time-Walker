using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class MCShade : MonoBehaviour
{
    private SpriteRenderer SR;
    [SerializeField]private float DashShadeTime;
    public void Shadef(Sprite _sprite)
    {
        SR = GetComponent<SpriteRenderer>();
        SR.sprite = _sprite;
        StartCoroutine(Shade());
    }
    IEnumerator Shade()
    {
        float timer = 0;
        while (timer < DashShadeTime)
        {
            SR.color=new Color(0.5f, 0.5f, 0.5f, 1 - timer / DashShadeTime);
            timer += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }
}
