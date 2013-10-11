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
    public static readonly int kQualityOffset = kSynthLevelOffset + kSynthLevelLength;
    public static readonly int kQualityLength = 11;
    public static readonly int kMaxQualityOffset = kQualityOffset + kQualityLength;
    public static readonly int kMaxQualityLength = 11;
    public static readonly int kCrafterLevelOffset = kMaxQualityOffset + kMaxQualityLength;
    public static readonly int kCrafterLevelLength = 6;
    public static readonly int kConditionOffset = kCrafterLevelOffset + kCrafterLevelLength;
    public static readonly int kConditionLength = 2;
    public static readonly int kManipulationOffset = kConditionOffset + kConditionLength;
    public static readonly int kManipulationLength = 3;
    public static readonly int kGreatStridesOffset = kManipulationOffset + kManipulationLength;
    public static readonly int kGreatStridesLength = 3;
    public static readonly int kIngenuityOffset = kGreatStridesOffset + kGreatStridesLength;
    public static readonly int kIngenuityLength = 3;
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
      Debug.Assert(value < (1 << bitlength));

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

    public uint Quality
    {
      get { return Retrieve(w2, kQualityOffset, kQualityLength); }
      set { Assign(ref w2, kQualityOffset, kQualityLength, value); }
    }

    public uint MaxQuality
    {
      get { return Retrieve(w2, kMaxQualityOffset, kMaxQualityLength); }
      set { Assign(ref w2, kMaxQualityOffset, kMaxQualityLength, value); }
    }

    public uint CrafterLevel
    {
      get { return Retrieve(w2, kCrafterLevelOffset, kCrafterLevelLength); }
      set { Assign(ref w2, kCrafterLevelOffset, kCrafterLevelLength, value); }
    }

    public int LevelSurplus
    {
      get 
      { 
        return (int)CrafterLevel - (int)SynthLevel; 
      }
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

    public SynthesisStatus Status
    {
      get
      {
        if (Progress < MaxProgress)
        {
          if (Durability == 0)
            return SynthesisStatus.BUSTED;
          else
            return SynthesisStatus.IN_PROGRESS;
        }
        else
          return SynthesisStatus.COMPLETED;
      }
    }
  }

  public class State
  {
    private StateDetails details;

    private State previousState;
    private Action leadingAction;
    private uint step;

    // It's a bit hackish to store these here, but actions are immutable so it's not
    // necessarily wrong to store static copies, just awkward.  It would be nice if
    // there were a generic mapping between these actions and the corresponding getters
    // setters in the StateDetails
    private static GreatStrides greatStrides;
    private static Ingenuity ingenunity;
    private static SteadyHand steadyHand;
    private static Manipulation manipulation;

    static State()
    {
      greatStrides = new GreatStrides();
      ingenunity = new Ingenuity();
      steadyHand = new SteadyHand();
      manipulation = new Manipulation();
    }
    public State()
    {
      details = new StateDetails();
      step = 1;
      previousState = null;
      leadingAction = null;
    }

    // Makes a new state which was arrived at by performing an action from a
    // previous state.
    public State(State oldState, Action leadingAction)
    {
      this.details = oldState.details;

      this.previousState = oldState;    // do we need to clone here?
      this.leadingAction = leadingAction;

      this.step = oldState.step + 1;
    }

    // Makes an exact copy of the original state
    public State(State oldState)
    {
      Debug.Assert(oldState != null);

      this.details = oldState.details;

      this.previousState = oldState.previousState;    // do we need to clone here?
      this.leadingAction = oldState.leadingAction;
      this.step = oldState.step;
    }

    public override bool Equals(object obj)
    {
      State other = obj as State;
      if (other == null)
        return false;

      if (!details.Equals(other.details))
        return false;

      // State comparison is only concerned with the state details, not the state that we
      // were in before or the action that got us here.  This is because 2 different states
      // can lead to the same state through the use of different actions.  Because of this
      // it's incorrect to compare previousState or leadingAction here.
      return true;
    }

    public override int GetHashCode()
    {
      return details.GetHashCode();
    }

    public void TickBuffs()
    {
      steadyHand.TickBuff(this);
      greatStrides.TickBuff(this);
      manipulation.TickBuff(this);
      ingenunity.TickBuff(this);
    }

    public uint Step
    {
      get { return step; }
    }
    public State PreviousState
    {
      get { return previousState; }
    }

    public Action LeadingAction
    {
      get { return leadingAction; }
    }

    public SynthesisStatus Status
    {
      get { return details.Status; }
    }
    public uint Craftsmanship
    {
      get { return details.Craftsmanship; }
      set { details.Craftsmanship = value; }
    }

    public uint Control
    {
      get { return details.Control; }
      set { details.Control = value; }
    }
    public uint CP
    {
      get { return details.CP; }
      set { details.CP = value; }
    }
    public uint MaxCP
    {
      get { return details.MaxCP; }
      set { details.MaxCP = value; }
    }
    public int LevelSurplus
    {
      get { return details.LevelSurplus; }
    }
    public uint CrafterLevel
    {
      get { return details.CrafterLevel; }
      set { details.CrafterLevel = value; }
    }
    public uint SynthLevel
    {
      get { return details.SynthLevel; }
      set { details.SynthLevel = value; }
    }
    public double SuccessBonus
    {
      get { return details.SuccessBonus; }
    }

    // The synthesis' stats in this state.
    public uint Durability
    {
      get { return details.Durability; }
      set { details.Durability = value; }
    }
    public uint MaxDurability
    {
      get { return details.MaxDurability; }
      set { details.MaxDurability = value; }
    }
    public uint Quality
    {
      get { return details.Quality; }
      set { details.Quality = value; }
    }
    public uint MaxQuality
    {
      get { return details.MaxQuality; }
      set { details.MaxQuality = value; }
    }
    public uint Progress
    {
      get { return details.Progress; }
      set { details.Progress = value; }
    }
    public uint MaxProgress
    {
      get { return details.MaxProgress; }
      set { details.MaxProgress = value; }
    }
    public Condition Condition
    {
      get { return details.Condition; }
      set { details.Condition = value; }
    }

    public uint ManipulationTurns
    {
      get { return details.ManipulationTurns; }
      set { details.ManipulationTurns = value; }
    }

    public uint GreatStridesTurns
    {
      get { return details.GreatStridesTurns; }
      set { details.GreatStridesTurns = value; }
    }

    public uint IngenuityTurns
    {
      get { return details.IngenuityTurns; }
      set { details.IngenuityTurns = value; }
    }

    public uint SteadyHandTurns
    {
      get { return details.SteadyHandTurns; }
      set { details.SteadyHandTurns = value; }
    }
    public double Score
    {
      get { return Compute.StateScore(this); }
    }
  }
}
