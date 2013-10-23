using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Engine
{
  public class CompletionAction : Action
  {
    private CompletionFlags completionFlags;
    private uint efficiency;
    private uint successRate;

    public CompletionAction()
    {
      CompletionActionAttribute attributes = SynthAction<CompletionActionAttribute>.Attributes(this);

      completionFlags = attributes.CompletionFlags;
      efficiency = attributes.Efficiency;
      successRate = attributes.SuccessRate;
    }

    public uint BaseDurability { get { return 10; } }
    public uint EffectiveDurability { get { return BaseDurability; } }

    public uint BaseEfficiency { get { return efficiency; } }
    public uint EffectiveEfficiency { get { return BaseEfficiency; } }

    public uint BaseSuccessRate { get { return successRate; } }
    public uint EffectiveSuccessRate { get { return BaseSuccessRate; } }

    protected void RemoveDurability(State newState)
    {
      newState.Durability = (newState.Durability < EffectiveDurability)
          ? 0
          : newState.Durability - EffectiveDurability;
    }

    public override bool CanFail
    {
      get 
      {
        return EffectiveSuccessRate < 100;
      }
    }

    public override bool CanUse(State state)
    {
      if ((completionFlags & CompletionFlags.Quality) != 0 && state.Quality < state.MaxQuality)
        return true;
      if ((completionFlags & CompletionFlags.Progress) != 0 && state.Progress < state.MaxProgress)
        return true;
      return false;
    }

    protected override void ApplyAction(State oldState, State newState, bool success)
    {
      base.ApplyAction(oldState, newState, success);

      RemoveDurability(newState);

      if (success)
      {
        if ((completionFlags & CompletionFlags.Progress) != 0)
        {
          uint delta = Compute.Progress(oldState, EffectiveEfficiency);
          newState.Progress = Math.Min(newState.Progress + delta, newState.MaxProgress);
        }
        if ((completionFlags & CompletionFlags.Quality) != 0)
        {
          uint effic = EffectiveEfficiency;
          if (((completionFlags & CompletionFlags.TouchAction) != 0) && newState.GreatStridesTurns > 0)
          {
            effic *= 2;
            newState.GreatStridesTurns = 1;
          }
          uint delta = Compute.Quality(oldState, effic);
          newState.Quality = Math.Min(newState.Quality + (uint)delta, newState.MaxQuality);
        }
      }
    }
  }

  [SynthAction(ActionType.Completion, "Basic Synthesis", 0)]
  [CompletionAction(CompletionFlags.Progress, 100, 90)]
  public class BasicSynthesis : CompletionAction
  {
  }

  [SynthAction(ActionType.Completion, "Rapid Synthesis", 0)]
  [CompletionAction(CompletionFlags.Progress, 250, 50)]
  public class RapidSynthesis : CompletionAction
  {
  }

  [SynthAction(ActionType.Completion, "Basic Touch", 18)]
  [CompletionAction(CompletionFlags.Quality | CompletionFlags.TouchAction, 100, 70)]
  public class BasicTouch : CompletionAction
  {
  }

  [SynthAction(ActionType.Completion, "Hasty Touch", 0)]
  [CompletionAction(CompletionFlags.Quality | CompletionFlags.TouchAction, 100, 50)]
  public class HastyTouch : CompletionAction
  {
  }

  [SynthAction(ActionType.Completion, "Standard Touch", 32)]
  [CompletionAction(CompletionFlags.Quality | CompletionFlags.TouchAction, 125, 80)]
  public class StandardTouch : CompletionAction
  {
  }

}
