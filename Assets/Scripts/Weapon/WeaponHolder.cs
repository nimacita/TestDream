using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    [Header("Weapon Prefabs")]
    [SerializeField] private List<GameObject> weaponPrefabs = new List<GameObject>();
    private Transform weaponParent;
    private Camera playerCamera;
    private PlayerUIController playerUIController;

    [Header("Settings")]
    private Dictionary<int, Weapon> _weaponInstances = new Dictionary<int, Weapon>();
    private Weapon _currentWeapon;
    private int _currentWeaponIndex = -1;

    public Weapon CurrentWeapon => _currentWeapon;
    public int WeaponCount => weaponPrefabs.Count;

    // Метод для принудительной инициализации (если нужно извне)
    public void Initialize(Camera cam, PlayerUIController uiController)
    {
        playerCamera = cam;
        playerUIController = uiController;
        weaponParent = transform;
        CreateWeapons();

        // Переинициализируем все оружия
        foreach (var weaponInstance in _weaponInstances.Values)
        {
            if (weaponInstance.gameObject.activeInHierarchy)
            {
                weaponInstance.Initialize(playerCamera, playerUIController);
            }
        }

    }

    private void CreateWeapons()
    {
        // Создаем экземпляры оружия из префабов
        for (int i = 0; i < weaponPrefabs.Count; i++)
        {
            if (weaponPrefabs[i] != null && weaponPrefabs[i] != null)
            {
                CreateWeaponInstance(i);
            }
            else
            {
                Debug.LogError($"Неполные данные для оружия с индексом {i}");
            }
        }
    }

    private void CreateWeaponInstance(int index)
    {
        if (_weaponInstances.ContainsKey(index)) return;

        // Создаем экземпляр из префаба
        GameObject weaponObj = Instantiate(weaponPrefabs[index], weaponParent);
        weaponObj.transform.localPosition = Vector3.zero;
        weaponObj.transform.localRotation = Quaternion.identity;
        Weapon weaponComponent = weaponObj.GetComponent<Weapon>();

        _weaponInstances[index] = weaponComponent;
        weaponObj.SetActive(false);
    }

    public void EquipWeapon(int weaponIndex)
    {
        if (weaponIndex < 0 || weaponIndex >= weaponPrefabs.Count)
        {
            Debug.LogWarning($"Неверный индекс оружия: {weaponIndex}");
            return;
        }

        if (weaponIndex == _currentWeaponIndex) return;

        if (_currentWeapon != null)
        {
            DisableCurrWeapon();
            playerUIController.ResetReloadProgress();
        }

        if (!_weaponInstances.ContainsKey(weaponIndex))
        {
            CreateWeaponInstance(weaponIndex);
        }

        _currentWeapon = _weaponInstances[weaponIndex];
        _currentWeaponIndex = weaponIndex;

        _currentWeapon.Initialize(playerCamera, playerUIController, _currentWeapon.SavedAmmo);

        playerUIController.SetCurrentWeapon(_currentWeapon);
        _currentWeapon.gameObject.SetActive(true);
        if (_currentWeapon.CurrentAmmo == 0) _currentWeapon.Reload();
    }

    private void DisableCurrWeapon()
    {
        if (_currentWeapon != null)
        {
            _currentWeapon.ForceStop();
            _currentWeapon.gameObject.SetActive(false);
        }
    }

    public void EquipWeapon1() => EquipWeapon(0);
    public void EquipWeapon2() => EquipWeapon(1);
    public void EquipWeapon3() => EquipWeapon(2);
    public void EquipWeapon4() => EquipWeapon(3);

    public void EquipNextWeapon()
    {
        if (weaponPrefabs.Count == 0) return;

        int nextIndex = (_currentWeaponIndex + 1) % weaponPrefabs.Count;
        EquipWeapon(nextIndex);
    }

    public void EquipPreviousWeapon()
    {
        if (weaponPrefabs.Count == 0) return;

        int previousIndex = (_currentWeaponIndex - 1 + weaponPrefabs.Count) % weaponPrefabs.Count;
        EquipWeapon(previousIndex);
    }

    // Метод для проверки наличия оружия по индексу
    public bool HasWeapon(int index)
    {
        return index >= 0 && index < weaponPrefabs.Count && weaponPrefabs[index] != null;
    }

    // Метод для получения текущего количества патронов оружия по индексу
    public int GetWeaponAmmo(int index)
    {
        if (_weaponInstances.ContainsKey(index))
        {
            return _weaponInstances[index].CurrentAmmo;
        }
        return 0;
    }

    // Метод для добавления нового оружия во время игры
    public void AddWeapon(GameObject newWeaponPrefab)
    {
        if (newWeaponPrefab == null || newWeaponPrefab == null) return;

        weaponPrefabs.Add(newWeaponPrefab);
        int newIndex = weaponPrefabs.Count - 1;
        CreateWeaponInstance(newIndex);
    }

    // Метод для удаления оружия
    public void RemoveWeapon(int index)
    {
        if (index < 0 || index >= weaponPrefabs.Count) return;

        // Если удаляем текущее оружие - переключаемся на другое
        if (index == _currentWeaponIndex)
        {
            DisableCurrWeapon();
            _currentWeapon = null;
            _currentWeaponIndex = -1;

            // Пытаемся переключиться на следующее оружие
            if (weaponPrefabs.Count > 1)
            {
                int nextIndex = (index + 1) % weaponPrefabs.Count;
                EquipWeapon(nextIndex);
            }
        }

        // Удаляем экземпляр оружия
        if (_weaponInstances.ContainsKey(index))
        {
            Destroy(_weaponInstances[index].gameObject);
            _weaponInstances.Remove(index);
        }

        weaponPrefabs.RemoveAt(index);

        // Переиндексируем словарь
        ReindexWeaponInstances();
    }

    private void ReindexWeaponInstances()
    {
        Dictionary<int, Weapon> newInstances = new Dictionary<int, Weapon>();

        for (int i = 0; i < weaponPrefabs.Count; i++)
        {
            foreach (var kvp in _weaponInstances)
            {
                if (kvp.Value.Settings == weaponPrefabs[i].GetComponent<Weapon>().Settings)
                {
                    newInstances[i] = kvp.Value;
                    break;
                }
            }
        }

        _weaponInstances = newInstances;
    }

    // Метод для получения всех экземпляров оружия (для сохранения)
    public Dictionary<int, Weapon> GetAllWeaponInstances()
    {
        return new Dictionary<int, Weapon>(_weaponInstances);
    }

    // Метод для проверки, инициализирован ли holder
    public bool IsInitialized()
    {
        return playerCamera != null && playerUIController != null;
    }
}
