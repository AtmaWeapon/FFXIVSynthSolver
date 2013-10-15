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
    static uint kMaxFactorial = 13;

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
      SetBinomials(12, new int[] { 1, 12, 66, 220, 495, 792, 924, 792, 495, 220, 66, 12, 1  });
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
      while (RequiredEvents <= TotalEvents)
      {
        uint combinations = Binomial(TotalEvents, RequiredEvents);
        result += (double)combinations * Math.Pow(p, RequiredEvents) * Math.Pow(1.0 - p, TotalEvents - RequiredEvents);
        ++RequiredEvents;
      }
      return result;
    }

    public static double FailureProbability(State state) 
    {
      uint progressRemaining = state.MaxProgress - state.Progress;
      uint basicProgressPerTurn = Compute.Progress(state, 100);
      uint successfulTurnsNeeded = (uint)Math.Ceiling((double)progressRemaining / (double)basicProgressPerTurn);
      uint turnsRemaining = (uint)Math.Ceiling((double)state.Durability / 10.0);
      double successProb = Probability(successfulTurnsNeeded, turnsRemaining, 0.9);
      return 1.0 - successProb;
    }

    public static double SuccessProbability(State state) 
    {
      return 1.0 - FailureProbability(state);
    }

    public static double StateScore(State state)
    {
      if (state.Status == SynthesisStatus.BUSTED)
        return 0.0f;

      double psucc = SuccessProbability(state);
      uint turnsRemaining = (uint)Math.Ceiling((double)state.Durability / 10.0);

      double q = (double)state.Quality;
      double qmax = (double)state.MaxQuality;
      double c = (double)state.CP;
      double cmax = (double)state.MaxCP;

      double cpct = c / cmax;
      double qpct = q / qmax;

      double result = psucc * (1.0 + Math.Log(turnsRemaining) + cpct + qpct);
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
