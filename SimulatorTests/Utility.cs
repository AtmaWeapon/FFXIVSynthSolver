using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Tests
{
  static class Utility
  {
    // Creates a state which is in progress (i.e. not busted, not complete)
    // with non-zero CP and max cp.
    public static Engine.State CreateDefaultState()
    {
      Engine.State result = new Engine.State();
      result.Condition = Engine.Condition.Good;
      result.Control = 102;
      result.Craftsmanship = 105;
      result.Durability = 70;
      result.MaxDurability = 70;
      result.Progress = 0;
      result.MaxProgress = 63;
      result.Quality = 0;
      result.MaxQuality = 512;
      result.CP = 213;
      result.MaxCP = 213;
      return result;
    }
  }
}
