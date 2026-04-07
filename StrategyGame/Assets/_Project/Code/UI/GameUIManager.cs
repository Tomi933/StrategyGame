using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

namespace Assets._Project.Code.UI
{
    public class GameUIManager : MonoBehaviour
    {
        [SerializeField] public List<SpawnUnitButton> spawnUnitButtons = new List<SpawnUnitButton>();
        public GridManager GridManager;

        private SpawnUnitButton _currentSelected;

        private void Start()
        {

            foreach (var item in spawnUnitButtons)
            {
                item.NameText.text = item.UnitConfig.Name;
                item.UnitImage.sprite = item.UnitConfig.Sprite;
                item.UnitCountText.text = item.UnitCount.ToString();
                item.HighlightFrame.gameObject.SetActive(false);

                item.Button.onClick.AddListener(() =>
                {
                    if (_currentSelected != null)
                    {
                        _currentSelected.HighlightFrame.gameObject.SetActive(false);

                        if (_currentSelected == item)
                        {
                            _currentSelected = null;
                            GridManager.UnHighlightPlacementCells();
                        }
                        else
                        {
                            _currentSelected = item;
                            _currentSelected.HighlightFrame.gameObject.SetActive(true);
                            GridManager.HighlightPlacementCells();
                        }
                    }
                    else
                    {
                        _currentSelected = item;
                        item.HighlightFrame.gameObject.SetActive(true);
                        GridManager.HighlightPlacementCells();
                    }
                });
            }
        }

        private void Update()
        {
            if (_currentSelected != null)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                    Collider2D hit = Physics2D.OverlapPoint(mousePosition);

                    if (hit != null)
                    {
                        if (hit.TryGetComponent(out Cell cellScript) && cellScript.isForStartDistribution
                            && !cellScript.isOccupied)
                        {
                            Instantiate(_currentSelected.UnitConfig.Prefab, cellScript.transform);
                            cellScript.isOccupied = true;

                            _currentSelected.UnitCount -= 1;

                            if (_currentSelected.UnitCount <= 0)
                            {
                                spawnUnitButtons.Remove(_currentSelected);
                                Destroy(_currentSelected.Button.gameObject);
                                GridManager.UnHighlightPlacementCells();
                                _currentSelected = null;
                            }
                            else
                                _currentSelected.UnitCountText.text = _currentSelected.UnitCount.ToString();
                        }
                    }
                }
            }
        }
    }
}
