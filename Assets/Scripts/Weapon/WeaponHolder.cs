using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    [Header("Weapon Setting")]
    [SerializeField] private WeaponHolderSettings settings;
    private Transform weaponParent;
    private Camera playerCamera;
    private PlayerUIController playerUIController;

    [Header("Settings")]
    private Dictionary<int, Weapon> _weaponInstances = new Dictionary<int, Weapon>();
    private Weapon _currentWeapon;
    private int _currentWeaponIndex = -1;

    public Weapon CurrentWeapon => _currentWeapon;
    public int WeaponCount => settings.weaponPrefabs.Count;

    public void Initialize(Camera cam, PlayerUIController uiController)
    {
        playerCamera = cam;
        playerUIController = uiController;
        weaponParent = transform;
        CreateWeapons();

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
        for (int i = 0; i < settings.weaponPrefabs.Count; i++)
        {
            if (settings.weaponPrefabs[i] != null && settings.weaponPrefabs[i] != null)
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

        GameObject weaponObj = Instantiate(settings.weaponPrefabs[index], weaponParent);
        weaponObj.transform.localPosition = Vector3.zero;
        weaponObj.transform.localRotation = Quaternion.identity;
        Weapon weaponComponent = weaponObj.GetComponent<Weapon>();

        _weaponInstances[index] = weaponComponent;
        weaponObj.SetActive(false);
    }

    public bool IsCurrWeapon(int currWeaponInd)
    {
        return currWeaponInd == _currentWeaponIndex;
    }

    public void EquipWeapon(int weaponIndex)
    {
        if (weaponIndex < 0 || weaponIndex >= settings.weaponPrefabs.Count)
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

    public int EquipNextWeapon()
    {
        if (settings.weaponPrefabs.Count == 0) return -1;

        int nextIndex = (_currentWeaponIndex + 1) % settings.weaponPrefabs.Count;
        return nextIndex;
        //EquipWeapon(nextIndex);
    }

    public int EquipPreviousWeapon()
    {
        if (settings.weaponPrefabs.Count == 0) return -1;

        int previousIndex = (_currentWeaponIndex - 1 + settings.weaponPrefabs.Count) % settings.weaponPrefabs.Count;
        return previousIndex;
        //EquipWeapon(previousIndex);
    }

    public bool HasWeapon(int index)
    {
        return index >= 0 && index < settings.weaponPrefabs.Count && settings.weaponPrefabs[index] != null;
    }

    public int GetWeaponAmmo(int index)
    {
        if (_weaponInstances.ContainsKey(index))
        {
            return _weaponInstances[index].CurrentAmmo;
        }
        return 0;
    }

    public void AddWeapon(GameObject newWeaponPrefab)
    {
        if (newWeaponPrefab == null || newWeaponPrefab == null) return;

        settings.weaponPrefabs.Add(newWeaponPrefab);
        int newIndex = settings.weaponPrefabs.Count - 1;
        CreateWeaponInstance(newIndex);
    }

    public void RemoveWeapon(int index)
    {
        if (index < 0 || index >= settings.weaponPrefabs.Count) return;

        if (index == _currentWeaponIndex)
        {
            DisableCurrWeapon();
            _currentWeapon = null;
            _currentWeaponIndex = -1;

            if (settings.weaponPrefabs.Count > 1)
            {
                int nextIndex = (index + 1) % settings.weaponPrefabs.Count;
                EquipWeapon(nextIndex);
            }
        }

        if (_weaponInstances.ContainsKey(index))
        {
            Destroy(_weaponInstances[index].gameObject);
            _weaponInstances.Remove(index);
        }

        settings.weaponPrefabs.RemoveAt(index);

        ReindexWeaponInstances();
    }

    private void ReindexWeaponInstances()
    {
        Dictionary<int, Weapon> newInstances = new Dictionary<int, Weapon>();

        for (int i = 0; i < settings.weaponPrefabs.Count; i++)
        {
            foreach (var kvp in _weaponInstances)
            {
                if (kvp.Value.Settings == settings.weaponPrefabs[i].GetComponent<Weapon>().Settings)
                {
                    newInstances[i] = kvp.Value;
                    break;
                }
            }
        }

        _weaponInstances = newInstances;
    }

    public Dictionary<int, Weapon> GetAllWeaponInstances()
    {
        return new Dictionary<int, Weapon>(_weaponInstances);
    }

    public bool IsInitialized()
    {
        return playerCamera != null && playerUIController != null;
    }
}
