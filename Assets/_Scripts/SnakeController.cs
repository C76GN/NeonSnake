using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// 定义蛇的三种颜色状态
public enum SnakeColor { Red, Green, Blue }

// 定义蛇的身体节点，包含它在网格中的位置和颜色
public class SnakeNode
{
    public Vector2Int GridPosition { get; set; }
    public SnakeColor Color { get; set; }
    public Transform NodeTransform { get; set; } // 对应的游戏对象的Transform

    public SnakeNode(Vector2Int gridPosition, SnakeColor color, Transform nodeTransform)
    {
        GridPosition = gridPosition;
        Color = color;
        NodeTransform = nodeTransform;
    }
}

public class SnakeController : MonoBehaviour
{
    [Header("游戏设置")]
    [SerializeField, Tooltip("蛇每移动一格所需的时间")]
    private float moveRate = 0.2f;
    [SerializeField, Tooltip("蛇的初始长度")]
    private int initialSize = 4;

    [Header("预制体和容器")]
    [SerializeField] private GameObject nodePrefab; // 蛇身体节点的预制体
    [SerializeField] private Transform nodeContainer; // 存放所有蛇节点的父对象
    [SerializeField] private GameObject foodPrefab; //食物的预制体

    private float _moveTimer; // 移动计时器
    private Vector2Int _currentDirection; // 当前移动方向
    private Vector2Int _inputDirection;   // 玩家的输入方向

    private List<SnakeNode> _snakeBody; // 存储蛇所有节点的列表
    private SnakeColor _currentSnakeColor; // 蛇头当前的颜色

    // 颜色映射表，将枚举转换为Unity的Color
    private Dictionary<SnakeColor, Color> _colorMap;

    private Food _food; // 对食物对象的引用
    private bool _shouldGrow = false; // 标记下一帧是否需要成长

    void Awake()
    {
        // 初始化颜色映射
        _colorMap = new Dictionary<SnakeColor, Color>
        {
            { SnakeColor.Red, Color.red },
            { SnakeColor.Green, Color.green },
            { SnakeColor.Blue, Color.blue }
        };
    }

    void Start()
    {
        InitializeSnake();
        SpawnFood();
    }

    void Update()
    {
        HandleInput();
        HandleMovement();
    }

    /// <summary>
    /// 初始化蛇的状态
    /// </summary>
    private void InitializeSnake()
    {
        _snakeBody = new List<SnakeNode>();
        _currentDirection = Vector2Int.right; // 初始向右移动
        _inputDirection = Vector2Int.right;
        _currentSnakeColor = SnakeColor.Red; // 初始为红色

        // 创建初始长度的蛇身
        for (int i = 0; i < initialSize; i++)
        {
            // 从中心点开始向左生成
            Vector2Int startPos = new Vector2Int(10 - i, 9);
            CreateNode(startPos, _currentSnakeColor);
        }

        // 蛇头稍大一点
        _snakeBody[0].NodeTransform.localScale = Vector3.one * 1.2f;
    }

