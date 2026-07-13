using UnityEngine;
using UnityEngine.UI;

public class StarView : MonoBehaviour
{
    [SerializeField] private Image _full;

    public void ShowStar()
    {
        _full.gameObject.SetActive(true);
    }

    public void HideStar()
    {
        _full.gameObject.SetActive(false);
    }
}
