using Godot;
using System;

public partial class Bullet : Area2D
{

	private static readonly uint WORLD_COLLISION_LAYER = 1;

	[Export]
	public float Speed { get; set; } = 250f;
	[Export]
	public float MaxLifetime { get; set; } = 2f;

	private Vector2 Direction = Vector2.Zero;
	private float lifetime = 0f;
	
	public override void _Ready()
	{
		lifetime = MaxLifetime;
		AreaEntered += Area2DEntered;
	}

	public void Setup(Vector2 direction)
	{
		if(direction != Vector2.Zero)
		    Direction = direction.Normalized();

		Rotation = Direction.Angle();
	}


    public override void _PhysicsProcess(double delta)
    {
        var currentPosition = GlobalPosition;
		var nextPosition = currentPosition + Direction * Speed * (float)delta;

		if(hitWorldCollision(currentPosition, nextPosition))
		{
			QueueFree();
			return;
		}

		lifetime -= (float)delta;
		if(lifetime <= 0f)
		{
			QueueFree();
			return;
		}

		GlobalPosition = nextPosition;

    }


	private bool hitWorldCollision(Vector2 from, Vector2 to)
	{
		var spaceState = GetWorld2D().DirectSpaceState;

		if(spaceState == null)
			return false;
		
		var query = new PhysicsRayQueryParameters2D()
		{
			From = from,
			To = to,
			CollisionMask = WORLD_COLLISION_LAYER,

			CollideWithBodies = true,
			CollideWithAreas = false
		};

		var result = spaceState.IntersectRay(query);
		return result.Count > 0;
	}
	



	private void Area2DEntered(Area2D area)
	{
		if(area is Bullet)
			return;

		QueueFree();
	}
}
