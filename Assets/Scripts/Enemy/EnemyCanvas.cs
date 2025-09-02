using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class EnemyCanvas : MonoBehaviour
{
    [Header("Canvas Settinggs")]
    [SerializeField] private Canvas enemyCanvas;
    [SerializeField] private Image fillHealthImg;

    public void Init()
    {
        enemyCanvas.gameObject.SetActive(true);
        fillHealthImg.fillAmount = 1f;
    }

    public void ChangeHealth(float currHp, float maxHp)
    {
        if (maxHp <= 0f) return;
        fillHealthImg.fillAmount = currHp / maxHp;
    }

    public void RotateCanvas(Transform targetTr)
    {
        enemyCanvas.transform.rotation = targetTr.rotation;
    }
}
