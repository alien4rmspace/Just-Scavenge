using Unity.Burst;
using Unity.Entities;
using Unity.Collections;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
partial struct HealthTestDeadSystem : ISystem {
  [BurstCompile]
  public void OnUpdate(ref SystemState state) {
    EntityCommandBuffer entityCommandBuffer = 
      SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

    foreach ((
      RefRO<Health> health,
      Entity entity)
      in SystemAPI.Query<
        RefRO<Health>>().WithEntityAccess()) {

      if (health.ValueRO.healthAmount <= 0) {
        // This entity is dead
        entityCommandBuffer.DestroyEntity(entity);
      }
    }
  }
}
