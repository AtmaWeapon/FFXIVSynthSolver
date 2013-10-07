using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Engine
{
  public class ActiveBuff
  {
    private BuffAction buff;
    private int turnsRemaining;
    private bool isNewBuff;

    public BuffAction Buff 
    { 
      get { return buff; }
      set { buff = value; }
    }

    public int TurnsRemaining
    {
      get { return turnsRemaining; }
      set { turnsRemaining = value; }
    }

    public bool IsNewBuff
    {
      get { return isNewBuff; }
      set { isNewBuff = value; }
    }

    public void Tick(State state)
    {
      Debug.Assert(!IsNewBuff);
      Debug.Assert(turnsRemaining > 0);
      Debug.Assert(buff != null);
      buff.TickBuff(state);
      --turnsRemaining;
    }

    public void Apply(State state)
    {
      Debug.Assert(turnsRemaining == 0);
      turnsRemaining = buff.Duration;
      buff.ApplyBuff(state);
    }

    public void Remove(State state)
    {
      Debug.Assert(turnsRemaining == 0);
      buff.RemoveBuff(state);
    }

    public override bool Equals(object obj)
    {
      ActiveBuff other = obj as ActiveBuff;
      if (other == null)
        return false;

      return object.ReferenceEquals(buff, other.buff) && (TurnsRemaining == other.TurnsRemaining);
    }
  }

  public class BuffAction : Action
  {
    public BuffAction()
    {
      usageChecks.Add(delegate(State state)
                      {
                        // Don't allow this buff to be used if this buff is already active.
                        return !state.IsBuffActive(this);
                      });
      successOperators.Add(delegate(State originalState, State newState) 
                           {
                             if (Attributes.BuffDuration > 0)
                             {
                               ActiveBuff activeBuff = new ActiveBuff();
                               activeBuff.Buff = this;
                               activeBuff.IsNewBuff = true;
                               activeBuff.Apply(newState);
                               newState.ActiveBuffs.Add(activeBuff);
                             }
                             else
                               ApplyBuff(newState);
                           });
    }

    public int Duration
    {
      get
      {
        return Attributes.BuffDuration;
      }
    }

    // Called when the Step advances while the buff is active.
    public virtual void TickBuff(State state)
    {
    }

    // Called when a buff is applied for the first time.
    public virtual void ApplyBuff(State state)
    {
    }

    // Called when a buff wears off.
    public virtual void RemoveBuff(State state)
    {
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
      state.SuccessBonus += 0.2f;
    }

    public override void RemoveBuff(State state)
    {
      state.SuccessBonus -= 0.2f;
    }
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
                        if (s.TransitionSequence.Count == 0)
                          return false;
                        // Don't run Observe more than once in a row.
                        State.Transition last = s.TransitionSequence.ElementAt(s.TransitionSequence.Count - 1);
                        return !(last.action is Observe);
                      });
    }
  }

  [SynthAction(ActionType.Buff, Name = "Tricks of the Trade", CP = 0, BuffDuration=0)]
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
  }

  // TODO
  [SynthAction(ActionType.Buff, Name = "Inner Quiet", CP = 18, Disabled = true, BuffDuration=0)]
  public class InnerQuiet : BuffAction
  {
  }

  // TODO
  [SynthAction(ActionType.Buff, Name = "Manipulation", CP = 88, BuffDuration = 3)]
  public class Manipulation : BuffAction
  {
    public override void ApplyBuff(State state)
    {
    }

    public override void TickBuff(State state)
    {
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

    public override void ApplyBuff(State state)
    {
      state.RecipeLevelReductionBonus = state.SynthLevel - state.CrafterLevel;
    }

    public override void RemoveBuff(State state)
    {
      state.RecipeLevelReductionBonus = 0;
    }
  }

  [SynthAction(ActionType.Buff, Name = "Great Strides", CP = 32, BuffDuration = 3)]
  public class GreatStrides : BuffAction
  {
    public GreatStrides()
    {
    }
  }
}
