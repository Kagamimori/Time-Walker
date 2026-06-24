using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour,ITimeControlable
{
    public bool CanReserveTime { get; set; }
    public List<GrassObject> grassObjects;
    public float SingleGrassTime;

    private float CurrentTime;

    private bool IsLighten;

    public BoxCollider2D GrassCollider;
    public float GrassColliderHeight;
    // Start is called before the first frame update
    void Start()
    {
        CanReserveTime = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Lighten(bool _isLighten)
    {
        if (IsLighten == _isLighten)
        {
            return;
        }

        foreach(GrassObject grass in grassObjects)
        {
            grass.Lighten(_isLighten);
        }
        IsLighten = _isLighten;
    }
    public void ChangeCurrentTime(float deltaTime)
    {
        CurrentTime += deltaTime;
        if (CurrentTime < 0)
        {
            CurrentTime = 0;
        }
        if (CurrentTime > SingleGrassTime*grassObjects.Count)
        {
            CurrentTime = SingleGrassTime * grassObjects.Count;
        }
        int a= (int) (CurrentTime / SingleGrassTime);
        float b=CurrentTime % SingleGrassTime;
        for (int i = 0; i < grassObjects.Count; i++)
        {
            if (i < a)
            {
                grassObjects[i].SetAnimatiorPercent(1);
            }
            else if (i == a)
            {
                grassObjects[i].SetAnimatiorPercent(b/SingleGrassTime);
            }
            else
            {
                grassObjects[i].SetAnimatiorPercent(0);
            }
        }
        GrassCollider.size = new Vector2(GrassCollider.size.x,GrassColliderHeight*(CurrentTime/ grassObjects.Count / SingleGrassTime));
        GrassCollider.offset = new Vector2(GrassCollider.offset.x, (GrassColliderHeight/2)* (CurrentTime / grassObjects.Count / SingleGrassTime));
    }
}