    /// <summary>
    /// 处理玩家输入
    /// </summary>
    private void HandleInput()
    {
        // --- 移动输入 ---
        // 只有在当前方向不是垂直方向时，才能接受水平输入
        if (_currentDirection.y != 0)
        {
            if (Input.GetKeyDown(KeyCode.A)) _inputDirection = Vector2Int.left;
            if (Input.GetKeyDown(KeyCode.D)) _inputDirection = Vector2Int.right;
        }
        // 只有在当前方向不是水平方向时，才能接受垂直输入
        if (_currentDirection.x != 0)
        {
            if (Input.GetKeyDown(KeyCode.W)) _inputDirection = Vector2Int.up;
            if (Input.GetKeyDown(KeyCode.S)) _inputDirection = Vector2Int.down;
        }

        // --- 颜色切换输入 ---
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            ChangeColor(true); // true代表下一个颜色
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ChangeColor(false); // false代表上一个颜色
        }
    }

    /// <summary>
    /// 处理蛇的定时移动
    /// </summary>
    private void HandleMovement()
    {
        _moveTimer += Time.deltaTime;
        if (_moveTimer >= moveRate)
        {
            _moveTimer -= moveRate;
            _currentDirection = _inputDirection;

            Vector2Int newHeadPos = _snakeBody[0].GridPosition + _currentDirection;
            newHeadPos = GridManager.Instance.WrapGridPosition(newHeadPos);

            // --- 新增：碰撞检测 ---
            CheckCollisions(newHeadPos);

            // --- 修改：成长逻辑 ---
            // 如果需要成长，我们就不移除蛇尾，身体自然就变长了
            // 同时，我们需要创建一个全新的GameObject作为新的身体部分
            if (_shouldGrow)
            {
                // 在原蛇尾的位置添加一个新节点
                var oldTail = _snakeBody.Last();
                CreateNode(oldTail.GridPosition, oldTail.Color);
                _shouldGrow = false; // 重置成长标记
            }

            // 贪吃蛇移动的核心
            SnakeNode tail = _snakeBody.Last();
            _snakeBody.Remove(tail);
            tail.GridPosition = newHeadPos;
            tail.Color = _currentSnakeColor;
            _snakeBody.Insert(0, tail);

            UpdateSnakeVisuals();
        }
    }

    /// <summary>
    /// 新增：检测所有可能的碰撞
    /// </summary>
    private void CheckCollisions(Vector2Int headPos)
    {
        // 1. 检测是否吃到食物
        if (headPos == _food.GridPosition)
        {
            if (_food.Color == _currentSnakeColor)
            {
                // 颜色正确：标记成长并生成新食物
                _shouldGrow = true;
                SpawnFood();
            }
            else
            {
                // 颜色错误：游戏结束
                GameOver("吃错了颜色！");
            }
        }
        else // 如果没吃到食物，才需要检测是否撞到身体
        {
            // 2. 检测是否撞到自己身体
            // 从第二个节点开始检查（第一个是头）
            for (int i = 1; i < _snakeBody.Count; i++)
            {
                if (headPos == _snakeBody[i].GridPosition)
                {
                    // 如果撞到的身体部分颜色和蛇头颜色相同
                    if (_snakeBody[i].Color == _currentSnakeColor)
                    {
                        GameOver("撞到了同色的身体！");
                    }
                    // 如果颜色不同，则安全穿过，不做任何事
                }
            }
        }
    }

    /// <summary>
    /// 新增：生成食物的逻辑
    /// </summary>
    private void SpawnFood()
    {
        if (_food == null)
        {
            // 如果是第一次生成，就从预制体实例化一个
            GameObject foodGO = Instantiate(foodPrefab);
            _food = foodGO.GetComponent<Food>();
        }

        Vector2Int foodPos;
        // 使用 do-while 循环确保食物不会生成在蛇的身体上
        do
        {
            foodPos = GridManager.Instance.GetRandomGridPosition();
        } while (IsPositionOnSnake(foodPos));

        // 随机选择一个颜色
        SnakeColor foodColor = (SnakeColor)Random.Range(0, 3);

        // 设置食物
        _food.Setup(foodColor, foodPos, _colorMap[foodColor]);
    }

    /// <summary>
    /// 新增：辅助函数，检查某个位置是否在蛇身上
    /// </summary>
    private bool IsPositionOnSnake(Vector2Int position)
    {
        foreach (var node in _snakeBody)
        {
            if (node.GridPosition == position)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 新增：游戏结束逻辑
    /// </summary>
    private void GameOver(string reason)
    {
        Debug.Log($"游戏结束: {reason}");
        this.enabled = false; // 暂停蛇的移动

        // 新增：通知GameManager显示UI
        GameManager.Instance.ShowGameOverUI();
    }

    /// <summary>
    /// 切换蛇的颜色
    /// </summary>
    /// <param name="next">true为下一个颜色，false为上一个</param>
    private void ChangeColor(bool next)
    {
        int colorIndex = (int)_currentSnakeColor;
        int colorCount = System.Enum.GetValues(typeof(SnakeColor)).Length;

        if (next)
        {
            colorIndex = (colorIndex + 1) % colorCount;
        }
        else
        {
            colorIndex = (colorIndex - 1 + colorCount) % colorCount;
        }

        _currentSnakeColor = (SnakeColor)colorIndex;

        // 立刻更新蛇头的视觉颜色
        UpdateSnakeVisuals();
    }

    /// <summary>
    /// 根据_snakeBody列表中的数据，更新场景中所有节点的Transform和颜色
    /// </summary>
    private void UpdateSnakeVisuals()
    {
        for (int i = 0; i < _snakeBody.Count; i++)
        {
            var node = _snakeBody[i];
            // 更新位置
            node.NodeTransform.position = GridManager.Instance.GetWorldPosition(node.GridPosition);
            // 更新颜色
            node.NodeTransform.GetComponent<SpriteRenderer>().color = _colorMap[node.Color];
            // 更新大小（只有蛇头稍大）
            node.NodeTransform.localScale = (i == 0) ? Vector3.one * 1.2f : Vector3.one;
        }
    }

    /// <summary>
    /// 创建一个新的蛇节点（仅在初始化时使用）
    /// </summary>
    private void CreateNode(Vector2Int gridPosition, SnakeColor color)
    {
        GameObject nodeGO = Instantiate(nodePrefab, nodeContainer);
        nodeGO.name = $"Node_{_snakeBody.Count}";

        SnakeNode newNode = new SnakeNode(gridPosition, color, nodeGO.transform);
        _snakeBody.Add(newNode);
    }
}