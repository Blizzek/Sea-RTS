using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

namespace SeaRTS.Core
{
    /// <summary>
    /// Handles unit selection and command input for the local player.
    /// </summary>
    public class SelectionManager : NetworkBehaviour
    {
        public static SelectionManager Instance { get; private set; }

        [Header("Selection Settings")]
        [SerializeField] private LayerMask unitLayerMask;
        [SerializeField] private LayerMask groundLayerMask;
        [SerializeField] private RectTransform selectionBox;

        private List<Units.UnitBase> selectedUnits = new List<Units.UnitBase>();
        private Vector3 selectionStartPos;
        private bool isSelecting;

        private Camera mainCamera;
        private ulong localPlayerId;

        public event System.Action<List<Units.UnitBase>> OnSelectionChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            mainCamera = Camera.main;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            localPlayerId = NetworkManager.Singleton.LocalClientId;
        }

        private void Update()
        {
            if (!IsSpawned || mainCamera == null) return;

            HandleInput();
        }

        private void HandleInput()
        {
            // Left click - start selection
            if (Input.GetMouseButtonDown(0))
            {
                StartSelection();
            }

            // Left click held - update selection box
            if (Input.GetMouseButton(0) && isSelecting)
            {
                UpdateSelectionBox();
            }

            // Left click released - finish selection
            if (Input.GetMouseButtonUp(0) && isSelecting)
            {
                FinishSelection();
            }

            // Right click - issue command
            if (Input.GetMouseButtonDown(1))
            {
                IssueCommand();
            }
        }

        private void StartSelection()
        {
            isSelecting = true;
            selectionStartPos = Input.mousePosition;

            if (selectionBox != null)
            {
                selectionBox.gameObject.SetActive(true);
            }
        }

        private void UpdateSelectionBox()
        {
            if (selectionBox == null) return;

            Vector3 currentPos = Input.mousePosition;
            float width = currentPos.x - selectionStartPos.x;
            float height = currentPos.y - selectionStartPos.y;

            selectionBox.anchoredPosition = selectionStartPos + new Vector3(width / 2, height / 2, 0);
            selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));
        }

        private void FinishSelection()
        {
            isSelecting = false;

            if (selectionBox != null)
            {
                selectionBox.gameObject.SetActive(false);
            }

            // Check if it's a single click or drag selection
            float dragDistance = Vector3.Distance(selectionStartPos, Input.mousePosition);

            if (dragDistance < 10f)
            {
                // Single click selection
                SelectSingleUnit();
            }
            else
            {
                // Box selection
                SelectUnitsInBox();
            }

            OnSelectionChanged?.Invoke(selectedUnits);
        }

        private void SelectSingleUnit()
        {
            ClearSelection();

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, unitLayerMask))
            {
                Units.UnitBase unit = hit.collider.GetComponent<Units.UnitBase>();
                if (unit != null && unit.BelongsToPlayer(localPlayerId))
                {
                    SelectUnit(unit);
                }
            }
        }

        private void SelectUnitsInBox()
        {
            ClearSelection();

            Vector3 min = Vector3.Min(selectionStartPos, Input.mousePosition);
            Vector3 max = Vector3.Max(selectionStartPos, Input.mousePosition);

            foreach (var unit in FindObjectsOfType<Units.UnitBase>())
            {
                if (!unit.BelongsToPlayer(localPlayerId)) continue;

                Vector3 screenPos = mainCamera.WorldToScreenPoint(unit.transform.position);

                if (screenPos.x >= min.x && screenPos.x <= max.x &&
                    screenPos.y >= min.y && screenPos.y <= max.y)
                {
                    SelectUnit(unit);
                }
            }
        }

        private void SelectUnit(Units.UnitBase unit)
        {
            selectedUnits.Add(unit);
            unit.SetSelectedServerRpc(true);
        }

        private void ClearSelection()
        {
            foreach (var unit in selectedUnits)
            {
                if (unit != null)
                {
                    unit.SetSelectedServerRpc(false);
                }
            }
            selectedUnits.Clear();
        }

        private void IssueCommand()
        {
            if (selectedUnits.Count == 0) return;

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            // Check if clicked on enemy unit
            if (Physics.Raycast(ray, out RaycastHit unitHit, 1000f, unitLayerMask))
            {
                Units.UnitBase targetUnit = unitHit.collider.GetComponent<Units.UnitBase>();
                if (targetUnit != null && !targetUnit.BelongsToPlayer(localPlayerId))
                {
                    // Attack command
                    foreach (var unit in selectedUnits)
                    {
                        unit.AttackTargetServerRpc(targetUnit.NetworkObjectId);
                    }
                    return;
                }
            }

            // Move command
            if (Physics.Raycast(ray, out RaycastHit groundHit, 1000f, groundLayerMask))
            {
                Vector3 targetPosition = groundHit.point;

                // Simple formation: offset units
                for (int i = 0; i < selectedUnits.Count; i++)
                {
                    float offsetX = (i % 5) * 2f;
                    float offsetZ = (i / 5) * 2f;
                    Vector3 offset = new Vector3(offsetX, 0, offsetZ);

                    selectedUnits[i].MoveToServerRpc(targetPosition + offset);
                }
            }
        }

        /// <summary>
        /// Gets the currently selected units.
        /// </summary>
        public List<Units.UnitBase> GetSelectedUnits()
        {
            return new List<Units.UnitBase>(selectedUnits);
        }

        /// <summary>
        /// Clears all selected units.
        /// </summary>
        public void DeselectAll()
        {
            ClearSelection();
            OnSelectionChanged?.Invoke(selectedUnits);
        }
    }
}
