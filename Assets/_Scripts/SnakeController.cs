using System.Collections.Generic;
using UnityEngine;

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

    private float _moveTimer; // �ƶ���ʱ��
    private Vector2Int _currentDirection; // ��ǰ�ƶ�����
    private Vector2Int _inputDirection;   // ��ҵ����뷽��

    private List<SnakeNode> _snakeBody; // �洢�����нڵ���б�
    private SnakeColor _currentSnakeColor; // ��ͷ��ǰ����ɫ

    // ��ɫӳ�����ö��ת��ΪUnity��Color
    private Dictionary<SnakeColor, Color> _colorMap;

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

            // ���ƶ�ǰ�������뷽��ͬ������ǰ����
            _currentDirection = _inputDirection;

            // �����µ���ͷλ��
            Vector2Int newHeadPos = _snakeBody[0].GridPosition + _currentDirection;
            // ����߽�ѭ��
            newHeadPos = GridManager.Instance.WrapGridPosition(newHeadPos);

            // --- ̰�����ƶ��ĺ��� ---
            // 1. ȡ����β
            SnakeNode tail = _snakeBody[_snakeBody.Count - 1];
            _snakeBody.Remove(tail);

            // 2. ����β�ƶ����µ���ͷλ�ã���������������
            tail.GridPosition = newHeadPos;
            tail.Color = _currentSnakeColor; // ��ͷ��ʹ�õ�ǰ��ɫ

            // 3. �������뵽�б����ǰ�棬��Ϊ�µ���ͷ
            _snakeBody.Insert(0, tail);

            // --- �������нڵ���Ӿ�Ч�� ---
            UpdateSnakeVisuals();
        }
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