using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Simulator.Engine;

namespace Simulator.Tests
{
  [TestClass]
  public class ActionUsageTests
  {

    public ActionUsageTests()
    {
    }

    [TestMethod]
    public void TestStepIncrement()
    {
      Engine.Action action = new Engine.BasicSynthesis();
      State state = Utility.CreateDefaultState();

      Assert.AreEqual<uint>(1, state.Step);

      State success = action.SuccessState(state);
      State failure = action.FailureState(state);

      Assert.AreEqual<uint>(2, success.Step);
      Assert.AreEqual<uint>(2, failure.Step);
    }

    [TestMethod]
    public void TestBasicSynthesis()
    {
      Engine.Action action = new Engine.BasicSynthesis();

      State state = Utility.CreateDefaultState();

      State success = action.SuccessState(state);
      State failure = action.FailureState(state);

      // Test that it doesn't use any CP
      Assert.AreEqual<uint>(state.CP, success.CP);
      Assert.AreEqual<uint>(state.CP, failure.CP);

      // Test that it uses the correct amount of durability.
      Assert.AreEqual<uint>(state.Durability - action.Attributes.Durability, success.Durability);
      Assert.AreEqual<uint>(state.Durability - action.Attributes.Durability, failure.Durability);

      // Test that failure does not increase progress, but success does.
      Assert.AreEqual<uint>(state.Progress, failure.Progress);
      Assert.AreEqual<uint>(state.Progress + Compute.Progress(state, action.Attributes.Efficiency), success.Progress);

      // Test that nothing changed except durability, progress, and condition.
      success.Condition = failure.Condition = state.Condition;
      success.Durability = failure.Durability = state.Durability;
      success.Progress = failure.Progress = state.Progress;
      bool equal = state.Equals(success);
      Assert.AreEqual<State>(state, success);
      Assert.AreEqual<State>(state, failure);
    }

    [TestMethod]
    public void TestBasicTouch()
    {
      Engine.Action action = new Engine.BasicTouch();

      State state = Utility.CreateDefaultState();

      State success = action.SuccessState(state);
      State failure = action.FailureState(state);

      // Test that it uses the correct amount of CP
      Assert.AreEqual<uint>(state.CP - action.Attributes.CP, success.CP);
      Assert.AreEqual<uint>(state.CP - action.Attributes.CP, failure.CP);

      // Test that it uses the correct amount of durability.
      Assert.AreEqual<uint>(state.Durability - action.Attributes.Durability, success.Durability);
      Assert.AreEqual<uint>(state.Durability - action.Attributes.Durability, failure.Durability);

      // Test that failure does not increase quality, but success does.
      Assert.AreEqual<uint>(state.Quality, failure.Quality);
      Assert.AreEqual<uint>(state.Quality + Compute.Quality(state, action.Attributes.Efficiency), success.Quality);

      // Test that nothing changed except durability, quality, CP, and condition.
      success.Condition = failure.Condition = state.Condition;
      success.Durability = failure.Durability = state.Durability;
      success.Quality = failure.Quality = state.Quality;
      success.CP = failure.CP = state.CP;
      Assert.AreEqual<State>(state, success);
      Assert.AreEqual<State>(state, failure);
    }

    [TestMethod]
    public void TestMastersMend()
    {
      Engine.Action action = new Engine.MastersMend();

      State state = Utility.CreateDefaultState();

      state.MaxDurability = 70;
      state.Durability = 40;

      State success = action.SuccessState(state);

      // Test that it uses the correct amount of CP
      Assert.AreEqual<uint>(state.CP - action.Attributes.CP, success.CP);

      // Test that it increases durability by 30.
      Assert.AreEqual<uint>(state.Durability+30, success.Durability);

      // Test that nothing changed except durability, CP, and condition.
      success.Condition = state.Condition;
      success.Durability = state.Durability;
      success.CP = state.CP;
      Assert.AreEqual<State>(state, success);

      // Test that we can't use if won't get the full effect.
      state.Durability = 40;
      Assert.IsTrue(action.CanUse(state));
      ++state.Durability;
      Assert.IsFalse(action.CanUse(state));
    }

