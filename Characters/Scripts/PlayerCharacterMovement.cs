using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;

public partial class PlayerCharacterMovement : CharacterBody3D
{
    public const float MouseSensitivity = 0.003f;

    [Export]
    public float WalkSpeed = 1f;
    [Export]
    public float RunSpeed = 5f;
    [Export]
    public float TopSpeed = 8f;
    [Export]
    public float JumpVelocity = 5f;

    [Export]
    public float runAccelarationTime = 0.3f;
    [Export]
    public float topAccelarationTime = 0.4f;
    [Export]
    public float toRunDecelartionTime = 0.2f;

    public float runAcellerationRate;
    public float topAcellerationRate;
    public float toRunDecelerationRate;
    public float stopRate;

    //Min and max vertical rotation
    public const float MinPitch = -45f;
	public const float MaxPitch = 75f;

    //Min and max Yaw rotation
    public const float MinYaw = -75f;
    public const float MaxYaw = 75f;

    public float MovementSpeed;
	public float coyoteTime = 0.1f;
    public float timeSinceLeftFloor = 0f;

    public bool hasDoubleJump = true;
    public bool isHurt = false;
    public bool hasWon = false;

    Vector2 inputDir;

    Vector3 velocity;
    Vector3 gravity;

    RandomNumberGenerator rng = new RandomNumberGenerator();

    Node3D cameraController;
	Node3D cameraTarget;
    Node3D sounds;

    Transform3D xTransform;

	RayCast3D groundDetector;

    AnimationPlayer animationPlayer;

    SoundManager soundManager;

    AudioStreamPlayer3D soundJump;
    AudioStreamPlayer3D soundDoubleJump;
    AudioStreamPlayer3D soundGetHit;
    AudioStreamPlayer3D soundWalkOnGrass;
    AudioStreamPlayer3D soundSkate;

    Node3D[] thrusters;

    Area3D[] hitboxes;
    Area3D[] hurtboxes;

    Node3D[] footLightTrails;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        //Get nodes
        soundManager = GetNode<SoundManager>("/root/SoundManager");

        cameraController = GetNode<Node3D>("Camera_Controller");
        cameraTarget = cameraController.GetNode<Node3D>("Camera_Target");
        groundDetector = GetNode<RayCast3D>("RayCast3D");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        sounds = GetNode<Node3D>("Sounds");
        soundJump = sounds.GetNode<AudioStreamPlayer3D>("Jump");
        soundDoubleJump = sounds.GetNode<AudioStreamPlayer3D>("DoubleJump");
        soundGetHit = sounds.GetNode<AudioStreamPlayer3D>("GetHit");
        soundWalkOnGrass = sounds.GetNode<AudioStreamPlayer3D>("WalkOnGrass");
        soundSkate = sounds.GetNode<AudioStreamPlayer3D>("Skate");

        thrusters = GetTree().GetNodesInGroup("Thrusters").Cast<Node3D>().ToArray();
        hitboxes = GetTree().GetNodesInGroup("HitBoxesPlayer").Cast<Area3D>().ToArray();
        hurtboxes = GetTree().GetNodesInGroup("HurtBoxesPlayer").Cast<Area3D>().ToArray();
        footLightTrails = GetTree().GetNodesInGroup("FootLightTrails").Cast<Node3D>().ToArray();
        
        //Set initial values
        Input.MouseMode = Input.MouseModeEnum.Captured;

        runAcellerationRate = RunSpeed / runAccelarationTime;
        topAcellerationRate = (TopSpeed - RunSpeed) / topAccelarationTime;
        toRunDecelerationRate = (TopSpeed - RunSpeed) / toRunDecelartionTime;

        isHurt = false;

