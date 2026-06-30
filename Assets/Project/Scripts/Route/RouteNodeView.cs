using System.Collections.Generic;
using UnityEngine;

public class RouteNodeView : MonoBehaviour
{
    [SerializeField] private List<SpriteRenderer> _glowGroupPainter;
    [SerializeField] private SpriteRenderer _rightLine;
    [SerializeField] private SpriteRenderer _rightGlow;
    [SerializeField] private SpriteRenderer _downLine;
    [SerializeField] private SpriteRenderer _downGlow;
    [SerializeField] private SpriteRenderer _leftLine;
    [SerializeField] private SpriteRenderer _leftGlow;
    [SerializeField] private SpriteRenderer _upLine;
    [SerializeField] private SpriteRenderer _upGlow;

    public void SetGlowColor(Color color)
    {
        foreach (SpriteRenderer spriteRenderer in _glowGroupPainter)
        {
            spriteRenderer.color = color;
        }
    }

    public void ShowRoutes(RouteNodeConnections connections)
    {
        _rightLine.enabled = connections.HasFlag(RouteNodeConnections.Right);
        _rightGlow.enabled = connections.HasFlag(RouteNodeConnections.Right);
        _downLine.enabled = connections.HasFlag(RouteNodeConnections.Down);
        _downGlow.enabled = connections.HasFlag(RouteNodeConnections.Down);
        _leftLine.enabled = connections.HasFlag(RouteNodeConnections.Left);
        _leftGlow.enabled = connections.HasFlag(RouteNodeConnections.Left);
        _upLine.enabled = connections.HasFlag(RouteNodeConnections.Up);
        _upGlow.enabled = connections.HasFlag(RouteNodeConnections.Up);
    }
}

[System.Flags]
public enum RouteNodeConnections
{
    None = 0,
    Right = 1,
    Down = 2,
    Left = 4,
    Up = 8,
}
