using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Engine
{
  public abstract class OneTimeEnhancementAction : Ability
  {
    public override bool CanFail { get { return false; } }
  }

  [SynthAction(ActionType.OneTimeEnhancement, AbilityId.MastersMend, "Master's Mend", 92)]
  [OneTimeEnhancement]
  public class MastersMend : OneTimeEnhancementAction
  {
    public override bool CanUse(State state)
    {
      if (!base.CanUse(state))
        return false;

      // Don't allow Master's Mend to be used if it won't be fully effective.
      return (state.MaxDurability - state.Durability >= 30);
    }

    protected override void ActivateInternal(State oldState, State newState, bool success)
    {
      newState.Durability = Math.Min(newState.Durability + 30, newState.MaxDurability);
    }
  }

  [SynthAction(ActionType.OneTimeEnhancement, AbilityId.Observe, "Observe", 14)]
  [OneTimeEnhancement]
  public class Observe : OneTimeEnhancementAction
  {
    public override bool CanUse(State state)
    {
      if (!base.CanUse(state))
        return false;

      // No point using observe if the condition isn't poor.
      if (state.Condition != Condition.Poor)
        return false;

      return true;
    }
  }

  [SynthAction(ActionType.OneTimeEnhancement, AbilityId.TricksOfTheTrade, "Tricks of the Trade", 0)]
  [OneTimeEnhancement]
  public class TricksOfTheTrade : OneTimeEnhancementAction
  {
    public override bool CanUse(State state)
    {
      if (!base.CanUse(state))
        return false;

      if (state.Condition != Condition.Good)
        return false;

      //Don't use Tricks of the Trade unless we're desparate for CP.
      if (state.CP >= 20)
        return false;

      return true;
    }
  }
}
