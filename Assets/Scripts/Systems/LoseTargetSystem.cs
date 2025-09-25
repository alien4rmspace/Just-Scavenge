using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct LoseTargetSystem : ISystem {
  [BurstCompile]
  public void OnUpdate(ref SystemState state) {
    foreach ((
      RefRO<LocalTransform> localTransform,
      RefRW<Target> target,
      RefRW<LoseTarget> loseTarget,
      RefRO<TargetOverride> targetOverride,
      RefRO<CharacterStats> characterStats)
      in SystemAPI.Query<
        RefRO<LocalTransform>,
        RefRW<Target>,
        RefRW<LoseTarget>,
        RefRO<TargetOverride>,
        RefRO<CharacterStats>
        >()) {

      if (target.ValueRO.targetEntity == Entity.Null) {
        continue;
      }

      if (targetOverride.ValueRO.targetEntity != Entity.Null) {
        continue;
      }

      LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.targetEntity);
      float targetDistance = math.distance(localTransform.ValueRO.Position, targetLocalTransform.Position);
      loseTarget.ValueRW.loseTargetDistance = characterStats.ValueRO.attackRange;
      if (targetDistance > loseTarget.ValueRO.loseTargetDistance) {
        // Target is too far. Reset.
        target.ValueRW.targetEntity = Entity.Null;
      }
    }
  }

}
