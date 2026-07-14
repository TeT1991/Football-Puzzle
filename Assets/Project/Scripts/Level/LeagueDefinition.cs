using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Football Puzzle/League")]
public class LeagueDefinition : ScriptableObject
{
    [SerializeField] private int _id;
    [SerializeField] private string _name;
    [SerializeField] private bool _isUnlocked = false;
    [SerializeField] private List<LevelDefinition> _levels;
    [SerializeField] private MetamapLocationView _metamaplocationView;

    public int ID => _id;
    public string Name => _name;
    public bool IsUnlocked => _isUnlocked;
    public IReadOnlyList<LevelDefinition> Levels => _levels;
    public MetamapLocationView MetamapLocationView => _metamaplocationView;
}
