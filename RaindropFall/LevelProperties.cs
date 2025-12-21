namespace RaindropFall;

public sealed class LevelProperties
{
    public string Title { get; }
    public Color BackgroundColor { get; }

    public double PlayerSpeed { get; }
    public double PlayerAcceleration { get; }
    public double FallingSpeed { get; }
    public int DamagePerHit { get; }

    public Action<FlowGroup> BuildFormation { get; }

    public LevelProperties(
        string title,
        Color backgroundColor,
        double playerSpeed,
        double playerAcceleration,
        double fallingSpeed,
        int damagePerHit,
        Action<FlowGroup> buildFormation)
    {
        Title = title;
        BackgroundColor = backgroundColor;
        PlayerSpeed = playerSpeed;
        PlayerAcceleration = playerAcceleration;
        FallingSpeed = fallingSpeed;
        DamagePerHit = damagePerHit;
        BuildFormation = buildFormation;
    }
}
