using Assets._Project.Code.Infrustructure;
using UnityEngine;
using UnityEngine.UI;

public class MusicToggleButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private GameObject _musicOnIcon;
    [SerializeField] private GameObject _musicOffIcon;

    private bool _isMusicOn = true;

    private void Awake()
    {
        _button.onClick.AddListener(Toggle);
        UpdateIcons();
    }

    private void Toggle()
    {
        _isMusicOn = !_isMusicOn;
        GlobalServices.AudioService.SetMusicEnabled(_isMusicOn);
        UpdateIcons();
    }

    private void UpdateIcons()
    {
        _musicOnIcon.SetActive(_isMusicOn);
        _musicOffIcon.SetActive(!_isMusicOn);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveAllListeners();
    }
}