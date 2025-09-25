using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using JetBrains.Annotations;

partial struct FindTargetSystem : ISystem {
  private ComponentLookup<LocalTransform> localTransformLookup;
  private ComponentLookup<Unit> unitLookup;
  private ComponentLookup<Faction> factionLookup;

  [BurstCompile]
  public void OnCreate(ref SystemState state) {
    localTransformLookup = state.GetComponentLookup<LocalTransform>(true);
    unitLookup = state.GetComponentLookup<Unit>(true);
    factionLookup = state.GetComponentLookup<Faction>(true);
  }

  [BurstCompile]
  public void OnUpdate(ref SystemState state) {
    localTransformLookup.Update(ref state);
    unitLookup.Update(ref state);
    factionLookup.Update(ref state);

    PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
    CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
    float deltaTime = SystemAPI.Time.DeltaTime;

    CollisionFilter collisionFilter = new CollisionFilter {
      BelongsTo = ~0u,
      CollidesWith = 1u << GameAssets.UNITS_LAYER | 1u << GameAssets.BUILDINGS_LAYER,
      GroupIndex = 0,
    };

    FindTargetJob findTargetJob = new FindTargetJob {
      deltaTime = deltaTime,
      localTransformLookup = localTransformLookup,
      unitLookup = unitLookup,
      factionLookup = factionLookup,
      collisionWorld = collisionWorld,
      collisionFilter = collisionFilter,
    };
    findTargetJob.ScheduleParallel();


    //foreach ((
    //  RefRO<LocalTransform> localTransform,
    //  RefRW<FindTarget> findTarget,
    //  RefRW<Target> target,
    //  RefRO<TargetOverride> targetOverride)
    //  in SystemAPI.Query<
    //    RefRO<LocalTransform>,
    //    RefRW<FindTarget>,
    //    RefRW<Target>,
    //    RefRO<TargetOverride>
    //    >()) {

    //  findTarget.ValueRW.timer -= SystemAPI.Time.DeltaTime;
    //  if (findTarget.ValueRO.timer > 0f) {
    //    // Timer not elapsed
    //    continue;
    //  }
    //  findTarget.ValueRW.timer = findTarget.ValueRO.timerMax;

    //  if (targetOverride.ValueRO.targetEntity != Entity.Null) {
    //    target.ValueRW.targetEntity = targetOverride.ValueRO.targetEntity;
    //    continue;
    //  }

    //  distanceHitList.Clear();

    //  Entity closestTargetEntity = Entity.Null;
    //  float closestTargetDistance = float.MaxValue;
    //  float currentTargetDistanceOffset = 0f;
    //  if (target.ValueRO.targetEntity != Entity.Null) {
    //    LocalTransform targetLocalTransform = localTransformLookup[target.ValueRO.targetEntity];

    //    closestTargetEntity = target.ValueRO.targetEntity;
    //    closestTargetDistance = math.distance(localTransform.ValueRO.Position, targetLocalTransform.Position);
    //    currentTargetDistanceOffset = 2f;
    //  }
    //  if (collisionWorld.OverlapSphere(localTransform.ValueRO.Position, findTarget.ValueRO.range, ref distanceHitList, collisionFilter)) {
    //    foreach (DistanceHit distanceHit in distanceHitList) {
    //      if (!unitLookup.HasComponent(distanceHit.Entity)) {
    //        continue;
    //      }
    //      Unit targetUnit = unitLookup[distanceHit.Entity];
    //      if (targetUnit.faction == findTarget.ValueRO.targetFaction) {
    //        // Valid target
    //        if (closestTargetEntity == Entity.Null) {
    //          // No target entity
    //          closestTargetEntity = distanceHit.Entity;
    //          closestTargetDistance = distanceHit.Distance;
    //        } 
    //        else {
    //          // Target entity exists
    //          if (distanceHit.Distance + currentTargetDistanceOffset < closestTargetDistance) {
    //            // Closer entity than current target entity. 
    //            closestTargetEntity = distanceHit.Entity;
    //            closestTargetDistance = distanceHit.Distance;
    //          }
    //        }
    //      }
    //    }
    //  }
    //  if (closestTargetEntity != Entity.Null) {
    //    target.ValueRW.targetEntity = closestTargetEntity;
    //  }
    //}
  }
}


[BurstCompile]
public partial struct FindTargetJob : IJobEntity {
  [ReadOnly] public ComponentLookup<LocalTransform> localTransformLookup;
  [ReadOnly] public ComponentLookup<Unit> unitLookup;
  [ReadOnly] public ComponentLookup<Faction> factionLookup;
  [ReadOnly] public CollisionWorld collisionWorld;
  [ReadOnly] public CollisionFilter collisionFilter;
  public float deltaTime;

  public void Execute(in CharacterStats characterStats, in LocalTransform localTransform, ref FindTarget findTarget, ref Target target, in TargetOverride targetOverride) {
    findTarget.range = characterStats.attackRange;

    findTarget.timer -= deltaTime;
    if (findTarget.timer > 0f) {
      // Timer not elapsed
      return;
    }
    findTarget.timer = findTarget.timerMax;

    if (targetOverride.targetEntity != Entity.Null) {
      target.targetEntity = targetOverride.targetEntity;
      return;
    }

    NativeList<DistanceHit> distanceHitList = new NativeList<DistanceHit>(Allocator.Temp);

    Entity closestTargetEntity = Entity.Null;
    float closestTargetDistanceSq = float.MaxValue;
    float currentTargetDistanceOffset = 0f;
    if (target.targetEntity != Entity.Null && localTransformLookup.HasComponent(target.targetEntity)) {
      LocalTransform targetLocalTransform = localTransformLookup[target.targetEntity];

      closestTargetEntity = target.targetEntity;
      closestTargetDistanceSq = math.distancesq(localTransform.Position, targetLocalTransform.Position);
      currentTargetDistanceOffset = 4f;
    }
    if (collisionWorld.OverlapSphere(localTransform.Position, findTarget.range, ref distanceHitList, collisionFilter)) {
      foreach (DistanceHit distanceHit in distanceHitList) {
        if (!factionLookup.HasComponent(distanceHit.Entity)) {
          continue;
        }
        Faction targetFaction = factionLookup[distanceHit.Entity];
        float distanceHitSq = distanceHit.Distance * distanceHit.Distance;
        if (targetFaction.factionType == findTarget.targetFaction) {
          // Valid target
          if (closestTargetEntity == Entity.Null) {
            // No target entity
            closestTargetEntity = distanceHit.Entity;
            closestTargetDistanceSq = distanceHitSq;
          } else {
            // Target entity exists
            if (distanceHitSq + currentTargetDistanceOffset < closestTargetDistanceSq) {
              // Closer entity than current target entity. 
              closestTargetEntity = distanceHit.Entity;
              closestTargetDistanceSq = distanceHitSq;
            }
          }
        }
      }
    }
    distanceHitList.Dispose();
    if (closestTargetEntity != Entity.Null) {
      target.targetEntity = closestTargetEntity;
    }
  }
}
