using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class BulletAuthoring : MonoBehaviour {
  public float speed;
  public int damageAmount;
  public GameObject bulletEntity;
  public class Baker : Baker<BulletAuthoring> {
    public override void Bake(BulletAuthoring authoring) {
      Entity entity = GetEntity(TransformUsageFlags.Dynamic);
      AddComponent(entity, new Bullet {
        speed = authoring.speed,
        damageAmount = authoring.damageAmount,
        bulletPrefab = GetEntity(TransformUsageFlags.Dynamic),
      });
    }
  }
}

public struct Bullet : IComponentData {
  public float speed;
  public float damageAmount;
  public float3 spawnPosition;
  public float3 moveDirection;
  public float travelDistance;
  public float maxTravelDistance;

  public Entity bulletPrefab;
}
