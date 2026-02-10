using Obsydian.Core.Math;
using Obsydian.Graphics;
using Obsydian.Input;

namespace Obsydian.UI;

/// <summary>
/// Base class for all UI elements. Provides layout, hit testing, and hierarchy.
/// </summary>
/// <summary>
/// Spacing around an element (top, right, bottom, left).
/// </summary>
public readonly record struct Thickness(float Top, float Right, float Bottom, float Left)
{
    public Thickness(float uniform) : this(uniform, uniform, uniform, uniform) { }
    public Thickness(float vertical, float horizontal) : this(vertical, horizontal, vertical, horizontal) { }
    public float TotalHorizontal => Left + Right;
    public float TotalVertical => Top + Bottom;
}

/// <summary>
/// Alignment options for child elements within a layout container.
/// </summary>
public enum HorizontalAlignment { Start, Center, End, Stretch }
public enum VerticalAlignment { Start, Center, End, Stretch }

public abstract class UIElement
{
    public string? Id { get; set; }
    public Rect Bounds { get; set; }
    public bool Visible { get; set; } = true;
    public bool Enabled { get; set; } = true;
    public UIElement? Parent { get; internal set; }
    public Thickness Margin { get; set; }
    public Vec2 MinSize { get; set; }
    public Vec2 MaxSize { get; set; } = new(float.MaxValue, float.MaxValue);
    public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Stretch;
    public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Start;

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
