using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Munitions & Dégâts")]
    public int ammoCount = 10;
    public int damage = 25;
    public float shootCooldown = 0.5f;

    [Header("Mode de Tir")]
    public bool usePrefabBullet = false;
    public GameObject bulletPrefab;
    public float bulletSpeed = 20f;

    [Header("Références & Mask")]
    public Transform shootingPoint;
    public LayerMask hitLayers;

    private PlayerController playerController;
    private Animator animator;
    private InputSystem_Actions controls;
    private float nextShootTime = 0f;

    private void Awake()
    {
        controls = new InputSystem_Actions();
        playerController = GetComponent<PlayerController>();
        animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        controls.Player.Enable();
    }

    private void OnDisable()
    {
        controls.Player.Disable();
    }

    private void Update()
    {
        // Tir autorisé uniquement en posture de visée
        if (playerController != null && playerController.isAiming)
        {
            if (controls.Player.Shoot.WasPressedThisFrame() && ammoCount > 0 && Time.time >= nextShootTime)
            {
                Shoot();
            }
        }
    }

    private void Shoot()
    {
        nextShootTime = Time.time + shootCooldown;
        ammoCount--;

        if (animator != null)
        {
            animator.SetTrigger("Shoot");
        }

        if (usePrefabBullet)
        {
            SpawnBulletPrefab();
        }
        else
        {
            PerformRaycast();
        }

        Debug.Log($"PAN ! Munitions restantes : {ammoCount}");
    }

    private void SpawnBulletPrefab()
    {
        if (bulletPrefab == null || shootingPoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, shootingPoint.position, shootingPoint.rotation);
        if (bullet.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.AddForce(shootingPoint.forward * bulletSpeed, ForceMode.Impulse);
        }
    }

    private void PerformRaycast()
    {
        if (shootingPoint == null) 
        {
            Debug.LogWarning("ShootingPoint n'est pas assigné dans l'Inspecteur !");
            return;
        }

        Debug.DrawRay(shootingPoint.position, shootingPoint.forward * 25f, Color.red, 1f);

        if (Physics.Raycast(shootingPoint.position, shootingPoint.forward, out RaycastHit hit, Mathf.Infinity, hitLayers))
        {
            Debug.Log($"Raycast a touché : {hit.transform.name}");

            // Cherche EnemyAI sur l'objet touché OU sur un de ses parents
            EnemyAI enemy = hit.transform.GetComponentInParent<EnemyAI>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log($"Dégâts infligés ! Santé restante de l'ennemi : {damage}");
            }
            else
            {
                Debug.LogWarning($"L'objet {hit.transform.name} a été touché mais n'a pas de composant EnemyAI !");
            }
        }
    }
}