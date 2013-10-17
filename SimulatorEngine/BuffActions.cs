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

    public uint Duration
    {
      get
      {
        return Attributes.BuffDuration;
      }
    }

    public abstract bool IsBuffActive(State state);

    // Called when the Step advances while the buff is active.
    public abstract void TickBuff(State state);

    // Called when a buff is applied for the first time.
    public abstract void ApplyBuff(State state);
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

    public override bool IsBuffActive(State state) { return false; }
    public override void TickBuff(State state) { }
  }

  [SynthAction(ActionType.Buff, Name = "Steady Hand", CP = 22, BuffDuration=5)]
  public class SteadyHand : BuffAction
  {
    public SteadyHand()
    {
      // Don't allow Steady hand with < 50 durability (since it would waste one of its procs on
      // restoring durability) unless we're in a bind and we need progress quickly.
      usageChecks.Add(delegate(State state)
                      {
                        if (state.Durability >= 50)
                          return true;
                        // If we have enough CP to restore durability, it's not urgent so don't
                        // allow Steady hand to be used.
                        if (state.CP >= Compute.CP(92, state))
                          return false;

                        // Otherwise either we must use SH for safety reasons, or there's no additional harm
                        // in doing so, so allow it to be used.
                        return true;
                      });
    }

    public override void ApplyBuff(State state)
    {
      state.SteadyHandTurns = Attributes.BuffDuration + 1;
    }

    public override void TickBuff(State state)
    {
      if (state.SteadyHandTurns == 0)
        return;

      --state.SteadyHandTurns;
    }

    public override bool IsBuffActive(State state)
    {
      return state.SteadyHandTurns > 0;
    }
  }

  [SynthAction(ActionType.Buff, Name = "Observe", CP = 14, BuffDuration=0, Disabled=true)]
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

    public override bool IsBuffActive(State state) { return false; }
    public override void ApplyBuff(State state) { }
    public override void TickBuff(State state) { }
  }

  [SynthAction(ActionType.Buff, Name = "Tricks of the Trade", CP = 0, BuffDuration=0, Disabled=true)]
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

    public override bool IsBuffActive(State state) { return false; }
    public override void TickBuff(State state) { }
  }

  // TODO
  [SynthAction(ActionType.Buff, Name = "Inner Quiet", CP = 18, Disabled = true, BuffDuration=0)]
  public class InnerQuiet : BuffAction
  {
    public override bool IsBuffActive(State state) { return false; }
    public override void ApplyBuff(State state) { }
    public override void TickBuff(State state) { }
  }

  // TODO
  [SynthAction(ActionType.Buff, Name = "Manipulation", CP = 88, BuffDuration = 3)]
  public class Manipulation : BuffAction
  {
    public override bool IsBuffActive(State state) 
    {
      return state.ManipulationTurns > 0;
    }

    public override void ApplyBuff(State state)
    {
      // Assign it 1 more turn than it really has, so it can tick right after it's been applied.
      state.ManipulationTurns = Attributes.BuffDuration + 1;
    }

    public override void TickBuff(State state)
    {
      if (state.ManipulationTurns == 0)
        return;

      // Only perform the actual effect if this was not the first tick.
      if (--state.ManipulationTurns < Attributes.BuffDuration)
        state.Durability = Math.Min(state.MaxDurability, state.Durability + 10);
    }
  }

  [SynthAction(ActionType.Buff, Name="Ingenuity", CP = 24, BuffDuration=3, Disabled=true)]
  public class Ingenuity : BuffAction
  {
    public Ingenuity()
    {
      // Only allow ingenuity to run if we're at least 2 levels below the synth
      usageChecks.Add(delegate(State state) { return state.LevelSurplus <= -2; });
    }
    public override bool IsBuffActive(State state) 
    { 
      return state.IngenuityTurns > 0;
    }

    public override void ApplyBuff(State state)
    {
      state.IngenuityTurns = Attributes.BuffDuration;
    }

    public override void TickBuff(State state)
    {
      if (state.IngenuityTurns == 0)
        return;

      --state.IngenuityTurns;
    }
  }

  [SynthAction(ActionType.Buff, Name = "Great Strides", CP = 32, BuffDuration = 3, Disabled=true)]
  public class GreatStrides : BuffAction
  {
    public GreatStrides()
    {
    }
    public override bool IsBuffActive(State state)
    {
      return state.GreatStridesTurns > 0;
    }

    public override void ApplyBuff(State state)
    {
      state.GreatStridesTurns = Attributes.BuffDuration;
    }

    public override void TickBuff(State state)
    {
      if (state.GreatStridesTurns == 0)
        return;

      --state.GreatStridesTurns;
    }
  }
}
