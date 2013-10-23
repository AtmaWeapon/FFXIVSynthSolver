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
    static uint[,] bincoeff;
    static uint kMaxFactorial = 16;

    static Compute()
    {
      bincoeff = new uint[kMaxFactorial, kMaxFactorial];
      SetBinomials(0, new int[] { 1 });
      SetBinomials(1, new int[] { 1, 1 });
      SetBinomials(2, new int[] { 1, 2, 1 });
      SetBinomials(3, new int[] { 1, 3, 3, 1 });
      SetBinomials(4, new int[] { 1, 4, 6, 4, 1 });
      SetBinomials(5, new int[] { 1, 5, 10, 10, 5, 1 });
      SetBinomials(6, new int[] { 1, 6, 15, 20, 15, 6, 1 });
      SetBinomials(7, new int[] { 1, 7, 21, 35, 35, 21, 7, 1 });
      SetBinomials(8, new int[] { 1, 8, 28, 56, 70, 56, 28, 8, 1 });
      SetBinomials(9, new int[] { 1, 9, 36, 84, 126, 126, 84, 36, 9, 1 });
      SetBinomials(10, new int[] { 1, 10, 45, 120, 210, 252, 210, 120, 45, 10, 1 });
      SetBinomials(11, new int[] { 1, 11, 55, 165, 330, 462, 462, 330, 165, 55, 11, 1 });
      SetBinomials(12, new int[] { 1, 12, 66, 220, 495, 792, 924, 792, 495, 220, 66, 12, 1 });
      SetBinomials(13, new int[] { 1, 13, 78, 286, 715, 1287, 1716, 1716, 1287, 715, 286, 78, 13, 1 });
      SetBinomials(14, new int[] { 1, 14, 91, 364, 1001, 2002, 3003, 3432, 3003, 2002, 1001, 364, 91, 14, 1 });
      SetBinomials(15, new int[] { 1, 15, 105, 455, 1365, 3003, 5005, 6435, 6435, 5005, 3003, 1365, 455, 105, 15, 1 });
    }

    private static void SetBinomials(int i, int[] binomials)
    {
      Buffer.BlockCopy(binomials, 0, bincoeff, (int)kMaxFactorial*i*sizeof(int), binomials.Length*sizeof(int));
    }

    public static uint DurabilityLoss(uint baseDurability, State state)
    {
      return baseDurability;
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
      if (TotalEvents >= 2 * RequiredEvents)
      {
        for (uint i=0; i < RequiredEvents; ++i)
        {
          uint combinations = Binomial(TotalEvents, i);
          result += (double)combinations * Math.Pow(p, i) * Math.Pow(1.0 - p, TotalEvents - i);
        }
        result = 1.0 - result;
      }
      else
      {
        while (RequiredEvents <= TotalEvents)
        {
          uint combinations = Binomial(TotalEvents, RequiredEvents);
          result += (double)combinations * Math.Pow(p, RequiredEvents) * Math.Pow(1.0 - p, TotalEvents - RequiredEvents);
          ++RequiredEvents;
        }
      }
      return result;
    }

    public static uint NumBitsRequired(uint value)
    {
      return (uint)Math.Ceiling(Math.Log((double)value, 2.0));
    }

    public static double FailureProbability(State state) 
    {
      uint progressRemaining = state.MaxProgress - state.Progress;
      uint basicProgressPerTurn = Compute.Progress(state, 100);
      uint successfulTurnsNeeded = (uint)Math.Ceiling((double)progressRemaining / (double)basicProgressPerTurn);
      uint numMastersMendsAvailable = state.CP / 92;
      uint effectiveDurability = Math.Min(state.MaxDurability, state.Durability + numMastersMendsAvailable * 30);
      uint turnsRemaining = (uint)Math.Ceiling((double)effectiveDurability / 10.0);
      double successProb = Probability(successfulTurnsNeeded, turnsRemaining, 0.9);
      return 1.0 - successProb;
    }

    public static double SuccessProbability(State state) 
    {
      return 1.0 - FailureProbability(state);
    }

    public static double StateScore(State state)
    {
      return MetricsDatabase.ExponentialQualityPlusFailureWeightedCP3ExtraSafe(state);
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
