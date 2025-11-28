using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SeaRTS.Core;
using System.Collections.Generic;

namespace SeaRTS.UI
{
    /// <summary>
    /// In-game HUD for displaying game information and unit selection.
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        [Header("Resource Panel")]
        [SerializeField] private TextMeshProUGUI resourceText;

        [Header("Unit Info Panel")]
        [SerializeField] private GameObject unitInfoPanel;
        [SerializeField] private TextMeshProUGUI unitNameText;
        [SerializeField] private TextMeshProUGUI unitHealthText;
        [SerializeField] private Slider unitHealthSlider;

        [Header("Selected Units Panel")]
        [SerializeField] private TextMeshProUGUI selectedUnitsText;

        [Header("Minimap")]
        [SerializeField] private RawImage minimapImage;

        [Header("Action Buttons")]
        [SerializeField] private Button moveButton;
        [SerializeField] private Button attackButton;
        [SerializeField] private Button stopButton;

        private SelectionManager selectionManager;
        private Network.PlayerNetwork localPlayer;

        private void Start()
        {
            selectionManager = SelectionManager.Instance;

            if (selectionManager != null)
            {
                selectionManager.OnSelectionChanged += HandleSelectionChanged;
            }

            SetupActionButtons();
            HideUnitInfo();
        }

        private void OnDestroy()
        {
            if (selectionManager != null)
            {
                selectionManager.OnSelectionChanged -= HandleSelectionChanged;
            }
        }

        private void SetupActionButtons()
        {
            if (stopButton != null)
            {
                stopButton.onClick.AddListener(StopSelectedUnits);
            }
        }

        public void SetLocalPlayer(Network.PlayerNetwork player)
        {
            localPlayer = player;

            if (localPlayer != null)
            {
                localPlayer.OnResourcesChanged += UpdateResources;
                UpdateResources(localPlayer.Resources.Value);
            }
        }

        private void UpdateResources(int amount)
        {
            if (resourceText != null)
            {
                resourceText.text = $"Resources: {amount}";
            }
        }

        private void HandleSelectionChanged(List<Units.UnitBase> selectedUnits)
        {
            if (selectedUnits == null || selectedUnits.Count == 0)
            {
                HideUnitInfo();
                return;
            }

            ShowUnitInfo(selectedUnits);
        }

        private void ShowUnitInfo(List<Units.UnitBase> units)
        {
            if (unitInfoPanel != null)
            {
                unitInfoPanel.SetActive(true);
            }

            if (units.Count == 1)
            {
                // Single unit selected
                Units.UnitBase unit = units[0];

                if (unitNameText != null)
                {
                    unitNameText.text = unit.GetUnitName();
                }

                UpdateUnitHealth(unit);
            }
            else
            {
                // Multiple units selected
                if (unitNameText != null)
                {
                    unitNameText.text = $"{units.Count} units selected";
                }

                if (unitHealthText != null)
                {
                    unitHealthText.text = "";
                }

                if (unitHealthSlider != null)
                {
                    unitHealthSlider.gameObject.SetActive(false);
                }
            }

            if (selectedUnitsText != null)
            {
                selectedUnitsText.text = $"Selected: {units.Count}";
            }
        }

        private void UpdateUnitHealth(Units.UnitBase unit)
        {
            if (unit == null) return;

            float healthPercent = unit.GetHealthPercent();

            if (unitHealthText != null)
            {
                unitHealthText.text = $"HP: {Mathf.RoundToInt(healthPercent * 100)}%";
            }

            if (unitHealthSlider != null)
            {
                unitHealthSlider.gameObject.SetActive(true);
                unitHealthSlider.value = healthPercent;
            }
        }

        private void HideUnitInfo()
        {
            if (unitInfoPanel != null)
            {
                unitInfoPanel.SetActive(false);
            }

            if (selectedUnitsText != null)
            {
                selectedUnitsText.text = "Selected: 0";
            }
        }

        private void StopSelectedUnits()
        {
            if (selectionManager == null) return;

            var selectedUnits = selectionManager.GetSelectedUnits();
            foreach (var unit in selectedUnits)
            {
                // Stop movement by setting target to current position
                unit.MoveToServerRpc(unit.transform.position);
            }
        }

        private void Update()
        {
            // Update health display for selected units
            if (selectionManager != null)
            {
                var selectedUnits = selectionManager.GetSelectedUnits();
                if (selectedUnits.Count == 1)
                {
                    UpdateUnitHealth(selectedUnits[0]);
                }
            }
        }
    }
}
