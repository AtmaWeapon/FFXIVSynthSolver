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

  public abstract class TouchAction : QualityAction
  {
    protected override void Advance(State originalState, State newState)
    {
      uint efficiency = Attributes.Efficiency;
      if (originalState.GreatStridesTurns > 0)
      {
        // If Great Strides was active, after doubling the efficiency, set its number
        // of turns remaining to 1 so that when it ticks at the end of this turn it
        // will wear off.
        efficiency *= 2;
        newState.GreatStridesTurns = 1;
      }
      float delta = Compute.Quality(originalState, efficiency);
      newState.Quality = Math.Min(newState.Quality + (uint)delta, newState.MaxQuality);
    }
  }

  [SynthAction(ActionType.Quality, Name="Basic Touch", Efficiency=100, SuccessRate=70, CP=18)]
  public class BasicTouch : TouchAction
  {
  }

  [SynthAction(ActionType.Quality, Name = "Hasty Touch", Efficiency = 100, SuccessRate = 50, CP = 0)]
  public class HastyTouch : TouchAction
  {
  }

  [SynthAction(ActionType.Quality, Name = "Standard Touch", Efficiency = 125, SuccessRate = 80, CP = 32)]
  public class StandardTouch : TouchAction
  {
  }
}
