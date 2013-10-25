using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Engine
{
  public enum Condition : uint
  {
    // We use 0 for normal so that it corresponds to the default value of an
    // uninitialized integer.
    Normal = 0,
    Poor = 1,
    Good = 2,
    Excellent = 3,
  }

  public delegate void StateOperator(State originalState, State newState);
  public delegate bool UsageCheck(State state);

  public abstract class Ability
  {
    private uint cp;
    private string name;
    private AbilityId id;
    private ActionType type;

    public Ability()
    {
      object[] objAttributes = GetType().GetCustomAttributes(typeof(SynthActionAttribute), false);
      Debug.Assert(objAttributes.Length == 1);
      SynthActionAttribute attributes = (SynthActionAttribute)objAttributes[0];

      cp = attributes.CP;
      name = attributes.Name;
      id = attributes.ActionId;
      type = attributes.ActionType;
    }

    public string Name { get { return name; } }
    public uint RequiredCP { get { return cp; } }
    public AbilityId AbilityId { get { return id; } }
    public ActionType ActionType { get { return type; } }

    public abstract bool CanFail { get; }

    public State Activate(State oldState, bool success)
    {
      if (!CanFail && !success)
        return null;

      State newState = new State(oldState, this);

      ActivateInternal(oldState, newState, success);

      if (newState.Status == SynthesisStatus.IN_PROGRESS)
      {
        // Don't use a foreach here, because if the effect wears off, the TickEnhancement
        // function can call back in and remove itself from the list of active
        // enhancements.
        int i = 0;
        while (i < newState.tempEffects.Count)
        {
          TemporaryEnhancementAbility effect = newState.tempEffects[i];
          effect.TickEnhancement(newState);
          if (effect.GetTurnsRemaining(newState) > 0)
            ++i;
        }
      }

      return newState;
    }

    public virtual bool CanUse(State state)
    {
      return (state.Status == SynthesisStatus.IN_PROGRESS && state.CP >= RequiredCP);
    }

    public virtual uint BaseSuccessRate { get { return 100; } }

    protected virtual void ActivateInternal(State oldState, State newState, bool success)
    {
      Debug.Assert(newState.CP >= RequiredCP);
      newState.CP -= RequiredCP;
    }
  }

}
