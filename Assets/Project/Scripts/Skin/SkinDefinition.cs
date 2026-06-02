using UnityEngine;

[CreateAssetMenu(menuName = "Football Puzzle/Skin")]
public class SkinDefinition : ScriptableObject
{
    [SerializeField] private int _id;
    [SerializeField] private int _price;
    [SerializeField] private string _name;

    public int ID => _id;

    public int Price => _price;

    public string Name => _name;

}
