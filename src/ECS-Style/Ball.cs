using MoonWorks.Math.Float;

namespace Pong.ECS;

public record struct Ball(
    Vector2 velocity,
    float speed
);
