﻿using System;
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
      uint turns = GetTurnsRemaining(state);
      Debug.Assert(turns > 0);

      // If this enhancement was not just applied on this same turn, apply
      // the periodic affect
      if (turns <= duration)
        ApplyPeriodicEffect(state);

      SetTurnsRemaining(state, turns - 1);

      // If we initially had 1 turn going into this tick, then we're now done.
      if (turns == 1)
        state.tempEffects.Remove(this);
    }

    protected virtual void ApplyPeriodicEffect(State state)
    {

    }
  }

  [SynthAction(ActionType.TemporaryEnhancement, AbilityId.InnerQuiet, "Inner Quiet", 18)]
  [TemporaryEnhancement(0)]
  public class InnerQuiet : TemporaryEnhancementAbility
  {
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
}