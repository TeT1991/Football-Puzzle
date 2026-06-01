using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Football Puzzle/Skins catalog")]
public class SkinsCatalog : ScriptableObject
{
    [SerializeField] private List<SkinDefinition> _catalog;

    public IReadOnlyList<SkinDefinition> Catalog => _catalog;

    public SkinDefinition GetById (int id)
    {
        return _catalog[id];
    }
}
