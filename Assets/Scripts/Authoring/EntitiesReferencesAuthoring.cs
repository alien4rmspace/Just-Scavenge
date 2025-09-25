using Unity.Entities;
using UnityEngine;

public class EntitiesReferencesAuthoring : MonoBehaviour {
  public GameObject bulletPrefabGameObject;
  public GameObject zombiePrefabEntity;
  public GameObject shootLightPrefabEntity;
  public class Baker : Baker<EntitiesReferencesAuthoring> {
    public override void Bake(EntitiesReferencesAuthoring authoring) {
      Entity entity = GetEntity(TransformUsageFlags.Dynamic);
      AddComponent(entity, new EntitiesReferences {
        bulletPrefabEntity = GetEntity(authoring.bulletPrefabGameObject, TransformUsageFlags.Dynamic),
        zombiePrefabEntity = GetEntity(authoring.zombiePrefabEntity, TransformUsageFlags.Dynamic),
        shootLightPrefabEntity = GetEntity(authoring.shootLightPrefabEntity, TransformUsageFlags.Dynamic),
      });
    }
  }
}

public struct EntitiesReferences : IComponentData {
  public Entity bulletPrefabEntity;
  public Entity zombiePrefabEntity;
  public Entity shootLightPrefabEntity;
}
