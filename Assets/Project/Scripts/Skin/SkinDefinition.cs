using UnityEngine;

[CreateAssetMenu(menuName = "Football Puzzle/Skin")]
public class SkinDefinition : ScriptableObject
{
    [SerializeField] private int _id;
    [SerializeField] private bool _isLocked;
    [SerializeField] private string _name;
    public int ID => _id;
    public bool IsLocked => _isLocked;

    public string Name => _name;

    public void Unlock()
    {
        _isLocked = false;
    }
}