    [TestMethod]
    public void TestSteadyHand()
    {
      Engine.Action sh = new Engine.SteadyHand();
      Engine.Action bs = new Engine.BasicSynthesis();

      State state = Utility.CreateDefaultState();

      state.Control = 10;
      state.MaxDurability = 100;
      state.Durability = 100;
      state.MaxProgress = 127;

      // Make sure we can actually use SH
      Assert.IsTrue(sh.CanUse(state));

      State afterSH = sh.SuccessState(state);
      // Make sure Steady Hand uses the right amount of TP.
      Assert.AreEqual<uint>(state.CP - sh.Attributes.CP, afterSH.CP);

      // Test that the buff is up, and is active for the correct number of turns.
      Assert.AreEqual<uint>(sh.Attributes.BuffDuration, afterSH.SteadyHandTurns);

      // Use Basic Synthesis 5 times, and make sure the success bonus decreases.
      for (int i = 5; i > 0; --i)
      {
        Assert.AreEqual<uint>((uint)i, afterSH.SteadyHandTurns);
        Assert.AreEqual<double>(0.2, afterSH.SuccessBonus);

        // Make sure we can't use SH while SH is up.
        Assert.IsFalse(sh.CanUse(afterSH));

        afterSH = bs.SuccessState(afterSH);
      }
      Assert.AreEqual<uint>(0, afterSH.SteadyHandTurns);
      Assert.AreEqual<double>(0, afterSH.SuccessBonus);
    }

    [TestMethod]
    public void TestObserve()
    {
      Engine.Action observe = new Engine.Observe();
      Engine.Action basic = new Engine.BasicSynthesis();

      State state = Utility.CreateDefaultState();

      // Test that Observe can't be used first.
      Assert.IsFalse(observe.CanUse(state));

      // Use Basic Synthesis so that observe becomes useable.
      state = basic.SuccessState(state);

      // Test the condition requirement for Observe
      state.Condition = Condition.Poor;
      Assert.IsTrue(observe.CanUse(state));
      state.Condition = Condition.Normal;
      Assert.IsFalse(observe.CanUse(state));
      state.Condition = Condition.Good;
      Assert.IsFalse(observe.CanUse(state));
      state.Condition = Condition.Excellent;
      Assert.IsFalse(observe.CanUse(state));
      
      // Test that observe can't be run twice in a row.
      state.Condition = Condition.Poor;
      Assert.IsTrue(observe.CanUse(state));
      State newState = observe.SuccessState(state);

      Assert.IsFalse(observe.CanUse(newState));
      // Make sure Steady Hand uses the right amount of TP.
      Assert.AreEqual<uint>(state.CP - observe.Attributes.CP, newState.CP);
    }

    [TestMethod]
    public void TestManipulation()
    {
      Engine.Action manipulation = new Engine.Manipulation();
      Engine.Action basic = new Engine.BasicSynthesis();
      Engine.Action steadyHand = new Engine.SteadyHand();

      State state = Utility.CreateDefaultState();
      state.Durability = 30;
      state.MaxDurability = 60;

      // Verify that Manipulation can be activated successfully.
      State s1 = manipulation.SuccessState(state);
      Assert.AreEqual<uint>(manipulation.Attributes.BuffDuration, s1.ManipulationTurns);

      // Verify that Manipulation can't be used if it's already up.
      Assert.IsFalse(manipulation.CanUse(s1));

      // Verify that using Basic Synthesis with Manipulation up doesn't decrease the durability.
      State s2 = basic.SuccessState(s1);
      Assert.AreEqual<uint>(s1.Durability, s2.Durability);
      Assert.AreEqual<uint>(2, s2.ManipulationTurns);
      Assert.IsFalse(manipulation.CanUse(s2));

      // Verify that using Steady hand with Manipulation up causes the durability to increase by 10.
      State s3 = steadyHand.SuccessState(s2);
      Assert.AreEqual<uint>(s2.Durability + 10, s3.Durability);
      Assert.AreEqual<uint>(1, s3.ManipulationTurns);
      Assert.IsFalse(manipulation.CanUse(s3));

      // Use another Basic Synthesis so we can verify that the buff de-activates.
      State s4 = basic.SuccessState(s3);
      Assert.IsTrue(manipulation.CanUse(s4));
    }

    [TestMethod]
    public void TestSteadyHandCanBeUsedWhenMightAsWell()
    {
      Engine.Action basicSynth = new Engine.BasicSynthesis();
      Engine.Action basicTouch = new Engine.BasicSynthesis();
      Engine.Action steadyHand = new Engine.SteadyHand();

      State state = Utility.CreateDefaultState();
      state.Quality = 142;
      state.MaxQuality = 726;
      state.Progress = 0;
      state.MaxProgress = 27;
      state.Durability = 20;
      state.MaxDurability = 40;
      state.CrafterLevel = 16;
      state.SynthLevel = 13;
      state.Craftsmanship = 105;
      state.Control = 102;
      state.Condition = Condition.Normal;
      state.CP = 55;
      state.MaxCP = 233;

      Assert.IsTrue(steadyHand.CanUse(state));
    }
  }
}
