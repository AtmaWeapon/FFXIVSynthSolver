using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Engine
{
  public enum Condition
  {
    Poor,
    Normal,
    Good,
    Excellent,
  }

  public delegate void StateOperator(State originalState, State newState);
  public delegate bool UsageCheck(State state);

  public class Action
  {
    // We apply state modifications and usage validity checks as ordered
    // delegates in a list, because it makes it easier to manage and manipulate
    // them as buffs are applied and removed dynamically.
    protected List<StateOperator> successOperators;
    protected List<StateOperator> failureOperators;

    protected List<UsageCheck> usageChecks;

    private SynthActionAttribute attributes;

    public Action()
    {
      object[] objAttributes = GetType().GetCustomAttributes(typeof(SynthActionAttribute), false);
      Debug.Assert(objAttributes.Length == 1);
      attributes = (SynthActionAttribute)objAttributes[0];

      successOperators = new List<StateOperator>();
      failureOperators = new List<StateOperator>();
      usageChecks = new List<UsageCheck>();

      successOperators.Add(DeductBasicCosts);
      failureOperators.Add(DeductBasicCosts);

      usageChecks.Add(delegate(State s) { return s.Status == SynthesisStatus.IN_PROGRESS && s.CP >= Attributes.CP; });
    }

    public SynthActionAttribute Attributes
    {
      get
      {
        return attributes;
      }
    }

    public bool CanFail { get { return Attributes.SuccessRate < 100; } }

    public State FailureState(State oldState)
    {
      if (!CanFail)
        return null;

      State newState = new State(oldState, this);
      foreach (StateOperator op in failureOperators)
        op(oldState, newState);

      // This is kind of hackish, but with buffs like Manipulation that restore durability, the durability is still
      // checked before the buff is applied.  So if you have 10 durability with Manipulation up, and go down to 0
      // then the synth will fail, even though Manipulation would restore that 10 durability.  So only tick the buffs
      // if the synth is still in an OK state.
      if (newState.Status == SynthesisStatus.IN_PROGRESS)
        newState.TickBuffs();

      return newState;
    }

    public State SuccessState(State oldState)
    {
      State newState = new State(oldState, this);

      foreach (StateOperator op in successOperators)
        op(oldState, newState);

      // This is kind of hackish, but with buffs like Manipulation that restore durability, the durability is still
      // checked before the buff is applied.  So if you have 10 durability with Manipulation up, and go down to 0
      // then the synth will fail, even though Manipulation would restore that 10 durability.  So only tick the buffs
      // if the synth is still in an OK state.
      if (newState.Status == SynthesisStatus.IN_PROGRESS)
        newState.TickBuffs();
      return newState;
    }

    public bool CanUse(State state)
    {
      foreach (UsageCheck check in usageChecks)
      {
        if (!check(state))
          return false;
      }
      return true;
    }

    private void DeductBasicCosts(State originalState, State newState)
    {
      Debug.Assert(originalState.CP >= Attributes.CP);
      Debug.Assert(originalState.Durability >= 0);

      // Deduct CP
      newState.CP = originalState.CP - Attributes.CP;

      // Deduct Durability
      newState.Durability = Math.Max(originalState.Durability - Attributes.Durability, 0);
    }
  }

}
