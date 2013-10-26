using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Engine
{
  public class TemporaryEnhancementAbility : Ability
  {
    private uint duration;

    public TemporaryEnhancementAbility()
    {
      TemporaryEnhancementAttribute attributes = SynthAction<TemporaryEnhancementAttribute>.Attributes(this);

      duration = attributes.Duration;
    }

    public uint Duration { get { return duration; } }

    public virtual bool IsPermanent
    {
      get { return false; }
    }
    public override bool CanFail
    {
      get { return false; }
    }

    public override bool CanUse(State state)
    {
      if (!base.CanUse(state))
        return false;

      if (GetTurnsRemaining(state) > 0)
        return false;

      return true;
    }

    public uint GetTurnsRemaining(State state)
    {
      switch (AbilityId)
      {
        case Engine.AbilityId.GreatStrides:
          return state.details.GreatStridesTurns;
        case Engine.AbilityId.Ingenuity:
          return state.details.IngenuityTurns;
        case Engine.AbilityId.Manipulation:
          return state.details.ManipulationTurns;
        case Engine.AbilityId.SteadyHand:
          return state.details.SteadyHandTurns;
        case Engine.AbilityId.InnerQuiet:
          return (state.InnerQuietIsActive) ? 1U : 0U;
        case Engine.AbilityId.WasteNot:
          return state.details.WasteNotTurns;
        case Engine.AbilityId.SteadyHand2:
          return state.details.SteadyHand2Turns;
        case Engine.AbilityId.Ingenuity2:
          return state.details.Ingenuity2Turns;
        case Engine.AbilityId.ComfortZone:
          return state.details.ComfortZoneTurns;
        case Engine.AbilityId.Innovation:
          return state.details.InnovationTurns;
        case Engine.AbilityId.WasteNot2:
          return state.details.WasteNot2Turns;
        default:
          throw new InvalidOperationException();
      }
    }

    public void SetTurnsRemaining(State state, uint value)
    {
      switch (AbilityId)
      {
        case Engine.AbilityId.GreatStrides:
          state.details.GreatStridesTurns = value;
          break;
        case Engine.AbilityId.Ingenuity:
          state.details.IngenuityTurns = value;
          break;
        case Engine.AbilityId.Manipulation:
          state.details.ManipulationTurns = value;
          break;
        case Engine.AbilityId.SteadyHand:
          state.details.SteadyHandTurns = value;
          break;
        case Engine.AbilityId.InnerQuiet:
          state.details.InnerQuietIsActive = (value > 0);
          break;
        case Engine.AbilityId.WasteNot:
          state.details.WasteNotTurns = value;
          break;
        case Engine.AbilityId.SteadyHand2:
          state.details.SteadyHand2Turns = value;
          break;
        case Engine.AbilityId.Ingenuity2:
          state.details.Ingenuity2Turns = value;
          break;
        case Engine.AbilityId.ComfortZone:
          state.details.ComfortZoneTurns = value;
          break;
        case Engine.AbilityId.Innovation:
          state.details.InnovationTurns = value;
          break;
        case Engine.AbilityId.WasteNot2:
          state.details.WasteNot2Turns = value;
          break;
        default:
          throw new InvalidOperationException();
      }
    }

    protected override void ActivateInternal(State oldState, State newState, bool success)
    {
      base.ActivateInternal(oldState, newState, success);

      // Set the number of turns to duration+1 so that we can tick it on the first turn
      // and avoid confusing logic surrounding whether or not the buff was just applied.
      SetTurnsRemaining(newState, duration+1);
      newState.tempEffects.Add(this);
    }

    public void TickEnhancement(State state)
    {
      if (IsPermanent)
        ApplyPeriodicEffect(state);
      else
      {
        uint turns = GetTurnsRemaining(state);
        Debug.Assert(turns > 0);

        // If this enhancement was not just applied on this same turn, apply
        // the periodic affect
        if (turns <= duration)
          ApplyPeriodicEffect(state);

        SetTurnsRemaining(state, turns - 1);
      }
    }

    protected virtual void ApplyPeriodicEffect(State state)
    {

    }
  }

  [SynthAction(ActionType.TemporaryEnhancement, AbilityId.InnerQuiet, "Inner Quiet", 18)]
  [TemporaryEnhancement(0)]
  public class InnerQuiet : TemporaryEnhancementAbility
  {
    public override bool IsPermanent
    {
      get
      {
        return true;
      }
    }
    public static bool IsActive(State state)
    {
      return state.InnerQuietIsActive;
    }

    public static uint InnerQuietStacks(State state)
    {
      return state.InnerQuietStacks;
    }
  }

  [SynthAction(ActionType.TemporaryEnhancement, AbilityId.SteadyHand, "Steady Hand", 22)]
  [TemporaryEnhancement(5)]
  public class SteadyHand : TemporaryEnhancementAbility
  {
    public static bool IsActive(State state)
    {
      return state.SteadyHandTurns > 0;
    }

    public static new uint GetTurnsRemaining(State state)
    {
      return state.SteadyHandTurns;
    }

    public static new void SetTurnsRemaining(State state, uint turns)
    {
      state.SteadyHandTurns = turns;
    }
  }

  // TODO
  [SynthAction(ActionType.TemporaryEnhancement, AbilityId.Manipulation, "Manipulation", 88)]
  [TemporaryEnhancement(3)]
  public class Manipulation : TemporaryEnhancementAbility
  {
    public static bool IsActive(State state)
    {
      return state.ManipulationTurns > 0;
    }

    public static new uint GetTurnsRemaining(State state)
    {
      return state.ManipulationTurns;
    }

    public static new void SetTurnsRemaining(State state, uint turns)
    {
      state.ManipulationTurns = turns;
    }

    protected override void ApplyPeriodicEffect(State state)
    {
      state.Durability = Math.Min(state.MaxDurability, state.Durability + 10);
    }
  }

  [SynthAction(ActionType.TemporaryEnhancement, AbilityId.Ingenuity, "Ingenuity", 24)]
  [TemporaryEnhancement(3)]
  public class Ingenuity : TemporaryEnhancementAbility
  {
    public static bool IsActive(State state)
    {
      return state.IngenuityTurns > 0;
    }

    public static new uint GetTurnsRemaining(State state)
    {
      return state.IngenuityTurns;
    }

    public static new void SetTurnsRemaining(State state, uint turns)
    {
      state.details.IngenuityTurns = turns;
    }

    public override bool CanUse(State state)
    {
      if (!base.CanUse(state))
        return false;

      return state.LevelSurplus <= -2;
    }
  }

  [SynthAction(ActionType.TemporaryEnhancement, AbilityId.GreatStrides, "Great Strides", 32)]
  [TemporaryEnhancement(3)]
  public class GreatStrides : TemporaryEnhancementAbility
  {
    public static bool IsActive(State state)
    {
      return state.GreatStridesTurns > 0;
    }

    public static new uint GetTurnsRemaining(State state)
    {
      return state.GreatStridesTurns;
    }

    public static new void SetTurnsRemaining(State state, uint turns)
    {
      state.GreatStridesTurns = turns;
    }

    public override bool CanUse(State state)
    {
      if (!base.CanUse(state))
        return false;

      return state.Quality <= state.MaxQuality;
    }
  }

  [SynthAction(ActionType.TemporaryEnhancement, AbilityId.WasteNot, "Waste Not", 53)]
  [TemporaryEnhancement(4)]
  public class WasteNot : TemporaryEnhancementAbility
  {
    public static bool IsActive(State state)
    {
      return state.WasteNotTurns > 0;
    }

    public static new uint GetTurnsRemaining(State state)
    {
      return state.WasteNotTurns;
    }

    public static new void SetTurnsRemaining(State state, uint turns)
    {
      state.WasteNotTurns = turns;
    }

    public override bool CanUse(State state)
    {
      if (!base.CanUse(state))
        return false;

      return !WasteNot2.IsActive(state);
    }
  }

  [SynthAction(ActionType.TemporaryEnhancement, AbilityId.SteadyHand2, "Steady Hand II", 35)]
  [TemporaryEnhancement(5)]
  public class SteadyHand2 : TemporaryEnhancementAbility
  {
    public static bool IsActive(State state)
    {
      return state.SteadyHand2Turns > 0;
    }

    public static new uint GetTurnsRemaining(State state)
    {
      return state.SteadyHand2Turns;
    }

    public static new void SetTurnsRemaining(State state, uint turns)
    {
      state.SteadyHand2Turns = turns;
    }

    public override bool CanUse(State state)
    {
      if (!base.CanUse(state))
        return false;

      return !SteadyHand.IsActive(state);
    }
  }

  [SynthAction(ActionType.TemporaryEnhancement, AbilityId.Ingenuity2, "Ingenuity II", 85)]
  [TemporaryEnhancement(3)]
  public class Ingenuity2 : TemporaryEnhancementAbility
  {
    public static bool IsActive(State state)
    {
      return state.Ingenuity2Turns > 0;
    }

    public static new uint GetTurnsRemaining(State state)
    {
      return state.Ingenuity2Turns;
    }

    public static new void SetTurnsRemaining(State state, uint turns)
    {
      state.details.Ingenuity2Turns = turns;
    }

    public override bool CanUse(State state)
    {
      if (!base.CanUse(state))
        return false;

      return state.LevelSurplus <= -2;
    }
  }

  [SynthAction(ActionType.TemporaryEnhancement, AbilityId.ComfortZone, "Comfort Zone", 58)]
  [TemporaryEnhancement(10)]
  public class ComfortZone : TemporaryEnhancementAbility
  {
    public static bool IsActive(State state)
    {
      return state.ComfortZoneTurns > 0;
    }

    public static new uint GetTurnsRemaining(State state)
    {
      return state.ComfortZoneTurns;
    }

    public static new void SetTurnsRemaining(State state, uint turns)
    {
      state.details.ComfortZoneTurns = turns;
    }

    protected override void ApplyPeriodicEffect(State state)
    {
      base.ApplyPeriodicEffect(state);

      state.CP = Math.Min(state.CP + 10, state.MaxCP);
    }
  }

  [SynthAction(ActionType.TemporaryEnhancement, AbilityId.Innovation, "Innovation", 18)]
  [TemporaryEnhancement(5)]
  public class Innovation : TemporaryEnhancementAbility
  {
    public static bool IsActive(State state)
    {
      return state.InnovationTurns > 0;
    }

    public static new uint GetTurnsRemaining(State state)
    {
      return state.InnovationTurns;
    }

    public static new void SetTurnsRemaining(State state, uint turns)
    {
      state.details.InnovationTurns = turns;
    }

  }

  [SynthAction(ActionType.TemporaryEnhancement, AbilityId.WasteNot2, "Waste Not II", 95)]
  [TemporaryEnhancement(8)]
  public class WasteNot2 : TemporaryEnhancementAbility
  {
    public static bool IsActive(State state)
    {
      return state.WasteNot2Turns > 0;
    }

    public static new uint GetTurnsRemaining(State state)
    {
      return state.WasteNot2Turns;
    }

    public static new void SetTurnsRemaining(State state, uint turns)
    {
      state.WasteNot2Turns = turns;
    }

    public override bool CanUse(State state)
    {
      if (!base.CanUse(state))
        return false;

      return !WasteNot.IsActive(state);
    }
  }

}
