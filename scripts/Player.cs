using Godot;
using System;

public partial class Player : CharacterBody2D
{
	private static readonly StringName NORMAL_ANIMATION_PREFIX = "normal";
	
	// ---------------------------------------
	[Export]
	public float Speed { get; set; } = 120f;

	private StringName faceDirection = "right";

	// ---------------------------------

	private AnimatedSprite2D bodySprite;

	public override void _Ready()
	{
		bodySprite = GetNode<AnimatedSprite2D>("BodySprite");

		updateAnimation();
	}

    public override void _PhysicsProcess(double delta)
	{
		var inputDir = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		Velocity = inputDir * Speed;
		MoveAndSlide();

		if(inputDir != Vector2.Zero)
		{
			faceDirection = input2FaceDirction(inputDir);
		}
		updateAnimation();
	}

	private StringName input2FaceDirction(Vector2 inputDir)
	{
		if(Math.Abs(inputDir.X) >= Math.Abs(inputDir.Y))
		{
			return inputDir.X > 0 ? "right" : "left";
		}
		else
		{
			return inputDir.Y > 0 ? "down" : "up";
		}
	}
	
	private void updateAnimation()
	{
		var animationName = $"{NORMAL_ANIMATION_PREFIX}_{faceDirection}";
		
		if(!bodySprite.SpriteFrames.HasAnimation(animationName))
			return;

		if(bodySprite.Animation == animationName)
			return;

		bodySprite.Play(animationName);
	}

}
