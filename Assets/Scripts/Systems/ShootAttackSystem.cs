using Rukhanka;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using static UnityEngine.Rendering.DebugUI;

partial struct ShootAttackSystem : ISystem {
  private ComponentLookup<LocalTransform> localTransformComponentLookup;
  private ComponentLookup<Bullet> bulletComponentLookup;
  private ComponentLookup<ShootVictim> shootVictimComponentLookup;
  private ComponentLookup<MoveOverride> moveOverrideComponentLookup;


  [BurstCompile]
  public void OnCreate(ref SystemState state) {
    localTransformComponentLookup = state.GetComponentLookup<LocalTransform>(false);
    bulletComponentLookup = state.GetComponentLookup<Bullet>(true);
    shootVictimComponentLookup = state.GetComponentLookup<ShootVictim>(true);
    moveOverrideComponentLookup = state.GetComponentLookup<MoveOverride>(true);

    state.RequireForUpdate<EntitiesReferences>();
  }

  [BurstCompile]
  public void OnUpdate(ref SystemState state) {
    localTransformComponentLookup.Update(ref state);
    bulletComponentLookup.Update(ref state);
    shootVictimComponentLookup.Update(ref state);
    moveOverrideComponentLookup.Update(ref state);

    EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
    var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
    var entityCommandBuffer = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
    var entityCommandBuffer2 = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

    uint randomSeed = (uint)UnityEngine.Random.Range(1, int.MaxValue);

    ShootAttackBulletJob shootAttackBulletJob = new ShootAttackBulletJob {
      localTransformComponentLookup = localTransformComponentLookup,
      bulletComponentLookup = bulletComponentLookup,
      shootVictimComponentLookup = shootVictimComponentLookup,
      moveOverrideComponentLookup = moveOverrideComponentLookup,
      entitiesReferences = entitiesReferences,
      deltaTime = SystemAPI.Time.DeltaTime,
      randomSeed = randomSeed,
      ecb = entityCommandBuffer
    };
    ShootAttackBulletBuildingJob shootAttackBulletBuildingJob = new ShootAttackBulletBuildingJob {
      localTransformComponentLookup = localTransformComponentLookup,
      bulletComponentLookup = bulletComponentLookup,
      shootVictimComponentLookup = shootVictimComponentLookup,
      moveOverrideComponentLookup = moveOverrideComponentLookup,
      entitiesReferences = entitiesReferences,
      deltaTime = SystemAPI.Time.DeltaTime,
      randomSeed = randomSeed,
      ecb = entityCommandBuffer2
    };

    var jobHandle1 = shootAttackBulletJob.ScheduleParallel(state.Dependency);
    var jobHandle2 = shootAttackBulletBuildingJob.ScheduleParallel(jobHandle1);
    state.Dependency = jobHandle2;

    //foreach ((
    //  RefRW<LocalTransform> localTransform,
    //  RefRW<ShootAttack> shootAttack,
    //  RefRO<Target> target,
    //  RefRO<CharacterStats> characterStats,
    //  RefRO<UnitMover> unitMover,
    //  Entity entity)
    //  in SystemAPI.Query<
    //    RefRW<LocalTransform>,
    //    RefRW<ShootAttack>,
    //    RefRO<Target>,
    //    RefRO<CharacterStats>,
    //    RefRO<UnitMover>>().WithDisabled<MoveOverride>().WithEntityAccess()) {

    //  if (target.ValueRO.targetEntity == Entity.Null) {
    //    shootAttack.ValueRW.loseTargetTimer -= SystemAPI.Time.DeltaTime;
    //    // Delay from losing target, so character won't put down weapon aninmation too quick.
    //    if (shootAttack.ValueRO.isShooting && shootAttack.ValueRO.loseTargetTimer <= 0f) {
    //      shootAttack.ValueRW.isShooting = false;
    //      shootAttack.ValueRW.aimTimer = shootAttack.ValueRO.aimTimerMax;
    //    }
    //    continue;
    //  }

    //  LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.targetEntity);

    //  float3 aimDirection = targetLocalTransform.Position - localTransform.ValueRO.Position;
    //  aimDirection = math.normalize(aimDirection);

    //  quaternion targetRotation = quaternion.LookRotation(aimDirection, math.up());
    //  localTransform.ValueRW.Rotation = math.slerp(localTransform.ValueRO.Rotation, targetRotation, SystemAPI.Time.DeltaTime * unitMover.ValueRO.rotationSpeed);

    //  // For animation
    //  shootAttack.ValueRW.aimTimer -= SystemAPI.Time.DeltaTime;

    //  if (shootAttack.ValueRO.aimTimer >= 0f) {
    //    // isAiming basically
    //    if (shootAttack.ValueRO.isShooting == false) {
    //      shootAttack.ValueRW.isShooting = true;
    //    }
    //    continue;
    //  }

    //  // Efficiency check. Read comparision vs writing
    //  if (shootAttack.ValueRO.loseTargetTimer != shootAttack.ValueRO.loseTargetTimerMax) {
    //    shootAttack.ValueRW.loseTargetTimer = shootAttack.ValueRO.loseTargetTimerMax;
    //  }

    //  // Shooting cooldown between shots
    //  shootAttack.ValueRW.timer -= SystemAPI.Time.DeltaTime;
    //  if (shootAttack.ValueRO.timer > 0f) {
    //    continue;
    //  }
    //  shootAttack.ValueRW.timer = characterStats.ValueRO.attackSpeed;

    //  Entity bulletEntity = state.EntityManager.Instantiate(entitiesReferences.bulletPrefabEntity);
    //  float3 bulletSpawnWorldPosition = localTransform.ValueRO.TransformPoint(shootAttack.ValueRO.bulletSpawnLocalPosition);
    //  SystemAPI.SetComponent(bulletEntity, LocalTransform.FromPosition(bulletSpawnWorldPosition));

    //  RefRW<Bullet> bulletBullet = SystemAPI.GetComponentRW<Bullet>(bulletEntity);
    //  bulletBullet.ValueRW.damageAmount = characterStats.ValueRO.attackDamage;
    //  bulletBullet.ValueRW.speed = shootAttack.ValueRO.bulletSpeed;
    //  bulletBullet.ValueRW.spawnPosition = new float3(bulletSpawnWorldPosition.x, 0, bulletSpawnWorldPosition.z);
    //  bulletBullet.ValueRW.maxTravelDistance = characterStats.ValueRO.bulletMaxTravelDistance;

    //  Unity.Mathematics.Random random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, int.MaxValue));

    //  // Define spread radius
    //  float spreadRadius = 1.5f;

    //  // Get random 2D direction
    //  float2 randomDirection2D = random.NextFloat2Direction();

    //  // Get random distance inside the spread radius
    //  float randomDistance = random.NextFloat(0f, spreadRadius);

    //  // Calculate final random offset
    //  float2 offset2D = randomDirection2D * randomDistance;

    //  // Apply spread XY plane
    //  float3 spreadOffset = new float3(offset2D.x, offset2D.y, 0f);

    //  // Final target position for the bullet with spread
    //  float3 targetPosition = targetLocalTransform.Position + new float3(0, 1.5f, 0);
    //  float3 spreadedTargetPosition = targetPosition + spreadOffset;

    //  // Calculate direction bullet should go towards in a 3d space
    //  float3 moveDirection = spreadedTargetPosition - bulletSpawnWorldPosition;
    //  moveDirection = math.normalize(moveDirection);
    //  bulletBullet.ValueRW.moveDirection = moveDirection;

    //  //Shooting cooldown
    //  shootAttack.ValueRW.onShoot.isTriggered = true;
    //  shootAttack.ValueRW.onShoot.shootFromPosition = bulletSpawnWorldPosition;

    //  ShootVictim targetShootVictim = SystemAPI.GetComponent<ShootVictim>(target.ValueRO.targetEntity);
    //  shootAttack.ValueRW.onShoot.shootToPosition = targetShootVictim.hitLocalPosition;
    //}
  }
}

