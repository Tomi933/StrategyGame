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


        private Unit _selectedUnit;
        private List<Cell> _availableCells = new List<Cell>();


        private bool _isPlacementPhase = true;

        [SerializeField] private List<ActionButton> actionButtons;
        private ActionMode _mode;

        [SerializeField] private GameObject modesPanel;


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

            foreach (var btn in actionButtons)
            {
                btn.Init(this);
                btn.SetActive(false);
            }

            modesPanel.SetActive(false);

        }

        private void Update()
        {
            // щоб не клікати крізь UI
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return;

            HandleUnitPlacement(); // твоя стара логіка (спавн)
            HandleUnitMovement();  // нова логіка (рух)
        }

        void HandleUnitPlacement()
        {
            if (!_isPlacementPhase) return;
            if (_currentSelected == null) return;

            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Collider2D hit = Physics2D.OverlapPoint(mousePosition);

                if (hit != null)
                {
                    if (hit.TryGetComponent(out Cell cellScript) && cellScript.isForStartDistribution
                        && !cellScript.isOccupied)
                    {
                        var unitGO = Instantiate(_currentSelected.UnitConfig.Prefab, cellScript.transform);

                        var unit = unitGO.GetComponent<Unit>();
                        unit.Init(cellScript);

                        cellScript.isOccupied = true;

                        _currentSelected.UnitCount -= 1;

                        if (_currentSelected.UnitCount <= 0)
                        {
                            spawnUnitButtons.Remove(_currentSelected);
                            Destroy(_currentSelected.Button.gameObject);
                            GridManager.UnHighlightPlacementCells();
                            _currentSelected = null;

                            if (spawnUnitButtons.Count == 0)
                            {
                                EndPlacementPhase();
                            }
                        }
                        else
                            _currentSelected.UnitCountText.text = _currentSelected.UnitCount.ToString();
                    }
                }
            }
        }

        void HandleUnitMovement()
        {
            if (_isPlacementPhase) return;

            if (_currentSelected != null) return;

            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Collider2D hit = Physics2D.OverlapPoint(mousePosition);
                
                // Клік в пустоту
                if (hit == null)
                {
                    ClearSelection();
                    return;
                }

                // клік по юніту
                if (hit.TryGetComponent(out Unit unit))
                {
                    if (_mode == ActionMode.Move)
                        SelectUnit(unit);
                    return;
                }
                // клік по клітинці
                if (hit.TryGetComponent(out Cell cell))
                {
                    if (_selectedUnit == null) return;

                    if (_availableCells.Contains(cell))
                    {
                        _selectedUnit.MoveTo(cell);
                    }

                    ClearSelection();
                }
            }
        }

        void SelectUnit(Unit unit)
        {
            ClearSelection();

            _selectedUnit = unit;
            _availableCells = unit.GetAvailableCells(GridManager);

            foreach (var cell in _availableCells)
                cell.SetMoveColor();
        }

        void ClearSelection()
        {
            foreach (var cell in _availableCells)
                cell.SetBaseColor();

            _availableCells.Clear();
            _selectedUnit = null;
        }
        void EndPlacementPhase()
        {
            _isPlacementPhase = false;

            GridManager.UnHighlightPlacementCells();
            ClearSelection();

            modesPanel.SetActive(true);

            foreach (var btn in actionButtons)
                btn.SetActive(true);

            SetMode(ActionMode.Move);
        }
        public void SetMode(ActionMode mode)
        {
            _mode = mode;
            ClearSelection();

            foreach (var btn in actionButtons)
                btn.SetHighlight(btn.Mode == mode);
        }




    }
}


