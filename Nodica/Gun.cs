using Nodica;

namespace Nodica;

public class Gun : Sprite
{
    public Color SexColor { get; set; } = new(25, 32, 37, 1);
    public ButtonThemePack Dollar { get; set; } = new();

    private RayCast rayCast = new();
    private readonly string gunshotAudioPath = "Res/Audio/SFX/Gunshot.mp3";

    private readonly float cooldown = 0.182f;
    private float lastFiredTime = 0f;
    private const float knockbackForce = 3f;

    public override void Ready()
    {
        base.Ready();

        RootNode.PrintChildren();

        HorizontalAlignment = HorizontalAlignment.Left;
        Offset = new(8, 0);

        rayCast = GetNode<RayCast>("RayCast");
        rayCast.Deactivate();
    }

    public override void Update()
    {
        base.Update();

        FlipV = Input.MousePosition.X < GlobalPosition.X;

        if (Input.IsActionDown("Fire") && Time.Elapsed - lastFiredTime >= cooldown)
        {
            Fire();
        }

        LookAt(Input.MousePosition);
    }

    private void PlayGunshotSound()
    {
        AudioPlayer newAudioPlayer = new()
        {
            Audio = ResourceLoader.Load<Audio>(gunshotAudioPath)
        };

        AddChild(newAudioPlayer);
        newAudioPlayer.Finished += OnGunshotAudioFinished;
        newAudioPlayer.Play();
    }

    private void OnGunshotAudioFinished(AudioPlayer sender)
    {
        sender.Destroy();
    }

    private void Fire()
    {
        lastFiredTime = Time.Elapsed;
        PlayGunshotSound();
        FireRaycast();
    }

    private void FireRaycast()
    {
        //Vector2 mousePosition = Input.MousePosition;
        //Vector2 angleVector = mousePosition - GlobalPosition;
        //float angle = MathF.Atan2(angleVector.Y, angleVector.X);
        //
        //rayCast.Rotation = angle * 180 / MathF.PI;
        //
        //rayCast.GlobalPosition = GlobalPosition;
        //
        //rayCast.Update();
        //
        //if (rayCast.IsColliding)
        //{
        //    Node2D? collider = rayCast.Collider;
        //
        //    if (collider is not null)
        //    {
        //        if (collider is Enemy enemy)
        //        {
        //            enemy.TakeDamage(1);
        //            enemy.ApplyKnockback(angleVector.Normalized() * knockbackForce);
        //        }
        //    }
        //}
        //
        //rayCast.Deactivate();
    }
}