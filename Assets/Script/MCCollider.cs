using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class MCCollider : MonoBehaviour
{  
    public MCscript MC;
    public Transform MCFollower;
    public float BallAddMP;
    private void Start()
    {

    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Grass"))
        {
            MC.IsInGrass = true;
        }
        if (other.CompareTag("MPBall"))
        {
            PlayerManager.Instance.PlayerMP += BallAddMP;
            Destroy(other.gameObject);
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Grass"))
        {
            MC.IsInGrass = false;
        }
    }
}
