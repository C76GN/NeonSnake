using UnityEngine;

public class Food : MonoBehaviour
{
    public SnakeColor Color { get; private set; }
    public Vector2Int GridPosition { get; private set; }

    /// <summary>
    /// 设置食物的新状态并更新其视觉效果
    /// </summary>
    public void Setup(SnakeColor color, Vector2Int gridPosition, Color unityColor)
    {
        Color = color;
        GridPosition = gridPosition;
        transform.position = GridManager.Instance.GetWorldPosition(gridPosition);
        GetComponent<SpriteRenderer>().color = unityColor;
    }
}