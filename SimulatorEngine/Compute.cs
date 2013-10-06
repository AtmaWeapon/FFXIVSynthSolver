using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Engine
{
  public static class Compute
  {
    public static int DurabilityLoss(int baseDurability, State state)
    {
      return baseDurability;
    }

    public static int Progress(State state, int efficiency)
    {
      double result = ((double)efficiency/100.0)*Math.Round((1 + 0.05f * state.LevelSurplus) * (0.21 * state.Craftsmanship + 1.6));
      return (int)result;
    }

    public static int Quality(State state, int efficiency)
    {
      float multiplier = 1.0f;
      if (state.Condition == Condition.Poor)
        multiplier = 0.5f;
      else if (state.Condition == Condition.Good)
        multiplier = 1.5f;
      else if (state.Condition == Condition.Excellent)
        multiplier = 4.0f;

      // Level difference is only used if it's negative (i.e. if crafter level is below the synth level)
      float levelFactor = 1.0f + 0.05f * (float)Math.Min(0, state.LevelSurplus);
      float controlFactor = 0.36f * (float)state.Control + 34.0f;
      double result = ((double)efficiency / 100.0) * multiplier * Math.Round(levelFactor * controlFactor);
      return (int)result;
    }

    public static int CP(int baseCP, State state)
    {
      return baseCP;
    }

    public static float SuccessRate(int baseSuccessRate, State state)
    {
      return Math.Min((float)baseSuccessRate/100.0f + state.SuccessBonus, 1.0f);
    }
  }
}
