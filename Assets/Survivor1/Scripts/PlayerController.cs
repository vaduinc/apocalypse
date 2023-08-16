
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Variables para la entrada de movimiento y apuntado
    Vector2 moveDirection; // se pasa como parametro dentro de Update
    Vector2 fireDirection; // donde esta apuntando el jugador -- sigue el movimiento del mouse
    public Transform bulletAim;

    // Variables para el movimiento
    public float moveSpeed; //  1;
    public float maxForwardSpeed; //  20;
    public float turnSpeed = 100;
    float desiredSpeed;
    float forwardSpeed; // la velocidad actual del jugador

    // Constantes para la aceleración y desaceleración en el suelo
    const float GROUND_ACCEL = 3;
    const float GROUND_DESCEL = 15;

    // Referencias para el manejo del arma y apuntado
    public Transform spine; // Hueso de la columna vertebral usado para la rotación del apuntado
    public Vector2 lastFireDirection; // Última dirección de apuntado para la sensibilidad del apuntado
    float xSensitivity = 0.5f; // sensibilidad para la rotacion cuando apunta con el mouse eje X
    float ySensitivity = 0.5f; // sensibilidad para la rotacion cuando apunta con el mouse eje Y

    public Transform weapon; // Referencia al transform del arma para manejar el recoger y soltar
    public Transform hand; // Referencia para la posición de la mano al sostener el arma
    public Transform hip; // Referencia para la posición de la cadera cuando el arma está guardada en la cintura del jugador

    // Referencia al componente Animator para controlar las animaciones
    Animator anim;

    [Header("Audio")]
    public AudioSource gunAudio;
    public AudioClip gun;

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Bullet")
        {
            anim.SetTrigger("HitLeft");
            Destroy(collision.gameObject);
        }
    }

    // Mover el arma de la cadera a la mano.
    public void PickupGun()
    {
        weapon.SetParent(hand);
        weapon.localPosition = new Vector3(-0.052f, 0.332f, 0.0027f);
        weapon.localRotation = Quaternion.Euler(-0.233f, -70.438f, 88.781f);
        weapon.localScale = new Vector3(80f, 80f, 80f);
    }

    // Mover el arma de la mano a la cadera.
    public void PutDownGun()
    {
        weapon.SetParent(hip);
        weapon.localPosition = new Vector3(-0.107f, 0.118f, -0.083f);
        weapon.localRotation = Quaternion.Euler(4.072f, -10.647f, 87.095f);
        weapon.localScale = new Vector3(80f, 80f, 80f);
    }

    // Comprobar si hay alguna entrada de movimiento.
    bool IsMoveInput
    {
        get { return !Mathf.Approximately(moveDirection.sqrMagnitude, 0f); }
    }

    // Callback para la entrada de movimiento.
    // LLamado por Unity
    public void OnMove(InputAction.CallbackContext context)
    {
        moveDirection = context.ReadValue<Vector2>();
    }

    // Callback para la entrada de "aim".
    // LLamado por Unity
    public void OnLook(InputAction.CallbackContext context)
    {
        fireDirection = context.ReadValue<Vector2>();
    }


    void CheckIfEnemyShoot()
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(bulletAim.position,bulletAim.forward, out hitInfo, 30))
        {
            GameObject hitEnemy = hitInfo.collider.gameObject;
            if ( hitEnemy.tag == "Enemies")
            {
                hitEnemy.GetComponent<EnemyController>().HitDamage();
            }
        }
    }

    //public void shotsWereFired()
    //{
    //    // Play the shoot sound
    //    Debug.Log("playing el sonido!!!! FUNCIONO");
    //    //AudioSource audio = new AudioSource();
    //    //audio = gunAudio;
    //    //audio.Play();

    //    //if (!gunAudio.isPlaying)
    //    //{
    //        gunAudio.PlayOneShot(gun);
    //    //}
        
    //}


    // Callback para la entrada de disparo.
    // LLamado por Unity
    public void OnFire(InputAction.CallbackContext context)
    {
        // Activa la animación "Fire" si la entrada de disparo está activada y el personaje está armado.
        if (context.ReadValue<float>() == 1 && anim.GetBool("Armed"))
        {
            CheckIfEnemyShoot();
            anim.SetTrigger("Fire");
            gunAudio.PlayOneShot(gun);
        }
    }

    // Callback para la entrada de armarse (para cambiar el parámetro "Armed" en el animator).
    // LLamado por Unity
    public void OnArmed(InputAction.CallbackContext context)
    {
        anim.SetBool("Armed", !anim.GetBool("Armed"));
    }

    // Movimiento
    // responsable del movimiento y rotación del personaje.
    // "ForwardSpeed" es un parametro del Animator que se actualiza cada
    // vez que se llama este metodo / update
    void Move(Vector2 direction)
    {
        float fDirection = direction.y;
        float turnAmount = direction.x;

        if (direction.sqrMagnitude > 1f)
        {
            direction.Normalize();
        }

        // Calcular la velocidad deseada basada en el input y la velocidad máxima "maxForwardSpeed"
        desiredSpeed = direction.magnitude * maxForwardSpeed * Mathf.Sign(fDirection);
        
        // Elegir entre aceleración y desaceleración si hay alguna movimiento.
        float acceleration = IsMoveInput ? GROUND_ACCEL : GROUND_DESCEL;

        // Ajustar la velocidad deseada.
        forwardSpeed = Mathf.MoveTowards(forwardSpeed, desiredSpeed, acceleration * Time.deltaTime);
        anim.SetFloat("ForwardSpeed", forwardSpeed);

        // Rotar el personaje horizontalmente según la dirección.
        transform.Rotate(0, turnAmount * Time.deltaTime * turnSpeed * Mathf.Sign(fDirection), 0);
    }

    void Start()
    {
        anim = this.GetComponent<Animator>();
    }

    void Update()
    {
        // llama Move para manejar el movimiento y la rotación del personaje.
        Move(moveDirection);
    }

    // LateUpdate se llama después de todos los demás métodos Update. 
    // Se utiliza para un "aim" más sensible / suave / smooth.
    public void LateUpdate()
    {
        // Aplicar sensibilidad a la dirección donde esta apuntando para una rotacion "smooth"
        lastFireDirection += new Vector2(-fireDirection.y * ySensitivity, fireDirection.x * xSensitivity);

        // Restringe los ángulos de "aim" para restringir el rango de rotación vertical y horizontal.
        lastFireDirection.x = Mathf.Clamp(lastFireDirection.x, -30, 30);
        lastFireDirection.y = Mathf.Clamp(lastFireDirection.y, -10, 60);

        // Aplicar la rotación calculada al "spine" del personje.
        spine.localEulerAngles = lastFireDirection;
    }
}
