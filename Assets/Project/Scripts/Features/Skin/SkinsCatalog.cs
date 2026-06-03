using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Football Puzzle/Skins catalog")]
public class SkinsCatalog : ScriptableObject
{
    [SerializeField] private List<SkinDefinition> _catalog;

    public IReadOnlyList<SkinDefinition> Catalog => _catalog;

    public SkinDefinition GetById (int id)
    {

        foreach (SkinDefinition skin in _catalog)
        {
            if (skin.ID == id)
            {
                return skin;
            }
        }

        throw new System.Exception($"Skin id {id} not found");
    }
}
