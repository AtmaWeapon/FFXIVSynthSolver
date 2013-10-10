using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Simulator.Tests
{
  [TestClass]
  public class StateTests
  {
    [TestMethod]
    public void TestCraftsmanship()
    {
      Engine.StateDetails2 details = new Engine.StateDetails2();
      Assert.AreEqual(0U, details.Craftsmanship);

      details.Craftsmanship = 50U;
      Assert.AreEqual(50U, details.Craftsmanship);

      details.Craftsmanship = 300U;
      Assert.AreEqual(300U, details.Craftsmanship);

      details.Craftsmanship = 0U;
      Assert.AreEqual(0U, details.Craftsmanship);
      Assert.AreEqual(0U, details.RawWord1);
      Assert.AreEqual(0U, details.RawWord2);
    }

    [TestMethod]
    public void TestControl()
    {
      Engine.StateDetails2 details = new Engine.StateDetails2();
      Assert.AreEqual(0U, details.Control);

      details.Control = 50U;
      Assert.AreEqual(50U, details.Control);

      details.Control = 300U;
      Assert.AreEqual(300U, details.Control);

      details.Control = 0U;
      Assert.AreEqual(0U, details.Control);
      Assert.AreEqual(0U, details.RawWord1);
      Assert.AreEqual(0U, details.RawWord2);
    }

    [TestMethod]
    public void TestDurability()
    {
      Engine.StateDetails2 details = new Engine.StateDetails2();
      Assert.AreEqual(0U, details.Durability);

      details.Durability = 50U;
      Assert.AreEqual(50U, details.Durability);

      details.Durability = 30U;
      Assert.AreEqual(30U, details.Durability);

      details.Durability = 0U;
      Assert.AreEqual(0U, details.Durability);
      Assert.AreEqual(0U, details.RawWord1);
      Assert.AreEqual(0U, details.RawWord2);
    }

    [TestMethod]
    public void TestMaxDurability()
    {
      Engine.StateDetails2 details = new Engine.StateDetails2();
      Assert.AreEqual(0U, details.MaxDurability);

      details.MaxDurability = 50U;
      Assert.AreEqual(50U, details.MaxDurability);

      details.MaxDurability = 30U;
      Assert.AreEqual(30U, details.MaxDurability);

      details.MaxDurability = 0U;
      Assert.AreEqual(0U, details.MaxDurability);
      Assert.AreEqual(0U, details.RawWord1);
      Assert.AreEqual(0U, details.RawWord2);
    }

    [TestMethod]
    public void TestCP()
    {
      Engine.StateDetails2 details = new Engine.StateDetails2();
      Assert.AreEqual(0U, details.CP);

      details.CP = 100U;
      Assert.AreEqual(100U, details.CP);

      details.CP = 400U;
      Assert.AreEqual(400U, details.CP);

      details.CP = 0U;
      Assert.AreEqual(0U, details.CP);
      Assert.AreEqual(0U, details.RawWord1);
      Assert.AreEqual(0U, details.RawWord2);
    }

    [TestMethod]
    public void TestMaxCP()
    {
      Engine.StateDetails2 details = new Engine.StateDetails2();
      Assert.AreEqual(0U, details.MaxCP);

      details.MaxCP = 100U;
      Assert.AreEqual(100U, details.MaxCP);

      details.MaxCP = 400U;
      Assert.AreEqual(400U, details.MaxCP);

      details.MaxCP = 0U;
      Assert.AreEqual(0U, details.MaxCP);
      Assert.AreEqual(0U, details.RawWord1);
      Assert.AreEqual(0U, details.RawWord2);
    }

    [TestMethod]
    public void TestProgress()
    {
      Engine.StateDetails2 details = new Engine.StateDetails2();
      Assert.AreEqual(0U, details.Progress);

      details.Progress = 60U;
      Assert.AreEqual(60U, details.Progress);

      details.Progress = 23U;
      Assert.AreEqual(23U, details.Progress);

      details.Progress = 0U;
      Assert.AreEqual(0U, details.Progress);
      Assert.AreEqual(0U, details.RawWord1);
      Assert.AreEqual(0U, details.RawWord2);
    }

    [TestMethod]
    public void TestMaxProgress()
    {
      Engine.StateDetails2 details = new Engine.StateDetails2();
      Assert.AreEqual(0U, details.MaxProgress);

      details.MaxProgress = 60U;
      Assert.AreEqual(60U, details.MaxProgress);

      details.MaxProgress = 23U;
      Assert.AreEqual(23U, details.MaxProgress);

      details.MaxProgress = 0U;
      Assert.AreEqual(0U, details.MaxProgress);
      Assert.AreEqual(0U, details.RawWord1);
      Assert.AreEqual(0U, details.RawWord2);
    }

    [TestMethod]
    public void TestMultipleStatAssignment1()
    {
      Engine.StateDetails2 details = new Engine.StateDetails2();

      details.Control = 115U;
      details.CP = 235U;
      details.Craftsmanship = 432U;
      details.Durability = 60U;
      details.MaxCP = 315U;
      details.MaxDurability = 80U;
      details.MaxProgress = 65U;
      details.Progress = 32U;

      Assert.AreEqual(115U, details.Control);
      Assert.AreEqual(235U, details.CP);
      Assert.AreEqual(432U, details.Craftsmanship);
      Assert.AreEqual(60U, details.Durability);
      Assert.AreEqual(315U, details.MaxCP);
      Assert.AreEqual(80U, details.MaxDurability);
      Assert.AreEqual(65U, details.MaxProgress);
      Assert.AreEqual(32U, details.Progress);
    }

    [TestMethod]
    public void TestCrafterLevel()
    {
      Engine.StateDetails2 details = new Engine.StateDetails2();
      Assert.AreEqual(0U, details.CrafterLevel);

      details.CrafterLevel = 40U;
      Assert.AreEqual(40U, details.CrafterLevel);

      details.CrafterLevel = 23U;
      Assert.AreEqual(23U, details.CrafterLevel);

      details.CrafterLevel = 0U;
      Assert.AreEqual(0U, details.CrafterLevel);
      Assert.AreEqual(0U, details.RawWord1);
      Assert.AreEqual(0U, details.RawWord2);
    }

    [TestMethod]
    public void TestSynthLevel()
    {
      Engine.StateDetails2 details = new Engine.StateDetails2();
      Assert.AreEqual(0U, details.SynthLevel);

      details.SynthLevel = 40U;
      Assert.AreEqual(40U, details.SynthLevel);

      details.SynthLevel = 23U;
      Assert.AreEqual(23U, details.SynthLevel);

      details.SynthLevel = 0U;
      Assert.AreEqual(0U, details.SynthLevel);
      Assert.AreEqual(0U, details.RawWord1);
      Assert.AreEqual(0U, details.RawWord2);
    }

    [TestMethod]
    public void TestCondition()
    {
      Engine.StateDetails2 details = new Engine.StateDetails2();
      Assert.AreEqual(Engine.Condition.Normal, details.Condition);

      details.Condition = Engine.Condition.Poor;
      Assert.AreEqual(Engine.Condition.Poor, details.Condition);

      details.Condition = Engine.Condition.Excellent;
      Assert.AreEqual(Engine.Condition.Excellent, details.Condition);

      details.Condition = Engine.Condition.Normal;
      Assert.AreEqual(Engine.Condition.Normal, details.Condition);
      Assert.AreEqual(0U, details.RawWord1);
      Assert.AreEqual(0U, details.RawWord2);
    }

    [TestMethod]
    public void TestManipulationTurns()
    {
      Engine.StateDetails2 details = new Engine.StateDetails2();
      Assert.AreEqual(0U, details.ManipulationTurns);

      details.ManipulationTurns = 3U;
      Assert.AreEqual(3U, details.ManipulationTurns);

      details.ManipulationTurns = 2U;
      Assert.AreEqual(2U, details.ManipulationTurns);

      details.ManipulationTurns = 0U;
      Assert.AreEqual(0U, details.ManipulationTurns);
      Assert.AreEqual(0U, details.RawWord1);
      Assert.AreEqual(0U, details.RawWord2);
    }

    [TestMethod]
    public void TestGreatStridesTurns()
    {
      Engine.StateDetails2 details = new Engine.StateDetails2();
      Assert.AreEqual(0U, details.GreatStridesTurns);

      details.GreatStridesTurns = 3U;
      Assert.AreEqual(3U, details.GreatStridesTurns);

      details.GreatStridesTurns = 2U;
      Assert.AreEqual(2U, details.GreatStridesTurns);

      details.GreatStridesTurns = 0U;
      Assert.AreEqual(0U, details.GreatStridesTurns);
      Assert.AreEqual(0U, details.RawWord1);
      Assert.AreEqual(0U, details.RawWord2);
    }

    [TestMethod]
    public void TestIngenuityTurns()
    {
      Engine.StateDetails2 details = new Engine.StateDetails2();
      Assert.AreEqual(0U, details.IngenuityTurns);

      details.IngenuityTurns = 3U;
      Assert.AreEqual(3U, details.IngenuityTurns);

      details.IngenuityTurns = 2U;
      Assert.AreEqual(2U, details.IngenuityTurns);

      details.IngenuityTurns = 0U;
      Assert.AreEqual(0U, details.IngenuityTurns);
      Assert.AreEqual(0U, details.RawWord1);
      Assert.AreEqual(0U, details.RawWord2);
    }

    [TestMethod]
    public void TestSteadyHandTurns()
    {
      Engine.StateDetails2 details = new Engine.StateDetails2();
      Assert.AreEqual(0U, details.SteadyHandTurns);

      details.SteadyHandTurns = 5U;
      Assert.AreEqual(5U, details.SteadyHandTurns);

      details.SteadyHandTurns = 2U;
      Assert.AreEqual(2U, details.SteadyHandTurns);

      details.SteadyHandTurns = 0U;
      Assert.AreEqual(0U, details.SteadyHandTurns);
      Assert.AreEqual(0U, details.RawWord1);
      Assert.AreEqual(0U, details.RawWord2);
    }

    [TestMethod]
    public void TestMultipleStatAssignment2()
    {
      Engine.StateDetails2 details = new Engine.StateDetails2();

      details.CrafterLevel = 13U;
      details.SynthLevel = 40U;
      details.Condition = Engine.Condition.Excellent;
      details.ManipulationTurns = 2U;
      details.GreatStridesTurns = 3U;
      details.IngenuityTurns = 1U;
      details.SteadyHandTurns = 4U;

      Assert.AreEqual(13U, details.CrafterLevel);
      Assert.AreEqual(40U, details.SynthLevel);
      Assert.AreEqual(Engine.Condition.Excellent, details.Condition);
      Assert.AreEqual(2U, details.ManipulationTurns);
      Assert.AreEqual(3U, details.GreatStridesTurns);
      Assert.AreEqual(1U, details.IngenuityTurns);
      Assert.AreEqual(4U, details.SteadyHandTurns);
    }

    [TestMethod]
    public void BustedSynthStatus()
    {
      // 0 Durability and < max progress is busted.
      Engine.State state = Utility.CreateDefaultState();
      state.Durability = 0;
      state.MaxDurability = 20;
      state.MaxProgress = 20;
      state.Progress = state.MaxProgress - 1;
      Assert.AreEqual(Engine.SynthesisStatus.BUSTED, state.Status);
    }

    [TestMethod]
    public void CompletedSynthStatus()
    {
      Engine.State state = Utility.CreateDefaultState();

      // Max Progress and non-zero durability is completed.
      state.MaxProgress = 20;
      state.Progress = state.MaxProgress;
      state.MaxDurability = 10;
      state.Durability = 10;
      Assert.AreEqual(Engine.SynthesisStatus.COMPLETED, state.Status);

      // Max Progress and zero durability is completed.
      state.Durability = 0;
      Assert.AreEqual(Engine.SynthesisStatus.COMPLETED, state.Status);
    }

    [TestMethod]
    public void InProgressSynthStatus()
    {
      Engine.State state = Utility.CreateDefaultState();


      state.MaxProgress = 20;
      state.Progress = state.MaxProgress - 1;
      state.MaxDurability = 20;
      state.Durability = 10;
      Assert.AreEqual(Engine.SynthesisStatus.IN_PROGRESS, state.Status);
    }
  }
}
