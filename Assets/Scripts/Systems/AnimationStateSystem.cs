using Rukhanka;
using Unity.Burst;
using Unity.Entities;

partial struct AnimationStateSystem : ISystem {
  [BurstCompile]
  public void OnCreate(ref SystemState state) {

  }

  [BurstCompile]
  public void OnUpdate(ref SystemState state) {
    AnimationStateSurvivorJob animationStateSurvivorJob = new AnimationStateSurvivorJob {

    };
    animationStateSurvivorJob.ScheduleParallel();
    AnimationStateZombieJob animationStateZombieJob = new AnimationStateZombieJob {

    };
    animationStateZombieJob.ScheduleParallel();
  }

  [BurstCompile]
  public partial struct AnimationStateSurvivorJob : IJobEntity {
    public void Execute(ref DynamicBuffer<AnimatorControllerParameterComponent> allParams, in UnitMover unitMover, in ShootAttack shootAttack) {
      var movingAnim = allParams[0];
      var shootingAnim = allParams[1];

      // Unit moves
      if (movingAnim.BoolValue != unitMover.isMoving) {
        movingAnim.BoolValue = unitMover.isMoving;
      }

      // Unit shoots
      if (shootingAnim.BoolValue != shootAttack.isShooting) {
        shootingAnim.BoolValue = shootAttack.isShooting;
      }

      allParams[0] = movingAnim;
      allParams[1] = shootingAnim;
    }
  }

  public partial struct AnimationStateZombieJob : IJobEntity {
    public void Execute(ref DynamicBuffer<AnimatorControllerParameterComponent> allParams, in UnitMover unitMover, in MeleeAttack meleeAttack, in Zombie zombie) {
      var movingAnim = allParams[0];
      var meleeAnim = allParams[1];

      // Unit moves
      if (movingAnim.BoolValue != unitMover.isMoving) {
        movingAnim.BoolValue = unitMover.isMoving;
      }

      // Unit melee
      if (meleeAnim.BoolValue != meleeAttack.isMelee) {
        meleeAnim.BoolValue = meleeAttack.isMelee;
      }

      allParams[0] = movingAnim;
      allParams[1] = meleeAnim;
    }
  }
}