[BurstCompile]
public partial struct ShootAttackBulletJob : IJobEntity {
  [NativeDisableParallelForRestriction] public ComponentLookup<LocalTransform> localTransformComponentLookup;
  [ReadOnly] public ComponentLookup<Bullet> bulletComponentLookup;
  [ReadOnly] public ComponentLookup<ShootVictim> shootVictimComponentLookup;
  [ReadOnly] public EntitiesReferences entitiesReferences;
  [ReadOnly] public ComponentLookup<MoveOverride> moveOverrideComponentLookup;

  public EntityCommandBuffer.ParallelWriter ecb;

  public float deltaTime;
  public uint randomSeed;
  public void Execute(
    [EntityIndexInQuery] int entityInQueryIndex,
    Entity entity,
    ref ShootAttack shootAttack,
    in Target target,
    in CharacterStats characterStats,
    in UnitMover unitMover
    ) {
    if (moveOverrideComponentLookup.IsComponentEnabled(entity)) {
      if (shootAttack.isShooting) {
        shootAttack.isShooting = false;
        shootAttack.aimTimer = shootAttack.aimTimerMax;
      }
      return;
    }
    if (target.targetEntity == Entity.Null) {
      shootAttack.loseTargetTimer -= deltaTime;
      // Delay from losing target, so character won't put down weapon aninmation too quick.
      if (shootAttack.isShooting && shootAttack.loseTargetTimer <= 0f) {
        shootAttack.isShooting = false;
        shootAttack.aimTimer = shootAttack.aimTimerMax;
      }
      return;
    }

    RefRO<LocalTransform> targetLocalTransform = localTransformComponentLookup.GetRefRO(target.targetEntity);
    RefRW<LocalTransform> localTransform = localTransformComponentLookup.GetRefRW(entity);
    float3 bulletSpawnWorldPosition = localTransform.ValueRO.TransformPoint(shootAttack.bulletSpawnLocalPosition);


    float3 aimDirection = targetLocalTransform.ValueRO.Position - localTransform.ValueRO.Position;
    aimDirection.y = 0;
    aimDirection = math.normalize(aimDirection);

    float3 bulletMoveDirection = targetLocalTransform.ValueRO.Position - localTransform.ValueRO.TransformPoint(shootAttack.bulletSpawnLocalPosition);
    bulletMoveDirection.y = 0;
    bulletMoveDirection = math.normalize(bulletMoveDirection);

    quaternion targetRotation = quaternion.LookRotationSafe(aimDirection, math.up());
    localTransform.ValueRW.Rotation = math.slerp(localTransform.ValueRO.Rotation, targetRotation, deltaTime * unitMover.rotationSpeed);

    // Check if unit is facing target before shooting
    // Compare two quaternions with math.dot 
    float3 unitForward = math.forward(localTransform.ValueRW.Rotation);
    float dot = math.dot(unitForward, aimDirection);

    if (dot >= 0.99) {
      // For animation
      shootAttack.aimTimer -= deltaTime;

      if (shootAttack.aimTimer >= 0f) {
        // isAiming basically
        if (shootAttack.isShooting == false) {
          shootAttack.isShooting = true;
        }
        return;
      }

      // Shooting cooldown between shots
      shootAttack.timer -= deltaTime;
      if (shootAttack.timer > 0f) {
        return;
      }

      if (shootAttack.loseTargetTimer != shootAttack.loseTargetTimerMax) {
        shootAttack.loseTargetTimer = shootAttack.loseTargetTimerMax;
      }

      shootAttack.timer = characterStats.attackSpeed;

      int pelletCount = 1; // TODO link to character weapon
      float spreadAngle = math.radians(7f); // x degrees cone spread

      for (int i = 0; i < pelletCount; i++) {
        Entity bulletEntity = ecb.Instantiate(entityInQueryIndex, entitiesReferences.bulletPrefabEntity);

        Unity.Mathematics.Random random = new Unity.Mathematics.Random(randomSeed + (uint)i); // different seed for each pellet

        // Check if it's the first pellet shot, and if so, add a random delay.
        if (i == 0) {
          float delay = random.NextFloat(0f, 0.2f);
          shootAttack.timer += delay;
        }

        //// Define spread radius
        //float spreadRadius = 1f;

        //// Get random 2D direction
        //float2 randomDirection2D = random.NextFloat2Direction();
        //float randomDistance = random.NextFloat(0f, spreadRadius);
        //float2 offset2D = randomDirection2D * randomDistance;

        //float3 spreadOffset = new float3(offset2D.x, 0f, offset2D.y);

        //float3 targetPosition = targetLocalTransform.ValueRO.Position + new float3(0, 1f, 0);
        //float3 spreadedTargetPosition = targetPosition + spreadOffset;

        //float3 moveDirection = math.normalize(spreadedTargetPosition - bulletSpawnWorldPosition);

        quaternion spreadRotation = quaternion.AxisAngle(math.up(), random.NextFloat(-spreadAngle, spreadAngle));
        float3 moveDirection = math.normalize(math.mul(spreadRotation, bulletMoveDirection));
        quaternion bulletRotation = quaternion.LookRotationSafe(moveDirection, math.up());

        ecb.SetComponent(entityInQueryIndex, bulletEntity, new LocalTransform {
          Position = bulletSpawnWorldPosition,
          Rotation = bulletRotation,
          Scale = 1f
        });

        ecb.SetComponent(entityInQueryIndex, bulletEntity, new Bullet {
          damageAmount = characterStats.attackDamage, // per pellet damage
          speed = shootAttack.bulletSpeed * random.NextFloat(0.95f, 1.05f),
          spawnPosition = bulletSpawnWorldPosition,
          maxTravelDistance = characterStats.bulletMaxTravelDistance,
          moveDirection = moveDirection
        });
      }


      // Play gun sound
      Entity request = ecb.CreateEntity(0);
      ecb.AddComponent(0, request, new GunshotAudioRequest {
        Position = bulletSpawnWorldPosition
      });

      //Shooting cooldown
      shootAttack.onShoot.isTriggered = true;
      shootAttack.onShoot.shootFromPosition = bulletSpawnWorldPosition;

      // Deprecated
      RefRO<ShootVictim> targetShootVictim = shootVictimComponentLookup.GetRefRO(target.targetEntity);
      shootAttack.onShoot.shootToPosition = targetShootVictim.ValueRO.hitLocalPosition;
    }
  }
}

