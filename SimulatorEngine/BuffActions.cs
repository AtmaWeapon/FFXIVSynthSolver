using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Engine
{

  public abstract class BuffAction : Action
  {
    public BuffAction()
    {
      usageChecks.Add(delegate(State state)
                      {
                        // Don't allow this buff to be used if this buff is already active.
                        return !IsBuffActive(state);
                      });
      successOperators.Add(delegate(State originalState, State newState) 
                           {
                             ApplyBuff(newState);
                           });
    }

    public abstract uint GetTurnsRemaining(State state);
    public abstract void SetTurnsRemaining(State state, uint turns);

    public bool IsBuffActive(State state)
    {
      return GetTurnsRemaining(state) > 0;
    }

    // Called when the Step advances while the buff is active.
    public virtual void TickBuff(State state)
    {
      uint remain = GetTurnsRemaining(state);
      if (remain > 0)
        SetTurnsRemaining(state, remain-1);
    }

    // Called when a buff is applied for the first time.
    public virtual void ApplyBuff(State state)
    {
      SetTurnsRemaining(state, Attributes.BuffDuration + 1);
    }
  }

  [SynthAction(ActionType.Buff, Name="Master's Mend", CP=92, BuffDuration=0)]
  public class MastersMend : BuffAction
  {
    public MastersMend()
    {
      // Don't allow Master's Mend to be used if it won't be fully effective.
      usageChecks.Add(delegate(State state) { return (state.MaxDurability - state.Durability >= 30); });
    }

    public override void ApplyBuff(State state)
    {
      state.Durability = Math.Min(state.Durability + 30, state.MaxDurability);
    }

    public override uint GetTurnsRemaining(State state) { return 0; }
    public override void SetTurnsRemaining(State state, uint turns) { }
  }

  [SynthAction(ActionType.Buff, Name = "Steady Hand", CP = 22, BuffDuration=5)]
  public class SteadyHand : BuffAction
  {
    public override uint GetTurnsRemaining(State state) { return state.SteadyHandTurns; }
    public override void SetTurnsRemaining(State state, uint turns) { state.SteadyHandTurns = turns; }
  }

  [SynthAction(ActionType.Buff, Name = "Observe", CP = 14, BuffDuration=0)]
  public class Observe : BuffAction
  {
    // Observe only consumes CP (to give the condition a chance to change).
    // It doesn't actually apply any buff or do anything.  Because the entire point
    // is to make the condition better, don't allow it to be considered if the 
    // condition is Good or Excellent.
    public Observe()
    {
      usageChecks.Add(delegate(State s) { return (s.Condition == Condition.Poor); });
      usageChecks.Add(delegate(State s)
                      {
                        // Don't let Observe run first.
                        if (s.Step == 1)
                          return false;
                        return !(s.LeadingAction is Observe);
                      });
    }
    public override uint GetTurnsRemaining(State state) { return 0; }
    public override void SetTurnsRemaining(State state, uint turns) { }
  }

  [SynthAction(ActionType.Buff, Name = "Tricks of the Trade", CP = 0, BuffDuration = 0)]
  public class TricksOfTheTrade : BuffAction
  {
    public TricksOfTheTrade()
    {
      // By definition the ability is disabled unless Condition==Good.
      usageChecks.Add(delegate(State state) { return state.Condition == Condition.Good; });
      // Only use Tricks of the trade if we actually  need the full 20 CP gain.
      usageChecks.Add(delegate(State state) { return state.MaxCP - state.CP >= 20; });
    }

    public override void ApplyBuff(State state)
    {
      state.CP = Math.Min(state.CP + 20, state.MaxCP);
    }

    public override uint GetTurnsRemaining(State state) { return 0; }
    public override void SetTurnsRemaining(State state, uint turns) { }
  }

  // TODO
  [SynthAction(ActionType.Buff, Name = "Inner Quiet", CP = 18, Disabled = true, BuffDuration=0)]
  public class InnerQuiet : BuffAction
  {
    public override uint GetTurnsRemaining(State state) { return 0; }
    public override void SetTurnsRemaining(State state, uint turns) { }
  }

  // TODO
  [SynthAction(ActionType.Buff, Name = "Manipulation", CP = 88, BuffDuration = 3)]
  public class Manipulation : BuffAction
  {
    public override uint GetTurnsRemaining(State state)
    {
      return state.ManipulationTurns;
    }

    public override void SetTurnsRemaining(State state, uint turns)
    {
      state.ManipulationTurns = turns;
    }

    public override void TickBuff(State state)
    {
      base.TickBuff(state);

      // Only perform the actual effect if this was not the first tick.
      if (GetTurnsRemaining(state) < Attributes.BuffDuration)
        state.Durability = Math.Min(state.MaxDurability, state.Durability + 10);
    }
  }

  [SynthAction(ActionType.Buff, Name="Ingenuity", CP = 24, BuffDuration=3)]
  public class Ingenuity : BuffAction
  {
    public Ingenuity()
    {
      // Only allow ingenuity to run if we're at least 2 levels below the synth
      usageChecks.Add(delegate(State state) { return state.LevelSurplus <= -2; });
    }
    public override uint GetTurnsRemaining(State state)
    {
      return state.IngenuityTurns;
    }

    public override void SetTurnsRemaining(State state, uint turns)
    {
      state.IngenuityTurns = turns;
    }
  }

  [SynthAction(ActionType.Buff, Name = "Great Strides", CP = 32, BuffDuration = 3)]
  public class GreatStrides : BuffAction
  {
    public GreatStrides()
    {
      usageChecks.Add(delegate(State state) { return state.Quality <= state.MaxQuality; });
    }

    public override uint GetTurnsRemaining(State state)
    {
      return state.GreatStridesTurns;
    }

    public override void SetTurnsRemaining(State state, uint turns)
    {
      state.GreatStridesTurns = turns;
    }
    }
}
