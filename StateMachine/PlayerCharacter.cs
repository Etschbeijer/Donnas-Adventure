using Godot;
using System;
using System.Linq;
using System.Xml.Xsl;
using System.Threading.Tasks;
using System.Runtime.Intrinsics.Arm;

public partial class PlayerCharacter : CharacterBody3D
{
    [Export]
    public float WalkSpeed { get; set; } = 1f;
    [Export]
    public float RunSpeed = 5f;
    [Export]
    public float TopSpeed = 8f;
    [Export]
    public float JumpVelocity = 5f;
    [Export]
    public float RunAccelarationTime = 0.3f;
    [Export]
    public float TopAccelarationTime = 0.4f;
    [Export]
    public float ToRunDecelartionTime = 0.2f;
    [Export]
    public float RotationSpeed { get; set; } = 12;
    [Export]
    public float MouseSensitivity { get; set; } = 0.003f;

    [Export]
    public int ComboCounter { get; set; } = 1;
    [Export]
    public int Friction { get; set; } = 20;
    [Export]
    public int AttackFriction { get; set; } = 4;
    [Export]
    public int AirFriction { get; set; } = 10;

    public Vector3 PlayerVelocity;
    public Vector3 Gravity;
    public Vector3 Direction;

    public Vector2 InputDir;

    public Node3D CameraController;

    Node3D cameraTarget;
    Node3D sounds;

    Transform3D xTransform;

    RandomNumberGenerator rng = new RandomNumberGenerator();

    public bool CanInput = true;
    public bool HasDoubleJump = true;
    public bool IsHurt = false;
    public bool HasWon = false;
    public bool ShallBounce = false;

    public float CoyoteTime = 0.1f;
    public float MovementSpeed;
    public float TimeSinceLeftFloor = 0f;

    //Min and max vertical rotation
    const float minPitch = -45f;
    const float maxPitch = 75f;

    //Min and max Yaw rotation
    const float minYaw = -75f;
    const float maxYaw = 75f;

    public int MaxHealth { get; set; } = 1;
    public int CurrentHealth { get; set; } = 1;
    public int HitStagger { get; set; } = 8;

    public AnimationTree AnimationTree { get; set; }

    public AnimationPlayer AnimationPlayer { get; set; }

    public AnimationNodeStateMachinePlayback AnimationNodeState { get; set; }

    public SoundManager SoundManager;

    RayCast3D groundDetector;

    AudioStreamPlayer3D soundJump;
    AudioStreamPlayer3D soundDoubleJump;
    AudioStreamPlayer3D soundGetHit;
    AudioStreamPlayer3D soundLandOnGrass;
    AudioStreamPlayer3D soundWalkOnGrass;
    AudioStreamPlayer3D soundSkate;

    Node3D[] thrusters;
    Node3D[] footLightTrails;

    Area3D[] hitboxes;
    Area3D[] hurtboxes;

    [Signal]
    public delegate void PlayerHitEventHandler();

    public float RunAcellerationRate;
    public float StopRate;
    public float TopAcellerationRate;
    public float ToRunDecelerationRate;

    public Global Global;

