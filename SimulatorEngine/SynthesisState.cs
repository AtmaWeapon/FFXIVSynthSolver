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

  public struct StateDetails2
  {
    public ulong w1;
    public ulong w2;

    public ulong RawWord1 { get { return w1; } }
    public ulong RawWord2 { get { return w2; } }

    /* 
       | max progress (7 bits) | progress (7 bits) | max durability (7 bits) | durability (7 bits) | maxcp (9 bits) | cp (9 bits) | control (9 bits) | craftsmanship (9 bits) |
     */

    public static readonly int kCraftsmanshipOffset = 0;
    public static readonly int kCraftsmanshipLength = 9;
    public static readonly int kControlOffset = kCraftsmanshipOffset + kCraftsmanshipLength;
    public static readonly int kControlLength = 9;
    public static readonly int kCPOffset = kControlOffset + kControlLength;
    public static readonly int kCPLength = 9;
    public static readonly int kMaxCPOffset = kCPOffset + kCPLength;
    public static readonly int kMaxCPLength = 9;
    public static readonly int kDurabilityOffset = kMaxCPOffset + kMaxCPLength;
    public static readonly int kDurabilityLength = 7;
    public static readonly int kMaxDurabilityOffset = kDurabilityOffset + kDurabilityLength;
    public static readonly int kMaxDurabilityLength = 7;
    public static readonly int kProgressOffset = kMaxDurabilityOffset + kMaxDurabilityLength;
    public static readonly int kProgressLength = 7;
    public static readonly int kMaxProgressOffset = kProgressOffset + kProgressLength;
    public static readonly int kMaxProgressLength = 7;

    /* 
       | steady hand (3 bits) | ingenuity (2 bits) | great strides (2 bits) | manipulation (2 bits) | condition (2 bits) | crafter level (6 bits) | synth level (6 bits) |
     */
    public static readonly int kSynthLevelOffset = 0;
    public static readonly int kSynthLevelLength = 6;
    public static readonly int kCrafterLevelOffset = kSynthLevelOffset + kSynthLevelLength;
    public static readonly int kCrafterLevelLength = 6;
    public static readonly int kConditionOffset = kCrafterLevelOffset + kCrafterLevelLength;
    public static readonly int kConditionLength = 2;
    public static readonly int kManipulationOffset = kConditionOffset + kConditionLength;
    public static readonly int kManipulationLength = 2;
    public static readonly int kGreatStridesOffset = kManipulationOffset + kManipulationLength;
    public static readonly int kGreatStridesLength = 2;
    public static readonly int kIngenuityOffset = kGreatStridesOffset + kGreatStridesLength;
    public static readonly int kIngenuityLength = 2;
    public static readonly int kSteadyHandOffset = kIngenuityOffset + kIngenuityLength;
    public static readonly int kSteadyHandLength = 3;

    private uint Retrieve(ulong bitfield, int bitoffset, int bitlength)
    {
      ulong mask = ((1UL << bitlength) - 1UL) << bitoffset;
      ulong result = bitfield & mask;
      return (uint)(result >> bitoffset);
    }

    private void Assign(ref ulong bitfield, int bitoffset, int bitlength, uint value)
    {
      ulong mask1 = ((1UL << bitlength) - 1UL) << bitoffset;  // 0000000000011111000000
      ulong mask2 = ~mask1;                                   // 1111111111100000111111

      // Clear the old value
      bitfield &= mask2;

      ulong assignmentMask = (ulong)value << bitoffset;
      // Or in the new value
      bitfield |= assignmentMask;
    }

    public uint Craftsmanship
    {
      get { return Retrieve(w1, kCraftsmanshipOffset, kCraftsmanshipLength); }
      set { Assign(ref w1, kCraftsmanshipOffset, kCraftsmanshipLength, value); }
    }

    public uint Control
    {
      get { return Retrieve(w1, kControlOffset, kControlLength); }
      set { Assign(ref w1, kControlOffset, kControlLength, value); }
    }

    public uint CP
    {
      get { return Retrieve(w1, kCPOffset, kCPLength); }
      set { Assign(ref w1, kCPOffset, kCPLength, value); }
    }

    public uint MaxCP
    {
      get { return Retrieve(w1, kMaxCPOffset, kMaxCPLength); }
      set { Assign(ref w1, kMaxCPOffset, kMaxCPLength, value); }
    }

    public uint Durability
    {
      get { return Retrieve(w1, kDurabilityOffset, kDurabilityLength); }
      set { Assign(ref w1, kDurabilityOffset, kDurabilityLength, value); }
    }

    public uint MaxDurability
    {
      get { return Retrieve(w1, kMaxDurabilityOffset, kMaxDurabilityLength); }
      set { Assign(ref w1, kMaxDurabilityOffset, kMaxDurabilityLength, value); }
    }

    public uint Progress
    {
      get { return Retrieve(w1, kProgressOffset, kProgressLength); }
      set { Assign(ref w1, kProgressOffset, kProgressLength, value); }
    }

    public uint MaxProgress
    {
      get { return Retrieve(w1, kMaxProgressOffset, kMaxProgressLength); }
      set { Assign(ref w1, kMaxProgressOffset, kMaxProgressLength, value); }
    }

    public uint SynthLevel
    {
      get { return Retrieve(w2, kSynthLevelOffset, kSynthLevelLength); }
      set { Assign(ref w2, kSynthLevelOffset, kSynthLevelLength, value); }
    }

    public uint CrafterLevel
    {
      get { return Retrieve(w2, kCrafterLevelOffset, kCrafterLevelLength); }
      set { Assign(ref w2, kCrafterLevelOffset, kCrafterLevelLength, value); }
    }

    public uint LevelSurplus
    {
      get { return CrafterLevel - SynthLevel; }
    }

    public Condition Condition
    {
      get { return (Condition)Retrieve(w2, kConditionOffset, kConditionLength); }
      set { Assign(ref w2, kConditionOffset, kConditionLength, (uint)value); }
    }

    public uint ManipulationTurns
    {
      get { return Retrieve(w2, kManipulationOffset, kManipulationLength); }
      set { Assign(ref w2, kManipulationOffset, kManipulationLength, value); }
    }

    public uint GreatStridesTurns
    {
      get { return Retrieve(w2, kGreatStridesOffset, kGreatStridesLength); }
      set { Assign(ref w2, kGreatStridesOffset, kGreatStridesLength, value); }
    }

    public uint IngenuityTurns
    {
      get { return Retrieve(w2, kIngenuityOffset, kIngenuityLength); }
      set { Assign(ref w2, kIngenuityOffset, kIngenuityLength, value); }
    }

    public uint SteadyHandTurns
    {
      get { return Retrieve(w2, kSteadyHandOffset, kSteadyHandLength); }
      set { Assign(ref w2, kSteadyHandOffset, kSteadyHandLength, value); }
    }

    public double SuccessBonus
    {
      get { return (SteadyHandTurns > 0) ? 0.2 : 0.0; }
    }
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

  public class State
  {
    private StateDetails state;

    private double score;
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
        // TODO: Deep clone the transition entries
        this.transitionSequence = new List<Transition>(oldState.transitionSequence);
        Transition transition = new Transition();
        transition.previousState = new State(oldState, null);
        transition.action = leadingAction;
        transition.newState = this;
        transitionSequence.Add(transition);
      }
      else
        this.transitionSequence = new List<Transition>(oldState.transitionSequence);
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

    private static readonly double kQualityWeight = 2.0;
    private static readonly double kProgressWeight = 1.5;
    private static readonly double kCPWeight = 0.4;
    private static readonly double kDurabilityWeight = 0.6;

    private static readonly double kMaxWeight = kQualityWeight + kProgressWeight + kCPWeight + kDurabilityWeight;

    public double ScoreEstimate
    {
      get
      {
        if (Status == SynthesisStatus.BUSTED)
          return 0.0f;

        double q = (double)Quality;
        double qmax = (double)MaxQuality;
        double p = (double)Progress;
        double pmax = (double)MaxProgress;
        double d = (double)Durability;
        double dmax = (double)MaxDurability;
        double c = (double)CP;
        double cmax = (double)MaxCP;

        double cpct = c/cmax;
        double dpct = d/dmax;
        double ppct = p/pmax;
        double qpct = q/qmax;

        double result = (1.0 + ppct) * (1.0 + q * pmax / p) * Math.Exp(cpct) * Math.Exp(dpct);
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
        double qualityPercent = (double)Quality / (double)MaxQuality;
        double durabilityPercent = (double)Durability / (double)MaxDurability;
        double cpPercent = (double)CP / (double)MaxCP;
        double progressPercent = (double)Progress / (double)MaxProgress;

        double result = 0.0;

        result += kQualityWeight * qualityPercent;
        result += kProgressWeight * progressPercent;
        result += kCPWeight * cpPercent;
        result += kDurabilityWeight * durabilityPercent;

        if (Condition == Engine.Condition.Poor)
          result -= 0.1;
        else if (Condition == Engine.Condition.Good)
          result += 0.1;
        else if (Condition == Engine.Condition.Excellent)
          result += 0.2;

        return (int)(100000000.0*result);
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

    public double Score
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
  }
}
