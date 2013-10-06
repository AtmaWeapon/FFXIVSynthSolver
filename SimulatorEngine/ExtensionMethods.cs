using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Engine
{
  public static class ExtensionMethods
  {
    public static string StatusString(this SynthesisStatus status)
    {
      switch (status)
      {
        case SynthesisStatus.BUSTED:
          return "Busted";
        case SynthesisStatus.COMPLETED:
          return "Complete";
        case SynthesisStatus.IN_PROGRESS:
          return "In Progress";
        default:
          return "Unknown";
      }
    }

    public static string ConditionString(this Condition condition)
    {
      switch (condition)
      {
        case Condition.Excellent:
          return "Excellent";
        case Condition.Good:
          return "Good";
        case Condition.Normal:
          return "Normal";
        case Condition.Poor:
          return "Poor";
        default:
          return "Unknown";
      }
    }
  }
}
