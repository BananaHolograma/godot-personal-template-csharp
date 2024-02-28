using GameRoot;
using Godot;
using System;

public partial class Node2D : Godot.Node2D
{
	public MusicManager MusicManager;

	public readonly AudioStream music = ResourceLoader.Load<AudioStream>("res://jaja.ogg");
	public readonly AudioStream music2 = ResourceLoader.Load<AudioStream>("res://jaja2.ogg");

	public Timer timer;

	public override void _Ready()
	{
		timer = GetNode<Timer>("Timer");
		MusicManager = GetTree().Root.GetNode<MusicManager>("MusicManager");

		MusicManager.AddStreamToMusicBank("jaja", music);
		MusicManager.AddStreamToMusicBank("jaja2", music2);

		MusicManager.PlayMusic("jaja");

		timer.Timeout += () => { MusicManager.PlayMusic("jaja2"); };

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
