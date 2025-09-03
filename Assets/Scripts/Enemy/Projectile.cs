using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Rigidbody rb;
    private float maxDistance;
    private Vector3 startPosition;
    private bool isLaunched;
    private float damage;

    //public System.Action<Projectile> onDeactivate;
    public System.Action onPlayerDamaged;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Launch(Vector3 direction, float speed, float maxDistance)
    {
        this.maxDistance = maxDistance;
        startPosition = transform.position;
        isLaunched = true;

        gameObject.SetActive(true);
        rb.linearVelocity = direction * speed;
    }

    private void Update()
    {
        if (isLaunched && Vector3.Distance(startPosition, transform.position) >= maxDistance)
        {
            Deactivate();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isLaunched) return;

        if (other.CompareTag("Player") || other.CompareTag("Environment"))
        {
            Deactivate();
            if (other.CompareTag("Player"))
            {
                onPlayerDamaged?.Invoke();
            }
        }
    }

    private void Deactivate()
    {
        isLaunched = false;
        rb.linearVelocity = Vector3.zero;
        gameObject.SetActive(false);
        //onDeactivate?.Invoke(this);
    }
}