[BurstCompile]
public partial struct ShootAttackBulletBuildingJob : IJobEntity {
  [NativeDisableParallelForRestriction] public ComponentLookup<LocalTransform> localTransformComponentLookup;
  [ReadOnly] public ComponentLookup<Bullet> bulletComponentLookup;
  [ReadOnly] public ComponentLookup<ShootVictim> shootVictimComponentLookup;
  [ReadOnly] public EntitiesReferences entitiesReferences;
  [ReadOnly] public ComponentLookup<MoveOverride> moveOverrideComponentLookup;

  public EntityCommandBuffer.ParallelWriter ecb;

  public float deltaTime;
  public uint randomSeed;
  public void Execute(
  [EntityIndexInQuery] int entityInQueryIndex,
  Entity entity,
  ref ShootAttack shootAttack,
  in Target target,
  in CharacterStats characterStats
  ) {
    if (moveOverrideComponentLookup.HasComponent(entity) && moveOverrideComponentLookup.IsComponentEnabled(entity)) {
      if (shootAttack.isShooting) {
        shootAttack.isShooting = false;
        shootAttack.aimTimer = shootAttack.aimTimerMax;
      }
      return;
    }
    if (target.targetEntity == Entity.Null) {
      shootAttack.loseTargetTimer -= deltaTime;
      // Delay from losing target, so character won't put down weapon aninmation too quick.
      if (shootAttack.isShooting && shootAttack.loseTargetTimer <= 0f) {
        shootAttack.isShooting = false;
        shootAttack.aimTimer = shootAttack.aimTimerMax;
      }
      return;
    }

    // Shooting cooldown between shots
    shootAttack.timer -= deltaTime;
    if (shootAttack.timer > 0f) {
      return;
    }

    if (shootAttack.loseTargetTimer != shootAttack.loseTargetTimerMax) {
      shootAttack.loseTargetTimer = shootAttack.loseTargetTimerMax;
    }

    RefRO<LocalTransform> targetLocalTransform = localTransformComponentLookup.GetRefRO(target.targetEntity);
    RefRW<LocalTransform> localTransform = localTransformComponentLookup.GetRefRW(entity);
    float3 bulletSpawnWorldPosition = localTransform.ValueRO.TransformPoint(shootAttack.bulletSpawnLocalPosition);


    float3 aimDirection = targetLocalTransform.ValueRO.Position - localTransform.ValueRO.Position;
    aimDirection = math.normalize(aimDirection);

    float3 bulletMoveDirection = targetLocalTransform.ValueRO.Position - localTransform.ValueRO.TransformPoint(shootAttack.bulletSpawnLocalPosition);
    bulletMoveDirection = math.normalize(bulletMoveDirection);

    quaternion targetRotation = quaternion.LookRotationSafe(aimDirection, math.up());


    int pelletCount = 1; // TODO link to character weapon
    float spreadAngle = math.radians(7f); // x degrees cone spread

    for (int i = 0; i < pelletCount; i++) {
      Entity bulletEntity = ecb.Instantiate(entityInQueryIndex, entitiesReferences.bulletPrefabEntity);
      Unity.Mathematics.Random random = new Unity.Mathematics.Random(randomSeed + (uint)i); // different seed for each pellet

      // Check if it's the first pellet shot, and if so, add a random delay.
      if (i == 0) {
        float delay = random.NextFloat(0f, 0.1f);
        shootAttack.timer += delay;
      }

      quaternion spreadRotation = quaternion.AxisAngle(math.up(), random.NextFloat(-spreadAngle, spreadAngle));
      float3 moveDirection = math.normalize(math.mul(spreadRotation, bulletMoveDirection));
      quaternion bulletRotation = quaternion.LookRotationSafe(moveDirection, math.up());

      ecb.SetComponent(entityInQueryIndex, bulletEntity, new LocalTransform {
        Position = bulletSpawnWorldPosition,
        Rotation = bulletRotation,
        Scale = 1f
      });

      ecb.SetComponent(entityInQueryIndex, bulletEntity, new Bullet {
        damageAmount = characterStats.attackDamage, // per pellet damage
        speed = shootAttack.bulletSpeed * random.NextFloat(0.95f, 1.05f),
        spawnPosition = bulletSpawnWorldPosition,
        maxTravelDistance = characterStats.bulletMaxTravelDistance,
        moveDirection = moveDirection
      });
    }


    // Play gun sound
    Entity request = ecb.CreateEntity(0);
    ecb.AddComponent(0, request, new GunshotAudioRequest {
      Position = bulletSpawnWorldPosition
    });

    //Shooting cooldown
    shootAttack.timer = characterStats.attackSpeed;
    shootAttack.onShoot.isTriggered = true;
    shootAttack.onShoot.shootFromPosition = bulletSpawnWorldPosition;

    // Deprecated
    RefRO<ShootVictim> targetShootVictim = shootVictimComponentLookup.GetRefRO(target.targetEntity);
    shootAttack.onShoot.shootToPosition = targetShootVictim.ValueRO.hitLocalPosition;
  }
}