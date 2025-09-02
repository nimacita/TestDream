using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private WeaponSettings settings;
    private LayerMask enemyHitLayers = ~0;
    private Camera playerCamera;

    [Header("VFX References")]
    [SerializeField] private BulletTrail bulletTrail;

    [Header("Debug Settings")]
    [SerializeField] private bool drawRaycast = true;
    [SerializeField] private Color rayColor = Color.red;
    [SerializeField] private float rayDuration = 0.1f;
    [SerializeField] private bool drawSpreadGizmo = true;
    [SerializeField] private Color gizmoColor = Color.green;

    private bool _isReloading;
    private bool _isShooting;
    private bool _isInit = false;
    private int _currentAmmo;
    private int _savedAmmo = -1;
    private int _shotsFiredInBurst;
    private float _nextFireTime;
    private float _currentSpreadAngle; 
    private float _baseSpreadAngle;
    private float _reloadProgress;
    private float _reloadTimer;
    private Coroutine burstCoroutine;
    private Coroutine reloadCoroutine;
    private PlayerUIController uIController;

    public WeaponSettings Settings => settings;
    public int CurrentAmmo => _currentAmmo;
    public int SavedAmmo => _savedAmmo;
    public bool IsReloading => _isReloading;
    public float ReloadProgress => _reloadProgress;
    public bool IsShooting => _isShooting;
    public float CurrentSpreadAngle => _currentSpreadAngle;

    public void Initialize(Camera cam, PlayerUIController playerUIController, int currAmmo = -1)
    {
        playerCamera = cam;

        if (currAmmo == -1)
            _currentAmmo = settings.maxAmmo;
        else
            _currentAmmo = currAmmo;

        _baseSpreadAngle = settings.spreadAngle;
        _currentSpreadAngle = _baseSpreadAngle;

        uIController = playerUIController;
        bulletTrail.Initialized();

        _isInit = true;
    }

    private void Update()
    {
        // Автоматическая стрельба для Auto режима
        if (_isShooting && settings.fireMode == FireMode.Auto)
        {
            PerformShot();
        }

        UpdateSpreadRecovery();
    }

    #region Shooting
    public void StartShooting()
    {
        if (_isReloading || _currentAmmo <= 0 || !_isInit) return;

        switch (settings.fireMode)
        {
            case FireMode.Single:
                PerformShot();
                break;

            case FireMode.Auto:
                _isShooting = true;
                break;

            case FireMode.Burst:
                if (burstCoroutine == null)
                {
                    burstCoroutine = StartCoroutine(BurstFireCoroutine());
                }
                break;
        }
    }

    public void StopShooting()
    {
        _isShooting = false;

        if (settings.fireMode == FireMode.Burst && burstCoroutine != null)
        {
            StopCoroutine(burstCoroutine);
            burstCoroutine = null;
        }
    }

    private void PerformShot()
    {
        if (Time.time < _nextFireTime || _currentAmmo <= 0 || _isReloading || !_isInit)
            return;

        _currentAmmo--;

        if (settings.fireMode != FireMode.Burst) 
        {
            _nextFireTime = Time.time + (1f / settings.fireRate);
        }
        else
        {
            _nextFireTime = 0;
        }

        uIController.UpdateAmmoTxt(_currentAmmo, settings.maxAmmo);

        //увеличиваем разброс для автоматического режима
        /*if (settings.fireMode == FireMode.Auto)
        {
            IncreaseSpread();
        }*/
        IncreaseSpread();

        // Raycast стрельба
        Vector3 shotDirection = CalculateShotDirection();
        Vector3 startPosition = playerCamera.transform.position;
        RaycastHit hit;
        Vector3 hitPoint = Vector3.zero;

        if (Physics.Raycast(startPosition, shotDirection, out hit, Mathf.Infinity, enemyHitLayers, QueryTriggerInteraction.Collide))
        {
            HandleHit(hit);

            if (drawRaycast)
            {
                Debug.DrawRay(startPosition, shotDirection * hit.distance, rayColor, rayDuration);
            }
            hitPoint = hit.point;
        }
        else
        {
            if (drawRaycast)
            {
                Debug.DrawRay(startPosition, shotDirection * 20f, rayColor, rayDuration);
            }
            hitPoint = startPosition + shotDirection * 20f;
        }

        HandleTrail(hitPoint);
        PlayShootEffects();

        if (_currentAmmo <= 0)
        {
            StopShooting();
            Reload();
        }
    }

    private void HandleTrail(Vector3 hitPoint)
    {
        if (bulletTrail == null) return;
        bulletTrail.ShowTrail(hitPoint);
    }

    private IEnumerator BurstFireCoroutine()
    {
        _shotsFiredInBurst = 0;

        while (_shotsFiredInBurst < settings.burstCount && _currentAmmo > 0 && !_isReloading)
        {
            PerformShot();
            _shotsFiredInBurst++;
            yield return new WaitForSeconds(settings.burstDelay);
        }

        burstCoroutine = null;
    }

    private Vector3 CalculateShotDirection()
    {
        Vector3 direction = playerCamera.transform.forward;

        // Применяем разброс в зависимости от режима
        switch (settings.fireMode)
        {
            case FireMode.Single:
            case FireMode.Auto:
            case FireMode.Burst:
                direction = ApplySpread(direction, _currentSpreadAngle * Mathf.Deg2Rad);
                break;
        }

        return direction;
    }
    #endregion

    #region Reload

    public void Reload()
    {
        if (_isReloading || _currentAmmo == settings.maxAmmo || !_isInit) return;

        if (reloadCoroutine == null && gameObject.activeSelf)
        {
            reloadCoroutine = StartCoroutine(ReloadCoroutine());
        }
        else
        {
            //StopCoroutine(reloadCoroutine);
        }
    }

    private IEnumerator ReloadCoroutine()
    {
        _isReloading = true;
        StopShooting();

        // Сбрасываем таймер и прогресс
        _reloadTimer = 0f;
        _reloadProgress = 0f;

        if (settings.reloadSound)
        {
            AudioSource.PlayClipAtPoint(settings.reloadSound, transform.position);
        }

        // Ждем с обновлением прогресса
        while (_reloadTimer < settings.reloadTime)
        {
            _reloadTimer += Time.deltaTime;
            _reloadProgress = Mathf.Clamp01(_reloadTimer / settings.reloadTime);

            // Обновляем UI каждый кадр
            uIController.UpdateReloadProgress(_reloadProgress);

            yield return null; // Ждем следующий кадр
        }

        // Завершение перезарядки
        _currentAmmo = settings.maxAmmo;
        _isReloading = false;
        _reloadProgress = 1f;
        reloadCoroutine = null;

        // Сбрасываем разброс после перезарядки
        _currentSpreadAngle = _baseSpreadAngle;

        // Финальное обновление UI
        uIController.UpdateAmmoTxt(_currentAmmo, settings.maxAmmo);
        uIController.UpdateReloadProgress(1f);
    }
    #endregion

    #region Spread

    //Восстановление разброса
    private void UpdateSpreadRecovery()
    {
        if (!_isShooting && _currentSpreadAngle > _baseSpreadAngle)
        {
            _currentSpreadAngle = Mathf.Max(_baseSpreadAngle,
                _currentSpreadAngle - settings.spreadRecoveryRate * Time.deltaTime);
        }
    }

    //Увеличиваем разброс
    private void IncreaseSpread()
    {
        _currentSpreadAngle = Mathf.Min(settings.maxSpreadAngle,
            _currentSpreadAngle + settings.spreadIncreasePerShot);
    }

    private Vector3 ApplySpread(Vector3 direction, float spreadRad)
    {
        // Случайный угол и величина разброса
        float randomAngle = Random.Range(0f, 2f * Mathf.PI);
        float randomSpread = Random.Range(0f, spreadRad);

        // Создаем вектор смещения
        Vector3 spreadOffset = new Vector3(
            Mathf.Sin(randomAngle) * randomSpread,
            Mathf.Cos(randomAngle) * randomSpread,
            0f
        );

        // Применяем поворот
        Quaternion spreadRotation = Quaternion.Euler(spreadOffset * Mathf.Rad2Deg);
        return spreadRotation * direction;
    }

    #endregion Spread

    #region Hit
    private void HandleHit(RaycastHit hit)
    {
        //Debug.Log("Попали");
        if (hit.collider.CompareTag("Enemy"))
        {
            EnemyBase enemy = hit.collider.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(settings.damage, hit.collider);
            }
            else
            {
                enemy = hit.collider.GetComponentInParent<EnemyBase>();
                if (enemy != null)
                {
                    enemy.TakeDamage(settings.damage, hit.collider);
                }
            }
        }
    }


    private void PlayShootEffects()
    {
        if (settings.shootSound)
        {
            AudioSource.PlayClipAtPoint(settings.shootSound, transform.position);
        }
    }

    #endregion 

    #region Gizmos
    private void OnDrawGizmos()
    {
        if (!drawSpreadGizmo || !Application.isPlaying || playerCamera == null) return;

        DrawSpreadGizmo();
    }

    private void DrawSpreadGizmo()
    {
        Vector3 origin = playerCamera.transform.position;
        Vector3 direction = playerCamera.transform.forward;
        float distance = 10f; // Дистанция для визуализации

        // Цвет в зависимости от текущего разброса
        float spreadRatio = _currentSpreadAngle / settings.maxSpreadAngle;
        Color color = Color.Lerp(Color.green, Color.red, spreadRatio);
        color.a = 0.3f;
        Gizmos.color = color;

        // Рисуем конус разброса
        if (settings.fireMode == FireMode.Single)
        {
            // Точный выстрел - тонкая линия
            Gizmos.DrawRay(origin, direction * distance);
        }
        else
        {
            // Конус разброса
            float radius = Mathf.Tan(_currentSpreadAngle * Mathf.Deg2Rad) * distance;

            // Основание конуса
            Vector3 coneBase = origin + direction * distance;

            // Рисуем линии от origin к краям основания
            int segments = 12;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2 / segments;
                Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
                Vector3 worldOffset = playerCamera.transform.TransformDirection(offset);
                Gizmos.DrawLine(origin, coneBase + worldOffset);
            }

            // Рисуем окружность в основании конуса
            for (int i = 0; i < segments; i++)
            {
                float angle1 = i * Mathf.PI * 2 / segments;
                float angle2 = (i + 1) * Mathf.PI * 2 / segments;

                Vector3 point1 = new Vector3(Mathf.Cos(angle1) * radius, Mathf.Sin(angle1) * radius, 0f);
                Vector3 point2 = new Vector3(Mathf.Cos(angle2) * radius, Mathf.Sin(angle2) * radius, 0f);

                Vector3 worldPoint1 = coneBase + playerCamera.transform.TransformDirection(point1);
                Vector3 worldPoint2 = coneBase + playerCamera.transform.TransformDirection(point2);

                Gizmos.DrawLine(worldPoint1, worldPoint2);
            }
        }
    }
    #endregion

    public void SetAmmo(int ammo)
    {
        _currentAmmo = Mathf.Clamp(ammo, 0, settings.maxAmmo);
    }

    // Метод для принудительной остановки всех корутин
    public void ForceStop()
    {
        StopAllCoroutines();
        _isShooting = false;
        _isReloading = false;
        burstCoroutine = null;
        reloadCoroutine = null;
        _savedAmmo = _currentAmmo;
    }

    public void ForceStopAndHide()
    {
        ForceStop();
        gameObject.SetActive(false);
    }
}
