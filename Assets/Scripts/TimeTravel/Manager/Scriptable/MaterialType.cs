using UnityEngine;

[CreateAssetMenu(fileName = "New Material Type", menuName = "Game/Material Type")]
public class MaterialType : ScriptableObject
{
    [Header("Visual Effects")]
    public ParticleSystem sparkParticlePrefab;
    public ParticleSystem dustParticlePrefab;
    public ParticleSystem debrisParticlePrefab;

    [Header("Impact Settings")]
    public float sparkIntensityMultiplier = 1f;
    public float dustIntensityMultiplier = 1f;
    public float debrisIntensityMultiplier = 1f;

    [Header("Sound Settings")]
    public AudioClip[] impactSounds;
    public float pitchVariation = 0.1f;

    [Header("Physical Properties")]
    public float hardness = 1f;
    public float brittleness = 0.5f;
}

[CreateAssetMenu(fileName = "New Material Database", menuName = "Game/Material Database")]
public class MaterialDatabase : ScriptableObject
{
    public MaterialType[] materials;

    public MaterialType GetMaterial(string materialName)
    {
        foreach (var material in materials)
        {
            if (material.name == materialName)
                return material;
        }
        return null;
    }
}