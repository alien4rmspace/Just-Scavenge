using Unity.Entities;
using UnityEngine;

class HealthBarAuthoring : MonoBehaviour {
  public GameObject barVisualGameObject;
  public GameObject healthGameObject;
}

class HealthBarAuthoringBaker : Baker<HealthBarAuthoring> {
  public override void Bake(HealthBarAuthoring authoring) {
    Entity entity = GetEntity(TransformUsageFlags.Dynamic);
    AddComponent(entity, new HealthBar {
      barVisualEntity = GetEntity(authoring.barVisualGameObject, TransformUsageFlags.NonUniformScale),
      healthEntity = GetEntity(authoring.healthGameObject, TransformUsageFlags.Dynamic),
    });
  }
}

public struct HealthBar : IComponentData {
  public Entity barVisualEntity;
  public Entity healthEntity;
}