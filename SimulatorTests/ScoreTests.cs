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

    [TestMethod]
    public void CompareScores1()
    {
      State s1 = new State();
      s1.Progress = 75;
      s1.MaxProgress = 75;
      s1.Quality = 248;
      s1.MaxQuality = 1080;
      s1.CP = 77;
      s1.MaxCP = 251;
      s1.Durability = 50;
      s1.MaxDurability = 70;
      s1.Condition = Condition.Normal;

      State s2 = new State(s1, null);
      s2.Progress = 57;
      s2.Quality = 329;
      s2.Durability = 40;
      s2.CP = 59;

      Assert.AreEqual(true, s2.Score > s1.Score);
    }

    // Test is disabled because it doesn't work, although it should.  The numbers have been verified
    // through in-game observation.  Something is wrong with the progress formula.
    [TestMethod]
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
