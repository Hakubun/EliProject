using TMPro;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    public static HUDManager instance;

    [SerializeField] private Canvas hudCanvas;
    [SerializeField] private TMP_Text lifeCount;
    [SerializeField] private TMP_Text currencyCount;
    private bool HUDActive = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }


    private void Start()
    {
        SetHUDActive(true);
        UpdateHUD();
    }

    public void UpdateHUD()
    {
        if (!HUDActive) return;
        SetTextLifeCount(PlayerStats.Lives);
        SetTextCurrencyCount(PlayerStats.Currency);
    }

    public void SetHUDActive(bool isActive)
    {
        HUDActive = isActive;
        hudCanvas.enabled = isActive;
        lifeCount.gameObject.SetActive(isActive);
        currencyCount.gameObject.SetActive(isActive);
    }

    public void SetTextLifeCount(int newCount)
    {
        if (HUDActive) lifeCount.text = newCount.ToString();
    }
    public void SetTextCurrencyCount(int newCount)
    {
        if (HUDActive) currencyCount.text = newCount.ToString();
    }
}

