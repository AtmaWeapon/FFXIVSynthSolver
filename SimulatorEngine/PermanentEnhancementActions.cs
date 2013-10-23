using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Engine
{
  public class PermanentEnhancementAction : Action
  {
    public override bool CanFail
    {
      get { return false; }
    }
  }

  [SynthAction(ActionType.PermanentEnhancement, ActionId.InnerQuiet, "Inner Quiet", 18)]
  [PermanentEnhancement]
  public class InnerQuiet : PermanentEnhancementAction
  {
  }
}
