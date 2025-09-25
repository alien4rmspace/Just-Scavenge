using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct RandomWalkingSystem : ISystem {

  [BurstCompile]
  public void OnUpdate(ref SystemState state) {
    new RandomWalkingJob().ScheduleParallel();
  }
}


[BurstCompile]
public partial struct RandomWalkingJob : IJobEntity {
  public void Execute(ref RandomWalking randomWalking, ref UnitMover unitMover, in LocalTransform transform) {
    if (math.distancesq(transform.Position, randomWalking.targetPosition) < UnitMoverSystem.REACHED_TARGET_POSITION_DISTANCE_SQ) {
      var random = randomWalking.random;

      float3 direction = random.NextFloat3Direction();
      direction.y = 0f;
      direction = math.normalizesafe(direction);

      float distance = random.NextFloat(randomWalking.distanceMin, randomWalking.distanceMax);
      randomWalking.targetPosition = randomWalking.originPosition + direction * distance;

      randomWalking.random = random; // write back
    } else {
      unitMover.targetPosition = randomWalking.targetPosition;
    }
  }
}