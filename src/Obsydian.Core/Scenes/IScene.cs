namespace Obsydian.Core.Scenes;

/// <summary>
/// A discrete game state — title screen, gameplay, pause menu, etc.
/// Managed by SceneManager for transitions.
/// </summary>
public interface IScene
{
    /// <summary>Called when the scene becomes active.</summary>
    void Enter();

    /// <summary>Called when the scene is deactivated (another scene pushed or this switched out).</summary>
    void Exit();

    /// <summary>Called every frame.</summary>
    void Update(float deltaTime);

    /// <summary>Called every frame for rendering.</summary>
    void Render(float deltaTime);
}
