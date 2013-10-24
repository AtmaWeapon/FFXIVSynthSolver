using System;
using System.Collections.Generic;
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
      return StateFields.GetValue(ref state.storage, AbilityId);
    }

    public void SetTurnsRemaining(State state, uint value)
    {
      StateFields.SetValue(ref state.storage, AbilityId, value);
    }

    protected override void ActivateInternal(State oldState, State newState, bool success)
    {
      base.ActivateInternal(oldState, newState, success);

      // Set the number of turns to duration+1 so that we can tick it on the first turn
      // and avoid confusing logic surrounding whether or not the buff was just applied.
      SetTurnsRemaining(newState, duration+1);
      newState.tempEffects.Add(this);
    }

    public virtual void TickEnhancement(State state)
    {
      uint turns = GetTurnsRemaining(state);

      SetTurnsRemaining(state, turns - 1);

      if (turns == 1)
        state.tempEffects.Remove(this);
    }
  }

  [SynthAction(ActionType.TemporaryEnhancement, AbilityId.InnerQuiet, "Inner Quiet", 18)]
  [TemporaryEnhancement(0)]
  public class InnerQuiet : TemporaryEnhancementAbility
  {
  }

  [SynthAction(ActionType.TemporaryEnhancement, AbilityId.SteadyHand, "Steady Hand", 22)]
  [TemporaryEnhancement(5)]
  public class SteadyHand : TemporaryEnhancementAbility
  {
    public static bool IsActive(State state)
    {
      return StateFields.GetValue(ref state.storage, AbilityId.SteadyHand) > 0;
    }

    public static new uint GetTurnsRemaining(State state)
    {
      return StateFields.GetValue(ref state.storage, AbilityId.SteadyHand);
    }

    public static new void SetTurnsRemaining(State state, uint turns)
    {
      StateFields.SetValue(ref state.storage, AbilityId.SteadyHand, turns);
    }
  }

  // TODO
  [SynthAction(ActionType.TemporaryEnhancement, AbilityId.Manipulation, "Manipulation", 88)]
  [TemporaryEnhancement(3)]
  public class Manipulation : TemporaryEnhancementAbility
  {
    public static bool IsActive(State state)
    {
      return StateFields.GetValue(ref state.storage, AbilityId.Manipulation) > 0;
    }

    public static new uint GetTurnsRemaining(State state)
    {
      return StateFields.GetValue(ref state.storage, AbilityId.Manipulation);
    }

    public static new void SetTurnsRemaining(State state, uint turns)
    {
      StateFields.SetValue(ref state.storage, AbilityId.Manipulation, turns);
    }

    public override void TickEnhancement(State state)
    {
      base.TickEnhancement(state);

      if (GetTurnsRemaining(state) > 0)
        state.Durability = Math.Min(state.MaxDurability, state.Durability + 10);
    }
  }

  [SynthAction(ActionType.TemporaryEnhancement, AbilityId.Ingenuity, "Ingenuity", 24)]
  [TemporaryEnhancement(3)]
  public class Ingenuity : TemporaryEnhancementAbility
  {
    public static bool IsActive(State state)
    {
      return StateFields.GetValue(ref state.storage, AbilityId.Ingenuity) > 0;
    }

    public static new uint GetTurnsRemaining(State state)
    {
      return StateFields.GetValue(ref state.storage, AbilityId.Ingenuity);
    }

    public static new void SetTurnsRemaining(State state, uint turns)
    {
      StateFields.SetValue(ref state.storage, AbilityId.Ingenuity, turns);
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
      return StateFields.GetValue(ref state.storage, AbilityId.GreatStrides) > 0;
    }

    public static new uint GetTurnsRemaining(State state)
    {
      return StateFields.GetValue(ref state.storage, AbilityId.GreatStrides);
    }

    public static new void SetTurnsRemaining(State state, uint turns)
    {
      StateFields.SetValue(ref state.storage, AbilityId.GreatStrides, turns);
    }

    public override bool CanUse(State state)
    {
      if (!base.CanUse(state))
        return false;

      return state.Quality <= state.MaxQuality;
    }
  }
}
