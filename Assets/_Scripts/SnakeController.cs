using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// �����ߵ�������ɫ״̬
public enum SnakeColor { Red, Green, Blue }

// �����ߵ�����ڵ㣬�������������е�λ�ú���ɫ
public class SnakeNode
{
    public Vector2Int GridPosition { get; set; }
    public SnakeColor Color { get; set; }
    public Transform NodeTransform { get; set; } // ��Ӧ����Ϸ�����Transform

    public SnakeNode(Vector2Int gridPosition, SnakeColor color, Transform nodeTransform)
    {
        GridPosition = gridPosition;
        Color = color;
        NodeTransform = nodeTransform;
    }
}

public class SnakeController : MonoBehaviour
{
    [Header("��Ϸ����")]
    [SerializeField, Tooltip("��ÿ�ƶ�һ�������ʱ��")]
    private float moveRate = 0.2f;
    [SerializeField, Tooltip("�ߵĳ�ʼ����")]
    private int initialSize = 4;

    [Header("Ԥ���������")]
    [SerializeField] private GameObject nodePrefab; // ������ڵ��Ԥ����
    [SerializeField] private Transform nodeContainer; // ��������߽ڵ�ĸ�����
    [SerializeField] private GameObject foodPrefab; //ʳ���Ԥ����

    private float _moveTimer; // �ƶ���ʱ��
    private Vector2Int _currentDirection; // ��ǰ�ƶ�����
    private Vector2Int _inputDirection;   // ��ҵ����뷽��

    private List<SnakeNode> _snakeBody; // �洢�����нڵ���б�
    private SnakeColor _currentSnakeColor; // ��ͷ��ǰ����ɫ

    // ��ɫӳ�����ö��ת��ΪUnity��Color
    private Dictionary<SnakeColor, Color> _colorMap;

    private Food _food; // ��ʳ����������
    private bool _shouldGrow = false; // �����һ֡�Ƿ���Ҫ�ɳ�

    void Awake()
    {
        // ��ʼ����ɫӳ��
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
    /// ��ʼ���ߵ�״̬
    /// </summary>
    private void InitializeSnake()
    {
        _snakeBody = new List<SnakeNode>();
        _currentDirection = Vector2Int.right; // ��ʼ�����ƶ�
        _inputDirection = Vector2Int.right;
        _currentSnakeColor = SnakeColor.Red; // ��ʼΪ��ɫ

        // ������ʼ���ȵ�����
        for (int i = 0; i < initialSize; i++)
        {
            // �����ĵ㿪ʼ��������
            Vector2Int startPos = new Vector2Int(10 - i, 9);
            CreateNode(startPos, _currentSnakeColor);
        }

        // ��ͷ�Դ�һ��
        _snakeBody[0].NodeTransform.localScale = Vector3.one * 1.2f;
    }

    /// <summary>
    /// �����������
    /// </summary>
    private void HandleInput()
    {
        // --- �ƶ����� ---
        // ֻ���ڵ�ǰ�����Ǵ�ֱ����ʱ�����ܽ���ˮƽ����
        if (_currentDirection.y != 0)
        {
            if (Input.GetKeyDown(KeyCode.A)) _inputDirection = Vector2Int.left;
            if (Input.GetKeyDown(KeyCode.D)) _inputDirection = Vector2Int.right;
        }
        // ֻ���ڵ�ǰ������ˮƽ����ʱ�����ܽ��ܴ�ֱ����
        if (_currentDirection.x != 0)
        {
            if (Input.GetKeyDown(KeyCode.W)) _inputDirection = Vector2Int.up;
            if (Input.GetKeyDown(KeyCode.S)) _inputDirection = Vector2Int.down;
        }

        // --- ��ɫ�л����� ---
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            ChangeColor(true); // true������һ����ɫ
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ChangeColor(false); // false������һ����ɫ
        }
    }

    /// <summary>
    /// �����ߵĶ�ʱ�ƶ�
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

            // --- ��������ײ��� ---
            CheckCollisions(newHeadPos);

            // --- �޸ģ��ɳ��߼� ---
            // �����Ҫ�ɳ������ǾͲ��Ƴ���β��������Ȼ�ͱ䳤��
            // ͬʱ��������Ҫ����һ��ȫ�µ�GameObject��Ϊ�µ����岿��
            if (_shouldGrow)
            {
                // ��ԭ��β��λ�����һ���½ڵ�
                var oldTail = _snakeBody.Last();
                CreateNode(oldTail.GridPosition, oldTail.Color);
                _shouldGrow = false; // ���óɳ����
            }

