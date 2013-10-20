using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Engine
{
  public static class MetricsDatabase
  {
    public static double ExponentialQualityPlusFailureWeightedCP1(State state)
    {
      if (state.Status == SynthesisStatus.BUSTED)
        return 0.0;

      double psucc = state.SuccessProbability;
      double pfail = 1.0 - psucc;

      double q = (double)state.Quality;
      double qmax = (double)state.MaxQuality;
      double c = (double)state.CP;
      double cmax = (double)state.MaxCP;

      double cpct = c / cmax;
      double qpct = q / qmax;


      return psucc * Math.Exp(0.5 + 5.0 * qpct) + pfail * cpct;
    }

    public static double ExponentialQualityPlusFailureWeightedCP2(State state)
    {
      if (state.Status == SynthesisStatus.BUSTED)
        return 0.0;

      double psucc = state.SuccessProbability;
      double pfail = 1.0 - psucc;

      double q = (double)state.Quality;
      double qmax = (double)state.MaxQuality;
      double c = (double)state.CP;
      double cmax = (double)state.MaxCP;

      double cpct = c / cmax;
      double qpct = q / qmax;


      return psucc * Math.Exp(0.5 + 2.0 * qpct) + pfail * cpct;
    }

    public static double ExponentialQualityPlusFailureWeightedCP3(State state)
    {
      if (state.Status == SynthesisStatus.BUSTED)
        return 0.0;

      double psucc = state.SuccessProbability;
      double pfail = 1.0 - psucc;

      double turnsRemaining = Math.Ceiling((double)state.Durability / 10.0);

      double q = (double)state.Quality;
      double qmax = (double)state.MaxQuality;
      double c = (double)state.CP;
      double cmax = (double)state.MaxCP;

      double cpct = c / cmax;
      double qpct = q / qmax;

      return psucc * (Math.Log(1.0+turnsRemaining) * cpct + Math.Exp(0.5 + 2.0 * qpct));
    }

    public static double ExponentialQualityPlusFailureWeightedCP3ExtraSafe(State state)
    {
      if (state.Status == SynthesisStatus.BUSTED)
        return 0.0;

      double psucc = state.SuccessProbability;
      double pfail = 1.0 - psucc;

      double turnsRemaining = Math.Ceiling((double)state.Durability / 10.0);

      double q = (double)state.Quality;
      double qmax = (double)state.MaxQuality;
      double c = (double)state.CP;
      double cmax = (double)state.MaxCP;

      double cpct = c / cmax;
      double qpct = q / qmax;

      return Math.Pow(psucc, 2.0) * (1.0 + Math.Log(1.0+turnsRemaining) * cpct + Math.Exp(0.5 + 2.0 * qpct));
    }

    public static double ExponentialQualityPlusFailureWeightedCP3WithLogFailure(State state)
    {
      if (state.Status == SynthesisStatus.BUSTED)
        return 0.0;

      double psucc = state.SuccessProbability;
      double pfail = 1.0 - psucc;

      double turnsRemaining = Math.Ceiling((double)state.Durability / 10.0);

      double q = (double)state.Quality;
      double qmax = (double)state.MaxQuality;
      double c = (double)state.CP;
      double cmax = (double)state.MaxCP;

      double cpct = c / cmax;
      double qpct = q / qmax;

      return Math.Pow(psucc, 3.0) * (1.0 + pfail*Math.Log(1.0+turnsRemaining) * cpct + Math.Exp(0.5 + 2.0 * qpct));
    }

    public static double SimpleMetric(State state)
    {
      if (state.Status == SynthesisStatus.BUSTED)
        return 0.0;

      double psucc = state.SuccessProbability;
      double pfail = 1.0 - psucc;

      double turnsRemaining = Math.Ceiling((double)state.Durability / 10.0);

      double q = (double)state.Quality;
      double qmax = (double)state.MaxQuality;
      double c = (double)state.CP;
      double cmax = (double)state.MaxCP;

      double cpct = c / cmax;
      double qpct = q / qmax;

      double BasicTouchCP = 18;
      double BasicTouchQuality = Compute.RawQuality(Condition.Normal, state.Control, 100, state.LevelSurplus);
      double dQdC = (BasicTouchQuality/qmax) / (BasicTouchCP/cmax);
      return psucc * (cpct*dQdC + qpct);
    }

    public static double ExhaustiveIdealMetric(State state)
    {
      if (state.Status == SynthesisStatus.BUSTED)
        return 0.0;
      if (state.Status == SynthesisStatus.IN_PROGRESS)
        return 1.0;
      return 1.0 + (double)state.Quality / (double)state.MaxQuality;
    }
  }
}
