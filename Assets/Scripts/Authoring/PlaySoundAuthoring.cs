using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

class PlaySoundAuthoring : MonoBehaviour
{
}

class PlaySoundAuthoringBaker : Baker<PlaySoundAuthoring>
{
    public override void Bake(PlaySoundAuthoring authoring)
    {
        
    }
}

public struct GunshotAudioRequest : IComponentData {
  public float3 Position;
}
