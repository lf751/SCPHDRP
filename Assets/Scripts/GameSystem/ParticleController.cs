using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ParticleData
{
    public GameObject Particle;
    public float ParticleDuration;

    public ParticleData(GameObject _Particle, float _Duration)
    {
        Particle = _Particle;
        ParticleDuration = _Duration;
    }
}

public class ParticleController : MonoBehaviour
{
    public ParticleData[] particles;
    private List<ParticleData> currentParticles = new();
    // Update is called once per frame
    void Update()
    {
        if (currentParticles.Count != 0)
        {
            for(int i = 0; i < currentParticles.Count; i++ )
            {
                currentParticles[i].ParticleDuration -= Time.deltaTime;
                if (currentParticles[i].ParticleDuration < 0)
                {
                    Destroy(currentParticles[i].Particle);
                    currentParticles.RemoveAt(i);
                    i--;
                }
                    
            }
        }
    }

    public void StartParticle(int Particle, Vector3 pos, Quaternion rota)
    {
        currentParticles.Add(new ParticleData(Instantiate(particles[Particle].Particle, pos, rota), particles[Particle].ParticleDuration));
        //Debug.Log("SpawningParticle");
    }
}
