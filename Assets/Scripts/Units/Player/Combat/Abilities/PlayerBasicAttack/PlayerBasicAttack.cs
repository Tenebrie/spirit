using Misc;
using Units.Common.Lightning;
using UnityEngine;

namespace Units.Player.Combat.Abilities.PlayerBasicAttack {
	public class PlayerBasicAttack : PlayerAbility {
		private readonly Assets Assets = AutowireFactory.GetInstanceOf<Assets>();

		public override void OnCast(GameObject caster, Maybe<Vector3> targetPoint, Maybe<GameObject> targetUnit) {
			var sourcePosition = caster.transform.position;
			Vector3 targetPosition;
			if (targetUnit.HasValue) {
				targetPosition = targetUnit.Value.GetComponent<UnitStats>().GetHitTargetPosition();
			} else {
				targetPosition = targetPoint.Value;
				targetPosition.y = sourcePosition.y;
			}

			var builder = new LightningAgent.Builder(sourcePosition, targetPosition)
				.SetAngularDeviation(50f)
				.SetSpeed(1000f)
				.SetMaximumBranchDepth(3)
				.SetFragmentResource(Resource.LightningEffectFragment);
			builder.Create();
			Cooldown.Start(0.7f);
		}

		public override int GetTargetType() {
			return AbilityTargetType.Unit;
		}

		public override float GetMaximumCastRange() {
			return 10f;
		}
	}
}