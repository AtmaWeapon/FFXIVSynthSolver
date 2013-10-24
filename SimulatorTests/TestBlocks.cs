using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simulator.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Tests
{
  internal static class TestBlocks
  {
    public static void TestCanUseWithCP(Ability ability, State baseState, uint cp)
    {
      State newState = new State(baseState);
      newState.CP = cp;

      Assert.IsTrue(ability.CanUse(newState));
    }

    public static void TestCannotUseWithCP(Ability ability, State baseState, uint cp)
    {
      State newState = new State(baseState);
      newState.CP = cp;

      Assert.IsFalse(ability.CanUse(newState));
    }
  }
}
