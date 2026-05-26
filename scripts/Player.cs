using Godot;
using System;

public partial class Player : CharacterBody2D
{
	private static readonly StringName NORMAL_ANIMATION_PREFIX = "normal";
	private static readonly StringName ARRMED_ANIMATION_PREFIX = "arrmed";

	private const int PLAYER_FORM_MODE_NORMAL = 0;
	private const int PLAYER_FORM_MODE_ARRMED = 1;
	private const int SHOT_PATTERN_NORMAL = 0;
	private const int SHOT_PATTERN_ARRMED = 1;

	private const float DEFAULT_FIRE_RATE_MULTIPLIER = 1f;
	private const float SPIRAL_PHASE_STEP = Mathf.Pi / 12f;
	
	// ---------------------------------------
	[Export]
	public float Speed { get; set; } = 120f;
	[Export]
	public PackedScene BulletPrefab;
	[Export]
	public float FireInterval { get; set; } = 0.18f;
	[Export]
	public float BulletSpawnOffset { get; set; } = 18f;

	[Export]
	public int currentFormMode = PLAYER_FORM_MODE_NORMAL;
	[Export]
	public int currentShotPattern = SHOT_PATTERN_NORMAL;
	[Export]
	public float rapidFireRateMultiplier = DEFAULT_FIRE_RATE_MULTIPLIER;
	[Export]
	public float formFireRateMultiplier = DEFAULT_FIRE_RATE_MULTIPLIER;


	private float sprialPhase = 0f;
	private StringName faceDirection = "right";

	
	// ---------------------------------

	private AnimatedSprite2D bodySprite;
	private AnimatedSprite2D arrmedSprite;
	private Timer shootTimer;

	public override void _Ready()
	{

		// currentFormMode = PLAYER_FORM_MODE_ARRMED;
		// currentShotPattern = SHOT_PATTERN_ARRMED;
		// formFireRateMultiplier *= 10;

		bodySprite = GetNode<AnimatedSprite2D>("BodySprite");
		arrmedSprite = GetNode<AnimatedSprite2D>("ArrmedEffectSprite");
		shootTimer = GetNode<Timer>("ShootTimer");

		shootTimer.OneShot = true;
		shootTimer.WaitTime = GetFireInterval();

		updateAnimation();
	}

    public override void _PhysicsProcess(double delta)
	{

		

		var inputDir = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		var shootInput = Input.GetVector("shoot_left", "shoot_right", "shoot_up", "shoot_down");

		Velocity = inputDir * Speed;
		MoveAndSlide();

		TryShoot(shootInput);
		UpdateFace(inputDir,shootInput);
		updateAnimation();
		UpdateArrmedAnimation();
	}

	private void TryShoot(Vector2 shootInput)
	{
		if(!shootTimer.IsStopped())
			return;

		if(FireBullet(shootInput.Normalized()))
			shootTimer.Start(GetFireInterval());
	}

	private bool FireBullet(Vector2 shootInput)
	{
		switch(currentShotPattern){
			case SHOT_PATTERN_NORMAL:
				return tryFireNormal(shootInput);
			case SHOT_PATTERN_ARRMED:
				return tryFireSpiral();

		}
		return false;
	}

	private bool spawnBullet(Vector2 shootInput)
	{
		var spaceState = GetWorld2D().DirectSpaceState;

		if(spaceState == null)
			return false;

		var target = GlobalPosition + shootInput * BulletSpawnOffset;

		var param = new PhysicsRayQueryParameters2D()
		{
			From=GlobalPosition,
			To=target,
			CollisionMask = 1,

			CollideWithBodies = true,
			CollideWithAreas = false	
		};
		if (spaceState.IntersectRay(param).Count > 0)
		{
			return false;
		}

		var bullet = BulletPrefab.Instantiate<Bullet>();
		bullet.TopLevel = true;

		bullet.Setup(shootInput);

		GetTree().CurrentScene.AddChild(bullet);

		bullet.GlobalPosition = target;
		return true;

	}

	private bool tryFireNormal(Vector2 shootInput)
	{
		if(shootInput == Vector2.Zero)
			return false;
		return spawnBullet(shootInput);
	}

	private bool tryFireSpiral()
	{
		var baseDirection = Vector2.Right.Rotated(sprialPhase);

		spawnBullet(baseDirection);
		spawnBullet(baseDirection.Rotated(Mathf.Pi));

		sprialPhase = Mathf.Wrap(sprialPhase + SPIRAL_PHASE_STEP, 0f, (float)Math.PI * 2f);
		return true;
	}


	private float GetFireInterval()
	{
		switch (currentFormMode)
		{
			case PLAYER_FORM_MODE_ARRMED:
				return Mathf.Max(FireInterval / formFireRateMultiplier, 0.01f);
			default:
				return Mathf.Max(FireInterval / rapidFireRateMultiplier, 0.01f);
		}
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
	


	private void UpdateFace(Vector2 moveInput, Vector2 shootInput)
	{
		if(shootInput != Vector2.Zero)
			faceDirection = input2FaceDirction(shootInput);
		else if (moveInput != Vector2.Zero)
			faceDirection = input2FaceDirction(moveInput);
	}

	private StringName GetFormAnimationPrefix()
	{
		switch (currentFormMode)
		{
			case PLAYER_FORM_MODE_ARRMED:
				return ARRMED_ANIMATION_PREFIX;
			default:
				return NORMAL_ANIMATION_PREFIX;
		}
	}

	private void updateAnimation()
	{
		var animationName = $"{GetFormAnimationPrefix()}_{faceDirection}";
		
		if(!bodySprite.SpriteFrames.HasAnimation(animationName))
			animationName = $"{NORMAL_ANIMATION_PREFIX}_{faceDirection}";
			if(!bodySprite.SpriteFrames.HasAnimation(animationName))
				return;

		if(bodySprite.Animation == animationName)
			return;

		bodySprite.Play(animationName);
	}


	private void UpdateArrmedAnimation()
	{
		bool isArrmed = currentFormMode == PLAYER_FORM_MODE_ARRMED;

		if (isArrmed)
		{
			if (arrmedSprite.SpriteFrames.HasAnimation("default"))
			{
				if(arrmedSprite.Animation != "default" || !arrmedSprite.IsPlaying())
				{
					arrmedSprite.Play("default");
				}
			}
		}
		else if (arrmedSprite.IsPlaying())
		{
			arrmedSprite.Stop();
		}

		arrmedSprite.Visible = isArrmed;

	}

}
