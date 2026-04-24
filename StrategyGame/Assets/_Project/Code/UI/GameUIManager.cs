using Assets._Project.Code.Configs.Units;
using Assets._Project.Code.Infrustructure;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._Project.Code.UI
{
    public class GameUIManager : MonoBehaviour
    {
        [SerializeField] public List<SpawnUnitButton> spawnUnitButtons = new List<SpawnUnitButton>();
        [SerializeField] private List<ActionButton> actionButtons;
        [SerializeField] private GameObject modesPanel;
        [SerializeField] private TurnIndicator turnIndicator;

        private SpawnUnitButton _currentSelected;
        private GridManager _gridManager;

        private Unit _selectedUnit;
        private List<Cell> _availableCells = new List<Cell>();

        private bool _isPlacementPhase = true;
        private ActionMode _mode;


        private bool _isPlayerPerformAction = false;

        private bool _actionLocked = false;

        private bool _isPlayerTurn = true;
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

        public void ClearPlayerPerformAction()
        {
            _isPlayerPerformAction = false;
            _actionLocked = false;
        }

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
            if (_actionLocked) return;
            if (!_isPlayerTurn) return;

            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                Collider2D[] hits = Physics2D.OverlapPointAll(mousePosition);

                Unit unit = null;
                Cell cell = null;

                foreach (var h in hits)
                {
                    if (unit == null && h.TryGetComponent(out Unit u))
                        unit = u;

                    if (cell == null && h.TryGetComponent(out Cell c))
                        cell = c;
                }

                // --- ПРІОРИТЕТ ЮНІТА ---
                if (unit != null)
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
                    {
                        if (_selectedUnit != null && unit.team != _selectedUnit.team)
                        {
                            if (_availableCells.Contains(unit.currentCell))
                            {
                                _selectedUnit.Attack(unit);
                                
                                _isPlayerPerformAction = true;
                                _actionLocked = true;
                                ClearSelection();
                            }
                            return;
                        }

                        SelectUnitForAttack(unit);
                    }

                    if (_mode == ActionMode.Scan)
                        SelectUnitForScan(unit);

                    return;
                }

                // --- ТЕПЕР CELL ---
                if (cell != null)
                {
                    if (_selectedUnit == null) return;

                    bool actionPerformed = false;

                    if (_mode == ActionMode.Move)
                    {
                        if (_availableCells.Contains(cell))
                        {
                            _selectedUnit.MoveTo(cell);

                            List<Unit> all = new List<Unit>(FindObjectsByType<Unit>(FindObjectsSortMode.None));
                            UpdateEnemyVisibility(_selectedUnit, all, _gridManager);

                            GlobalServices.AudioService.PlayClip("MoveUnit");

                            actionPerformed = true;
                        }
                    }

                    if (_mode == ActionMode.Attack)
                    {
                        Unit target = cell.GetComponentInChildren<Unit>();

                        if (target != null && _availableCells.Contains(cell))
                        {
                            _selectedUnit.Attack(target);
                        
                            actionPerformed = true;
                        }
                    }

                    if (actionPerformed)
                    {
                        _isPlayerPerformAction = true;
                        _actionLocked = true;
                    }

                    ClearSelection();
                    return;
                }

                // нічого не клікнули
                ClearSelection();
            }
        }

        private void UpdateEnemyVisibility(Unit playerUnit, List<Unit> allUnits, GridManager grid)
        {
            Debug.Log($"playerUnit: {playerUnit.name}, scanRange: {playerUnit.Config.scanRange}, scanType: {playerUnit.Config.ScanType}");
            Debug.Log($"currentCell: {playerUnit.currentCell}, x:{playerUnit.currentCell?.x} y:{playerUnit.currentCell?.y}");

            List<Cell> scanCells = playerUnit.GetScanCells(grid);
            Debug.Log($"scanCells count: {scanCells.Count}");

            foreach (var unit in allUnits)
            {
                if (unit.team == playerUnit.team) continue;
                if (unit.currentCell == null) continue;
                Debug.Log($"enemy: {unit.name}, currentCell: {unit.currentCell}, visible: {scanCells.Contains(unit.currentCell)}");
                unit.SetModelVisible(scanCells.Contains(unit.currentCell));
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


            // збираємо всі видимі клітинки від усіх гравцівських юнітів
            var all = FindObjectsByType<Unit>(FindObjectsSortMode.None);
            HashSet<Cell> visibleCells = new HashSet<Cell>();
            foreach (var u in all)
            {
                if (u.team != Team.Player) continue;
                foreach (var cell in u.GetScanCells(_gridManager))
                    visibleCells.Add(cell);
            }

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
                    if (visibleCells.Contains(cell))
                        cell.SetEnemyColor();
                    else
                        cell.SetAttackColor();
                }
                else
                {
                    cell.SetAttackColor();
                }
            }
        }

        private void SelectUnitForScan(Unit unit)
        {
            ClearSelection();

            _selectedUnit = unit;
            _availableCells = unit.GetScanCells(_gridManager);

            foreach (var cell in _availableCells)
            {
                Unit target = cell.GetComponentInChildren<Unit>();

                if (target == null)
                {
                    cell.SetScanColor();
                    continue;
                }

                if (target == _selectedUnit)
                {
                    cell.SetScanColor();
                    continue;
                }

                if (target.team != _selectedUnit.team)
                {
                    cell.SetScanEnemyColor();
                }
                else
                {
                    cell.SetScanColor();
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

        public void RefreshEnemyVisibility()
        {
            List<Unit> all = new List<Unit>(FindObjectsByType<Unit>(FindObjectsSortMode.None));
            HashSet<Cell> visibleCells = new HashSet<Cell>();

            foreach (var unit in all)
            {
                if (unit.team != Team.Player) continue;
                foreach (var cell in unit.GetScanCells(_gridManager))
                    visibleCells.Add(cell);
            }

            
            foreach (var unit in all)
            {
                if (unit.team == Team.Player) continue;
                if (unit.currentCell == null) continue;
                unit.SetModelVisible(visibleCells.Contains(unit.currentCell));
            }
        }

        public void SetPlayerTurn(bool isPlayerTurn)
        {
            _isPlayerTurn = isPlayerTurn;
            turnIndicator.SetTurn(isPlayerTurn);
        }
    }
}


