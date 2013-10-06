using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Engine
{
  public abstract class ProgressAction : Action
  {
    public ProgressAction()
    {
      successOperators.Add(Advance);
    }
    protected virtual void Advance(State originalState, State newState)
    {
      float delta = Compute.Progress(originalState, Attributes.Efficiency);
      newState.Progress = Math.Min(newState.Progress + (int)delta, newState.MaxProgress);
    }
  }

  [SynthAction(ActionType.Progress, Name="Basic Synthesis", Efficiency=100, SuccessRate=90)]
  public class BasicSynthesis : ProgressAction
  {
  }

  [SynthAction(ActionType.Progress, Name="Rapid Synthesis", Efficiency=250, SuccessRate=50)]
  public class RapidSynthesis : ProgressAction
  {
  }
}
