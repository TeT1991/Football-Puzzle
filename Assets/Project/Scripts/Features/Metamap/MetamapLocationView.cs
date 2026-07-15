using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D;

public class MetamapLocationView : MonoBehaviour
{
    [SerializeField] private Transform _entryPointPosition;
    [SerializeField] private Transform _exitPointPosition;
    [SerializeField] private SortingGroup _sortingGroup;
    [SerializeField] private SpriteShapeController _path;
    [SerializeField] private Renderer _renderer;
    [SerializeField] private LevelMarkerPoints _levelMarkerPoints;

    public Transform EntryPointPosition => _entryPointPosition;
    public Transform ExitPointPosition => _exitPointPosition;
    public SortingGroup SortingGroup => _sortingGroup;

    public SpriteShapeController Path => _path;
    public Renderer Renderer => _renderer;

}
