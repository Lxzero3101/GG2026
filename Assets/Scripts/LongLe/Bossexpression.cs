/// <summary>
/// Represents the possible facial expressions the boss portrait can display,
/// ordered from calmest to angriest. This order matters: it's used to compute
/// which expression corresponds to a given patience stage and to determine the
/// "next angrier" expression for flash previews. Add new values here to expand
/// the range of expressions; no other code changes are required as long as a
/// matching sprite is assigned in the Inspector.
/// </summary>
public enum BossExpression
{
    Neutral,
    Annoyed,
    Disappointed,
    Angry,
    Furious
}