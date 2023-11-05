using Godot;
using System;

public partial class NoteView : Control
{
	public int NoteId
	{ get; set; }
	NoteGraph parent;
	TextEdit editor;
	Label output;
	public AnimationPlayer Animation;

	[Signal]
	public delegate void NoteClosedEventHandler();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		editor = GetNode<TextEdit>("HBoxContainer/TextEdit");
		parent = GetParent<NoteGraph>();
		editor.Text = parent.graph.NodeAt(NoteId).Markup;
		editor.TextChanged += OnTextChanged;
		output = GetNode<Label>("HBoxContainer/Label");
		output.Text = editor.Text;

		Animation = GetNode<AnimationPlayer>("AnimationPlayer");
		Animation.Play("Open");
	}

	private void OnTextChanged()
	{
		parent.graph.NodeAt(NoteId).Markup = editor.Text;
		output.Text = editor.Text;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override async void _Input(InputEvent @event)
	{
		if (Input.IsActionPressed("exit"))
		{
			Animation.Play("Close");
			EmitSignal(SignalName.NoteClosed);
			await ToSignal(Animation, "animation_finished");
			QueueFree();
		}
	}
}