        //Call important methods
        controlTrusters(false);
        controllHurtBoxes(true);
        controllHitBoxes(false);
    }

    public override void _PhysicsProcess(double delta)
	{
		//Controll camera position manually
		Vector3 characterPosition = Position;

		//Get camera controller position
		Vector3 cameraPosition = cameraController.Position;

		velocity = Velocity;

		gravity = GetGravity();

        // Get the input direction and handle the movement/deceleration.
        inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

        //Play animations
        playAnimation();

        if (groundDetector.IsColliding())
        {
            controllFeetCollision(true, false);
        }

        // Add the gravity.
        if (IsOnFloor())
		{
            timeSinceLeftFloor = 0;
            hasDoubleJump = true;
        }
		else
		{
            velocity += gravity * (float)delta;
            timeSinceLeftFloor += (float)delta;
        }

        if (MovementSpeed > RunSpeed)
        {
            controllLightTrail(false);
        }
        else
        {
            controllLightTrail(true);
        }

        // Handle Jump.
        if (Input.IsActionJustPressed("ui_accept") && (IsOnFloor() || timeSinceLeftFloor < coyoteTime))
        {
            controllFeetCollision(false, true);

            soundJump.PitchScale = rng.RandfRange(0.9f, 1.1f);
            soundJump.Play();

            velocity.Y = JumpVelocity;
        }
        else if (Input.IsActionJustPressed("ui_accept") && !IsOnFloor() && hasDoubleJump)
        {
            soundDoubleJump.PitchScale = 1.5f * rng.RandfRange(0.9f, 1.1f);
            soundDoubleJump.Play();

            velocity.Y = JumpVelocity;
            hasDoubleJump = false;
        }

        if (inputDir != Vector2.Zero)
        {
            accelerate(delta);

            //Get Yaw camera degree
            Vector3 camForward = -cameraController.GlobalTransform.Basis.Z;
            camForward.Y = 0;
            camForward = camForward.Normalized();

            //Get Yaw camera degree
            Vector3 camYaw = -cameraController.GlobalTransform.Basis.X;
            camYaw.Y = 0;
            camYaw = camYaw.Normalized();

            Vector3 direction = (camYaw * inputDir.X + camForward * inputDir.Y).Normalized();

            if (direction != Vector3.Zero)
            {
                velocity.X = direction.X * MovementSpeed;
                velocity.Z = direction.Z * MovementSpeed;

                if (inputDir.Y < 0)
                {
                    //Rotate player to camera angle
                    float targetYaw = Mathf.Atan2(direction.X, direction.Z);
                    float currentYaw = Rotation.Y;
                    float newYaw = Mathf.LerpAngle(currentYaw, targetYaw, 10f * (float)delta);
                    Rotation = new Vector3(Rotation.X, newYaw, Rotation.Z);
                }

                //Align camera slowly with palyer movement
                if (inputDir.Y > 0)
                {
                    float targetYaw = RotationDegrees.Y;
                    float currentYaw = cameraController.RotationDegrees.Y;
                    float newYaw = Mathf.LerpAngle(currentYaw, targetYaw, 5f * (float)delta);

                    cameraController.RotationDegrees = new Vector3(cameraController.RotationDegrees.X, newYaw, cameraController.RotationDegrees.Z);
                }
            }
        }
        else
        {
            //Reduce movement speed when not pressing a button
            var stopRate =
                (MovementSpeed > 6f)
                ? TopSpeed / topAccelarationTime
                : TopSpeed / runAccelarationTime;

            MovementSpeed = Mathf.MoveToward(MovementSpeed, 0f, stopRate * (float)delta);

            velocity.X = Mathf.MoveToward(Velocity.X, 0, stopRate * (float)delta);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, stopRate * (float)delta);
        }

        //Align character to floor
        if (IsOnFloor())
        {
            alignWithFloor(groundDetector.GetCollisionNormal());
            GlobalTransform = GlobalTransform.InterpolateWith(xTransform, 0.3f);
        }
        else
        {
            alignWithFloor(Vector3.Up);
            GlobalTransform = GlobalTransform.InterpolateWith(xTransform, 0.3f);
        }

        Velocity = velocity;
		MoveAndSlide();

		//Update camera controller position smoothly
		cameraController.Position = cameraPosition.Lerp(characterPosition, 5f * (float)delta);
	}

	public override void _Input(InputEvent @event)
	{
        if(@event is InputEventMouseMotion motion)
		{
			cameraController.RotateY(-motion.Relative.X * MouseSensitivity);

			//if (inputDir == Vector2.Zero)
			//{
			//	//Control min and max Yaw rotation
			//	float yaw = cameraController.RotationDegrees.Y;
			//	yaw -= motion.Relative.X * MouseSensitivity * Mathf.RadToDeg(1f);
			//	yaw = Mathf.Clamp(yaw, MinYaw, MaxYaw);
			//	cameraController.RotationDegrees = new Vector3(cameraController.RotationDegrees.X, yaw, cameraController.RotationDegrees.Z);
			//}

			//Control min and max vertical rotation
			float pitch = cameraTarget.RotationDegrees.X;
            pitch -= motion.Relative.Y * MouseSensitivity * Mathf.RadToDeg(1f);
            pitch = Mathf.Clamp(pitch, MinPitch, MaxPitch);
            cameraTarget.RotationDegrees = new Vector3(pitch, cameraTarget.RotationDegrees.Y, cameraTarget.RotationDegrees.Z);
        }
    }

    public void playAnimation()
    {
        //Play animations
        if (Input.IsActionJustPressed("ui_accept") && IsOnFloor() && !isHurt && !hasWon)
        {
            animationPlayer.Play("Jump_Rise", 0.1);
            controlTrusters(false);
        }
        else if (Input.IsActionJustPressed("ui_accept") && !IsOnFloor() && hasDoubleJump && !isHurt && !hasWon)
        {
            animationPlayer.Play("Jump_Double", 0.1);
            controlTrusters(true);
        }
        else if (!IsOnFloor() && velocity.Y < 0f && !isHurt && !hasWon)
        {
            animationPlayer.Play("Jump_Drop", 0.1);
            gravity *= 2f;
        }
        else if (inputDir != Vector2.Zero && IsOnFloor() && !isHurt && !hasWon)
        {
            if (MovementSpeed < 3)
            {
                animationPlayer.Play("Walk_Forward", 0.1);
                controlTrusters(false);
            }
            else if (MovementSpeed <= 6)
            {
                animationPlayer.Play("Run", 0.1);
                controlTrusters(false);
            }
            else
            {
                animationPlayer.Play("Skate", 0.1);
                controlTrusters(true);
            }

        }
        else if (inputDir == Vector2.Zero && IsOnFloor() && !isHurt && !hasWon)
        {
            animationPlayer.Play("Idle", 0.1);
            controlTrusters(false);
        }
    }

    public void accelerate(double delta)
    {
        if (IsOnFloor() && Input.IsActionPressed("ui_accelerate"))
        {
            MovementSpeed = Mathf.MoveToward(MovementSpeed, TopSpeed, topAcellerationRate * (float)delta);
        }
        else if (!IsOnFloor() && Input.IsActionPressed("ui_accelerate"))
        {
        }
        else if (MovementSpeed >= RunSpeed)
        {
            MovementSpeed = Mathf.MoveToward(MovementSpeed, RunSpeed, toRunDecelerationRate * (float)delta);
        }
        else
        {
            MovementSpeed = Mathf.MoveToward(MovementSpeed, RunSpeed, runAcellerationRate * (float)delta);
        }
    }

    public async void onHurtboxEntered(Area3D hitbox)
    {
        if (isHurt)
        {
            return;
        }

        isHurt = true;
        CallDeferred(nameof(controllHurtBoxes), false);

        if (hitbox.Name == "FallZone")
        {
            await gameOver("res://Menu/Nodes/menu_defeat.tscn");
        }
        else
        {
            soundJump.PitchScale = rng.RandfRange(0.9f, 1.1f);
            soundGetHit.Play();
            await playDefeatedAnimation();
            await gameOver("res://Menu/Nodes/menu_defeat.tscn");
        }
    }

    public async Task playDefeatedAnimation()
    {
        animationPlayer.Play("Defeat", 0.1);
        if (soundManager != null)
        {
            soundManager.playSoundLose();
        }
        controlTrusters(false);
        await ToSignal(animationPlayer, AnimationPlayer.SignalName.AnimationFinished);
    }

    public async Task playWinAnimation()
    {
        animationPlayer.Play("Win", 0.1);
        if (soundManager != null)
        {
            soundManager.playSoundWin();
        }
        controlTrusters(true);
        await ToSignal(animationPlayer, AnimationPlayer.SignalName.AnimationFinished);
    }

    public async Task gameOver(string nextScene)
    {
        var sceneTree = GetTree();

        var timer = sceneTree.CreateTimer(0.1);
        await ToSignal(timer, Timer.SignalName.Timeout);

        if (sceneTree != null)
        {
            loadScene(sceneTree, nextScene);
        }
    }

    public void loadScene(SceneTree sceneTree, string sceneName)
    {
        foreach (Node3D trail in footLightTrails)
        {
            if (IsInstanceValid(trail))
            {
                trail.QueueFree();
            }
        }

        if (sceneTree != null)
        {
            sceneTree.CallDeferred("change_scene_to_file", sceneName);
        }
    }

    public void bounce()
    {
        Velocity = new Vector3(Velocity.X, JumpVelocity * 0.7f, Velocity.Z);
    }

    public void doubleJump()
    {
        hasDoubleJump = false;
        Velocity = new Vector3(Velocity.X, JumpVelocity * 1f, Velocity.Z);
    }

    public void alignWithFloor(Vector3 floorNormal)
	{
		xTransform = GlobalTransform;
		xTransform.Basis.Y = floorNormal;
		xTransform.Basis.X = -xTransform.Basis.Z.Cross(floorNormal);
		xTransform.Basis = xTransform.Basis.Orthonormalized();
	}

    public void controlTrusters(bool onOff)
    {
        for (int i = 0; i < thrusters.Length; i++)
        {
            var thruster = thrusters[i].GetNode<GpuParticles3D>("Fire");
            if (thruster != null)
            {
                thruster.Emitting = onOff;
            }
        }
    }

    public void controllHurtbox(Area3D hurtbox, bool onOff)
    {
        var collisionBox = hurtbox.GetChild<CollisionShape3D>(0);

        hurtbox.Monitorable = onOff;
        collisionBox.Disabled = !onOff;

        hurtbox.SetCollisionLayerValue(6, onOff);
        hurtbox.SetCollisionMaskValue(5, onOff);
        hurtbox.SetCollisionMaskValue(9, onOff);
    }

    public void controllHurtBoxes(bool onOff)
    {
        foreach (Area3D hurtbox in hurtboxes)
        {
            controllHurtbox(hurtbox, onOff);
        }
    }

    public void controllHitbox(Area3D hitbox, bool onOff)
    {
        var collisionBox = hitbox.GetChild<CollisionShape3D>(0);

        hitbox.Monitorable = onOff;
        collisionBox.Disabled = !onOff;

        hitbox.SetCollisionLayerValue(7, onOff);
        hitbox.SetCollisionMaskValue(8, onOff);
    }

    public void controllHitBoxes(bool onOff)
    {
        foreach (Area3D hitbox in hitboxes)
        {
            controllHitbox(hitbox, onOff);
        }
    }

    public void controllFeetCollision(bool onOffHurt, bool onOffHit)
    {
        var feetHitboxes = hitboxes.Where(hitbox => hitbox.Name.ToString().ToLower().Contains("foot"));
        var feetHurtboxes = hurtboxes.Where(hurtBox => hurtBox.Name.ToString().ToLower().Contains("foot"));

        foreach (Area3D footHurtbox in feetHurtboxes)
        {
            controllHurtbox(footHurtbox, onOffHurt);
        }

        foreach (Area3D footHitbox in feetHitboxes)
        {
            controllHitbox(footHitbox, onOffHit);
        }
    }

    public void controllLightTrail(bool isHidden)
    {
        foreach(Node3D trail in footLightTrails)
        {
            if (isHidden)
            {
                trail.Hide();
            }
            else
            {
                trail.Show();
            }
        }
    }

    public void playSoundSkate()
    {
        soundSkate.PitchScale = rng.RandfRange(0.9f, 1.1f);
        soundSkate.Play();
    }

    public void playSoundWalkOnGrass()
    {
        soundWalkOnGrass.PitchScale = rng.RandfRange(0.9f, 1.1f);
        soundWalkOnGrass.Play();
    }
}