using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using Rukhanka;
using Unity.Collections;

partial struct UnitMoverSystem : ISystem {


  public const float REACHED_TARGET_POSITION_DISTANCE_SQ = 1.5f;
  private ComponentLookup<ShootAttack> shootAttackComponentLookup;

  [BurstCompile]
  public void OnCreate(ref SystemState state) {
    shootAttackComponentLookup = state.GetComponentLookup<ShootAttack>();
  }

  [BurstCompile]
  public void OnUpdate(ref SystemState state) {
    shootAttackComponentLookup.Update(ref state);
    UnitMoverJobUpdated unitMoverJobUpdated = new UnitMoverJobUpdated {
      shootAttackComponentLookup = shootAttackComponentLookup,
      deltaTime = SystemAPI.Time.DeltaTime,
    };

    //unitMoverJob.ScheduleParallel();
    unitMoverJobUpdated.ScheduleParallel();
  }

}

[BurstCompile]
public partial struct UnitMoverJobUpdated : IJobEntity {

  [NativeDisableParallelForRestriction] public ComponentLookup<ShootAttack> shootAttackComponentLookup;

  public float deltaTime;

  public void Execute(
    Entity entity,
    ref LocalTransform localTransform,
    ref UnitMover unitMover,
    ref PhysicsVelocity physicsVelocity) {
    float3 moveDirection = unitMover.targetPosition - localTransform.Position;
    float reachedTargetDistanceSq = UnitMoverSystem.REACHED_TARGET_POSITION_DISTANCE_SQ;

    if (math.lengthsq(moveDirection) <= reachedTargetDistanceSq) {
      // Reached the target position
      physicsVelocity.Linear = float3.zero;
      physicsVelocity.Angular = float3.zero;

      unitMover.isMoving = false;
      return;
    }

    // Is moving
    unitMover.isMoving = true;

    // Checks for units with ShootAttack component and reset their aim timer when they move.
    if (shootAttackComponentLookup.HasComponent(entity)) {
      RefRW<ShootAttack> shootAttack = shootAttackComponentLookup.GetRefRW(entity);
      // Condition check if aim timer has not already been resetted.
      if (shootAttack.ValueRO.aimTimer == shootAttack.ValueRO.aimTimerMax) {
        shootAttack.ValueRW.aimTimer = shootAttack.ValueRW.aimTimerMax;
      }
    }

    moveDirection = math.normalize(moveDirection);
    moveDirection.y = 0f;

    localTransform.Rotation =
        math.slerp(localTransform.Rotation,
                    quaternion.LookRotation(moveDirection, math.up()),
                    deltaTime * unitMover.rotationSpeed);

    physicsVelocity.Linear = moveDirection * unitMover.moveSpeed;
    physicsVelocity.Angular = float3.zero;


  }
}