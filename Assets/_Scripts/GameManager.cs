using UnityEngine;
using UnityEngine.UI; // ���ڲ���UI���
using UnityEngine.SceneManagement; // �������¼��س���
using System.IO; // �����ļ�����
using System.Collections; // ����ʹ��Э��

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("��Ϸ��������")]
    [SerializeField] private SnakeController snakeController;
    [SerializeField] private GridManager gridManager;

    [Header("UI Ԫ��")]
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

        // --- ΪUI��ť��Ӽ����¼� ---
        restartButton.onClick.AddListener(RestartGame);
        saveButton.onClick.AddListener(OnSaveButtonClick);
    }

    /// <summary>
    /// ����Ϸ����ʱ��SnakeController����
    /// </summary>
    public void ShowGameOverUI()
    {
        gameOverPanel.SetActive(true);
    }

    /// <summary>
    /// ���¿�ʼ��ť�Ĺ���
    /// </summary>
    private void RestartGame()
    {
        // ���¼��ص�ǰ��ĳ���
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// ������ͼƬ����ť�����ʱ����
    /// </summary>
    private void OnSaveButtonClick()
    {
        // ʹ��Э����ִ�н�ͼ�����Ա��⿨�٣�����ȷ������һ֡��Ⱦ���ͼ
        StartCoroutine(CaptureScreenshot());
    }

    /// <summary>
    /// ��ͼ�ĺ���Э��
    /// </summary>
    private IEnumerator CaptureScreenshot()
    {
        // 1. ��ͼǰ���ȸ���Toggle��״̬�����ǵ�����׼���ó���
        gameOverPanel.SetActive(false); // ����UI
        gridManager.SetGridVisibility(gridToggle.isOn); // ����Toggle��������Ŀɼ���

        // 2. �ȴ�һ֡��ȷ��������޸ı���Ⱦ����Ļ��
        yield return new WaitForEndOfFrame();

        // 3. ִ�н�ͼ
        // ����һ������Ļһ�����Texture2D
        Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        // ��ȡ��ǰ��Ļ���ص�Texture2D
        screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenshot.Apply();

        // 4. ��Texture2Dת��ΪPNG��ʽ���ֽ�����
        byte[] bytes = screenshot.EncodeToPNG();
        Destroy(screenshot); // ������ʱ��Texture2D����

        // 5. ���屣��·�����ļ���
        // Application.persistentDataPath ��һ��������ƽ̨�϶��ɶ�д�İ�ȫ·��
        string path = Path.Combine(Application.persistentDataPath, "Artworks");
        // ����ļ��в����ڣ��򴴽���
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        // ����һ������ʱ�����Ψһ�ļ���
        string fileName = $"NeonSnake_Art_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png";
        string fullPath = Path.Combine(path, fileName);

        // 6. ���ֽ�����д���ļ�
        File.WriteAllBytes(fullPath, bytes);
        Debug.Log($"��ͼ�ѱ��浽: {fullPath}");

        // 7. ��ͼ��ɺ󣬻ָ�UI��ʾ
        gameOverPanel.SetActive(true);
    }
}