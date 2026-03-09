# Zombie RTS / Looter Shooter (Unity DOTS)

A high-performance multiplayer zombie game built using Unity DOTS/ECS, focused on scalability, server authority, and deterministic combat.
This project is separate from the multiplayer version of the project, which remains private.




https://github.com/user-attachments/assets/64a3e2fa-e16b-4cf4-afbd-1467a45f3857

## Code Structure

- `Assets/Scripts/Authoring/` — ECS authoring components and bakers for converting GameObjects into entities
- `Assets/Scripts/MonoBehaviors/` — Hybrid MonoBehaviour logic (bootstrapping, NetCode setup, non-ECS systems)
- `Assets/Scripts/Systems/` — Core ECS systems (movement, combat, weapons, AI, projectiles)
- `Assets/Scripts/UI/` — User interface logic and HUD systems
