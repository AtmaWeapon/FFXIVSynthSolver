using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Engine
{

  public enum SynthesisStatus
  {
    BUSTED,
    IN_PROGRESS,
    COMPLETED
  }

  internal unsafe struct StateStorage
  {
    public fixed byte storage[16];
  }

  public class State
  {
    internal StateStorage storage;

    private State previousState;
    private Ability leadingAction;
    private uint step;
    private bool scoreComputed = false;
    private double computedScore = 0.0;

    internal List<TemporaryEnhancementAbility> tempEffects;

    public State()
    {
      storage = new StateStorage();
      tempEffects = new List<TemporaryEnhancementAbility>();
      step = 1;
      previousState = null;
      leadingAction = null;
      computedScore = 0.0;
    }

    // Makes a new state which was arrived at by performing an action from a
    // previous state.
    public State(State oldState, Ability leadingAction)
    {
      this.storage = oldState.storage;

      this.previousState = oldState;    // do we need to clone here?
      this.leadingAction = leadingAction;
      this.scoreComputed = oldState.scoreComputed;
      this.computedScore = oldState.computedScore;

      this.step = oldState.step + 1;
    }

    // Makes an exact copy of the original state
    public State(State oldState)
    {
      Debug.Assert(oldState != null);

      this.storage = oldState.storage;

      this.previousState = oldState.previousState;    // do we need to clone here?
      this.leadingAction = oldState.leadingAction;
      this.scoreComputed = oldState.scoreComputed;
      this.computedScore = oldState.computedScore;
      this.step = oldState.step;
    }

    public override bool Equals(object obj)
    {
      State other = obj as State;
      if (other == null)
        return false;

      if (!storage.Equals(other.storage))
        return false;

      // State comparison is only concerned with the state details, not the state that we
      // were in before or the action that got us here.  This is because 2 different states
      // can lead to the same state through the use of different actions.  Because of this
      // it's incorrect to compare previousState or leadingAction here.
      return true;
    }

    public override int GetHashCode()
    {
      return storage.GetHashCode();
    }

    public uint Step
    {
      get { return step; }
    }
    public State PreviousState
    {
      get { return previousState; }
    }

    public Ability LeadingAction
    {
      get { return leadingAction; }
    }

    public SynthesisStatus Status
    {
      get 
      {
        if (Progress == MaxProgress)
          return SynthesisStatus.COMPLETED;
        if (Durability == 0)
          return SynthesisStatus.BUSTED;
        return SynthesisStatus.IN_PROGRESS;
      }
    }
    public uint Craftsmanship
    {
      get 
      { 
        return StateFields.GetValue(ref storage, KnownStateField.Craftsmanship); 
      }
      set 
      {
        StateFields.SetValue(ref storage, KnownStateField.Craftsmanship, value);
        scoreComputed = false; 
      }
    }

    public uint Control
    {
      get 
      {
        return StateFields.GetValue(ref storage, KnownStateField.Control);
      }
      set 
      {
        StateFields.SetValue(ref storage, KnownStateField.Control, value);
        scoreComputed = false;
      }
    }
    public uint CP
    {
      get
      {
        return StateFields.GetValue(ref storage, KnownStateField.CP);
      }
      set
      {
        StateFields.SetValue(ref storage, KnownStateField.CP, value);
        scoreComputed = false;
      }
    }
    public uint MaxCP
    {
      get
      {
        return StateFields.GetValue(ref storage, KnownStateField.MaxCP);
      }
      set
      {
        StateFields.SetValue(ref storage, KnownStateField.MaxCP, value);
        scoreComputed = false;
      }
    }
    public int LevelSurplus
    {
      get { return (int)CrafterLevel - (int)SynthLevel; }
    }
    public uint CrafterLevel
    {
      get
      {
        return StateFields.GetValue(ref storage, KnownStateField.CrafterLevel);
      }
      set
      {
        StateFields.SetValue(ref storage, KnownStateField.CrafterLevel, value);
        scoreComputed = false;
      }
    }
    public uint SynthLevel
    {
      get
      {
        return StateFields.GetValue(ref storage, KnownStateField.SynthLevel);
      }
      set
      {
        StateFields.SetValue(ref storage, KnownStateField.SynthLevel, value);
        scoreComputed = false;
      }
    }

    // The synthesis' stats in this state.
    public uint Durability
    {
      get
      {
        return StateFields.GetValue(ref storage, KnownStateField.Durability);
      }
      set
      {
        StateFields.SetValue(ref storage, KnownStateField.Durability, value);
        scoreComputed = false;
      }
    }
    public uint MaxDurability
    {
      get
      {
        return StateFields.GetValue(ref storage, KnownStateField.MaxDurability);
      }
      set
      {
        StateFields.SetValue(ref storage, KnownStateField.MaxDurability, value);
        scoreComputed = false;
      }
    }
    public uint Quality
    {
      get
      {
        return StateFields.GetValue(ref storage, KnownStateField.Quality);
      }
      set
      {
        StateFields.SetValue(ref storage, KnownStateField.Quality, value);
        scoreComputed = false;
      }
    }
    public uint MaxQuality
    {
      get
      {
        return StateFields.GetValue(ref storage, KnownStateField.MaxQuality);
      }
      set
      {
        StateFields.SetValue(ref storage, KnownStateField.MaxQuality, value);
        scoreComputed = false;
      }
    }
    public uint Progress
    {
      get
      {
        return StateFields.GetValue(ref storage, KnownStateField.Progress);
      }
      set
      {
        StateFields.SetValue(ref storage, KnownStateField.Progress, value);
        scoreComputed = false;
      }
    }
    public uint MaxProgress
    {
      get
      {
        return StateFields.GetValue(ref storage, KnownStateField.MaxProgress);
      }
      set
      {
        StateFields.SetValue(ref storage, KnownStateField.MaxProgress, value);
        scoreComputed = false;
      }
    }
    public Condition Condition
    {
      get
      {
        return (Condition)StateFields.GetValue(ref storage, KnownStateField.Condition);
      }
      set
      {
        StateFields.SetValue(ref storage, KnownStateField.Condition, (uint)value);
        scoreComputed = false;
      }
    }

    public double Score
    {
      get 
      {
        if (!scoreComputed)
        {
          scoreComputed = true;
          computedScore = Compute.StateScore(this);
        }
        return computedScore;
      }
    }

    public double FailureProbability 
    {
      get { return Compute.FailureProbability(this);  }
    }

    public double SuccessProbability 
    {
      get { return 1.0 - FailureProbability; }
    }
  }
}