            // ̰�����ƶ��ĺ���
            SnakeNode tail = _snakeBody.Last();
            _snakeBody.Remove(tail);
            tail.GridPosition = newHeadPos;
            tail.Color = _currentSnakeColor;
            _snakeBody.Insert(0, tail);

            UpdateSnakeVisuals();
        }
    }

    /// <summary>
    /// ������������п��ܵ���ײ
    /// </summary>
    private void CheckCollisions(Vector2Int headPos)
    {
        // 1. ����Ƿ�Ե�ʳ��
        if (headPos == _food.GridPosition)
        {
            if (_food.Color == _currentSnakeColor)
            {
                // ��ɫ��ȷ����ǳɳ���������ʳ��
                _shouldGrow = true;
                SpawnFood();
            }
            else
            {
                // ��ɫ������Ϸ����
                GameOver("�Դ�����ɫ��");
            }
        }
        else // ���û�Ե�ʳ�����Ҫ����Ƿ�ײ������
        {
            // 2. ����Ƿ�ײ���Լ�����
            // �ӵڶ����ڵ㿪ʼ��飨��һ����ͷ��
            for (int i = 1; i < _snakeBody.Count; i++)
            {
                if (headPos == _snakeBody[i].GridPosition)
                {
                    // ���ײ�������岿����ɫ����ͷ��ɫ��ͬ
                    if (_snakeBody[i].Color == _currentSnakeColor)
                    {
                        GameOver("ײ����ͬɫ�����壡");
                    }
                    // �����ɫ��ͬ����ȫ�����������κ���
                }
            }
        }
    }

    /// <summary>
    /// ����������ʳ����߼�
    /// </summary>
    private void SpawnFood()
    {
        if (_food == null)
        {
            // ����ǵ�һ�����ɣ��ʹ�Ԥ����ʵ����һ��
            GameObject foodGO = Instantiate(foodPrefab);
            _food = foodGO.GetComponent<Food>();
        }

        Vector2Int foodPos;
        // ʹ�� do-while ѭ��ȷ��ʳ�ﲻ���������ߵ�������
        do
        {
            foodPos = GridManager.Instance.GetRandomGridPosition();
        } while (IsPositionOnSnake(foodPos));

        // ���ѡ��һ����ɫ
        SnakeColor foodColor = (SnakeColor)Random.Range(0, 3);

        // ����ʳ��
        _food.Setup(foodColor, foodPos, _colorMap[foodColor]);
    }

    /// <summary>
    /// �������������������ĳ��λ���Ƿ���������
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
    /// ��������Ϸ�����߼�
    /// </summary>
    private void GameOver(string reason)
    {
        Debug.Log($"��Ϸ����: {reason}");
        this.enabled = false; // ��ͣ�ߵ��ƶ�

        // ������֪ͨGameManager��ʾUI
        GameManager.Instance.ShowGameOverUI();
    }

    /// <summary>
    /// �л��ߵ���ɫ
    /// </summary>
    /// <param name="next">trueΪ��һ����ɫ��falseΪ��һ��</param>
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

        // ���̸�����ͷ���Ӿ���ɫ
        UpdateSnakeVisuals();
    }

    /// <summary>
    /// ����_snakeBody�б��е����ݣ����³��������нڵ��Transform����ɫ
    /// </summary>
    private void UpdateSnakeVisuals()
    {
        for (int i = 0; i < _snakeBody.Count; i++)
        {
            var node = _snakeBody[i];
            // ����λ��
            node.NodeTransform.position = GridManager.Instance.GetWorldPosition(node.GridPosition);
            // ������ɫ
            node.NodeTransform.GetComponent<SpriteRenderer>().color = _colorMap[node.Color];
            // ���´�С��ֻ����ͷ�Դ�
            node.NodeTransform.localScale = (i == 0) ? Vector3.one * 1.2f : Vector3.one;
        }
    }

    /// <summary>
    /// ����һ���µ��߽ڵ㣨���ڳ�ʼ��ʱʹ�ã�
    /// </summary>
    private void CreateNode(Vector2Int gridPosition, SnakeColor color)
    {
        GameObject nodeGO = Instantiate(nodePrefab, nodeContainer);
        nodeGO.name = $"Node_{_snakeBody.Count}";

        SnakeNode newNode = new SnakeNode(gridPosition, color, nodeGO.transform);
        _snakeBody.Add(newNode);
    }
}