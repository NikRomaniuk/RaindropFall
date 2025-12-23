namespace RaindropFall;

public static class LevelCatalog
{
    public static readonly LevelProperties Level1 = new(
        title: "Level 1",
        backgroundColor: Color.FromArgb("#BFD7FF"), // Pastel blue
        playerSpeed: 40,
        playerAcceleration: 50,
        fallingSpeed: 40,
        damagePerHit: 40,
        buildFormation: BuildVFormation);

    public static readonly LevelProperties Level2 = new(
        title: "Level 2",
        backgroundColor: Color.FromArgb("#BFF5E1"), // Mint
        playerSpeed: 40,
        playerAcceleration: 50,
        fallingSpeed: 50,
        damagePerHit: 10,
        buildFormation: BuildLineFormation);

    private static void BuildVFormation(FlowGroup g)
    {
        g.AddObstacle(5, 5, Colors.Red, 5);
        g.AddObstacle(5, -5, Colors.Red, 5);
        g.AddObstacle(-5, 5, Colors.Red, 5);
        g.AddObstacle(-5, -5, Colors.Red, 5);
        g.AddObstacle(-25, 0, Colors.DarkRed, 5);
        g.AddObstacle(25, 0, Colors.PaleVioletRed, 5);
    }

    private static void BuildLineFormation(FlowGroup g)
    {
        g.AddObstacle(-50, 5, Colors.PaleVioletRed, 5);
        g.AddObstacle(-25, -5, Colors.Red, 5);
        g.AddObstacle(0, 5, Colors.DarkRed, 5);
        g.AddObstacle(25, -5, Colors.Red, 5);
        g.AddObstacle(50, 5, Colors.PaleVioletRed, 5);
    }
}
