using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

//游戏流程管理：测试用快捷键
public class GameFlow : MonoBehaviour
{
    private void Update()
    {
        //R键：重开当前场景（重置玩家/敌人/一切）
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            ReloadScene();
        }
        //ESC键：退出游戏
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            QuitGame();
        }
    }
    //重开当前场景
    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    //退出游戏（编辑器内停止播放，打包后关闭程序）
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
