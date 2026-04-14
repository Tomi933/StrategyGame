using Assets._Project.Code.Infrustructure;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private GameObject _winImage;
    [SerializeField] private GameObject _loseImage;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _homeButton;

    private void Start()
    {
        _panel.SetActive(false);
        _winImage.SetActive(false);
        _loseImage.SetActive(false);
    }

    public void Show(bool playerWon)
    {
        _panel.SetActive(true);
        _winImage.SetActive(playerWon);
        _loseImage.SetActive(!playerWon);

        _restartButton.onClick.AddListener(() =>
            GlobalServices.SceneLoader.LoadScene("Game"));

        _homeButton.onClick.AddListener(() =>
            GlobalServices.SceneLoader.LoadScene("Menu"));
    }

    private void OnDestroy()
    {
        _restartButton.onClick.RemoveAllListeners();
        _homeButton.onClick.RemoveAllListeners();
    }
}