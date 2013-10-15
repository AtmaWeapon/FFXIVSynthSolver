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

      Assert.AreEqual<SynthesisStatus>(Engine.SynthesisStatus.BUSTED, state.Status);
      Assert.AreEqual<double>(0.0, state.Score);
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

      uint progress = Compute.Progress(state, (SynthAction<BasicSynthesis>.Attributes.Efficiency));
      Assert.AreEqual<uint>(19, progress);
    }

    [TestMethod]
    public void TestQualityPreferredOverProgressWhenSuccessProbabilityIsHigh()
    {
      State s1 = new State();
      s1.Progress = 57;
      s1.Control = 50;
      s1.Craftsmanship = 50;
      s1.MaxProgress = 75;
      s1.Quality = 248;
      s1.MaxQuality = 1080;
      s1.CP = 77;
      s1.MaxCP = 251;
      s1.Durability = 50;
      s1.MaxDurability = 70;
      s1.Condition = Condition.Normal;

      // Basic Synthesis
      State s2 = new State(s1);
      s2.Progress += 18;
      s2.Durability -= 10;

      // Basic Touch
      State s3 = new State(s1, null);
      s3.Quality += 91;
      s3.Durability -= 10;
      s3.CP -= 18;

      // Since the probability of failing is extremely low, we should prefer
      // quality actions.
      Assert.IsTrue(s3.Score > s2.Score);
    }

    [TestMethod]
    public void CompareScores2()
    {
      State s1 = new State();
      s1.Condition = Condition.Normal;
      s1.Control = 115;
      s1.Craftsmanship = 123;
      s1.CP = 251;
      s1.MaxCP = 251;
      s1.MaxDurability = 70;
      s1.Durability = 10;
      s1.Progress = 73;
      s1.MaxProgress = 74;
      s1.Quality = 0;
      s1.MaxQuality = 1053;
      s1.SynthLevel = 20;
      s1.CrafterLevel = 18;

      State completedState = new State(s1, null);
      completedState.Progress = completedState.MaxProgress;

      State mastersMendState = new State(s1, null);
      mastersMendState.CP -= 92;
      mastersMendState.Durability += 30;

      Assert.IsTrue(completedState.Score < mastersMendState.Score);
    }

    [TestMethod]
    public void TestBasicTouchPreferredOverTricksOfTheTradeWithHighDurability()
    {
      Simulator.Engine.Action tott = new TricksOfTheTrade();
      Simulator.Engine.Action basicTouch = new BasicTouch();

      State state = new State();
      state.Condition = Simulator.Engine.Condition.Good;
      state.Control = 119;
      state.Craftsmanship = 131;
      state.CP = 254;
      state.MaxCP = 254;
      state.MaxDurability = 70;
      state.Durability = 70;
      state.MaxProgress = 74;
      state.Quality = 284;
      state.MaxQuality = 1053;
      state.SynthLevel = 20;
      state.CrafterLevel = 19;

      State tottState = tott.SuccessState(state);
      tottState.Condition = Condition.Normal;

      State basicTouchState = basicTouch.SuccessState(state);
      basicTouchState.Condition = Condition.Normal;

      Assert.IsTrue(basicTouchState.Score > tottState.Score);
    }

    [TestMethod]
    public void TestBasicTouchPreferredOverTricksOfTheTradeWithHighDurability2()
    {
      Simulator.Engine.Action tott = new TricksOfTheTrade();
      Simulator.Engine.Action basicTouch = new BasicTouch();
      Simulator.Engine.Action steadyHand = new SteadyHand();

      State state = new State();
      state.Condition = Simulator.Engine.Condition.Good;
      state.Control = 119;
      state.Craftsmanship = 131;
      state.CP = 254;
      state.MaxCP = 254;
      state.MaxDurability = 70;
      state.Durability = 70;
      state.MaxProgress = 74;
      state.Quality = 284;
      state.MaxQuality = 1053;
      state.SynthLevel = 20;
      state.CrafterLevel = 19;

      state = steadyHand.SuccessState(state);
      State tottState = tott.SuccessState(state);
      tottState.Condition = Condition.Normal;

      State basicTouchState = basicTouch.SuccessState(state);
      basicTouchState.Condition = Condition.Normal;

      Assert.IsTrue(basicTouchState.Score > tottState.Score);
    }
  }
}
