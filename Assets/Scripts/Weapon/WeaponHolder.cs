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

    // ����� ��� �������������� ������������� (���� ����� �����)
    public void Initialize(Camera cam, PlayerUIController uiController)
    {
        playerCamera = cam;
        playerUIController = uiController;
        weaponParent = transform;
        CreateWeapons();

        // ������������������ ��� ������
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
        // ������� ���������� ������ �� ��������
        for (int i = 0; i < weaponPrefabs.Count; i++)
        {
            if (weaponPrefabs[i] != null && weaponPrefabs[i] != null)
            {
                CreateWeaponInstance(i);
            }
            else
            {
                Debug.LogError($"�������� ������ ��� ������ � �������� {i}");
            }
        }
    }

    private void CreateWeaponInstance(int index)
    {
        if (_weaponInstances.ContainsKey(index)) return;

        // ������� ��������� �� �������
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
            Debug.LogWarning($"�������� ������ ������: {weaponIndex}");
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

    // ����� ��� �������� ������� ������ �� �������
    public bool HasWeapon(int index)
    {
        return index >= 0 && index < weaponPrefabs.Count && weaponPrefabs[index] != null;
    }

    // ����� ��� ��������� �������� ���������� �������� ������ �� �������
    public int GetWeaponAmmo(int index)
    {
        if (_weaponInstances.ContainsKey(index))
        {
            return _weaponInstances[index].CurrentAmmo;
        }
        return 0;
    }

    // ����� ��� ���������� ������ ������ �� ����� ����
    public void AddWeapon(GameObject newWeaponPrefab)
    {
        if (newWeaponPrefab == null || newWeaponPrefab == null) return;

        weaponPrefabs.Add(newWeaponPrefab);
        int newIndex = weaponPrefabs.Count - 1;
        CreateWeaponInstance(newIndex);
    }

    // ����� ��� �������� ������
    public void RemoveWeapon(int index)
    {
        if (index < 0 || index >= weaponPrefabs.Count) return;

        // ���� ������� ������� ������ - ������������� �� ������
        if (index == _currentWeaponIndex)
        {
            DisableCurrWeapon();
            _currentWeapon = null;
            _currentWeaponIndex = -1;

            // �������� ������������� �� ��������� ������
            if (weaponPrefabs.Count > 1)
            {
                int nextIndex = (index + 1) % weaponPrefabs.Count;
                EquipWeapon(nextIndex);
            }
        }

        // ������� ��������� ������
        if (_weaponInstances.ContainsKey(index))
        {
            Destroy(_weaponInstances[index].gameObject);
            _weaponInstances.Remove(index);
        }

        weaponPrefabs.RemoveAt(index);

        // ��������������� �������
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

    // ����� ��� ��������� ���� ����������� ������ (��� ����������)
    public Dictionary<int, Weapon> GetAllWeaponInstances()
    {
        return new Dictionary<int, Weapon>(_weaponInstances);
    }

    // ����� ��� ��������, ��������������� �� holder
    public bool IsInitialized()
    {
        return playerCamera != null && playerUIController != null;
    }
}
