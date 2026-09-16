using UnityEngine;
using UnityEngine.UI;

//战斗HUD：运行时用代码构建血条/架势条界面（不依赖场景UI资产）
//设计说明：原型阶段用代码建UI，避免场景资产依赖，方便快速迭代；数据源是双方Controller的只读属性
public class CombatHUD : MonoBehaviour
{
    private PlayerController _player;//玩家引用
    private EnemyController _enemy;//敌人引用

    private Image _playerHpFill;//玩家血条填充
    private GameObject _enemyGroup;//敌人条组(死亡时隐藏)
    private Image _enemyHpFill;//敌人血条填充
    private Image _enemyPostureFill;//敌人架势条填充

    private static Sprite _whiteSprite;//共用的1x1白色精灵
    private static Font _font;//共用的内置字体

    private void Start()
    {
        _player = FindObjectOfType<PlayerController>();
        _enemy = FindObjectOfType<EnemyController>();
        BuildUI();
    }

    private void Update()
    {
        UpdatePlayerBar();
        UpdateEnemyBars();
    }

    //玩家血条同步
    private void UpdatePlayerBar()
    {
        if (_player == null || _playerHpFill == null) return;
        _playerHpFill.fillAmount = _player.MaxHp > 0f ? _player.CurrentHp / _player.MaxHp : 0f;
    }
    //敌人血条/架势条同步（死亡时整组隐藏）
    private void UpdateEnemyBars()
    {
        if (_enemy == null || _enemyGroup == null) return;
        bool visible = !_enemy.IsDead;
        if (_enemyGroup.activeSelf != visible) _enemyGroup.SetActive(visible);
        if (!visible) return;
        _enemyHpFill.fillAmount = _enemy.MaxHp > 0f ? _enemy.CurrentHp / _enemy.MaxHp : 0f;
        _enemyPostureFill.fillAmount = _enemy.MaxPosture > 0f ? _enemy.CurrentPosture / _enemy.MaxPosture : 0f;
    }

    //=========== UI 构建 ============
    private void BuildUI()
    {
        //画布：屏幕空间覆盖 + 按分辨率等比缩放
        GameObject canvasGo = new GameObject("CombatHUD_Canvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        //玩家血条（左下角）
        _playerHpFill = BuildBar(canvasGo.transform, "PlayerHP",
            new Vector2(0f, 0f), new Vector2(60f, 60f), new Vector2(420f, 26f),
            new Color(0.80f, 0.20f, 0.20f));

        //敌人条组（顶部居中）：血条 + 架势条 + 名字
        _enemyGroup = new GameObject("EnemyGroup");
        RectTransform groupRt = _enemyGroup.AddComponent<RectTransform>();
        groupRt.SetParent(canvasGo.transform, false);
        groupRt.anchorMin = new Vector2(0.5f, 1f);
        groupRt.anchorMax = new Vector2(0.5f, 1f);
        groupRt.pivot = new Vector2(0.5f, 1f);
        groupRt.anchoredPosition = new Vector2(0f, -44f);
        groupRt.sizeDelta = new Vector2(520f, 64f);

        _enemyHpFill = BuildBar(_enemyGroup.transform, "EnemyHP",
            new Vector2(0.5f, 1f), new Vector2(0f, 0f), new Vector2(520f, 22f),
            new Color(0.72f, 0.15f, 0.15f));
        _enemyPostureFill = BuildBar(_enemyGroup.transform, "EnemyPosture",
            new Vector2(0.5f, 1f), new Vector2(0f, -28f), new Vector2(520f, 8f),
            new Color(0.90f, 0.68f, 0.16f));
        CreateLabel(_enemyGroup.transform, "ENEMY",
            new Vector2(0.5f, 1f), new Vector2(0f, -40f));
    }

    //构建"半透明黑底 + 彩色填充"的条；返回填充Image供同步数值
    private Image BuildBar(Transform parent, string name, Vector2 anchor, Vector2 offset, Vector2 size, Color color)
    {
        //背景
        GameObject bgGo = new GameObject(name);
        RectTransform bgRt = bgGo.AddComponent<RectTransform>();
        bgRt.SetParent(parent, false);
        bgRt.anchorMin = anchor;
        bgRt.anchorMax = anchor;
        bgRt.pivot = anchor;//pivot跟随锚点，便于用"角/边"定位
        bgRt.anchoredPosition = offset;
        bgRt.sizeDelta = size;
        Image bgImg = bgGo.AddComponent<Image>();
        bgImg.sprite = GetWhiteSprite();
        bgImg.color = new Color(0f, 0f, 0f, 0.6f);

        //填充层（铺满背景，用fillAmount控制显示比例）
        GameObject fillGo = new GameObject("Fill");
        RectTransform fillRt = fillGo.AddComponent<RectTransform>();
        fillRt.SetParent(bgGo.transform, false);
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;
        Image fillImg = fillGo.AddComponent<Image>();
        fillImg.sprite = GetWhiteSprite();
        fillImg.color = color;
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImg.fillAmount = 1f;
        return fillImg;
    }

    //创建一行小字（用内置字体，仅英文避免缺字）
    private Text CreateLabel(Transform parent, string content, Vector2 anchor, Vector2 offset)
    {
        GameObject go = new GameObject("Label");
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = offset;
        rt.sizeDelta = new Vector2(520f, 20f);
        Text text = go.AddComponent<Text>();
        text.text = content;
        text.font = GetFont();
        text.fontSize = 14;
        text.alignment = TextAnchor.UpperCenter;
        text.color = new Color(0.85f, 0.85f, 0.85f, 0.9f);
        return text;
    }

    //共享的1x1白色精灵（Image无sprite时无法使用Filled类型，故运行时生成）
    private static Sprite GetWhiteSprite()
    {
        if (_whiteSprite == null)
        {
            Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            _whiteSprite = Sprite.Create(tex, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f));
        }
        return _whiteSprite;
    }
    //共享的内置字体（2022起内置名为LegacyRuntime.ttf）
    private static Font GetFont()
    {
        if (_font == null)
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        return _font;
    }
}
