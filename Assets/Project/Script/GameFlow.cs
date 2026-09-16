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
    }
    //重开当前场景
    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