    public override void _Ready()
    {
        //Get nodes
        SoundManager = GetNode<SoundManager>("/root/SoundManager");

        CameraController = GetNode<Node3D>("Camera_Controller");
        cameraTarget = CameraController.GetNode<Node3D>("Camera_Target");
        groundDetector = GetNode<RayCast3D>("RayCast3D");
        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        Global = GetNode<Global>("/root/Global");

        sounds = GetNode<Node3D>("Sounds");
        soundJump = sounds.GetNode<AudioStreamPlayer3D>("Jump");
        soundDoubleJump = sounds.GetNode<AudioStreamPlayer3D>("DoubleJump");
        soundGetHit = sounds.GetNode<AudioStreamPlayer3D>("GetHit");
        soundLandOnGrass = sounds.GetNode<AudioStreamPlayer3D>("LandOnGrass");
        soundWalkOnGrass = sounds.GetNode<AudioStreamPlayer3D>("WalkOnGrass");
        soundSkate = sounds.GetNode<AudioStreamPlayer3D>("Skate");

        thrusters = GetTree().GetNodesInGroup("Thrusters").Cast<Node3D>().ToArray();
        hitboxes = GetTree().GetNodesInGroup("HitBoxesPlayer").Cast<Area3D>().ToArray();
        hurtboxes = GetTree().GetNodesInGroup("HurtBoxesPlayer").Cast<Area3D>().ToArray();
        footLightTrails = GetTree().GetNodesInGroup("FootLightTrails").Cast<Node3D>().ToArray();

        //AnimationTree = GetNode<AnimationTree>("AnimationTree");
        //AnimationNodeState = (AnimationNodeStateMachinePlayback) AnimationTree.Get("parameters/playback");

        //Set initial values
        Input.MouseMode = Input.MouseModeEnum.Captured;

        //Call important methods
        ControlTrusters(false);
        ControllHurtBoxes(true);
        ControllHitBoxes(false);
        ControllSaltoCollision(false);

        RunAcellerationRate = RunSpeed / RunAccelarationTime;
        TopAcellerationRate = (TopSpeed - RunSpeed) / TopAccelarationTime;
        ToRunDecelerationRate = (TopSpeed - RunSpeed) / ToRunDecelartionTime;
    }

