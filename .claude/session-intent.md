# Session Intent Contract

**Created:** 2026-04-05
**Session:** priceless-davinci

## Job Statement
Build a complete, working MVP of "Fast and Acro" — an arcade-style auto-driving racing mobile game in Unity (2022 LTS+, URP, C#). The user is new to Unity and needs clear architecture, step-by-step guidance, and production-quality code they can drop into a Unity project.

## User Profile
- **Goal:** Build something (MVP implementation)
- **Knowledge Level:** Just starting (new to Unity/C# game dev)
- **Scope Clarity:** General direction (detailed spec provided, open to suggestions on specifics)
- **Success Criteria:** Working solution + Clear understanding + Production-ready
- **Constraints:** Time pressure — needs results quickly

## Success Criteria
1. **Buildable MVP scripts** — All C# scripts compile and work in Unity 2022 LTS+ with URP
2. **Clear understanding** — User knows what each script does and how they connect
3. **Production-ready quality** — Clean architecture, proper separation of concerns, mobile-optimized
4. **MVP-scoped** — No overengineering, only the 9 features listed in scope

## MVP Scope (9 Features)
1. One car (3D model placeholder)
2. One level (simple track + obstacles)
3. Auto forward movement
4. Left/right player control (touch input)
5. Basic collision + HP system
6. Level completion detection
7. Basic UI (start screen, end screen, HUD)
8. Simple star scoring system (1-3 stars)
9. Basic face image upload and display on car

## Boundaries
- **IN scope:** MVP features listed above, clean architecture, mobile touch input
- **OUT of scope:** Multiple levels, car upgrades, abilities (boost/shield/slow-mo), progression system, multiple cars, localization, analytics, IAP, ads
- **Platform:** Android first (iOS later)
- **Engine:** Unity 2022 LTS+, URP, C# .NET Standard 2.1

## Architecture Requirements
- GameManager (game state machine)
- CarController (auto movement + left/right input)
- CollisionHandler (HP management)
- LevelManager (level flow, completion)
- UIManager (screens, HUD)
- ScoreManager (star calculation)
- FaceCustomizationManager (image upload + display)
- ImageLoader utility (gallery/camera → Texture2D)

## Key Decisions
- Touch input: swipe or tap-and-hold for left/right
- Collision physics: fun, non-realistic (bounce/knockback)
- Track design: straight road with lanes, obstacles placed along path
- Face display: simple plane/quad above car with uploaded texture
