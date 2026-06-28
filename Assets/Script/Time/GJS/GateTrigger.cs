using UnityEngine;

public class GateTrigger : MonoBehaviour
{
    private GameUIManager uiManager;

    private void Start()
    {
        uiManager = FindObjectOfType<GameUIManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            MCscript mc = other.GetComponent<MCscript>();
            if (mc != null)
            {
                mc.SetWinState(true);
            }
            if (uiManager != null)
                uiManager.ShowWinUI();
        }
    }

}