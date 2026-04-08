using UnityEngine;

public class Cell : MonoBehaviour
{
    public SpriteRenderer BackgroundSpriteRenderer;
    public Color32 HighlightColor;
    public Color32 BaseColor;

    public int x;
    public int y;

    public bool isOccupied = false;
    public bool isForStartDistribution = false;

    public void SetMoveColor()
    {
        BackgroundSpriteRenderer.color = HighlightColor;
    }

    public void SetBaseColor()
    {
        BackgroundSpriteRenderer.color = BaseColor;
    }

    public void SetEnemyColor()
    {
        BackgroundSpriteRenderer.color = Color.red;
    }

    public void SetAttackColor()
    {
        BackgroundSpriteRenderer.color = Color.yellow;
    }

    public void ResetColor()
    {
        BackgroundSpriteRenderer.color = Color.white;
    }
}