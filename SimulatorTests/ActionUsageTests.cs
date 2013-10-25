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
      Engine.Ability action = new Engine.BasicSynthesis();
      State state = Utility.CreateDefaultState();

      Assert.AreEqual<uint>(1, state.Step);

      State success = action.Activate(state, true);
      State failure = action.Activate(state, false);

      Assert.AreEqual<uint>(2, success.Step);
      Assert.AreEqual<uint>(2, failure.Step);
    }

    [TestMethod]
    public void TestBasicSynthesis()
    {
      BasicSynthesis action = new BasicSynthesis();

      State state = Utility.CreateDefaultState();

      State success = action.Activate(state, true);
      State failure = action.Activate(state, false);

      // Test that we can use it even with 0 CP
      TestBlocks.TestCanUseWithCP(action, state, 0);

      // Test that it doesn't use any CP
      Assert.AreEqual<uint>(state.CP, success.CP);
      Assert.AreEqual<uint>(state.CP, failure.CP);

      // Test that it uses the correct amount of durability.
      Assert.AreEqual<uint>(state.Durability - action.BaseDurability, success.Durability);
      Assert.AreEqual<uint>(state.Durability - action.BaseDurability, failure.Durability);

      // Test that failure does not increase progress, but success does.
      Assert.AreEqual<uint>(state.Progress, failure.Progress);
      Assert.AreEqual<uint>(state.Progress + Compute.Progress(state, action.BaseEfficiency), success.Progress);

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
      BasicTouch action = new BasicTouch();

      State state = Utility.CreateDefaultState();

      // Test that if not enough CP, we can't use it
      TestBlocks.TestCannotUseWithCP(action, state, 0);

      State success = action.Activate(state, true);
      State failure = action.Activate(state, false);

      // Test that it uses the correct amount of CP
      Assert.AreEqual<uint>(state.CP - action.RequiredCP, success.CP);
      Assert.AreEqual<uint>(state.CP - action.RequiredCP, failure.CP);

      // Test that it uses the correct amount of durability.
      Assert.AreEqual<uint>(state.Durability - action.BaseDurability, success.Durability);
      Assert.AreEqual<uint>(state.Durability - action.BaseDurability, failure.Durability);

      // Test that failure does not increase quality, but success does.
      Assert.AreEqual<uint>(state.Quality, failure.Quality);
      Assert.AreEqual<uint>(state.Quality + Compute.Quality(state, action.BaseEfficiency), success.Quality);

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
      Engine.Ability action = new Engine.MastersMend();

      State state = Utility.CreateDefaultState();

      state.MaxDurability = 70;
      state.Durability = 40;

      State success = action.Activate(state, true);

      // Test that it uses the correct amount of CP
      Assert.AreEqual<uint>(state.CP - action.RequiredCP, success.CP);

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
      SteadyHand sh = new SteadyHand();
      HastyTouch ht = new HastyTouch();

      State state = Utility.CreateDefaultState();

      state.Control = 10;
      state.MaxDurability = 100;
      state.Durability = 100;
      state.MaxProgress = 127;

      // Make sure we can actually use SH
      Assert.IsTrue(sh.CanUse(state));

      State afterSH = sh.Activate(state, true);
      // Make sure Steady Hand uses the right amount of TP.
      Assert.AreEqual<uint>(state.CP - sh.RequiredCP, afterSH.CP);

      // Test that the buff is up, and is active for the correct number of turns.
      Assert.AreEqual<uint>(sh.Duration, SteadyHand.GetTurnsRemaining(afterSH));

      // Use Basic Synthesis 5 times, and make sure the success bonus decreases.
      for (int i = 5; i > 0; --i)
      {
        Assert.AreEqual<uint>((uint)i, SteadyHand.GetTurnsRemaining(afterSH));
        Assert.IsTrue(Compute.SuccessRate(ht.BaseSuccessRate, afterSH) > Compute.SuccessRate(ht.BaseSuccessRate, state));

        // Make sure we can't use SH while SH is up.
        Assert.IsFalse(sh.CanUse(afterSH));

        afterSH = ht.Activate(afterSH, true);
      }
      Assert.AreEqual<uint>(0, SteadyHand.GetTurnsRemaining(afterSH));
      Assert.AreEqual<double>(Compute.SuccessRate(sh.BaseSuccessRate, state), Compute.SuccessRate(sh.BaseSuccessRate, afterSH));
    }

    [TestMethod]
    public void TestObserve()
    {
      Engine.Ability observe = new Engine.Observe();
      Engine.Ability basic = new Engine.BasicSynthesis();

      State state = Utility.CreateDefaultState();

      // Test that Observe can't be used first.
      Assert.IsFalse(observe.CanUse(state));

      // Use Basic Synthesis so that observe becomes useable.
      state = basic.Activate(state, true);

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
      State newState = observe.Activate(state, true);

      // Make sure Observe uses the right amount of CP.
      Assert.AreEqual<uint>(state.CP - observe.RequiredCP, newState.CP);
    }

    [TestMethod]
    public void TestManipulationWithTenDurabilityBusts()
    {
      Manipulation manipulation = new Manipulation();
      BasicSynthesis basic = new BasicSynthesis();

      State state = Utility.CreateDefaultState();
      state.Durability = 10;
      State s2 = manipulation.Activate(state, true);
      Assert.AreEqual<uint>(10, s2.Durability);
      Assert.AreEqual(SynthesisStatus.IN_PROGRESS, s2.Status);

      s2 = basic.Activate(s2, true);
      Assert.AreEqual<uint>(0, s2.Durability);
      Assert.AreEqual(SynthesisStatus.BUSTED, s2.Status);
    }

    [TestMethod]
    public void TestManipulationWithTouchDoesntChangeDurability()
    {
      Manipulation manipulation = new Manipulation();
      BasicTouch touch = new BasicTouch();

      State state = Utility.CreateDefaultState();
      state.Durability = 20;
      State s2 = manipulation.Activate(state, true);
      Assert.AreEqual<uint>(20, s2.Durability);
      Assert.AreEqual(SynthesisStatus.IN_PROGRESS, s2.Status);

      s2 = touch.Activate(s2, true);
      Assert.AreEqual<uint>(20, s2.Durability);
      Assert.AreEqual(SynthesisStatus.IN_PROGRESS, s2.Status);
    }

    [TestMethod]
    public void TestSubsequentAbilitiesStillTickAfterOneWearsOff()
    {
      Manipulation manipulation = new Manipulation();
      GreatStrides strides = new GreatStrides();
      BasicTouch touch = new BasicTouch();

      State state = Utility.CreateDefaultState();
      state.Durability = 20;
      State s2 = strides.Activate(state, true);
      s2 = manipulation.Activate(s2, true);

      GreatStrides.SetTurnsRemaining(s2, 1);

      s2 = touch.Activate(s2, true);
      // Make sure Manipulation still ticked even though great strides wore off.
      Assert.AreEqual<uint>(20, s2.Durability);
    }

    [TestMethod]
    public void TestManipulation()
    {
      Manipulation manipulation = new Manipulation();
      BasicSynthesis basic = new BasicSynthesis();
      SteadyHand steadyHand = new SteadyHand();

      State state = Utility.CreateDefaultState();
      state.Durability = 30;
      state.MaxDurability = 60;

      // Verify that Manipulation can be activated successfully.
      State s1 = manipulation.Activate(state, true);
      Assert.AreEqual<uint>(manipulation.Duration, Manipulation.GetTurnsRemaining(s1));

      // Verify that the durability has not changed simply as a result of activating manipulation.
      Assert.AreEqual<uint>(state.Durability, s1.Durability);

      // Verify that Manipulation can't be used if it's already up.
      Assert.IsFalse(manipulation.CanUse(s1));

      // Verify that using Basic Synthesis with Manipulation up doesn't decrease the durability.
      State s2 = basic.Activate(s1, true);
      Assert.AreEqual<uint>(s1.Durability, s2.Durability);
      Assert.AreEqual<uint>(2, Manipulation.GetTurnsRemaining(s2));
      Assert.IsFalse(manipulation.CanUse(s2));

      // Verify that using Steady hand with Manipulation up causes the durability to increase by 10.
      State s3 = steadyHand.Activate(s2, true);
      Assert.AreEqual<uint>(s2.Durability + 10, s3.Durability);
      Assert.AreEqual<uint>(1, Manipulation.GetTurnsRemaining(s3));
      Assert.IsFalse(manipulation.CanUse(s3));

      // Use another Basic Synthesis so we can verify that the buff de-activates.
      State s4 = basic.Activate(s3, true);
      Assert.IsTrue(manipulation.CanUse(s4));
    }

    [TestMethod]
    public void TestSteadyHandCanBeUsedWhenMightAsWell()
    {
      Engine.Ability basicSynth = new Engine.BasicSynthesis();
      Engine.Ability basicTouch = new Engine.BasicSynthesis();
      Engine.Ability steadyHand = new Engine.SteadyHand();

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

    [TestMethod]
    public void TestGreatStridesDuration()
    {
      GreatStrides greatStrides = new GreatStrides();
      BasicSynthesis basicSynth = new BasicSynthesis();

      State state = Utility.CreateDefaultState();
      state.Quality = 142;
      state.MaxQuality = 726;
      state.Progress = 0;
      state.MaxProgress = 100;
      state.Durability = 100;
      state.MaxDurability = 100;
      state.CrafterLevel = 16;
      state.SynthLevel = 13;
      state.Craftsmanship = 105;
      state.Control = 102;
      state.Condition = Condition.Normal;
      state.CP = 55;
      state.MaxCP = 233;

      Assert.IsTrue(greatStrides.CanUse(state));
      state = greatStrides.Activate(state, true);
      for (uint i = greatStrides.Duration; i > 0; --i)
      {
        Assert.AreEqual<uint>(i, GreatStrides.GetTurnsRemaining(state));
        state = basicSynth.Activate(state, true);
      }
      Assert.AreEqual<uint>(0U, GreatStrides.GetTurnsRemaining(state));
    }

    [TestMethod]
    public void TestGreatStridesQualityMultiplier()
    {
      Engine.Ability greatStrides = new Engine.GreatStrides();
      Engine.Ability basicTouch = new Engine.BasicTouch();

      State state = Utility.CreateDefaultState();
      state.Quality = 142;
      state.MaxQuality = 726;
      state.Progress = 0;
      state.MaxProgress = 100;
      state.Durability = 20;
      state.MaxDurability = 40;
      state.CrafterLevel = 16;
      state.SynthLevel = 13;
      state.Craftsmanship = 105;
      state.Control = 102;
      state.Condition = Condition.Normal;
      state.CP = 55;
      state.MaxCP = 233;

      Assert.IsTrue(greatStrides.CanUse(state));

      // Figure out how much quality it normally gives, then use great strides
      // and make sure the delta is double.
      State baselineState = basicTouch.Activate(state, true);

      State gsState = greatStrides.Activate(state, true);
      State comparisonState = basicTouch.Activate(gsState, true);

      uint baselineDelta = baselineState.Quality - state.Quality;
      uint comparisonDelta = comparisonState.Quality - state.Quality;
      Assert.AreEqual<uint>(baselineDelta * 2, comparisonDelta);
    }

    [TestMethod]
    public void TestGreatStridesRemovedImmediatelyOnTouch()
    {
      GreatStrides greatStrides = new GreatStrides();
      BasicTouch basicTouch = new BasicTouch();

      State state = Utility.CreateDefaultState();
      state.Quality = 142;
      state.MaxQuality = 726;
      state.Progress = 0;
      state.MaxProgress = 100;
      state.Durability = 20;
      state.MaxDurability = 40;
      state.CrafterLevel = 16;
      state.SynthLevel = 13;
      state.Craftsmanship = 105;
      state.Control = 102;
      state.Condition = Condition.Normal;
      state.CP = 55;
      state.MaxCP = 233;

      Assert.IsTrue(greatStrides.CanUse(state));

      state = greatStrides.Activate(state, true);
      Assert.AreEqual<uint>(3U, GreatStrides.GetTurnsRemaining(state));
      state = basicTouch.Activate(state, true);
      Assert.AreEqual<uint>(0U, GreatStrides.GetTurnsRemaining(state));
    }
  }
}
