using UnityEngine;

public class ParticleSystemManager : MonoBehaviour
{
    public static ParticleSystemManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void Play(ParticleSystem particleSystem)
    {
        particleSystem.Play();
    }

    public void Stop(ParticleSystem particleSystem)
    {
        particleSystem.Stop();
    }
}