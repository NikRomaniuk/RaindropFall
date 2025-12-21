namespace RaindropFall;

public static class LevelCatalog
{
    public static readonly LevelProperties Level1 = new(
        title: "Level 1",
        backgroundColor: Color.FromArgb("#BFD7FF"), // Pastel blue
        playerSpeed: 70,
        fallingSpeed: 60,
        damagePerHit: 10,
        buildFormation: BuildVFormation);

    public static readonly LevelProperties Level2 = new(
        title: "Level 2",
        backgroundColor: Color.FromArgb("#BFF5E1"), // Mint
        playerSpeed: 60,
        fallingSpeed: 70,
        damagePerHit: 40,
        buildFormation: BuildLineFormation);

    private static void BuildVFormation(FlowGroup g)
    {
        g.AddObstacle(0.0, 0.0, Colors.Red, 10);
        g.AddObstacle(-0.25, 0.1, Colors.DarkRed, 10);
        g.AddObstacle(0.25, 0.1, Colors.PaleVioletRed, 10);
    }

    private static void BuildLineFormation(FlowGroup g)
    {
        g.AddObstacle(-0.5, 0.0, Colors.PaleVioletRed, 5);
        g.AddObstacle(-0.25, 0.0, Colors.Red, 5);
        g.AddObstacle(0.0, 0.0, Colors.DarkRed, 5);
        g.AddObstacle(0.25, 0.0, Colors.Red, 5);
        g.AddObstacle(0.5, 0.0, Colors.PaleVioletRed, 5);
    }
}
