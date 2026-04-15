using UnityEngine;
using UnityEngine.UI;

public class DifficultySelector : MonoBehaviour
{
    [SerializeField] private Button _easyButton;
    [SerializeField] private Button _mediumButton;
    [SerializeField] private Button _hardButton;

    [SerializeField] private Color _selectedColor = Color.white;
    [SerializeField] private Color _normalColor = Color.gray;

    private void Awake()
    {
        _easyButton.onClick.AddListener(() => Select(BotDifficulty.Easy));
        _mediumButton.onClick.AddListener(() => Select(BotDifficulty.Medium));
        _hardButton.onClick.AddListener(() => Select(BotDifficulty.Hard));

        Select(BotDifficulty.Easy);
    }

    private void Select(BotDifficulty difficulty)
    {
        PlayerPrefs.SetInt("Difficulty", (int)difficulty);
        PlayerPrefs.Save();

        _easyButton.image.color = difficulty == BotDifficulty.Easy ? _selectedColor : _normalColor;
        _mediumButton.image.color = difficulty == BotDifficulty.Medium ? _selectedColor : _normalColor;
        _hardButton.image.color = difficulty == BotDifficulty.Hard ? _selectedColor : _normalColor;
    }

    private void OnDestroy()
    {
        _easyButton.onClick.RemoveAllListeners();
        _mediumButton.onClick.RemoveAllListeners();
        _hardButton.onClick.RemoveAllListeners();
    }
}