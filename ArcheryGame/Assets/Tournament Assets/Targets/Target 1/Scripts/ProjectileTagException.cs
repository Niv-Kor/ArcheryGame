public class ProjectileTagException : System.Exception
{
    private static readonly string MESSAGE =
        "Must provide 2 different, non-empty values for \"Projectile Tag\"" +
        "and \"Cleared Projectile Tag\" parameters,\nunder the ScoreManager script." +
        " Values must be existing tags.";

    public ProjectileTagException() : base(MESSAGE) {}
}