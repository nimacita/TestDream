using UnityEngine;

public class GameScenarist : InitializedBehaviour
{

    private PlayerController _playerController;
    private PlayerUIController _playerUIController;
    private EnemyScenarist _enemyScenarist;
    private CursorController _cursorController;

    [Header("Record Settings")]
    private int _currScore;
    private int MaxScore
    {
        get
        {
            return PlayerPrefs.GetInt("MaxRecord", 0);
        }
        set
        {
            PlayerPrefs.SetInt("MaxRecord", value);
        }
    }

    public override void Entry(params object[] dependencies)
    {
        _playerController = GetDependency<PlayerController>(dependencies);
        _enemyScenarist = GetDependency<EnemyScenarist>(dependencies);
        _playerUIController = GetDependency<PlayerUIController>(dependencies);
        _cursorController = GetDependency<CursorController>(dependencies);

        _playerController.onPlayerDieded += GameEnd;
        EnemyBase.onEnemyDied += PlusRecord;
    }

    private void PlusRecord()
    {
        _currScore++;
        if(_currScore > MaxScore) MaxScore = _currScore;
        _playerUIController.UpdateRecord(_currScore);
    }

    private void GameEnd()
    {
        _cursorController.UnlockCursor();
        _enemyScenarist.GameEnd();
        _playerUIController.OpenGameOverView(_currScore, MaxScore);
    }

    private void OnDestroy()
    {
        _playerController.onPlayerDieded -= GameEnd;
        EnemyBase.onEnemyDied -= PlusRecord;
    }
}
