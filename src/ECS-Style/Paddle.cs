using MoonWorks.Input;

namespace Pong.ECS;

public record struct Paddle(
    KeyCode up,
    KeyCode down,
    bool left
);
