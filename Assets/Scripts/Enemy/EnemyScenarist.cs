using UnityEngine;

public class EnemyScenarist : InitializedBehaviour
{
    [Header("Enemys")]
    [SerializeField] private EnemyBase[] enemys;

    [Header("Components")]
    private PlayerController _playerController;
    private Transform _playerTr;

    public override void Entry(params object[] dependencies)
    {
        _playerController = GetDependency<PlayerController>(dependencies);
        _playerTr = _playerController.gameObject.transform;

        InitAllEnemys();
    }

    private void InitAllEnemys()
    {
        foreach (var enemy in enemys)
        {
            enemy.Initialize(_playerTr);
        }
    }

    public void GameEnd()
    {
        foreach (var enemy in enemys)
        {
            enemy.GameEnd();
        }
    }
}
