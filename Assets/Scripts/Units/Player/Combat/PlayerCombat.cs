using System.Collections.Generic;
using Misc;
using Settings;
using UI.UserInput;
using Units.Player.Combat.Abilities;
using Units.Player.Targeting;
using UnityEngine;

namespace Units.Player.Combat {
	public class PlayerCombat : MonoBehaviour {
		public enum AbilityQueueReason {
			Cast,
			Range,
		}
		
		private PlayerTargeting PlayerTargeting;
		private readonly MouseStatus MouseStatus = AutowireFactory.GetInstanceOf<MouseStatus>();
		private readonly CommandStatus CommandStatus = AutowireFactory.GetInstanceOf<CommandStatus>();

		private Maybe<QueuedPlayerAbility> QueuedAbility = Maybe<QueuedPlayerAbility>.None;
		private readonly Dictionary<CommandBinding.Command, PlayerAbility> AbilityLibrary = new Dictionary<CommandBinding.Command, PlayerAbility>();
		
		private void Start() {
			PlayerTargeting = GetComponent<PlayerTargeting>();
			AbilityLibrary.Add(CommandBinding.Command.MoveToMouse, new PlayerBasicAttack());
		}

		private void Update() {
			var targetUnit = PlayerTargeting.GetTargetedEnemy();
			var targetPoint = MouseStatus.GetWalkableWorldPoint();
			var abilityToQueue = Maybe<QueuedPlayerAbility>.None;
			foreach (var entry in AbilityLibrary) {
				var command = entry.Key;
				var ability = entry.Value;
				if (!CommandStatus.IsActive(command) || !ability.IsReady()) {
					continue;
				}
				
				if (!IsInRange(ability) && (ability.IsTargetingPoint() && targetPoint.HasValue || ability.IsTargetingUnit() && targetUnit.HasValue)) {
					abilityToQueue = CreateQueuedAbility(ability, AbilityQueueReason.Range);
					Debug.Log("Queueing at range " + ability.GetMaximumRange());
					continue;
				}

				if (ability.IsTargetingSelf()
						|| ability.IsTargetingUnit() && targetUnit.HasValue
					    || ability.IsTargetingPoint() && targetPoint.HasValue) {
					ability.OnCast(targetPoint, targetUnit);
				}
			}

			if (CommandStatus.IsAnyIssuedThisFrame(CommandBinding.Command.MoveToMouse, CommandBinding.Command.ForceMoveToMouse)) {
				QueuedAbility = Maybe<QueuedPlayerAbility>.None;
			}
			
			if (QueuedAbility.HasValue && QueuedAbility.Value.GetReason() == AbilityQueueReason.Range && IsInRange(QueuedAbility.Value.GetAbility())) {
				QueuedAbility.Value.GetAbility().OnCast(QueuedAbility.Value.GetTargetPoint(), QueuedAbility.Value.GetTargetUnit());
				QueuedAbility = Maybe<QueuedPlayerAbility>.None;
			}

			if (abilityToQueue.HasValue) {
				QueuedAbility = abilityToQueue;
			}
		}

		private bool IsInRange(PlayerAbility ability) {
			var target = PlayerTargeting.GetTargetedEnemy();
			var targetPoint = target.HasValue ? Maybe<Vector3>.Some(target.Value.transform.position) : MouseStatus.GetWalkableWorldPoint();

			if (!targetPoint.HasValue) {
				return false;
			}

			return Vector3.Distance(transform.position, targetPoint.Value) <= ability.GetMaximumRange();
		}

		private Maybe<QueuedPlayerAbility> CreateQueuedAbility(PlayerAbility ability, AbilityQueueReason reason) {
			var targetUnit = PlayerTargeting.GetTargetedEnemy();
			if (targetUnit.HasValue) {
				return Maybe<QueuedPlayerAbility>.Some(new QueuedPlayerAbility(ability, targetUnit.Value, reason));
			}
			
			var targetPoint = MouseStatus.GetWalkableWorldPoint();
			if (targetPoint.HasValue) {
				return Maybe<QueuedPlayerAbility>.Some(new QueuedPlayerAbility(ability, targetPoint.Value, reason));
			}
			return Maybe<QueuedPlayerAbility>.None;
		}

		public Maybe<QueuedPlayerAbility> GetQueuedAbility() {
			return QueuedAbility;
		}
	}
}