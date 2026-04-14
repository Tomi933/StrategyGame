using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnIndicator : MonoBehaviour
{
    [SerializeField] private Image _dot;
    [SerializeField] private TextMeshProUGUI _label;

    [SerializeField] private Color _playerColor = Color.green;
    [SerializeField] private Color _botColor = Color.red;

    public void SetTurn(bool isPlayerTurn)
    {
        _dot.color = isPlayerTurn ? _playerColor : _botColor;
        _label.text = isPlayerTurn ? "Ваш хід" : "Хід бота";
        _label.color = isPlayerTurn ? _playerColor : _botColor;
    }
}