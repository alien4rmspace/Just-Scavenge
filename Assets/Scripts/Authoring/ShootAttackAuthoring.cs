using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class ShootAttackAuthoring : MonoBehaviour {
  public float aimTimerMax;
  public float loseTargetTimerMax = 5;
  public float attackDistance;
  public float bulletSpeed;
  public Transform bulletSpawnPositionTransform;


  public class Baker : Baker<ShootAttackAuthoring> {
    public override void Bake(ShootAttackAuthoring authoring) {
      Entity entity = GetEntity(TransformUsageFlags.Dynamic);
      AddComponent(entity, new ShootAttack {
        aimTimerMax = authoring.aimTimerMax,
        aimTimer = authoring.aimTimerMax,
        loseTargetTimer = authoring.loseTargetTimerMax,
        loseTargetTimerMax = authoring.loseTargetTimerMax,
        attackDistance = authoring.attackDistance,
        bulletSpeed = authoring.bulletSpeed,
        bulletSpawnLocalPosition = authoring.bulletSpawnPositionTransform.localPosition,
      });
    }
  }
}

public struct ShootAttack : IComponentData {
  public float timer;
  public float timerMax;
  public float aimTimer;
  public float aimTimerMax;
  public float loseTargetTimer;
  public float loseTargetTimerMax;
  public float attackDistance;
  public float bulletSpeed;
  public float3 bulletSpawnLocalPosition;
  public OnShootEvent onShoot;

  public bool isShooting;

  public struct OnShootEvent {
    public bool isTriggered;
    public float3 shootFromPosition;
    public float3 shootToPosition;
  }
}
