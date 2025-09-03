using UnityEngine;

public class GameScenarist : InitializedBehaviour
{

    private PlayerController _playerController;
    private EnemyScenarist _enemyScenarist;

    public override void Entry(params object[] dependencies)
    {
        _playerController = GetDependency<PlayerController>(dependencies);
        _enemyScenarist = GetDependency<EnemyScenarist>(dependencies);

        _playerController.onPlayerDieded += GameEnd;
    }

    private void GameEnd()
    {
        Debug.Log("Игра закончена");
        _enemyScenarist.GameEnd();
    }

    private void OnDestroy()
    {
        _playerController.onPlayerDieded -= GameEnd;
    }
}
