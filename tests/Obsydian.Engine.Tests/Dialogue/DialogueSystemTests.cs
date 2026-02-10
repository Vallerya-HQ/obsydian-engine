using Obsydian.UI.Dialogue;

namespace Obsydian.Engine.Tests.Dialogue;

public class DialogueSystemTests
{
    private static DialogueTree CreateSimpleTree()
    {
        var tree = new DialogueTree { Id = "test", StartNodeId = "start" };
        tree.Nodes["start"] = new DialogueNode
        {
            Id = "start",
            Speaker = "NPC",
            Text = "Hello!",
            NextNodeId = "second"
        };
        tree.Nodes["second"] = new DialogueNode
        {
            Id = "second",
            Speaker = "NPC",
            Text = "Goodbye!"
        };
        return tree;
    }

    private static DialogueTree CreateChoiceTree()
    {
        var tree = new DialogueTree { Id = "choice_test", StartNodeId = "start" };
        tree.Nodes["start"] = new DialogueNode
        {
            Id = "start",
            Speaker = "NPC",
            Text = "Pick one:",
            Choices =
            [
                new DialogueChoice { Text = "Option A", TargetNodeId = "a" },
                new DialogueChoice { Text = "Option B", TargetNodeId = "b" }
            ]
        };
        tree.Nodes["a"] = new DialogueNode { Id = "a", Text = "You chose A!" };
        tree.Nodes["b"] = new DialogueNode { Id = "b", Text = "You chose B!" };
        return tree;
    }

    [Fact]
    public void Start_SetsCurrentNodeToFirst()
    {
        var runner = new DialogueRunner();
        var tree = CreateSimpleTree();
        runner.Start(tree);

        Assert.True(runner.IsActive);
        Assert.Equal("start", runner.CurrentNode?.Id);
        Assert.Equal("Hello!", runner.CurrentNode?.Text);
    }

    [Fact]
    public void Advance_MovesToNextNode()
    {
        var runner = new DialogueRunner();
        runner.Start(CreateSimpleTree());
        runner.Advance();

        Assert.Equal("second", runner.CurrentNode?.Id);
    }

    [Fact]
    public void Advance_OnLastNode_EndsDialogue()
    {
        var runner = new DialogueRunner();
        runner.Start(CreateSimpleTree());
        runner.Advance(); // → second
        runner.Advance(); // → end (no NextNodeId)

        Assert.False(runner.IsActive);
    }

    [Fact]
    public void SelectChoice_AdvancesToTargetNode()
    {
        var runner = new DialogueRunner();
        runner.Start(CreateChoiceTree());
        runner.SelectChoice(1); // Option B

        Assert.Equal("b", runner.CurrentNode?.Id);
        Assert.Equal("You chose B!", runner.CurrentNode?.Text);
    }

    [Fact]
    public void OnNodeEntered_Fires()
    {
        var runner = new DialogueRunner();
        var enteredNodes = new List<string>();
        runner.OnNodeEntered += node => enteredNodes.Add(node.Id);

        runner.Start(CreateSimpleTree());
        runner.Advance();

        Assert.Equal(2, enteredNodes.Count);
        Assert.Equal("start", enteredNodes[0]);
        Assert.Equal("second", enteredNodes[1]);
    }

    [Fact]
    public void OnDialogueEnded_Fires()
    {
        var runner = new DialogueRunner();
        bool ended = false;
        runner.OnDialogueEnded += () => ended = true;

        runner.Start(CreateSimpleTree());
        runner.Advance();
        runner.Advance();

        Assert.True(ended);
    }

    [Fact]
    public void ConditionEvaluator_FiltersChoices()
    {
        var tree = new DialogueTree { Id = "cond", StartNodeId = "start" };
        tree.Nodes["start"] = new DialogueNode
        {
            Id = "start",
            Text = "Choose:",
            Choices =
            [
                new DialogueChoice { Text = "Always", TargetNodeId = "a" },
                new DialogueChoice { Text = "Locked", TargetNodeId = "b", Condition = "has_key" }
            ]
        };
        tree.Nodes["a"] = new DialogueNode { Id = "a", Text = "A" };
        tree.Nodes["b"] = new DialogueNode { Id = "b", Text = "B" };

        var runner = new DialogueRunner
        {
            ConditionEvaluator = cond => cond == "has_key" ? false : true
        };
        runner.Start(tree);

        var choices = runner.GetAvailableChoices();
        Assert.Single(choices);
        Assert.Equal("Always", choices[0].Text);
    }

    [Fact]
    public void FromNodes_BuildsTree()
    {
        var nodes = new[]
        {
            new DialogueNode { Id = "start", Text = "Hi" },
            new DialogueNode { Id = "end", Text = "Bye" }
        };

        var tree = DialogueTree.FromNodes("test", "start", nodes);
        Assert.Equal("test", tree.Id);
        Assert.NotNull(tree.GetNode("start"));
        Assert.NotNull(tree.GetNode("end"));
    }

    [Fact]
    public void End_ClearsState()
    {
        var runner = new DialogueRunner();
        runner.Start(CreateSimpleTree());
        runner.End();

        Assert.False(runner.IsActive);
        Assert.Null(runner.CurrentNode);
        Assert.Null(runner.CurrentTree);
    }
}
