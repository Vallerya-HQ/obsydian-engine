using Obsydian.Core.Math;
using Obsydian.Graphics;
using Obsydian.Input;
using Obsydian.UI.Dialogue;

namespace Obsydian.UI.Widgets;

/// <summary>
/// Visual dialogue box widget. Renders speaker name, typewriter text,
/// and choice buttons. Drives a DialogueRunner for state management.
/// </summary>
public sealed class DialogueBox : UIElement
{
    public DialogueRunner Runner { get; }
    public Color BackgroundColor { get; set; } = new(20, 20, 40, 220);
    public Color BorderColor { get; set; } = Color.White;
    public Color TextColor { get; set; } = Color.White;
    public Color SpeakerColor { get; set; } = Color.Gold;
    public Color ChoiceColor { get; set; } = new(180, 180, 255);
    public Color ChoiceHighlightColor { get; set; } = Color.Gold;
    public float TextScale { get; set; } = 2f;
    public float TypewriterSpeed { get; set; } = 30f;

    private string _displayedText = "";
    private string _fullText = "";
    private float _charProgress;
    private int _selectedChoice;
    private bool _needsWrap = true;

    public DialogueBox()
    {
        Runner = new DialogueRunner();
        Runner.OnNodeEntered += OnNodeEntered;
    }

    public void StartDialogue(DialogueTree tree)
    {
        Visible = true;
        _selectedChoice = 0;
        Runner.Start(tree);
    }

    public override void Update(float deltaTime, InputManager input)
    {
        if (!Visible || !Runner.IsActive) return;

        // Typewriter effect
        if (_charProgress < _fullText.Length)
        {
            _charProgress += TypewriterSpeed * deltaTime;
            int count = System.Math.Min((int)_charProgress, _fullText.Length);
            _displayedText = _fullText[..count];

            // Skip typewriter on confirm
            if (input.IsKeyPressed(Key.Space) || input.IsKeyPressed(Key.Enter))
            {
                _charProgress = _fullText.Length;
                _displayedText = _fullText;
                return;
            }
        }
        else
        {
            // Text fully displayed — handle input
            var choices = Runner.GetAvailableChoices();

            if (choices.Count > 0)
            {
                if (input.IsKeyPressed(Key.Up) || input.IsKeyPressed(Key.W))
                    _selectedChoice = (_selectedChoice - 1 + choices.Count) % choices.Count;
                if (input.IsKeyPressed(Key.Down) || input.IsKeyPressed(Key.S))
                    _selectedChoice = (_selectedChoice + 1) % choices.Count;

                if (input.IsKeyPressed(Key.Space) || input.IsKeyPressed(Key.Enter))
                {
                    Runner.SelectChoice(_selectedChoice);
                    _selectedChoice = 0;
                }
            }
            else
            {
                if (input.IsKeyPressed(Key.Space) || input.IsKeyPressed(Key.Enter))
                    Runner.Advance();
            }
        }

        if (!Runner.IsActive)
            Visible = false;
    }

    public override void Draw(IRenderer renderer)
    {
        if (!Visible || !Runner.IsActive) return;

        renderer.DrawRect(Bounds, BackgroundColor);
        renderer.DrawRect(Bounds, BorderColor, filled: false);

        float padding = 12f;
        float x = Bounds.X + padding;
        float y = Bounds.Y + 8;

        // Speaker name
        if (!string.IsNullOrEmpty(Runner.CurrentNode?.Speaker))
        {
            renderer.DrawText(Runner.CurrentNode!.Speaker, new Vec2(x, y), SpeakerColor, TextScale);
            y += TextScale * 10;
        }

        // Word-wrap text on first draw after node change
        if (_needsWrap)
        {
            float maxWidth = Bounds.Width - padding * 2;
            _fullText = renderer.WrapText(_fullText, maxWidth, TextScale);
            int count = System.Math.Min((int)_charProgress, _fullText.Length);
            _displayedText = _fullText[..count];
            _needsWrap = false;
        }

        // Dialogue text
        renderer.DrawText(_displayedText, new Vec2(x, y), TextColor, TextScale);

        // Choices
        if (_charProgress >= _fullText.Length)
        {
            var choices = Runner.GetAvailableChoices();
            if (choices.Count > 0)
            {
                float choiceY = Bounds.Bottom - 12 - choices.Count * (TextScale * 10);
                for (int i = 0; i < choices.Count; i++)
                {
                    var color = i == _selectedChoice ? ChoiceHighlightColor : ChoiceColor;
                    var prefix = i == _selectedChoice ? "> " : "  ";
                    renderer.DrawText($"{prefix}{choices[i].Text}", new Vec2(x, choiceY), color, TextScale);
                    choiceY += TextScale * 10;
                }
            }
            else
            {
                // "Continue" indicator
                renderer.DrawText("[Space]", new Vec2(Bounds.Right - 80, Bounds.Bottom - 20), new Color(150, 150, 150), 1.5f);
            }
        }
    }

    private void OnNodeEntered(DialogueNode node)
    {
        _fullText = node.Text;
        _displayedText = "";
        _charProgress = 0;
        _selectedChoice = 0;
        _needsWrap = true;
    }
}
