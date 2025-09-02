using System.Linq;
using UnityEngine;

public abstract class InitializedBehaviour : MonoBehaviour, IInitialized
{
    public abstract void Entry(params object[] dependencies);

    // Вспомогательные методы для извлечения зависимостей
    protected T GetDependency<T>(params object[] dependencies) where T : class
    {
        return dependencies.OfType<T>().FirstOrDefault();
    }
}
