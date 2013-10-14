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
    static uint[] factorials;
    static uint[,] bincoeff;
    static uint kMaxFactorial = 13;

    static Compute()
    {
      factorials = new uint[kMaxFactorial];
      factorials[0] = 1U;
      factorials[1] = 1U;
      factorials[2] = 2U;
      factorials[3] = 6U;
      factorials[4] = 24U;
      factorials[5] = 120U;
      factorials[6] = 720U;
      factorials[7] = 5040U;
      factorials[8] = 40320U;
      factorials[9] = 362880U;
      factorials[10] = 3628800U;
      factorials[11] = 39916800U;
      factorials[12] = 479001600U;

      bincoeff = new uint[kMaxFactorial, kMaxFactorial];
      for (uint i = 0; i < kMaxFactorial; ++i)
      {
        for (uint j = 0; j < kMaxFactorial; ++j)
          bincoeff[i, j] = Binomial(i, j);
      }
    }

    public static uint DurabilityLoss(uint baseDurability, State state)
    {
      return baseDurability;
    }

    // Computes m choose n
    private static uint SlowBinomial(uint m, uint n)
    {
      if (m < n)
        return SlowBinomial(n, m);
      return (factorials[m] / factorials[n]) / factorials[m - n];
    }

    public static uint Binomial(uint m, uint n)
    {
      return bincoeff[m, n];
    }

    public static double Probability(uint RequiredEvents, uint TotalEvents, double p)
    {
      if (RequiredEvents == 0)
        return 1.0;

      double result = 0.0f;
      while (RequiredEvents <= TotalEvents)
      {
        uint combinations = Binomial(TotalEvents, RequiredEvents);
        result += (double)combinations * Math.Pow(p, RequiredEvents) * Math.Pow(1.0 - p, TotalEvents - RequiredEvents);
        ++RequiredEvents;
      }
      return result;
    }

    public static double StateScore(State state)
    {
      if (state.Status == SynthesisStatus.BUSTED)
        return 0.0f;

      double failureProbability = 0.0;
      uint ProgressRemaining = state.MaxProgress - state.Progress;
      uint BasicProgressPerTurn = Compute.Progress(state, 100);
      uint successfulTurnsNeeded = (uint)Math.Ceiling((double)ProgressRemaining / (double)BasicProgressPerTurn);
      uint turnsRemaining = (uint)Math.Ceiling((double)state.Durability / 10.0);
      double successProb = Probability(successfulTurnsNeeded, turnsRemaining, 0.9);
      failureProbability = 1.0 - successProb;

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

      //double qfactor = (state.Progress == 0) ? 1.0 : (1.0 + (q * pmax) / (p*qmax));
      //double result = (1.0 + failureProbability*ppct) * qfactor * Math.Exp(cpct) * Math.Exp(dpct);
      // By using (1.0+failureProbability)*ppct, progress is considered more valuable as we get closer to failing.
      double result = (1.0 + failureProbability * ppct) * (1.0 + cpct) * (1.0 + dpct) * (1.0 + qpct);
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
