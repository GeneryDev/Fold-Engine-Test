# Project Organization

A project using the Fold Engine should consist of 3 types of solutions or assemblies, each serving different purposes:

1. **Engine-Fold**: The Fold Engine code. This is platform-agnostic, game-agnostic logic that drives all games built using Fold.
    Created from a MonoGame Game Library template.
2. **Game Solution**: This should the code and assets specific to the game, designed to run for any platform. Created from a MonoGame Game Library template, with dependencies on the Fold Engine assembly.
    Convention is to prefix the solution name with `Game-`.
3. **Entry Solution**: One or more solutions, each for a specific target platform. Communicates with the Engine to start the Game using whatever platform-specific configurations are appropriate for the target platform.
    Created from a MonoGame project template for the specific targeted platform, with dependencies on both the game and engine assemblies.
    Convention is to prefix the solution name with `Entry-`, followed by some identifier for the platform name it is targeting.

