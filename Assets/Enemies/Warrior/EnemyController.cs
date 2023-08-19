using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    Animator anim;
    NavMeshAgent agent;
    float destroyHeight;
    int health = 2;
    bool isDead = false;

    public GameObject target;
    public float walkingSpeed;
    public float runningSpeed;
    public Transform hand;
    public GameObject bullet;

    public delegate void CreatedInsatnceDelegate(EnemyController me);
    public static event CreatedInsatnceDelegate OnInstanceCreatedEnemy;

    public delegate void DeadsUpdatedDelegate();
    public static event DeadsUpdatedDelegate OnDeadEnemy;

    [Header("Audio")]
    public AudioClip hitClip;
    public AudioClip dieClip;
    public AudioClip moanClip;

    enum STATE {  IDLE, AROUND , ATTACK, CHASE, DEAD };
    STATE state = STATE.IDLE;

    void TurnOff()
    {
        anim.SetBool("isWalking", false);
        anim.SetBool("isAttacking", false);
        anim.SetBool("isDead", false);
        anim.SetBool("isRunning", false);
    }


    public void RenderBullet()
    {
        bullet.GetComponent<MeshRenderer>().enabled = true;
    }

    private Transform FindChildRecursively(Transform parent, string childName)
    {
        Transform result = parent.Find(childName);
        if (result != null)
        {
            return result;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            result = FindChildRecursively(child, childName);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }


    public void ThrowBullet()
    {
        
        GameObject bulletObj = Instantiate(bullet, hand.transform.position + hand.transform.forward, hand.transform.rotation);
        ParticleSystem fireBall=  bulletObj.GetComponentInChildren<ParticleSystem>();
        fireBall.Play();
        Transform spineTransform = FindChildRecursively(target.transform, "mixamorig:Spine2");

        if (spineTransform != null)
        {
            bulletObj.transform.LookAt(spineTransform.position);
            bulletObj.GetComponent<Rigidbody>().AddForce(bulletObj.transform.forward * 2000);
        }

    }

    public void Moan()
    {
        // TODO cambiar de sonido
        // GetComponent<AudioSource>().PlayOneShot(moanClip);
    }
    

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        OnInstanceCreatedEnemy(this);
    }

    float DistanceToPlayer()
    {
        if (PlayGlobals.gameOver)
        {
            return Mathf.Infinity;
        }
        return Vector3.Distance(target.transform.position, this.transform.position);
    }


    public void HitDamage()
    {
        if (health <= 0) {
            GetComponent<AudioSource>().PlayOneShot(dieClip);
            TurnOff();
            anim.SetBool("isDead", true);
            state = STATE.DEAD;
        } else  {
            GetComponent<AudioSource>().PlayOneShot(hitClip);
            anim.SetTrigger("isHit");
        }
        health--;
    }

    bool CanSeePlayer()
    {
        if (DistanceToPlayer() < 20)
        {
            return true;
        }
        return false;
    }

    bool ForgetPlayer()
    {
        if (DistanceToPlayer() > 30)
        {
            return true;
        }
        return false;
    }

    void BurriedBody()
    {
        if (!isDead)
        {
            OnDeadEnemy();
            isDead = true;
        }
        destroyHeight = Terrain.activeTerrain.SampleHeight(this.transform.position) - 5;
        Collider[] colliders = transform.GetComponentsInChildren<Collider>();
        foreach ( Collider c in colliders)
        {
            Destroy(c);
        }

        InvokeRepeating("IntoGround", 1, 0.5f);
    }

    void IntoGround()
    {
        transform.Translate(0, -0.001f, 0);
        if (transform.position.y < destroyHeight)
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        switch (state)
        {
            case STATE.IDLE:
                if (CanSeePlayer())
                {
                    state = STATE.CHASE;
                }
                else if (Random.Range(0, 5000) < 5)
                {
                    state = STATE.AROUND;
                }
                break;
            case STATE.AROUND:
                if (!agent.hasPath)
                {
                    float newX = this.transform.position.x + Random.Range(-15, 15);
                    float newZ = this.transform.position.z + Random.Range(-15, 15);
                    float newY = Terrain.activeTerrain.SampleHeight(new Vector3(newX, 0, newZ));
                    Vector3 dest = new Vector3(newX, newY, newZ);
                    agent.SetDestination(dest);
                    agent.stoppingDistance = 0;
                    TurnOff();
                    agent.speed = walkingSpeed;
                    anim.SetBool("isWalking", true);
                }
                if (CanSeePlayer())
                {
                    state = STATE.CHASE;
                }
                else if (Random.Range(0, 5000) < 5)
                {
                    state = STATE.IDLE;
                    TurnOff();
                    agent.ResetPath();
                }
                break;
            case STATE.CHASE:
                agent.SetDestination(target.transform.position);
                agent.stoppingDistance = 15;
                TurnOff();
                agent.speed = runningSpeed;
                anim.SetBool("isRunning", true);

                if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
                {
                    state = STATE.ATTACK;
                }

                if (ForgetPlayer())
                {
                    state = STATE.AROUND;
                    agent.ResetPath();
                }

                break;
            case STATE.ATTACK:
                TurnOff();
                anim.SetBool("isAttacking", true);
                this.transform.LookAt(target.transform.position);
                if (DistanceToPlayer() > agent.stoppingDistance + 2)
                {
                    state = STATE.CHASE;
                }
                break;
            case STATE.DEAD:
                Destroy(agent);
                BurriedBody();
                break;
        }
    }
}
