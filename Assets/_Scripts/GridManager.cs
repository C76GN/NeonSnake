using UnityEngine;

// ����ű��������������Ϸ������ϵͳ
public class GridManager : MonoBehaviour
{
    // ʹ�õ���ģʽ�����������ű����ٷ��ʵ�GridManager��ʵ��
    public static GridManager Instance { get; private set; }

    [Header("��������")]
    [SerializeField] private int width = 32;  // ������
    [SerializeField] private int height = 18; // ����߶�

    private void Awake()
    {
        // ����ģʽ�ĳ�ʼ��
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    /// <summary>
    /// ����������ת��Ϊ��������
    /// ��������Ϊ(0,0)��������Ҫƫ����
    /// </summary>
    /// <param name="gridPosition">�������� (x, y)</param>
    /// <returns>��������</returns>
    public Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        return new Vector3(gridPosition.x, gridPosition.y) - new Vector3(width / 2f - 0.5f, height / 2f - 0.5f);
    }

    /// <summary>
    /// ����һ�����������������Ч��λ��
    /// ��֮�������չ��������������������ɣ�
    /// </summary>
    /// <returns>�������������</returns>
    public Vector2Int GetRandomGridPosition()
    {
        return new Vector2Int(
            Random.Range(0, width),
            Random.Range(0, height)
        );
    }

    /// <summary>
    /// ����߽�ѭ���ĺ����߼�
    /// ���һ�����곬���˱߽磬����������һ�߳���
    /// </summary>
    /// <param name="gridPosition">��ǰ����������</param>
    /// <returns>�����˱߽�ѭ�����������</returns>
    public Vector2Int WrapGridPosition(Vector2Int gridPosition)
    {
        int x = gridPosition.x;
        int y = gridPosition.y;

        if (x < 0) x = width - 1;
        if (x >= width) x = 0;

        if (y < 0) y = height - 1;
        if (y >= height) y = 0;

        return new Vector2Int(x, y);
    }

    // --- ������������ʾ���񱳾��Ĵ��� (��ѡ) ---

    [Header("������ӻ�")]
    [SerializeField] private GameObject gridTilePrefab; // ������ʾ�����Ԥ����
    [SerializeField] private Transform gridContainer;   // �������������Ƭ�ĸ�����
    [SerializeField] private bool showGrid = true;      // �Ƿ���ʾ����

    void Start()
    {
        // �����ѡ����ʾ����������Ϸ��ʼʱ��������
        if (showGrid)
        {
            CreateGridVisuals();
        }
    }

    private void CreateGridVisuals()
    {
        if (gridTilePrefab == null || gridContainer == null) return;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);
                Vector3 worldPos = GetWorldPosition(gridPos);

                GameObject tile = Instantiate(gridTilePrefab, worldPos, Quaternion.identity);
                tile.transform.SetParent(gridContainer);

                // �ø�������ǳ�����Ч���������������ש
                bool isOffset = (x + y) % 2 == 1;
                tile.GetComponent<SpriteRenderer>().color = isOffset
                    ? new Color(0.2f, 0.2f, 0.2f, 0.5f)
                    : new Color(0.15f, 0.15f, 0.15f, 0.5f);
            }
        }
    }

    // �л�����ɼ��ԵĹ����������������ű�����UI��ť������
    public void SetGridVisibility(bool isVisible)
    {
        if (gridContainer != null)
        {
            gridContainer.gameObject.SetActive(isVisible);
        }
    }
}