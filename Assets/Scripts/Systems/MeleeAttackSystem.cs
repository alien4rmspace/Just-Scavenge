using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

partial struct MeleeAttackSystem : ISystem {
  [BurstCompile]
  public void OnCreate(ref SystemState state)
  {
    state.RequireForUpdate<PhysicsWorldSingleton>();
  }

  [BurstCompile]
  public void OnUpdate(ref SystemState state) {
    PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
    CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
    NativeList<RaycastHit> raycastHitList = new NativeList<RaycastHit>(Allocator.Temp);

    foreach ((
       RefRO<LocalTransform> localTransform,
       RefRW<MeleeAttack> meleeAttack,
       RefRW<Target> target,
       RefRO<CharacterStats> characterStats,
       RefRW<UnitMover> unitMover)
       in SystemAPI.Query<
           RefRO<LocalTransform>,
           RefRW<MeleeAttack>,
           RefRW<Target>,
           RefRO<CharacterStats>,
           RefRW<UnitMover>>().WithDisabled<MoveOverride>()) {

      if (target.ValueRO.targetEntity == Entity.Null) {
        continue;
      }

      LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.targetEntity);
      float meleeAttackDistanceSq = 4.5f;
      bool isCloseEnoughToAttack = math.distancesq(localTransform.ValueRO.Position, targetLocalTransform.Position) < meleeAttackDistanceSq;

      bool isTouchingTarget = false;
      if (!isCloseEnoughToAttack) {
        float3 dirToTarget = targetLocalTransform.Position - localTransform.ValueRO.Position;
        dirToTarget = math.normalize(dirToTarget);
        float distanceExtraToTestRaycast = 1f;
        RaycastInput raycastInput = new RaycastInput {
          Start = localTransform.ValueRO.Position,
          End = localTransform.ValueRO.Position + dirToTarget * (meleeAttack.ValueRO.colliderSize + distanceExtraToTestRaycast),
          Filter = CollisionFilter.Default,
        };
        raycastHitList.Clear();
        if (collisionWorld.CastRay(raycastInput, ref raycastHitList)) {
          foreach (RaycastHit raycastHit in raycastHitList) {
            if (raycastHit.Entity == target.ValueRO.targetEntity) {
              // Raycast hit target, close enough to attack this entity
              isTouchingTarget = true;
              break;
            }
          }
        }
      }

      if (!isCloseEnoughToAttack && !isTouchingTarget) {
        // Target is too far
        if (!meleeAttack.ValueRO.isMelee) {
          meleeAttack.ValueRW.isMelee = false;
        }
        unitMover.ValueRW.targetPosition = targetLocalTransform.Position;
      } else {
        // Target is close enough to attack
        meleeAttack.ValueRW.isMelee = true;

        unitMover.ValueRW.targetPosition = localTransform.ValueRO.Position;

        meleeAttack.ValueRW.timer -= SystemAPI.Time.DeltaTime;
        if (meleeAttack.ValueRO.timer >= 0) {
          continue;
        }
        meleeAttack.ValueRW.timer = meleeAttack.ValueRO.timerMax;

        RefRW<Health> targetHealth = SystemAPI.GetComponentRW<Health>(target.ValueRW.targetEntity);
        targetHealth.ValueRW.healthAmount -= characterStats.ValueRO.attackDamage;
        targetHealth.ValueRW.onHealthChanged = true;
      }
    }
  }



}