# Testing Guide

## Overview

This project uses **Hexagonal Architecture** (Ports & Adapters) to make the business logic testable despite heavy dependencies on Unity and IL2CPP.

## Architecture

```
┌─────────────────────────────────────────────────┐
│         ItemTooltipsMod (MelonMod)             │
│         Unity/IL2CPP Dependent Layer           │
└────────────────┬────────────────────────────────┘
                 │
                 │ implements interfaces
                 ▼
┌─────────────────────────────────────────────────┐
│         Abstractions (Interfaces)               │
│  IGameDataProvider, IPlayerStateProvider, etc. │
└────────────────┬────────────────────────────────┘
                 │
                 │ injected into
                 ▼
┌─────────────────────────────────────────────────┐
│      Services (Testable Business Logic)        │
│    EvolutionCalculator, PopupPositioner, etc.  │
└─────────────────────────────────────────────────┘
```

## What CAN Be Tested

✅ **Business Logic**:
- Evolution formula calculations
- Popup positioning algorithms
- Icon grid layout
- State transitions (controller modes, navigation)

✅ **Data Processing**:
- Sprite name lookups
- Type mappings
- Ownership/ban checking logic

✅ **Pure Functions**:
- Position clamping
- Grid calculations
- String processing

## What CANNOT Be Tested Easily

❌ **Unity/IL2CPP Specifics**:
- Harmony patches (runtime IL modification)
- Actual GameObject creation
- IL2CPP pointer manipulation
- Reflection-based type discovery

These are tested **manually** by running the mod in-game.

## Running Tests Locally

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter FullyQualifiedName~EvolutionCalculatorTests

# Watch mode (re-run on file changes)
dotnet watch test
```

## Running Tests in GitHub Actions

Tests run automatically on:
- Push to `main` or `cleanup` branches
- Pull requests

See `.github/workflows/test.yml` for configuration.

## Writing New Tests

### 1. Create Mock Dependencies

```csharp
var gameData = new MockGameDataProvider();
gameData.AddWeapon(WeaponType.WHIP, "Whip", "Basic weapon");
gameData.AddWeapon(WeaponType.HOLLOW_HEART, "Hollow Heart", "Passive", 
    evolvesInto: "BLOODY_TEAR", WeaponType.PUMMAROLA);

var playerState = new MockPlayerStateProvider();
playerState.SetOwnsWeapon(WeaponType.WHIP, true);
```

### 2. Create Service Under Test

```csharp
var calculator = new EvolutionCalculator(gameData, playerState, new MockLogger());
```

### 3. Test the Behavior

```csharp
[Fact]
public void GetWeaponEvolutions_ReturnsFormula_WhenWeaponEvolves()
{
    // Arrange - setup done in constructor
    
    // Act
    var formulas = calculator.GetWeaponEvolutions(WeaponType.WHIP);
    
    // Assert
    Assert.Single(formulas);
    Assert.Equal(WeaponType.HOLLOW_HEART, formulas[0].EvolvedWeapon);
}
```

## Current Limitations

1. **IL2CPP Types**: Can't create real `WeaponData`, `ItemData` objects in tests
   - **Solution**: Use DTOs or extend abstractions

2. **Sprite Loading**: Can't load actual game sprites
   - **Solution**: Mocks return `null`, or use placeholder sprites

3. **Unity API**: Can't test actual Unity UI creation
   - **Solution**: Test layout calculations, not rendering

## Future Improvements

- [ ] Extract more business logic into testable services
- [ ] Create DTOs to avoid IL2CPP types in tests
- [ ] Add integration tests (run mod in headless Unity)
- [ ] Add snapshot testing for popup layouts
- [ ] Mock reflection operations for better coverage

## Refactoring for Testability Checklist

When adding new features, ask:

1. ✅ Can this logic run without Unity/IL2CPP?
2. ✅ Can I inject dependencies instead of using statics?
3. ✅ Can I separate "what to do" from "how to do it"?
4. ✅ Can I test this with plain .NET types?

If yes to all → Put it in a **Service** class  
If no → Keep it in the **Adapter** layer (mod/patches)
