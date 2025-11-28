using UnityEngine;
using Unity.Netcode;

namespace SeaRTS.Units
{
    /// <summary>
    /// Base class for all units in the Sea-RTS game.
    /// Handles networked unit behavior including movement, selection, and combat.
    /// </summary>
    public class UnitBase : NetworkBehaviour
    {
        [Header("Unit Properties")]
        [SerializeField] protected string unitName = "Unit";
        [SerializeField] protected float moveSpeed = 5f;
        [SerializeField] protected float rotationSpeed = 10f;
        [SerializeField] protected int maxHealth = 100;
        [SerializeField] protected int attackDamage = 10;
        [SerializeField] protected float attackRange = 5f;
        [SerializeField] protected float attackCooldown = 1f;

        [Header("Visual")]
        [SerializeField] protected GameObject selectionIndicator;
        [SerializeField] protected GameObject healthBar;

        // Network Variables
        public NetworkVariable<int> CurrentHealth = new NetworkVariable<int>();
        public NetworkVariable<Vector3> TargetPosition = new NetworkVariable<Vector3>();
        public NetworkVariable<ulong> OwnerPlayerId = new NetworkVariable<ulong>();
        public NetworkVariable<bool> IsSelected = new NetworkVariable<bool>();

        protected bool isMoving;
        protected float lastAttackTime;
        protected UnitBase currentTarget;

        public event System.Action<UnitBase> OnUnitDestroyed;
        public event System.Action<int, int> OnHealthChanged;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {
                CurrentHealth.Value = maxHealth;
                TargetPosition.Value = transform.position;
                OwnerPlayerId.Value = OwnerClientId;
            }

            CurrentHealth.OnValueChanged += HandleHealthChanged;
            IsSelected.OnValueChanged += HandleSelectionChanged;

            UpdateSelectionVisual(IsSelected.Value);
        }

        public override void OnNetworkDespawn()
        {
            CurrentHealth.OnValueChanged -= HandleHealthChanged;
            IsSelected.OnValueChanged -= HandleSelectionChanged;
            base.OnNetworkDespawn();
        }

        protected virtual void Update()
        {
            if (!IsSpawned) return;

            if (IsServer)
            {
                HandleMovement();
            }
        }

        protected virtual void HandleMovement()
        {
            if (!isMoving) return;

            Vector3 direction = (TargetPosition.Value - transform.position).normalized;
            direction.y = 0;

            if (Vector3.Distance(transform.position, TargetPosition.Value) > 0.1f)
            {
                transform.position += direction * moveSpeed * Time.deltaTime;

                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
            }
            else
            {
                isMoving = false;
            }
        }

        /// <summary>
        /// Commands the unit to move to a position.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void MoveToServerRpc(Vector3 position, ServerRpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != OwnerPlayerId.Value) return;

            TargetPosition.Value = position;
            isMoving = true;
            currentTarget = null;
        }

        /// <summary>
        /// Commands the unit to attack a target.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void AttackTargetServerRpc(ulong targetNetworkId, ServerRpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != OwnerPlayerId.Value) return;

            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetNetworkId, out NetworkObject targetObj))
            {
                currentTarget = targetObj.GetComponent<UnitBase>();
            }
        }

        /// <summary>
        /// Applies damage to this unit.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void TakeDamageServerRpc(int damage)
        {
            if (!IsServer) return;

            CurrentHealth.Value = Mathf.Max(0, CurrentHealth.Value - damage);

            if (CurrentHealth.Value <= 0)
            {
                HandleDeath();
            }
        }

        protected virtual void HandleDeath()
        {
            OnUnitDestroyed?.Invoke(this);
            NetworkObject.Despawn();
        }

        private void HandleHealthChanged(int previousValue, int newValue)
        {
            OnHealthChanged?.Invoke(newValue, maxHealth);
            UpdateHealthBar();
        }

        private void HandleSelectionChanged(bool previousValue, bool newValue)
        {
            UpdateSelectionVisual(newValue);
        }

        protected virtual void UpdateSelectionVisual(bool selected)
        {
            if (selectionIndicator != null)
            {
                selectionIndicator.SetActive(selected);
            }
        }

        protected virtual void UpdateHealthBar()
        {
            if (healthBar != null)
            {
                float healthPercent = (float)CurrentHealth.Value / maxHealth;
                healthBar.transform.localScale = new Vector3(healthPercent, 1, 1);
            }
        }

        /// <summary>
        /// Selects or deselects this unit.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void SetSelectedServerRpc(bool selected, ServerRpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != OwnerPlayerId.Value) return;
            IsSelected.Value = selected;
        }

        /// <summary>
        /// Gets the unit's current health percentage.
        /// </summary>
        public float GetHealthPercent()
        {
            return (float)CurrentHealth.Value / maxHealth;
        }

        /// <summary>
        /// Checks if this unit belongs to the specified player.
        /// </summary>
        public bool BelongsToPlayer(ulong playerId)
        {
            return OwnerPlayerId.Value == playerId;
        }

        /// <summary>
        /// Gets the unit's name.
        /// </summary>
        public string GetUnitName()
        {
            return unitName;
        }
    }
}
