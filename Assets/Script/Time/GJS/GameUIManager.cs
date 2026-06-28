using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    public GameObject deathPanel;
    public GameObject winPanel;

    private void Start()
    {
        // 确保初始隐藏
        deathPanel.SetActive(false);
        winPanel.SetActive(false);
    }

    // 显示死亡UI
    public void ShowDeathUI()
    {
        deathPanel.SetActive(true);
        Time.timeScale = 0f; // 暂停游戏
    }

    // 显示胜利UI
    public void ShowWinUI()
    {
        winPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    // 重新开始按钮点击事件（挂载到两个Button上）
    public void RestartGame()
    {
        Time.timeScale = 1f; // 恢复时间
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}