using System.Diagnostics;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using static ResetHealthEventsJob;

[UpdateInGroup(typeof(LateSimulationSystemGroup), OrderLast = true)]
partial struct ResetEventSystem : ISystem {
  [BurstCompile]
  public void OnUpdate(ref SystemState state) {
    new ResetShootAttackEventsJob().ScheduleParallel();
    JobHandle resetHealthHandle = new ResetHealthEventsJob().ScheduleParallel(state.Dependency);
    resetHealthHandle.Complete();
    new ResetSelectedEventsJob().ScheduleParallel();

    //foreach (RefRW<Selected> selected in SystemAPI.Query<RefRW<Selected>>().WithPresent<Selected>()) {
    //  selected.ValueRW.onSelected = false;
    //  selected.ValueRW.onDeselected = false;
    //}
    //foreach (RefRW<Health> health in SystemAPI.Query<RefRW<Health>>()) {
    //  health.ValueRW.onHealthChanged = false;
    //}
    //foreach (RefRW<ShootAttack> shootAttack in SystemAPI.Query<RefRW<ShootAttack>>()) {
    //  shootAttack.ValueRW.onShoot.isTriggered = false;
    //}
  }
}



[BurstCompile]
public partial struct ResetShootAttackEventsJob : IJobEntity {
  public void Execute(ref ShootAttack shootAttack) {
    shootAttack.onShoot.isTriggered = false;
  }
}

[BurstCompile]
public partial struct ResetHealthEventsJob : IJobEntity {
  public void Execute(ref CharacterStats characterStats) {
    characterStats.onHealthChanged = false;
  }
}

[BurstCompile]
public partial struct ResetSelectedEventsJob : IJobEntity {
  public void Execute(ref Selected selected) {
    selected.onSelected = false;
    selected.onDeselected = false;
  }
}