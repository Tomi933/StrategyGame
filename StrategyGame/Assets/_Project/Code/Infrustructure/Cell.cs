using UnityEngine;

public class Cell : MonoBehaviour
{
    public SpriteRenderer BackgroundSpriteRenderer;
    public Color32 HighlightColor;
    public Color32 BaseColor;
    public Color32 AttackColor;
    public Color32 EnemyColor;
    public Color32 ScanColor;
    public Color32 ScanEnemyColor;

    public int x;
    public int y;

    [HideInInspector] public bool isOccupied = false;
    [HideInInspector] public Unit unit;
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
        BackgroundSpriteRenderer.color = EnemyColor;
    }

    public void SetAttackColor()
    {
        BackgroundSpriteRenderer.color = AttackColor;
    }

    public void SetScanColor()
    {
        BackgroundSpriteRenderer.color = ScanColor;
    }

    public void SetScanEnemyColor()
    {
        BackgroundSpriteRenderer.color = ScanEnemyColor;
    }

    public void ResetColor()
    {
        BackgroundSpriteRenderer.color = Color.white;
    }
}