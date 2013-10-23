using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Engine
{
  class PermanentEnhancementAction : Action
  {
  }

  [SynthAction(ActionType.PermanentEnhancement, "Inner Quiet", 18)]
  [PermanentEnhancement]
  public class InnerQuiet : PermanentEnhancementAction
  {
  }
}
