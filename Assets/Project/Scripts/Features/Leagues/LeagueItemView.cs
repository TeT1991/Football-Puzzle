using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeagueItemView : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI _leagueName;

    public void Init(string name)
    {
        _leagueName.text = name;
    }
}
