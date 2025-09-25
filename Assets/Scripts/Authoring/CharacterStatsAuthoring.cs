using Unity.Entities;
using UnityEngine;

class CharacterStatsAuthoring : MonoBehaviour
{
  public float moveSpeed;
  public float attackSpeed;
  public float attackDamage;
  public float defense;
  public float attackRange;
  public float bulletMaxTravelDistance;
}

class CharacterStatsAuthoringBaker : Baker<CharacterStatsAuthoring>
{
    public override void Bake(CharacterStatsAuthoring authoring)
    {
    Entity entity = GetEntity(TransformUsageFlags.Dynamic);
    AddComponent(entity, new CharacterStats {
      moveSpeed = authoring.moveSpeed,
      attackSpeed = authoring.attackSpeed,
      attackDamage = authoring.attackDamage,
      defense = authoring.defense,

      attackRange = authoring.attackRange,
      bulletMaxTravelDistance = authoring.bulletMaxTravelDistance,

      onHealthChanged = true  // Needs to be true so health bar can update once.
    });
    }
}

public struct CharacterStats : IComponentData {
  public float attackSpeed;
  public float attackDamage;
  public float attackRange;
  public float bulletMaxTravelDistance;


  public float defense;

  public float moveSpeed;

  public bool onHealthChanged;
}