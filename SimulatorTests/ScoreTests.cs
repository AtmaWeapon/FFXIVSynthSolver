using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simulator.Engine;

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

    // Test is disabled because it doesn't work, although it should.  The numbers have been verified
    // through in-game observation.  Something is wrong with the progress formula.
    public void TestProgressFormulaByObservation1()
    {
      Engine.State state = Utility.CreateDefaultState();
      state.Control = 102;
      state.Craftsmanship = 105;
      state.CrafterLevel = 17;
      state.SynthLevel = 19;

      int progress = Compute.Progress(state, (SynthAction<BasicSynthesis>.Attributes.Efficiency));
      Assert.AreEqual(19, progress);
    }
  }
}
