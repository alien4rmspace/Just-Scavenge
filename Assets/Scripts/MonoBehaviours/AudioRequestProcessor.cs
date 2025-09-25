using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class AudioRequestProcessor : MonoBehaviour {
  public AudioClip gunshotClip;
  public GameObject audioPrefab; // prefab with AudioSource set to PlayOnAwake

  private EntityManager em;
  private EntityQuery audioRequestQuery;

  void Start() {
    em = World.DefaultGameObjectInjectionWorld.EntityManager;
    audioRequestQuery = em.CreateEntityQuery(typeof(GunshotAudioRequest));
  }

  void Update() {
    var requestEntities = audioRequestQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
    var requests = audioRequestQuery.ToComponentDataArray<GunshotAudioRequest>(Unity.Collections.Allocator.Temp);

    for (int i = 0; i < requestEntities.Length; i++) {
      PlayGunshot(requests[i].Position);
      em.DestroyEntity(requestEntities[i]);
    }

    requestEntities.Dispose();
    requests.Dispose();
  }

  void PlayGunshot(float3 position) {
    var obj = Instantiate(audioPrefab, position, Quaternion.identity);
    Destroy(obj, gunshotClip.length); // Or return to pool if pooling
  }
}
