using UnityEngine;

// 这个脚本负责管理整个游戏的网格系统
public class GridManager : MonoBehaviour
{
    // 使用单例模式，方便其他脚本快速访问到GridManager的实例
    public static GridManager Instance { get; private set; }

    [Header("网格设置")]
    [SerializeField] private int width = 32;  // 网格宽度
    [SerializeField] private int height = 18; // 网格高度

    private void Awake()
    {
        // 单例模式的初始化
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
    /// 将网格坐标转换为世界坐标
    /// 网格中心为(0,0)，所以需要偏移量
    /// </summary>
    /// <param name="gridPosition">网格坐标 (x, y)</param>
    /// <returns>世界坐标</returns>
    public Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        return new Vector3(gridPosition.x, gridPosition.y) - new Vector3(width / 2f - 0.5f, height / 2f - 0.5f);
    }

    /// <summary>
    /// 生成一个在网格内随机且有效的位置
    /// （之后可以扩展，例如避免在蛇身上生成）
    /// </summary>
    /// <returns>随机的网格坐标</returns>
    public Vector2Int GetRandomGridPosition()
    {
        return new Vector2Int(
            Random.Range(0, width),
            Random.Range(0, height)
        );
    }

    /// <summary>
    /// 处理边界循环的核心逻辑
    /// 如果一个坐标超出了边界，就让它从另一边出来
    /// </summary>
    /// <param name="gridPosition">当前的网格坐标</param>
    /// <returns>处理了边界循环后的新坐标</returns>
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

    // --- 以下是用于显示网格背景的代码 (可选) ---

    [Header("网格可视化")]
    [SerializeField] private GameObject gridTilePrefab; // 用于显示网格的预制体
    [SerializeField] private Transform gridContainer;   // 存放所有网格瓦片的父对象
    [SerializeField] private bool showGrid = true;      // 是否显示网格

    void Start()
    {
        // 如果勾选了显示网格，则在游戏开始时创建它们
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

                // 让格子有深浅交错的效果，看起来更像瓷砖
                bool isOffset = (x + y) % 2 == 1;
                tile.GetComponent<SpriteRenderer>().color = isOffset
                    ? new Color(0.2f, 0.2f, 0.2f, 0.5f)
                    : new Color(0.15f, 0.15f, 0.15f, 0.5f);
            }
        }
    }

    // 切换网格可见性的公共方法，供其他脚本（如UI按钮）调用
    public void SetGridVisibility(bool isVisible)
    {
        if (gridContainer != null)
        {
            gridContainer.gameObject.SetActive(isVisible);
        }
    }
}