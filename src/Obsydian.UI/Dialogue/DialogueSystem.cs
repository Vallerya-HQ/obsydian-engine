namespace Obsydian.UI.Dialogue;

/// <summary>
/// A single node in a dialogue tree. Contains text, optional speaker name,
/// and choices that link to other nodes.
/// </summary>
public sealed class DialogueNode
{
    public string Id { get; set; } = "";
    public string Speaker { get; set; } = "";
    public string Text { get; set; } = "";

    /// <summary>Choices the player can pick. Empty = auto-advance to NextNodeId.</summary>
    public List<DialogueChoice> Choices { get; set; } = [];

    /// <summary>Next node if there are no choices (linear dialogue).</summary>
    public string? NextNodeId { get; set; }

    /// <summary>Optional condition expression evaluated by the game.</summary>
    public string? Condition { get; set; }
}

/// <summary>
/// A player choice within a dialogue node.
/// </summary>
public sealed class DialogueChoice
{
    public string Text { get; set; } = "";
    public string TargetNodeId { get; set; } = "";

    /// <summary>Optional condition — choice is hidden if condition fails.</summary>
    public string? Condition { get; set; }
}

/// <summary>
/// A complete dialogue conversation. Holds nodes keyed by ID.
/// Can be loaded from JSON via JsonDataLoader.
/// </summary>
public sealed class DialogueTree
{
    public string Id { get; set; } = "";
    public string StartNodeId { get; set; } = "start";
    public Dictionary<string, DialogueNode> Nodes { get; set; } = [];

    public DialogueNode? GetNode(string nodeId) =>
        Nodes.GetValueOrDefault(nodeId);

    /// <summary>Build from a list of nodes (e.g., deserialized from JSON).</summary>
    public static DialogueTree FromNodes(string id, string startNodeId, IEnumerable<DialogueNode> nodes)
    {
        var tree = new DialogueTree { Id = id, StartNodeId = startNodeId };
        foreach (var node in nodes)
            tree.Nodes[node.Id] = node;
        return tree;
    }
}

/// <summary>
/// Runs a dialogue tree, tracking current node and handling player choices.
/// Game code drives this by calling Advance() or SelectChoice().
/// </summary>
public sealed class DialogueRunner
{
    public DialogueTree? CurrentTree { get; private set; }
    public DialogueNode? CurrentNode { get; private set; }
    public bool IsActive => CurrentNode is not null;

    /// <summary>Predicate to evaluate dialogue conditions. Return true if condition is met.</summary>
    public Func<string, bool>? ConditionEvaluator { get; set; }

    /// <summary>Fired when a new node is displayed.</summary>
    public event Action<DialogueNode>? OnNodeEntered;

    /// <summary>Fired when the dialogue ends (no more nodes).</summary>
    public event Action? OnDialogueEnded;

    /// <summary>Start a dialogue from its first node.</summary>
    public void Start(DialogueTree tree)
    {
        CurrentTree = tree;
        EnterNode(tree.StartNodeId);
    }

    /// <summary>
    /// Advance to the next node (for linear dialogue with no choices).
    /// </summary>
    public void Advance()
    {
        if (CurrentNode?.NextNodeId is not null)
            EnterNode(CurrentNode.NextNodeId);
        else if (CurrentNode?.Choices.Count == 0)
            End();
    }

    /// <summary>
    /// Select a choice by index. Advances to that choice's target node.
    /// </summary>
    public void SelectChoice(int index)
    {
        if (CurrentNode is null) return;

        var available = GetAvailableChoices();
        if (index >= 0 && index < available.Count)
            EnterNode(available[index].TargetNodeId);
    }

    /// <summary>Get choices filtered by conditions.</summary>
    public List<DialogueChoice> GetAvailableChoices()
    {
        if (CurrentNode is null) return [];

        return CurrentNode.Choices
            .Where(c => c.Condition is null || (ConditionEvaluator?.Invoke(c.Condition) ?? true))
            .ToList();
    }

    public void End()
    {
        CurrentNode = null;
        CurrentTree = null;
        OnDialogueEnded?.Invoke();
    }

    private void EnterNode(string nodeId)
    {
        if (CurrentTree is null) return;

        var node = CurrentTree.GetNode(nodeId);
        if (node is null)
        {
            End();
            return;
        }

        // Check condition
        if (node.Condition is not null && ConditionEvaluator is not null && !ConditionEvaluator(node.Condition))
        {
            // Skip conditional node — advance to next
            if (node.NextNodeId is not null)
                EnterNode(node.NextNodeId);
            else
                End();
            return;
        }

        CurrentNode = node;
        OnNodeEntered?.Invoke(node);
    }
}
