using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Simulator.Tests
{
  [TestClass]
  public class ScoreTests
  {
    [TestMethod]
    public void BustedSynthWorthNothing()
    {
      Engine.State state = Utility.CreateDefaultState();

      state.Durability = 0;
      state.MaxDurability = 70;

      state.Progress = 50;
      state.MaxProgress = 70;

      Assert.AreEqual(Engine.SynthesisStatus.BUSTED, state.Status);
      Assert.AreEqual(0.0f, state.Score);
    }
  }
}
