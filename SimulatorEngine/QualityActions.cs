using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Engine
{
  public abstract class QualityAction : Action
  {
    public QualityAction()
    {
      successOperators.Add(Advance);
      usageChecks.Add(delegate(State s) { return s.Quality < s.MaxQuality; });
    }

    protected virtual void Advance(State originalState, State newState)
    {
      float delta = Compute.Quality(originalState, Attributes.Efficiency);
      newState.Quality = Math.Min(newState.Quality + (uint)delta, newState.MaxQuality);
    }
  }

  [SynthAction(ActionType.Quality, Name="Basic Touch", Efficiency=100, SuccessRate=70, CP=18)]
  public class BasicTouch : QualityAction
  {
  }

  [SynthAction(ActionType.Quality, Name = "Hasty Touch", Efficiency = 100, SuccessRate = 50, CP = 0)]
  public class HastyTouch : QualityAction
  {
  }

  [SynthAction(ActionType.Quality, Name = "Standard Touch", Efficiency = 125, SuccessRate = 80, CP = 32)]
  public class StandardTouch : QualityAction
  {
  }
}
