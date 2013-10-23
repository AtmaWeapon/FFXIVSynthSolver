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

  public abstract class Action
  {
    private uint cp;
    private string name;

    public Action()
    {
      object[] objAttributes = GetType().GetCustomAttributes(typeof(SynthActionAttribute), false);
      Debug.Assert(objAttributes.Length == 1);
      SynthActionAttribute attributes = (SynthActionAttribute)objAttributes[0];

      cp = attributes.CP;
      name = attributes.Name;
    }

    public string Name { get { return name; } }
    public uint RequiredCP { get { return cp; } }

    public abstract bool CanFail { get; }

    public State ApplyAction(State oldState, bool success)
    {
      State newState = new State(oldState, this);

      ApplyAction(oldState, newState, success);
      return newState;
    }

    protected virtual void ApplyAction(State oldState, State newState, bool success)
    {
      Debug.Assert(newState.CP >= RequiredCP);
      newState.CP -= RequiredCP;
    }

    public virtual bool CanUse(State state)
    {
      return (state.Status == SynthesisStatus.IN_PROGRESS && state.CP >= RequiredCP);
    }
  }

}
