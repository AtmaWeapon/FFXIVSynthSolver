using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Engine
{
  public class FixedDurationEnhancementAction : Action
  {
    private uint duration;

    public FixedDurationEnhancementAction()
    {
      FixedDurationEnhancementAttribute attributes = SynthAction<FixedDurationEnhancementAttribute>.Attributes(this);

      duration = attributes.Duration;
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
      return StateFields.GetValue(ref state.storage, ActionId);
    }

    public void SetTurnsRemaining(State state, uint value)
    {
      StateFields.SetValue(ref state.storage, ActionId, value);
    }

    protected override void ApplyAction(State oldState, State newState, bool success)
    {
      base.ApplyAction(oldState, newState, success);

      // Set the number of turns to duration+1 so that we can tick it on the first turn
      // and avoid confusing logic surrounding whether or not the buff was just applied.
      SetTurnsRemaining(newState, duration+1);
    }

    public virtual void TickEnhancement(State oldState, State newState)
    {
      uint turns = GetTurnsRemaining(newState);

      SetTurnsRemaining(newState, turns - 1);
    }
  }

  [SynthAction(ActionType.FixedDurationEnhancement, ActionId.SteadyHand, "Steady Hand", 22)]
  [FixedDurationEnhancement(5)]
  public class SteadyHand : FixedDurationEnhancementAction
  {
    public static bool IsActive(State state)
    {
      return StateFields.GetValue(ref state.storage, ActionId.SteadyHand) > 0;
    }
  }

  // TODO
  [SynthAction(ActionType.FixedDurationEnhancement, ActionId.Manipulation, "Manipulation", 88)]
  [FixedDurationEnhancement(3)]
  public class Manipulation : FixedDurationEnhancementAction
  {
    public override void TickEnhancement(State oldState, State newState)
    {
      base.TickEnhancement(oldState, newState);

      if (GetTurnsRemaining(newState) > 0)
        newState.Durability = Math.Min(newState.MaxDurability, newState.Durability + 10);
    }
  }

  [SynthAction(ActionType.FixedDurationEnhancement, ActionId.Ingenuity, "Ingenuity", 24)]
  [FixedDurationEnhancement(3)]
  public class Ingenuity : FixedDurationEnhancementAction
  {
    public override bool CanUse(State state)
    {
      if (!base.CanUse(state))
        return false;

      return state.LevelSurplus <= -2;
    }
  }

  [SynthAction(ActionType.FixedDurationEnhancement, ActionId.GreatStrides, "Great Strides", 32)]
  [FixedDurationEnhancement(3)]
  public class GreatStrides : FixedDurationEnhancementAction
  {
    public static bool IsActive(State state)
    {
      return StateFields.GetValue(ref state.storage, ActionId.GreatStrides) > 0;
    }

    public static new void SetTurnsRemaining(State state, uint turns)
    {
      StateFields.SetValue(ref state.storage, ActionId.GreatStrides, turns);
    }

    public override bool CanUse(State state)
    {
      if (!base.CanUse(state))
        return false;

      return state.Quality <= state.MaxQuality;
    }
  }
}
