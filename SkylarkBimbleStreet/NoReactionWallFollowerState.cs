namespace SkylarkBimbleStreet;

/// <summary>
/// 片手壁伝い法の［無反応］状態を表すクラス。
/// </summary>
internal sealed class NoReactionWallFollowerState : IWallFollowerState
{
    public WallFollowerStateKind Kind => WallFollowerStateKind.NoReaction;

    public void Move(WallFollowerContext context)
    {
        if (!context.TryMove(out var hitWallContact))
        {
            context.MoveBeforeWall(hitWallContact);
            return;
        }

        context.SetState(this);
        context.RememberWallParallelMove();
    }

    public void Continue(WallFollowerContext context) => context.SetState(this);
}
