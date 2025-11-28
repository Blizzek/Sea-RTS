using UnityEngine;
using Unity.Netcode;

namespace SeaRTS.Units
{
    /// <summary>
    /// Ship unit class for naval combat in Sea-RTS.
    /// Extends UnitBase with ship-specific functionality.
    /// </summary>
    public class Ship : UnitBase
    {
        [Header("Ship Properties")]
        [SerializeField] private ShipType shipType = ShipType.Scout;
        [SerializeField] private int cargoCapacity = 100;
        [SerializeField] private float repairRate = 1f;

        public NetworkVariable<int> CurrentCargo = new NetworkVariable<int>();
        public NetworkVariable<bool> IsRepairing = new NetworkVariable<bool>();

        public enum ShipType
        {
            Scout,
            Fighter,
            Destroyer,
            Carrier,
            Battleship
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {
                CurrentCargo.Value = 0;
                IsRepairing.Value = false;
            }
        }

        protected override void Update()
        {
            base.Update();

            if (IsServer && IsRepairing.Value)
            {
                HandleRepair();
            }
        }

        private void HandleRepair()
        {
            int maxHealthValue = GetMaxHealth();
            if (CurrentHealth.Value < maxHealthValue)
            {
                int healAmount = Mathf.CeilToInt(repairRate * Time.deltaTime);
                CurrentHealth.Value = Mathf.Min(maxHealthValue, CurrentHealth.Value + healAmount);
            }
            else
            {
                IsRepairing.Value = false;
            }
        }

        /// <summary>
        /// Starts repairing the ship.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void StartRepairServerRpc(ServerRpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != OwnerPlayerId.Value) return;
            IsRepairing.Value = true;
        }

        /// <summary>
        /// Stops repairing the ship.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void StopRepairServerRpc(ServerRpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != OwnerPlayerId.Value) return;
            IsRepairing.Value = false;
        }

        /// <summary>
        /// Adds cargo to the ship.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void AddCargoServerRpc(int amount, ServerRpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != OwnerPlayerId.Value) return;
            CurrentCargo.Value = Mathf.Min(cargoCapacity, CurrentCargo.Value + amount);
        }

        /// <summary>
        /// Unloads cargo from the ship.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void UnloadCargoServerRpc(int amount, ServerRpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != OwnerPlayerId.Value) return;
            int unloadAmount = Mathf.Min(amount, CurrentCargo.Value);
            CurrentCargo.Value -= unloadAmount;
        }

        /// <summary>
        /// Gets the ship type.
        /// </summary>
        public ShipType GetShipType()
        {
            return shipType;
        }

        /// <summary>
        /// Gets the cargo capacity.
        /// </summary>
        public int GetCargoCapacity()
        {
            return cargoCapacity;
        }

        /// <summary>
        /// Gets the current cargo amount.
        /// </summary>
        public int GetCurrentCargo()
        {
            return CurrentCargo.Value;
        }
    }
}
