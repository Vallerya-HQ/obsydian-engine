using Obsydian.Core.Math;
using Obsydian.Graphics;
using Obsydian.Input;

namespace Obsydian.UI;

/// <summary>
/// Base class for all UI elements. Provides layout, hit testing, and hierarchy.
/// </summary>
public abstract class UIElement
{
    public string? Id { get; set; }
    public Rect Bounds { get; set; }
    public bool Visible { get; set; } = true;
    public bool Enabled { get; set; } = true;
    public UIElement? Parent { get; internal set; }

    private readonly List<UIElement> _children = [];
    public IReadOnlyList<UIElement> Children => _children;

    public void AddChild(UIElement child)
    {
        child.Parent = this;
        _children.Add(child);
    }

    public void RemoveChild(UIElement child)
    {
        child.Parent = null;
        _children.Remove(child);
    }

    public virtual bool HitTest(Vec2 point) => Visible && Bounds.Contains(point);

    public virtual void Update(float deltaTime, InputManager input)
    {
        if (!Visible) return;
        foreach (var child in _children)
            child.Update(deltaTime, input);
    }

    public virtual void Draw(IRenderer renderer)
    {
        if (!Visible) return;
        foreach (var child in _children)
            child.Draw(renderer);
    }
}
