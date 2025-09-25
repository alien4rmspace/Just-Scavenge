using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Collections;
using UnityEngine;
using System.Security.Principal;
using System;
using JetBrains.Annotations;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSystemGroup))]

partial struct RaycastBulletMoverSystem : ISystem {
  ComponentLookup<CharacterStats> characterStatsLookup;
  ComponentLookup<Health> healthLookup;
  ComponentLookup<Target> targetLookup;
  ComponentLookup<Zombie> zombieLookup;
  ComponentLookup<Faction> factionLookup;
  ComponentLookup<RandomWalking> randomWalkingLookup;
  ComponentLookup<LocalTransform> localTransformLookup;

  [BurstCompile]
  public void OnCreate(ref SystemState state) {
    characterStatsLookup = state.GetComponentLookup<CharacterStats>();
    healthLookup = state.GetComponentLookup<Health>();
    targetLookup = state.GetComponentLookup<Target>(true);
    zombieLookup = state.GetComponentLookup<Zombie>(true);
    factionLookup = state.GetComponentLookup<Faction>(true);
    randomWalkingLookup = state.GetComponentLookup<RandomWalking>();
    localTransformLookup = state.GetComponentLookup<LocalTransform>(true);

    state.RequireForUpdate<PhysicsWorldSingleton>();
    state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
  }

  [BurstCompile]
  public void OnUpdate(ref SystemState state) {
    characterStatsLookup.Update(ref state);
    healthLookup.Update(ref state);
    targetLookup.Update(ref state);
    zombieLookup.Update(ref state);
    factionLookup.Update(ref state);
    randomWalkingLookup.Update(ref state);
    localTransformLookup.Update(ref state);

    PhysicsWorld physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;
    CollisionWorld collisionWorld = physicsWorld.CollisionWorld;
    float deltaTime = SystemAPI.Time.DeltaTime;

    EntityCommandBuffer entityCommandBuffer =
      SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

    //CollisionFilter for raycast to hit
    CollisionFilter collisionFilter = new CollisionFilter() {
      BelongsTo = ~0u,
      CollidesWith = 1u << GameAssets.UNITS_LAYER | 1u << GameAssets.BUILDINGS_LAYER,
      GroupIndex = 0
    };

    foreach ((
      RefRW<LocalTransform> localTransform,
      RefRW<Bullet> bullet,
      RefRO<Target> target,
      Entity entity)
      in SystemAPI.Query<
        RefRW<LocalTransform>,
        RefRW<Bullet>,
        RefRO<Target>>().WithEntityAccess()) {

      float3 currentPosition = localTransform.ValueRO.Position;
      float3 moveDirection = bullet.ValueRO.moveDirection;
      float moveDistance = bullet.ValueRO.speed * deltaTime;
      float3 targetPosition = currentPosition + moveDirection * moveDistance;

      // Check travel distance for bullet.
      bullet.ValueRW.travelDistance += moveDistance;
      float bulletTravelDistance = bullet.ValueRW.travelDistance + moveDistance;

      if (bulletTravelDistance > bullet.ValueRW.maxTravelDistance) {
        entityCommandBuffer.DestroyEntity(entity);
        continue;
      }

      // Preparing Raycast
      RaycastInput rayInput = new RaycastInput {
        Start = currentPosition,
        End = targetPosition,
        Filter = collisionFilter //hit units only
      };

      if (collisionWorld.CastRay(rayInput, out Unity.Physics.RaycastHit hit)) {
        // Hit something
        Entity entityHit = physicsWorld.Bodies[hit.RigidBodyIndex].Entity;

        // Check if enemy
        if (factionLookup.HasComponent(entityHit)) {
          Faction faction = factionLookup[entityHit];

          if (faction.factionType == FactionType.Zombie) {
            RefRW<Health> health = healthLookup.GetRefRW(entityHit);
            health.ValueRW.healthAmount -= bullet.ValueRO.damageAmount;
            health.ValueRW.onHealthChanged = true;

            if (targetLookup.HasComponent(entityHit)) {
              RefRO<Target> hitTarget = targetLookup.GetRefRO(entityHit);

              // Enemy goes towards target if hit
              if (hitTarget.ValueRO.targetEntity == Entity.Null && SystemAPI.HasComponent<RandomWalking>(entityHit)) {
                RefRW<RandomWalking> enemyRandomWalking = randomWalkingLookup.GetRefRW(entityHit);
                RefRO<LocalTransform> enemyLocalTransform = localTransformLookup.GetRefRO(entityHit);
                float distanceNearSq = 100f;

                if (math.distancesq(enemyRandomWalking.ValueRO.targetPosition, bullet.ValueRO.spawnPosition) > distanceNearSq) {
                  enemyRandomWalking.ValueRW.targetPosition = bullet.ValueRO.spawnPosition;
                }
              }
            }

            entityCommandBuffer.DestroyEntity(entity);
          }

        }
      }
      localTransform.ValueRW.Position = targetPosition;
    }
  }

}
