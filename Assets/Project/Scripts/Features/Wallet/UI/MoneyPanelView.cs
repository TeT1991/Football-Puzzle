using TMPro;
using UnityEngine;

public class MoneyPanelView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _moneyText;

    public void SetText(int value)
    {
        _moneyText.text = value.ToString();
    }
}
