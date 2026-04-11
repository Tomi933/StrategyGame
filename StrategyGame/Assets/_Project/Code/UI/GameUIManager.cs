using Assets._Project.Code.Configs.Units;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

namespace Assets._Project.Code.UI
{
    public class GameUIManager : MonoBehaviour
    {
        [SerializeField] public List<SpawnUnitButton> spawnUnitButtons = new List<SpawnUnitButton>();
        [SerializeField] private List<ActionButton> actionButtons;
        [SerializeField] private GameObject modesPanel;

        private SpawnUnitButton _currentSelected;
        private GridManager _gridManager;

        private Unit _selectedUnit;
        private List<Cell> _availableCells = new List<Cell>();

        private bool _isPlacementPhase = true;
        private ActionMode _mode;


        private bool _isPlayerPerformAction = false;

        public bool IsPlacemantEnded => !_isPlacementPhase;
        public bool IsPlayerPerformAction => _isPlayerPerformAction;

        public void Init(GridManager gridManager)
        {
            _gridManager = gridManager;

            ClearPlayerPerformAction();
            PrepareUnitSpawnButtons();
        }


        private void Update()
        {
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return;

            HandleUnitPlacement();
            HandleUnitAction();
        }

        public void ClearPlayerPerformAction() =>
            _isPlayerPerformAction = false;

        private void PrepareUnitSpawnButtons()
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
                            _gridManager.UnHighlightPlacementCells();
                        }
                        else
                        {
                            _currentSelected = item;
                            _currentSelected.HighlightFrame.gameObject.SetActive(true);
                            _gridManager.HighlightPlacementCells();
                        }
                    }
                    else
                    {
                        _currentSelected = item;
                        item.HighlightFrame.gameObject.SetActive(true);
                        _gridManager.HighlightPlacementCells();
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


        private void HandleUnitPlacement()
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
                        unit.Init(cellScript, _currentSelected.UnitConfig);

                        cellScript.isOccupied = true;

                        _currentSelected.UnitCount -= 1;

                        if (_currentSelected.UnitCount <= 0)
                        {
                            spawnUnitButtons.Remove(_currentSelected);
                            Destroy(_currentSelected.Button.gameObject);
                            _gridManager.UnHighlightPlacementCells();
                            _currentSelected = null;

                            if (spawnUnitButtons.Count == 0)
                                EndPlacementPhase();
                        }
                        else
                            _currentSelected.UnitCountText.text = _currentSelected.UnitCount.ToString();
                    }
                }
            }
        }

        private void HandleUnitAction()
        {
            if (_isPlacementPhase) return;

            if (_currentSelected != null) return;

            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Collider2D hit = Physics2D.OverlapPoint(mousePosition);
                
                if (hit == null)
                {
                    ClearSelection();
                    return;
                }

                if (hit.TryGetComponent(out Unit unit))
                {
                    if (_mode == ActionMode.Move)
                    {
                        if (unit.Config.Behavior == UnitBehavior.Static)
                        {
                            ClearSelection();
                            return;
                        }

                        SelectUnitForMove(unit);
                    }
                    if (_mode == ActionMode.Attack)
                        SelectUnitForAttack(unit);
                    return;
                }

                if (hit.TryGetComponent(out Cell cell))
                {
                    if (_selectedUnit == null) return;

                    if (_mode == ActionMode.Move)
                    {
                        if (_availableCells.Contains(cell))
                            _selectedUnit.MoveTo(cell);
                    }

                    if (_mode == ActionMode.Attack)
                    {
                        Unit target = cell.GetComponentInChildren<Unit>();

                        if (target != null)
                            _selectedUnit.Attack(target);
                    }

                    _isPlayerPerformAction = true;

                    ClearSelection();
                }
            }
        }

        private void SelectUnitForMove(Unit unit)
        {
            ClearSelection();

            _selectedUnit = unit;
            _availableCells = unit.FindAvailableCellsForMove(_gridManager);

            foreach (var cell in _availableCells)
                cell.SetMoveColor();
        }

        private void SelectUnitForAttack(Unit unit)
        {
            ClearSelection();

            _selectedUnit = unit;
            _availableCells = unit.GetAttackCells(_gridManager, unit.team);

            foreach (var cell in _availableCells)
            {
                Unit target = cell.GetComponentInChildren<Unit>();

                if (target == null)
                {
                    cell.SetAttackColor();
                    continue;
                }

                if (target == _selectedUnit)
                {
                    cell.SetAttackColor();
                    continue;
                }

                if (target.team != _selectedUnit.team)
                {
                    cell.SetEnemyColor();
                }
                else
                {
                    cell.SetAttackColor();
                }
            }
        }

        private void ClearSelection()
        {
            foreach (var cell in _availableCells)
                cell.SetBaseColor();

            _availableCells.Clear();
            _selectedUnit = null;
        }
        private void EndPlacementPhase()
        {
            _isPlacementPhase = false;

            _gridManager.UnHighlightPlacementCells();
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

        //public void EnablePlayerControls(bool enabled)
        //{
        //    EnableActions(enabled);
        //    _canClickUnits = enabled;
        //}

        private void EnableActions(bool enabled)
        {
            foreach (var btn in actionButtons)
                btn.SetActive(enabled);
        }
    }
}


