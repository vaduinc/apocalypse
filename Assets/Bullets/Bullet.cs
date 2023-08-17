using UnityEngine;

public class Bullet : MonoBehaviour
{
    private bool hasExploded = false;

    void Start()
    {
        Invoke("Explote", 4f);
    }

    private void Update()
    {
        if (hasExploded)
        {
            Destroy(gameObject);
        }
    }

    public void Explote()
    {
        ParticleSystem particleSystem = this.GetParticleByName("Explosion_PSPrefab");
        ParticleSystem particleBallFile = this.GetParticleByName("Meteor_PS");

        if (particleSystem != null && particleBallFile != null)
            {
                particleBallFile.Stop();
                transform.localScale = Vector3.zero;
                particleSystem.Play();
                StartCoroutine(WaitForParticleSystemCompletion(particleSystem));
            }
    }

    private ParticleSystem GetParticleByName(string particleName)
    {

        ParticleSystem particleSystem = null;
        Transform childTransform = transform.Find(particleName);

        if (childTransform != null)
        {
            particleSystem = childTransform.GetComponent<ParticleSystem>();
        }

        return particleSystem;
    }

    private System.Collections.IEnumerator WaitForParticleSystemCompletion(ParticleSystem ps)
    {
        while (ps.isPlaying)
        {
            yield return null;
        }

        // Particle system animation has finished
        hasExploded = true;
    }
}