    public override void _PhysicsProcess(double delta)
    {
        //Controll camera position manually
        Vector3 characterPosition = Position;

        //Get camera controller position
        Vector3 cameraPosition = CameraController.Position;

        if (MovementSpeed > RunSpeed)
        {
            ControllLightTrail(false);
        }
        else
        {
            ControllLightTrail(true);
        }

        InputDir = GetInputDirection();

        AlignWithFloor(groundDetector.GetCollisionNormal());

        AlignCamera(cameraPosition, characterPosition, delta);

        MoveAndSlide();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motion)
        {
            CameraController.RotateY(-motion.Relative.X * MouseSensitivity);

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
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            cameraTarget.RotationDegrees = new Vector3(pitch, cameraTarget.RotationDegrees.Y, cameraTarget.RotationDegrees.Z);
        }
    }

    public void AlignCamera(Vector3 cameraPosition, Vector3 characterPosition, double delta)
    {
        //Get Yaw camera degree
        Vector3 camForward = -CameraController.GlobalTransform.Basis.Z;
        camForward.Y = 0;
        camForward = camForward.Normalized();

        //Get Yaw camera degree
        Vector3 camYaw = -CameraController.GlobalTransform.Basis.X;
        camYaw.Y = 0;
        camYaw = camYaw.Normalized();

        Direction = (camYaw * InputDir.X + camForward * InputDir.Y).Normalized();

        //if (direction != Vector3.Zero)
        //{
        //    if (InputDir.Y < 0)
        //    {
        //        //Rotate player to camera angle
        //        float targetYaw = Mathf.Atan2(direction.X, direction.Z);
        //        float currentYaw = Rotation.Y;
        //        float newYaw = Mathf.LerpAngle(currentYaw, targetYaw, 10f * (float)delta);
        //        Rotation = new Vector3(Rotation.X, newYaw, Rotation.Z);
        //    }

        //    //Align camera slowly with palyer movement
        //    if (InputDir.Y > 0)
        //    {
        //        float targetYaw = RotationDegrees.Y;
        //        float currentYaw = CameraController.RotationDegrees.Y;
        //        float newYaw = Mathf.LerpAngle(currentYaw, targetYaw, 5f * (float)delta);

        //        CameraController.RotationDegrees = new Vector3(CameraController.RotationDegrees.X, newYaw, CameraController.RotationDegrees.Z);
        //    }
        //}

        CameraController.Position = cameraPosition.Lerp(characterPosition, 5f * (float)delta);
    }


    //Align character to floor
    public void AlignWithFloor(Vector3 floorNormal)
    {
        if (IsOnFloor())
        {
            calculateAligninWithFloor(groundDetector.GetCollisionNormal());
            GlobalTransform = GlobalTransform.InterpolateWith(xTransform, 0.3f);
        }
        else
        {
            calculateAligninWithFloor(Vector3.Up);
            GlobalTransform = GlobalTransform.InterpolateWith(xTransform, 0.3f);
        }
    }

    private void calculateAligninWithFloor(Vector3 floorNormal)
    {
        xTransform = GlobalTransform;
        xTransform.Basis.Y = floorNormal;
        xTransform.Basis.X = -xTransform.Basis.Z.Cross(floorNormal);
        xTransform.Basis = xTransform.Basis.Orthonormalized();
    }

    public void CalculateMovementSpeed(double delta)
    {
        if (IsOnFloor() && Input.IsActionPressed("Accelerate"))
        {
            MovementSpeed = Mathf.MoveToward(MovementSpeed, TopSpeed, TopAcellerationRate * (float)delta);
        }
        else if (MovementSpeed >= RunSpeed)
        {
            MovementSpeed = Mathf.MoveToward(MovementSpeed, RunSpeed, ToRunDecelerationRate * (float)delta);
        }
        else
        {
            MovementSpeed = Mathf.MoveToward(MovementSpeed, RunSpeed, RunAcellerationRate * (float)delta);
        }
    }

    public void CalculateVelocity(Vector2 inputDir, Vector3 direction, double delta)
    {
        if (inputDir != Vector2.Zero)
        {
            CalculateMovementSpeed(delta);

            if (direction != Vector3.Zero)
            {
                PlayerVelocity.X = direction.X * MovementSpeed;
                PlayerVelocity.Z = direction.Z * MovementSpeed;

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
                    float currentYaw = CameraController.RotationDegrees.Y;
                    float newYaw = Mathf.LerpAngle(currentYaw, targetYaw, 5f * (float)delta);

                    CameraController.RotationDegrees = new Vector3(CameraController.RotationDegrees.X, newYaw, CameraController.RotationDegrees.Z);
                }
            }
        }
        else
        {
            //Reduce movement speed when not pressing a button
            var stopRate =
                (MovementSpeed > 6f)
                ? TopSpeed / TopAccelarationTime
                : TopSpeed / RunAccelarationTime;

            MovementSpeed = Mathf.MoveToward(MovementSpeed, 0f, stopRate * (float)delta);

            PlayerVelocity.X = Mathf.MoveToward(Velocity.X, 0, stopRate * (float)delta);
            PlayerVelocity.Z = Mathf.MoveToward(Velocity.Z, 0, stopRate * (float)delta);
        }
    }

    public void ControllLightTrail(bool isHidden)
    {
        foreach (Node3D trail in footLightTrails)
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

    public void ControlTrusters(bool onOff)
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

    public void ControllHurtbox(Area3D hurtbox, bool onOff)
    {
        var collisionBox = hurtbox.GetChild<CollisionShape3D>(0);

        hurtbox.Monitorable = onOff;
        collisionBox.Disabled = !onOff;

        hurtbox.SetCollisionLayerValue(6, onOff);
        hurtbox.SetCollisionMaskValue(5, onOff);
        hurtbox.SetCollisionMaskValue(9, onOff);
    }

    public void ControllHurtBoxes(bool onOff)
    {
        foreach (Area3D hurtbox in hurtboxes)
        {
            ControllHurtbox(hurtbox, onOff);
        }
    }

    public void ControllHitbox(Area3D hitbox, bool onOff)
    {
        var collisionBox = hitbox.GetChild<CollisionShape3D>(0);

        hitbox.Monitorable = onOff;
        collisionBox.Disabled = !onOff;

        hitbox.SetCollisionLayerValue(7, onOff);
        hitbox.SetCollisionMaskValue(8, onOff);
    }

    public void ControllHitBoxes(bool onOff)
    {
        foreach (Area3D hitbox in hitboxes)
        {
            ControllHitbox(hitbox, onOff);
        }
    }

    public void ControllFeetCollision(bool onOffHurt, bool onOffHit)
    {
        var feetHitboxes = hitboxes.Where(hitbox => hitbox.Name.ToString().ToLower().Contains("foot"));
        var feetHurtboxes = hurtboxes.Where(hurtBox => hurtBox.Name.ToString().ToLower().Contains("foot"));

        foreach (Area3D footHurtbox in feetHurtboxes)
        {
            ControllHurtbox(footHurtbox, onOffHurt);
        }

        foreach (Area3D footHitbox in feetHitboxes)
        {
            ControllHitbox(footHitbox, onOffHit);
        }
    }

    public void ControllSaltoCollision(bool onOffSalto)
    {
        var saltoHitboxes = hitboxes.Where(hitbox => hitbox.Name.ToString().ToLower().Contains("salto"));

        foreach (Area3D saltoHitbox in saltoHitboxes)
        {
            ControllHitbox(saltoHitbox, onOffSalto);
        }
    }

    public async Task GameOver(string nextScene)
    {
        var sceneTree = GetTree();

        var timer = sceneTree.CreateTimer(0.1);
        await ToSignal(timer, Timer.SignalName.Timeout);

        if (sceneTree != null)
        {
            LoadScene(sceneTree, nextScene);
        }
    }

    public Vector2 GetInputDirection()
    {
        if (!CanInput)
        {
            return new Vector2();
        }

        var input = Input.GetVector("Left", "Right", "Forward", "Backward");
        return input;
    }

    public void LoadScene(SceneTree sceneTree, string sceneName)
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

    public void OnHurtboxEntered(Area3D hitbox)
    {
        if (hitbox.Name == "FallZone")
        {
            CurrentHealth = 0;
        }
        else
        {
            IsHurt = true;
        }

        //if (isHurt)
        //{
        //    return;
        //}

        //isHurt = true;
        //CallDeferred(nameof(ControllHurtBoxes), false);

        //if (hitbox.Name == "FallZone")
        //{
        //    if (soundManager != null)
        //    {
        //        soundManager.PlaySoundLose();
        //    }

        //    await GameOver("res://Menu/Nodes/menu_defeat.tscn");
        //}
        //else
        //{
        //    soundJump.PitchScale = rng.RandfRange(0.9f, 1.1f);
        //    soundGetHit.Play();
        //    await PlayDefeatedAnimation();
        //    await GameOver("res://Menu/Nodes/menu_defeat.tscn");
        //}
    }

    public void PlaySoundDoubleJump()
    {
        soundDoubleJump.PitchScale = 1.5f * rng.RandfRange(0.9f, 1.1f);
        soundDoubleJump.Play();
    }

    public void PlaySoundJump()
    {
        soundJump.PitchScale = 1.5f * rng.RandfRange(0.9f, 1.1f);
        soundJump.Play();
    }

    public void PlaySoundSkate()
    {
        soundSkate.PitchScale = rng.RandfRange(0.9f, 1.1f);
        soundSkate.Play();
    }

    public void PlaySoundOnHit()
    {
        soundGetHit.PitchScale = rng.RandfRange(0.9f, 1.1f);
        soundGetHit.Play();
    }

    public void PlaySoundGameOver()
    {
        if (SoundManager != null)
        {
            SoundManager.PlaySoundLose();
        }
    }

    public void PlaySoundLandOnGrass()
    {
        soundLandOnGrass.PitchScale = rng.RandfRange(0.9f, 1.1f);
        soundLandOnGrass.Play();
    }

    public void PlaySoundWalkOnGrass()
    {
        soundWalkOnGrass.PitchScale = rng.RandfRange(0.9f, 1.1f);
        soundWalkOnGrass.Play();
    }

    public void ReadyForInput()
    {
        CanInput = true;
    }
}