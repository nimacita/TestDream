using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EntryPoint : MonoBehaviour
{
    [Header("All Dependencies")]
    [SerializeField] private List<MonoBehaviour> allDependencies = new List<MonoBehaviour>();

    private void Awake()
    {
        InitializeAll();
    }

    private void InitializeAll()
    {
        object[] dependenciesArray = allDependencies.Cast<object>().ToArray();

        foreach (var dependency in allDependencies)
        {
            if (dependency is IInitialized initialized)
            {
                initialized.Entry(dependenciesArray);
            }
        }
    }
}
