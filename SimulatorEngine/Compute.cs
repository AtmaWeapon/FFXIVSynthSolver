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
    public static uint DurabilityLoss(uint baseDurability, State state)
    {
      return baseDurability;
    }

    public static double StateScore(State state)
    {
      if (state.Status == SynthesisStatus.BUSTED)
        return 0.0f;

      double q = (double)state.Quality;
      double qmax = (double)state.MaxQuality;
      double p = (double)state.Progress;
      double pmax = (double)state.MaxProgress;
      double d = (double)state.Durability;
      double dmax = (double)state.MaxDurability;
      double c = (double)state.CP;
      double cmax = (double)state.MaxCP;

      double cpct = c / cmax;
      double dpct = d / dmax;
      double ppct = p / pmax;
      double qpct = q / qmax;

      double result = (1.0 + ppct) * (1.0 + q * pmax / p) * Math.Exp(cpct) * Math.Exp(dpct);
      return result;
    }

    public static uint RawProgress(uint craftsmanship, uint efficiency, int levelSurplus)
    {
      double levelFactor = 1.0;
      if (levelSurplus > 0)
        levelFactor += (double)Math.Min(5, levelSurplus) * 0.05;
      else if (levelSurplus < 0)
        levelFactor += (double)Math.Max(-5, levelSurplus) * 0.1;

      double craftsmanshipFactor = (0.21 * craftsmanship + 1.6);
      double result = ((double)efficiency / 100.0) * levelFactor * craftsmanshipFactor;
      uint progress = (uint)Math.Round(result);
      return progress;
    }

    public static uint Progress(State state, uint efficiency)
    {
      return RawProgress(state.Craftsmanship, efficiency, state.LevelSurplus);
    }

    public static uint Quality(State state, uint efficiency)
    {
      return RawQuality(state.Condition, state.Control, efficiency, state.LevelSurplus);
    }

    public static uint RawQuality(Condition condition, uint control, uint efficiency, int levelSurplus)
    {
      double multiplier = 1.0;
      if (condition == Condition.Poor)
        multiplier = 0.5;
      else if (condition == Condition.Good)
        multiplier = 1.5;
      else if (condition == Condition.Excellent)
        multiplier = 4.0;

      // Level difference is only used if it's negative (i.e. if crafter level is below the synth level)
      double levelFactor = 1.0 + 0.05 * (double)Math.Min(0, levelSurplus);
      double controlFactor = 0.36 * (double)control + 34.0;
      double result = ((double)efficiency / 100.0) * multiplier * Math.Round(levelFactor * controlFactor);
      return (uint)result;
    }

    public static uint CP(uint baseCP, State state)
    {
      return baseCP;
    }

    public static double SuccessRate(uint baseSuccessRate, State state)
    {
      return Math.Min((double)baseSuccessRate/100.0 + state.SuccessBonus, 1.0);
    }
  }
}
