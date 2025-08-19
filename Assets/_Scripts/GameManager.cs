using UnityEngine;
using UnityEngine.UI; // 用于操作UI组件
using UnityEngine.SceneManagement; // 用于重新加载场景
using System.IO; // 用于文件操作
using System.Collections; // 用于使用协程

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("游戏对象引用")]
    [SerializeField] private SnakeController snakeController;
    [SerializeField] private GridManager gridManager;

    [Header("UI 元素")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Toggle gridToggle;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        // --- 为UI按钮添加监听事件 ---
        restartButton.onClick.AddListener(RestartGame);
        saveButton.onClick.AddListener(OnSaveButtonClick);
    }

    /// <summary>
    /// 当游戏结束时由SnakeController调用
    /// </summary>
    public void ShowGameOverUI()
    {
        gameOverPanel.SetActive(true);
    }

    /// <summary>
    /// 重新开始按钮的功能
    /// </summary>
    private void RestartGame()
    {
        // 重新加载当前活动的场景
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// “保存图片”按钮被点击时触发
    /// </summary>
    private void OnSaveButtonClick()
    {
        // 使用协程来执行截图，可以避免卡顿，并能确保在下一帧渲染后截图
        StartCoroutine(CaptureScreenshot());
    }

    /// <summary>
    /// 截图的核心协程
    /// </summary>
    private IEnumerator CaptureScreenshot()
    {
        // 1. 截图前，先根据Toggle的状态和我们的需求准备好场景
        gameOverPanel.SetActive(false); // 隐藏UI
        gridManager.SetGridVisibility(gridToggle.isOn); // 根据Toggle设置网格的可见性

        // 2. 等待一帧，确保上面的修改被渲染到屏幕上
        yield return new WaitForEndOfFrame();

        // 3. 执行截图
        // 创建一个和屏幕一样大的Texture2D
        Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        // 读取当前屏幕像素到Texture2D
        screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenshot.Apply();

        // 4. 将Texture2D转换为PNG格式的字节数组
        byte[] bytes = screenshot.EncodeToPNG();
        Destroy(screenshot); // 销毁临时的Texture2D对象

        // 5. 定义保存路径和文件名
        // Application.persistentDataPath 是一个在所有平台上都可读写的安全路径
        string path = Path.Combine(Application.persistentDataPath, "Artworks");
        // 如果文件夹不存在，则创建它
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        // 创建一个基于时间戳的唯一文件名
        string fileName = $"NeonSnake_Art_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png";
        string fullPath = Path.Combine(path, fileName);

        // 6. 将字节数组写入文件
        File.WriteAllBytes(fullPath, bytes);
        Debug.Log($"截图已保存到: {fullPath}");

        // 7. 截图完成后，恢复UI显示
        gameOverPanel.SetActive(true);
    }
}