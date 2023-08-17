using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    Animator anim;
    NavMeshAgent agent;
    float destroyHeight;
    public GameObject target;
    public float walkingSpeed;
    public float runningSpeed;
    public Transform hand;
    public GameObject bullet;
    
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
    

    // Start is called before the first frame update
    void Start()
    {
        anim = this.GetComponent<Animator>();
        agent = this.GetComponent<NavMeshAgent>();
        if (target == null)
        {
            target = GameObject.FindWithTag("Player");
            return;
        }
    }

    float DistanceToPlayer()
    {
        return Vector3.Distance(target.transform.position, this.transform.position);
    }


    public void HitDamage ()
    {
        TurnOff();
        anim.SetBool("isDead", true);
        this.state = STATE.DEAD;
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
        destroyHeight = Terrain.activeTerrain.SampleHeight(this.transform.position) - 5;
        Collider[] colliders = this.transform.GetComponentsInChildren<Collider>();
        foreach ( Collider c in colliders)
        {
            Destroy(c);
        }

        InvokeRepeating("IntoGround", 1, 0.5f);
    }

    void IntoGround()
    {
        this.transform.Translate(0, -0.001f, 0);
        if (this.transform.position.y < destroyHeight)
        {
            Destroy(this.gameObject);
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
