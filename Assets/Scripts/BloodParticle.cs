using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BloodParticle : MonoBehaviour {
    [SerializeField] [Range(0.0f, 1.0f)] private float spawnChance = 0.1f;
    [SerializeField] private LayerMask hitMask = 0;
    [SerializeField] private float range = 1f;

    private DecalController decalController = null;
    private ParticleSystem particle;
    private List<ParticleCollisionEvent> collisionEvents;

    private void Start() {
        particle = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
        decalController = DecalController.Instance;
    }

    private void SpawnBlood(Vector3 pos) {
        for (int i = 0; i < 6; i++) {
            Vector3 dir = Vector3.zero;
            dir = i switch {
                0 => -Vector3.up,
                1 => Vector3.forward,
                2 => -Vector3.forward,
                4 => Vector3.right,
                5 => -Vector3.right,
                _ => dir
            };

            Ray ray = new Ray(pos, dir);
            if (Physics.Raycast(ray, out RaycastHit hit, range, hitMask)) {
                decalController.SpawnDecal(hit.point, ray.direction.normalized, -0.5f);
            }
        }
    }

    private void OnParticleCollision(GameObject col) {
        if (Random.value > spawnChance)
            return;
        
        int numCollisionEvents = particle.GetCollisionEvents(col, collisionEvents);
        int i = 0;

        while (i < numCollisionEvents)
        {
            SpawnBlood(collisionEvents[i].intersection);
            i++;
        }
    }
}