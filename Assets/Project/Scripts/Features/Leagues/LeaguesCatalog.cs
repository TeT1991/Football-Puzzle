using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Football Puzzle/Leagues catalog")]
public class LeaguesCatalog : ScriptableObject
{
    [SerializeField] private List<LeagueDefinition> _catalog;

    public IReadOnlyList<LeagueDefinition> Catalog => _catalog;

    public LeagueDefinition GetById(int id)
    {

        foreach (LeagueDefinition league in _catalog)
        {
            if (league.ID == id)
            {
                return league;
            }
        }

        throw new System.Exception($"League id {id} not found");
    }
}
