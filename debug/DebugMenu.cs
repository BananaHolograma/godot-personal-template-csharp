using GameRoot;
using Godot;
using System;
using System.Diagnostics;
using System.Linq;

public partial class DebugMenu : Control
{
	[Export] public FirstPersonController Actor;

	public Label FpsLabel;
	public Label StateLabel;
	public Label VelocityLabel;
	public Label VideoAdapterLabel;

	public override void _Ready()
	{
		FpsLabel = GetNode<Label>("%FpsLabel");
		StateLabel = GetNode<Label>("%StateLabel");
		VelocityLabel = GetNode<Label>("%VelocityLabel");
		VideoAdapterLabel = GetNode<Label>("%VideoAdapterLabel");

		UpdateVideoAdapterLabel(VideoAdapterLabel);

		Actor.GetNode<FiniteStateMachine>("FiniteStateMachine").StateChanged += OnStateChanged;
	}

	public override void _Process(double delta)
	{
		FpsLabel.Text = $"FPS: {Engine.GetFramesPerSecond()}";
		VelocityLabel.Text = $"Velocity: {Actor.Velocity}";
	}

	private void UpdateVideoAdapterLabel(Label label)
	{
		string adaptorVendor = RenderingServer.GetVideoAdapterName();
		string adaptorName = RenderingServer.GetVideoAdapterName();

		if (adaptorName.Contains(adaptorVendor.TrimSuffix("Corporation")))
		{
			label.Text = adaptorName.Contains(adaptorVendor.TrimSuffix("Corporation")) ?
				adaptorName.TrimSuffix("/PCIe/SSE2") :
				$"{adaptorVendor} - {adaptorName.TrimSuffix("/PCIe/SSE2")}";
		}

	}

	private void OnStateChanged(State from, State to)
	{
		StateLabel.Text = $"{from.Name} -> {to.Name}";
	}


}
