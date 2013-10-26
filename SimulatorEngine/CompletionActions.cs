using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Engine
{
  public class CompletionAction : Ability
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

    public uint BaseEfficiency { get { return efficiency; } }

    public CompletionFlags CompletionFlags { get { return completionFlags; } }

    public override uint BaseSuccessRate { get { return successRate; } }

    protected void RemoveDurability(State newState)
    {
      uint durabilityLoss = Compute.DurabilityLoss(BaseDurability, newState);
      newState.Durability = (newState.Durability < durabilityLoss)
          ? 0
          : newState.Durability - durabilityLoss;
    }

    public override bool CanFail
    {
      get 
      {
        return BaseSuccessRate < 100;
      }
    }

    public override bool CanUse(State state)
    {
      if (!base.CanUse(state))
        return false;

      if ((completionFlags & CompletionFlags.Quality) != 0 && state.Quality < state.MaxQuality)
        return true;
      if ((completionFlags & CompletionFlags.Progress) != 0 && state.Progress < state.MaxProgress)
        return true;
      return false;
    }

    protected override void ActivateInternal(State oldState, State newState, bool success)
    {
      base.ActivateInternal(oldState, newState, success);

      RemoveDurability(newState);

      if (success)
      {
        if ((completionFlags & CompletionFlags.Progress) != 0)
        {
          uint delta = Compute.Progress(oldState, BaseEfficiency);
          newState.Progress = Math.Min(newState.Progress + delta, newState.MaxProgress);
        }
        if ((completionFlags & CompletionFlags.Quality) != 0)
        {
          uint effic = BaseEfficiency;
          if (((completionFlags & CompletionFlags.TouchAction) != 0) && GreatStrides.IsActive(newState))
          {
            effic *= 2;
            GreatStrides.SetTurnsRemaining(newState, 1);
          }
          uint delta = Compute.Quality(oldState, effic);
          newState.Quality = Math.Min(newState.Quality + (uint)delta, newState.MaxQuality);
          if (newState.InnerQuietIsActive)
            ++newState.InnerQuietStacks;
        }
      }
    }
  }

  [SynthAction(ActionType.Completion, AbilityId.BasicSynthesis, "Basic Synthesis", 0)]
  [CompletionAction(CompletionFlags.Progress, 100, 90)]
  public class BasicSynthesis : CompletionAction
  {
  }

  [SynthAction(ActionType.Completion, AbilityId.RapidSynthesis, "Rapid Synthesis", 0)]
  [CompletionAction(CompletionFlags.Progress, 250, 50)]
  public class RapidSynthesis : CompletionAction
  {
  }

  [SynthAction(ActionType.Completion, AbilityId.BasicTouch, "Basic Touch", 18)]
  [CompletionAction(CompletionFlags.Quality | CompletionFlags.TouchAction, 100, 70)]
  public class BasicTouch : CompletionAction
  {
  }

  [SynthAction(ActionType.Completion, AbilityId.HastyTouch, "Hasty Touch", 0)]
  [CompletionAction(CompletionFlags.Quality | CompletionFlags.TouchAction, 100, 50)]
  public class HastyTouch : CompletionAction
  {
  }

  [SynthAction(ActionType.Completion, AbilityId.StandardTouch, "Standard Touch", 32)]
  [CompletionAction(CompletionFlags.Quality | CompletionFlags.TouchAction, 125, 80)]
  public class StandardTouch : CompletionAction
  {
  }

}
