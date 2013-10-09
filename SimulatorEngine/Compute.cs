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

    public static int RawProgress(int craftsmanship, int efficiency, int levelSurplus)
    {
      double levelFactor = 1.0f;
      if (levelSurplus > 0)
        levelFactor += (double)Math.Min(5, levelSurplus) * 0.05;
      else if (levelSurplus < 0)
        levelFactor += (double)Math.Max(-5, levelSurplus) * 0.1;

      double craftsmanshipFactor = (0.21 * craftsmanship + 1.6);
      double result = ((double)efficiency / 100.0) * levelFactor * craftsmanshipFactor;
      int progress = (int)Math.Round(result);
      return progress;
    }

    public static int Progress(State state, int efficiency)
    {
      return RawProgress(state.Craftsmanship, efficiency, state.LevelSurplus);
    }

    public static int Quality(State state, int efficiency)
    {
      return RawQuality(state.Condition, state.Control, efficiency, state.LevelSurplus);
    }

    public static int RawQuality(Condition condition, int control, int efficiency, int levelSurplus)
    {
      float multiplier = 1.0f;
      if (condition == Condition.Poor)
        multiplier = 0.5f;
      else if (condition == Condition.Good)
        multiplier = 1.5f;
      else if (condition == Condition.Excellent)
        multiplier = 4.0f;

      // Level difference is only used if it's negative (i.e. if crafter level is below the synth level)
      float levelFactor = 1.0f + 0.05f * (float)Math.Min(0, levelSurplus);
      float controlFactor = 0.36f * (float)control + 34.0f;
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
