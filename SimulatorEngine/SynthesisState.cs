using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

  public struct StateDetails
  {
    // The user's stats in this state.
    public int craftsmanship;
    public int control;
    public int cp;
    public int maxCp;
    public int synthLevel;
    public int crafterLevel;

    public float successBonus;
    public int recipeLevelReductionBonus;

    // The synthesis' stats in this state.
    public int durability;
    public int maxDurability;
    public int quality;
    public int maxQuality;
    public int progress;
    public int maxProgress;
    public Condition condition;
  }

  public class State : ICloneable
  {
    private StateDetails state;

    private float score;
    private bool scoreComputed;

    public struct Transition
    {
      public Action action;
      public State previousState;
      public State newState;
    }
    private List<Transition> transitionSequence = new List<Transition>();
    public List<ActiveBuff> activeBuffs = new List<ActiveBuff>();

    public override bool Equals(object obj)
    {
      State other = obj as State;
      if (other == null)
        return false;

      if (!state.Equals(other.state))
        return false;

      if (activeBuffs.Count != other.activeBuffs.Count)
        return false;
      for (int i = 0; i < activeBuffs.Count; ++i)
      {
        ActiveBuff currentBuff = activeBuffs[i];
        ActiveBuff otherBuff = other.activeBuffs[i];
        if (!currentBuff.Equals(otherBuff))
          return false;
      }
      return true;
    }

    public override int GetHashCode()
    {
      return state.GetHashCode();
    }

    public State()
    {
      state = new StateDetails();
      score = 0.0f;
      scoreComputed = false;
    }

    public State(State oldState, Action leadingAction)
    {
      this.state = oldState.state;

      this.score = oldState.score;
      this.scoreComputed = oldState.scoreComputed;
      if (leadingAction != null)
      {
        this.transitionSequence = new List<Transition>(oldState.transitionSequence);
        Transition transition = new Transition();
        transition.previousState = oldState;
        transition.action = leadingAction;
        transition.newState = this;
        transitionSequence.Add(transition);
      }
      else
        this.transitionSequence = oldState.transitionSequence;
      // Make sure to do a deep copy, so that the ActiveBuff structures are cloned instead of
      // copied by reference.  Otherwise after advancing a state, ticking a buff in the new state
      // will cause it to tick in the old state as well.
      foreach (ActiveBuff oldBuff in oldState.ActiveBuffs)
      {
        ActiveBuff copy = new ActiveBuff();
        copy.Buff = oldBuff.Buff;
        copy.TurnsRemaining = oldBuff.TurnsRemaining;
        this.activeBuffs.Add(copy);
      }
    }

    private static readonly float kQualityWeight = 60.0f;
    private static readonly float kProgressWeight = 10.0f;
    private static readonly float kCPWeight = 2.0f;
    private static readonly float kDurabilityWeight = 3.0f;

    private static readonly float kMaxWeight = kQualityWeight + kProgressWeight + kCPWeight + kDurabilityWeight;

    public float ScoreEstimate
    {
      get
      {
        if (Status == SynthesisStatus.BUSTED)
          return 0.0f;

        float qualityPercent = (float)Quality / (float)MaxQuality;
        float durabilityPercent = (float)Durability / (float)MaxDurability;
        float cpPercent = (float)CP / (float)MaxCP;
        float progressPercent = (float)Progress / (float)MaxProgress;

        float result = 0.0f;

        result += kQualityWeight * qualityPercent;
        result += kProgressWeight * progressPercent;
        result += kCPWeight * cpPercent;
        result += kDurabilityWeight * durabilityPercent;

        result /= kMaxWeight;
        return result;
      }
    }

    public IList<Transition> TransitionSequence
    {
      get
      {
        return transitionSequence;
      }
    }

    public int HashCode
    {
      get
      {
        return (int)(10000000.0f*ScoreEstimate);
      }
    }

    public void TickBuffs()
    {
      // Tick all the active buffs, and remove the ones that are expired.
      for (int i = 0; i < ActiveBuffs.Count; )
      {
        ActiveBuff buff = ActiveBuffs[i];
        Debug.Assert(buff.TurnsRemaining > 0);
        // Buffs that were just added this pass, don't tick them yet.
        if (!buff.IsNewBuff)
        {
          buff.Tick(this);
          if (buff.TurnsRemaining == 0)
          {
            buff.Remove(this);
            ActiveBuffs.RemoveAt(i);
          }
          else
            ++i;
        }
        else
        {
          buff.IsNewBuff = false;
          ++i;
        }
      };
    }

    public List<ActiveBuff> ActiveBuffs
    {
      get { return activeBuffs; }
      set { activeBuffs = value; }
    }

    public bool IsBuffActive(BuffAction buff)
    {
      return ActiveBuffs.Exists(delegate(ActiveBuff abuff) { return (abuff.Buff == buff); });
    }
    
    public int Craftsmanship
    {
      get { return state.craftsmanship; }
      set { scoreComputed = false; state.craftsmanship = value; }
    }

    public int Control
    {
      get { return state.control; }
      set { scoreComputed = false; state.control = value; }
    }
    public int CP
    {
      get { return state.cp; }
      set { scoreComputed = false; state.cp = value; }
    }
    public int MaxCP
    {
      get { return state.maxCp; }
      set { scoreComputed = false; state.maxCp = value; }
    }
    public int LevelSurplus
    {
      get { return state.crafterLevel - state.synthLevel - state.recipeLevelReductionBonus; }
    }
    public int CrafterLevel
    {
      get { return state.crafterLevel; }
      set { scoreComputed = false; state.crafterLevel = value; }
    }
    public int SynthLevel
    {
      get { return state.synthLevel; }
      set { scoreComputed = false; state.synthLevel = value; }
    }

    public int RecipeLevelReductionBonus
    {
      get { return state.recipeLevelReductionBonus; }
      set { scoreComputed = false; state.recipeLevelReductionBonus = value; }
    }
    public float SuccessBonus
    {
      get { return state.successBonus; }
      set { scoreComputed = false; state.successBonus = value; }
    }

    // The synthesis' stats in this state.
    public int Durability
    {
      get { return state.durability; }
      set { scoreComputed = false; state.durability = value; }
    }
    public int MaxDurability
    {
      get { return state.maxDurability; }
      set { scoreComputed = false; state.maxDurability = value; }
    }
    public int Quality
    {
      get { return state.quality; }
      set { scoreComputed = false; state.quality = value; }
    }
    public int MaxQuality
    {
      get { return state.maxQuality; }
      set { scoreComputed = false; state.maxQuality = value; }
    }
    public int Progress
    {
      get { return state.progress; }
      set { scoreComputed = false; state.progress = value; }
    }
    public int MaxProgress
    {
      get { return state.maxProgress; }
      set { scoreComputed = false; state.maxProgress = value; }
    }
    public Condition Condition
    {
      get { return state.condition; }
      set { scoreComputed = false; state.condition = value; }
    }

    public SynthesisStatus Status
    {
      get
      {
        if (Progress < MaxProgress)
        {
          if (Durability <= 0)
            return SynthesisStatus.BUSTED;
          else
            return SynthesisStatus.IN_PROGRESS;
        }
        else
          return SynthesisStatus.COMPLETED;
      }
    }

    public float Score
    {
      get
      {
        if (!scoreComputed)
        {
          score = ScoreEstimate;
          scoreComputed = true;
        }

        return score;
      }
    }

    public object Clone()
    {
      return new State(this, null);
    }
  }
}
