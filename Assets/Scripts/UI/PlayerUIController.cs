using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerUIController : InitializedBehaviour
{

    [Header("Health Settings")]
    [SerializeField] private TMP_Text currHealthTxt;

    [Header("Aim Settings")]
    [SerializeField] private RectTransform aimTarget;
    [SerializeField] private float minAimSize = 75f;
    [SerializeField] private float maxAimSize = 200f;
    [SerializeField] private float lerpSpeed = 0.5f;
    [SerializeField] private float spreadProportionCoefficient = 1.2f;

    [Header("Aim Hit Settings")]
    [SerializeField] private Animation aimHitTargetAnim;
    [SerializeField] private Animation aimHitHeadTargetAnim;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverView;
    [SerializeField] private TMP_Text currScoreTxt;
    [SerializeField] private TMP_Text maxScoreTxt;
    [SerializeField] private Button restartBtn;

    [Header("Record Settings")]
    [SerializeField] private TMP_Text recordTxt;

    [Header("Ammo Settings")]
    [SerializeField] private TMP_Text ammoTxt;
    [SerializeField] private GameObject reloadPanel;
    [SerializeField] private Image realoadFillImg;

    [Header("WeaponStats Settings")]
    [SerializeField] private TMP_Text weaponName;

    private Weapon _currentWeapon;
    private bool _isWeaponActived = false;
    private float _currentAimSize;

    public override void Entry(params object[] dependencies)
    {
        UpdateReloadProgress(1f);
        gameOverView.SetActive(false);

        restartBtn.onClick.AddListener(RestartBtnClick);
        EnemyBase.onGotDamage += PlayHitAnim;
    }

    void Update()
    {
        if (_currentWeapon == null || aimTarget == null || !_isWeaponActived) return;

        UpdateAimSizeWithLerp();
    }

    public void SetCurrentWeapon(Weapon weapon)
    {
        _currentWeapon = weapon;
        _isWeaponActived = true;
        _currentAimSize = GetTargetAimSize();
        aimTarget.sizeDelta = new Vector2(_currentAimSize, _currentAimSize);
        weaponName.text = _currentWeapon.Settings.weaponName;
        UpdateAmmoTxt(_currentWeapon.CurrentAmmo, _currentWeapon.Settings.maxAmmo);
    }

    #region Aim Settings
    private void UpdateAimSizeWithLerp()
    {
        float targetAimSize = GetTargetAimSize();

        // ѕлавна€ интерпол€ци€ с помощью Lerp
        _currentAimSize = Mathf.Lerp(
            _currentAimSize,
            targetAimSize,
            lerpSpeed * Time.deltaTime
        );

        aimTarget.sizeDelta = new Vector2(_currentAimSize, _currentAimSize);
    }

    private float GetTargetAimSize()
    {
        //if (_currentWeapon.Settings.fireMode == FireMode.Single)
            //return minAimSize;

        float spreadNormalized = _currentWeapon.CurrentSpreadAngle / _currentWeapon.Settings.maxSpreadAngle;
        float proportion = spreadNormalized * spreadProportionCoefficient;

        return minAimSize + (maxAimSize - minAimSize) * Mathf.Clamp01(proportion);
    }
    #endregion

    #region Ammo

    public void UpdateAmmoTxt(int currAmmo, int maxAmmo)
    {
        ammoTxt.text = $"{currAmmo}/{maxAmmo}";
    }

    public void UpdateReloadProgress(float progress)
    {
        reloadPanel.SetActive(!(progress >= 1f));
        realoadFillImg.fillAmount = progress;
    }

    public void ResetReloadProgress()
    {
        reloadPanel.SetActive(false);
        realoadFillImg.fillAmount = 0f;
    }
    #endregion

    #region Health

    public void SetCurrHealth(float currHealth)
    {
        currHealthTxt.text = $"{currHealth}";
    }

    #endregion

    #region Record

    public void UpdateRecord(int currRecord)
    {
        recordTxt.text = $"{currRecord}";
    }

    #endregion

    #region Game Over

    public void OpenGameOverView(int currentScore, int maxScore)
    {
        if (gameOverView.activeSelf) return;

        currScoreTxt.text = $"{currentScore}";
        maxScoreTxt.text = $"{maxScore}";
        gameOverView.SetActive(true);
    }

    private void RestartBtnClick()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    #endregion

    #region Target Hit

    public void PlayHitAnim(bool isHead = false)
    {
        if (isHead)
        {
            aimHitHeadTargetAnim.Play();
        }
        else
        {
            aimHitTargetAnim.Play();
        }
    }

    #endregion

    private void OnDestroy()
    {
        EnemyBase.onGotDamage -= PlayHitAnim;
        restartBtn.onClick.RemoveListener(RestartBtnClick);
    }
}
