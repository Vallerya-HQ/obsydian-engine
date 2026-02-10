using Obsydian.Core.Logging;

namespace Obsydian.Core.Scenes;

/// <summary>
/// Manages scene transitions with a stack. The top scene is active.
/// Push for overlay scenes (pause menu), Switch for full transitions.
/// </summary>
public sealed class SceneManager
{
    private readonly Stack<IScene> _scenes = new();
    private IScene? _pendingSwitch;
    private bool _pendingPop;

    /// <summary>The currently active scene (top of stack), or null if empty.</summary>
    public IScene? Current => _scenes.Count > 0 ? _scenes.Peek() : null;

    public int SceneCount => _scenes.Count;

    /// <summary>
    /// Replace the current scene with a new one.
    /// The old scene's Exit() is called, then the new scene's Enter().
    /// </summary>
    public void Switch(IScene scene)
    {
        _pendingSwitch = scene;
    }

    /// <summary>
    /// Push a new scene on top (e.g., pause menu over gameplay).
    /// The current scene's Exit() is called, then the new scene's Enter().
    /// </summary>
    public void Push(IScene scene)
    {
        Current?.Exit();
        _scenes.Push(scene);
        scene.Enter();
        Log.Debug("SceneManager", $"Pushed scene: {scene.GetType().Name} (depth: {_scenes.Count})");
    }

    /// <summary>
    /// Pop the current scene and resume the one underneath.
    /// </summary>
    public void Pop()
    {
        _pendingPop = true;
    }

    /// <summary>
    /// Process pending transitions, update, and render the active scene.
    /// Call this once per frame.
    /// </summary>
    public void Update(float deltaTime)
    {
        ProcessPending();
        Current?.Update(deltaTime);
    }

    public void Render(float deltaTime)
    {
        Current?.Render(deltaTime);
    }

    private void ProcessPending()
    {
        if (_pendingPop)
        {
            _pendingPop = false;
            if (_scenes.Count > 0)
            {
                var old = _scenes.Pop();
                old.Exit();
                Log.Debug("SceneManager", $"Popped scene: {old.GetType().Name}");
                Current?.Enter();
            }
        }

        if (_pendingSwitch is not null)
        {
            var next = _pendingSwitch;
            _pendingSwitch = null;

            // Exit and clear all current scenes
            while (_scenes.Count > 0)
            {
                var old = _scenes.Pop();
                old.Exit();
            }

            _scenes.Push(next);
            next.Enter();
            Log.Debug("SceneManager", $"Switched to scene: {next.GetType().Name}");
        }
    }
}
