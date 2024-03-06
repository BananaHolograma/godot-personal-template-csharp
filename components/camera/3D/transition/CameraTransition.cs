using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotExtensions;

namespace GameRoot;

[GlobalClass]
public partial class CameraTransition : Node
{
	[Export] public float TransitionDuration = 1.5f;
	[Export] public Camera3D GlobalTransitionCamera;
	public GameEvents GameEvents;

	// In this variable are saved the transition movements with the structure {"from": camera, "to": camera, "duration": 1.0}
	// Using this approach it's easy to come back to the previous transitioned camera until reach the isometric core camera.
	// If we want to return on previous steps
	public List<TransitionStep> TransitionSteps = new();
	public Tween TransitionTween;
	public bool Transitioning = false;

	public override void _Ready()
	{
		GameEvents = this.GetAutoloadNode<GameEvents>("GameEvents");
		GameEvents.GlobalTransitionCamera3DRequested += OnGlobalTransitionCamera3DRequestedEventHandler;
	}

	public async void TransitionToRequestedCamera(Camera3D from, Camera3D to, float duration = 0)
	{
		if (duration == 0)
			duration = TransitionDuration;

		if (IsTransitioning())
			return;

		Transitioning = true;
		GlobalTransitionCamera.Projection = to.Projection;

		if (GlobalTransitionCamera.Projection.Equals(Camera3D.ProjectionType.Orthogonal))
		{
			GlobalTransitionCamera.Size = from.Size;
			GlobalTransitionCamera.Far = from.Far;
		}

		GlobalTransitionCamera.Fov = from.Fov;
		GlobalTransitionCamera.CullMask = from.CullMask;
		GlobalTransitionCamera.GlobalTransform = from.GlobalTransform;
		GlobalTransitionCamera.MakeCurrent();

		TransitionTween = CreateTween();
		TransitionTween.SetParallel(true).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Cubic);
		TransitionTween.TweenProperty(GlobalTransitionCamera, "global_transform", to.GlobalTransform, TransitionDuration).From(GlobalTransitionCamera.GlobalTransform);
		TransitionTween.TweenProperty(GlobalTransitionCamera, "fov", to.Fov, TransitionDuration).From(GlobalTransitionCamera.Fov);

		await ToSignal(TransitionTween, Tween.SignalName.Finished);
		Transitioning = false;

		TransitionSteps.Add(new TransitionStep(from, to, TransitionDuration));
	}

	private bool IsTransitioning() => Transitioning || TransitionTween.IsRunning();

	public void TransitionToNextCamera(Camera3D To, float duration = 0)
	{
		if (TransitionSteps.Count == 0 || IsTransitioning())
			return;

		TransitionToRequestedCamera(TransitionSteps.Last().To, To, duration);
	}

	public void TransitionToPreviousCamera()
	{
		if (TransitionSteps.Count == 0 || IsTransitioning())
			return;

		TransitionStep lastStep = TransitionSteps.Last();

		TransitionToRequestedCamera(lastStep.To, lastStep.From, lastStep.Duration);
	}

	private void OnGlobalTransitionCamera3DRequestedEventHandler(Camera3D from, Camera3D to)
	{
		throw new NotImplementedException();
	}

}

public record TransitionStep(Camera3D From, Camera3D To, float Duration)
{
	public bool IsValid => Duration > 0;

	public string ToDescription() => $"Transition from {From.Name} to {To.Name} in {Duration} seconds";

}
