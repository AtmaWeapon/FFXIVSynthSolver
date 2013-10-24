using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simulator.Engine;

namespace Simulator.Tests
{
  [TestClass]
  public class StateTests
  {
    [TestMethod]
    public void TestCraftsmanship()
    {
      State details = new State();
      Assert.AreEqual<uint>(0, details.Craftsmanship);

      details.Craftsmanship = 50U;
      Assert.AreEqual<uint>(50, details.Craftsmanship);

      details.Craftsmanship = 300U;
      Assert.AreEqual<uint>(300, details.Craftsmanship);

      details.Craftsmanship = 0U;
      Assert.AreEqual<uint>(0, details.Craftsmanship);
    }

    [TestMethod]
    public void TestControl()
    {
      State details = new State();
      Assert.AreEqual<uint>(0, details.Control);

      details.Control = 50U;
      Assert.AreEqual<uint>(50, details.Control);

      details.Control = 300U;
      Assert.AreEqual<uint>(300, details.Control);

      details.Control = 0U;
      Assert.AreEqual<uint>(0, details.Control);
    }

    [TestMethod]
    public void TestDurability()
    {
      State details = new State();
      Assert.AreEqual<uint>(0, details.Durability);

      details.Durability = 50U;
      Assert.AreEqual<uint>(50, details.Durability);

      details.Durability = 30U;
      Assert.AreEqual<uint>(30, details.Durability);

      details.Durability = 0U;
      Assert.AreEqual<uint>(0, details.Durability);
    }

    [TestMethod]
    public void TestMaxDurability()
    {
      State details = new State();
      Assert.AreEqual<uint>(0, details.MaxDurability);

      details.MaxDurability = 50U;
      Assert.AreEqual<uint>(50, details.MaxDurability);

      details.MaxDurability = 30U;
      Assert.AreEqual<uint>(30, details.MaxDurability);

      details.MaxDurability = 0U;
      Assert.AreEqual<uint>(0, details.MaxDurability);
    }

    [TestMethod]
    public void TestCP()
    {
      State details = new State();
      Assert.AreEqual<uint>(0, details.CP);

      details.CP = 100U;
      Assert.AreEqual<uint>(100, details.CP);

      details.CP = 400U;
      Assert.AreEqual<uint>(400, details.CP);

      details.CP = 0U;
      Assert.AreEqual<uint>(0, details.CP);
    }

    [TestMethod]
    public void TestMaxCP()
    {
      State details = new State();
      Assert.AreEqual<uint>(0, details.MaxCP);

      details.MaxCP = 100U;
      Assert.AreEqual<uint>(100, details.MaxCP);

      details.MaxCP = 400U;
      Assert.AreEqual<uint>(400, details.MaxCP);

      details.MaxCP = 0U;
      Assert.AreEqual<uint>(0, details.MaxCP);
    }

    [TestMethod]
    public void TestProgress()
    {
      State details = new State();
      Assert.AreEqual<uint>(0, details.Progress);

      details.Progress = 60U;
      Assert.AreEqual<uint>(60, details.Progress);

      details.Progress = 23U;
      Assert.AreEqual<uint>(23, details.Progress);

      details.Progress = 0U;
      Assert.AreEqual<uint>(0, details.Progress);
    }

    [TestMethod]
    public void TestMaxProgress()
    {
      State details = new State();
      Assert.AreEqual<uint>(0, details.MaxProgress);

      details.MaxProgress = 60U;
      Assert.AreEqual<uint>(60, details.MaxProgress);

      details.MaxProgress = 23U;
      Assert.AreEqual<uint>(23, details.MaxProgress);

      details.MaxProgress = 0U;
      Assert.AreEqual<uint>(0, details.MaxProgress);
    }

    [TestMethod]
    public void TestMultipleStatAssignment1()
    {
      State details = new State();

      details.Control = 115U;
      details.CP = 235U;
      details.Craftsmanship = 432U;
      details.Durability = 60U;
      details.MaxCP = 315U;
      details.MaxDurability = 80U;
      details.MaxProgress = 65U;
      details.Progress = 32U;

      Assert.AreEqual<uint>(115, details.Control);
      Assert.AreEqual<uint>(235, details.CP);
      Assert.AreEqual<uint>(432, details.Craftsmanship);
      Assert.AreEqual<uint>(60, details.Durability);
      Assert.AreEqual<uint>(315, details.MaxCP);
      Assert.AreEqual<uint>(80, details.MaxDurability);
      Assert.AreEqual<uint>(65, details.MaxProgress);
      Assert.AreEqual<uint>(32, details.Progress);
    }

    [TestMethod]
    public void TestCrafterLevel()
    {
      State details = new State();
      Assert.AreEqual<uint>(0, details.CrafterLevel);

      details.CrafterLevel = 40U;
      Assert.AreEqual<uint>(40, details.CrafterLevel);

      details.CrafterLevel = 23U;
      Assert.AreEqual<uint>(23, details.CrafterLevel);

      details.CrafterLevel = 0U;
      Assert.AreEqual<uint>(0, details.CrafterLevel);
    }

    [TestMethod]
    public void TestSynthLevel()
    {
      State details = new State();
      Assert.AreEqual<uint>(0, details.SynthLevel);

      details.SynthLevel = 40U;
      Assert.AreEqual<uint>(40, details.SynthLevel);

      details.SynthLevel = 23U;
      Assert.AreEqual<uint>(23, details.SynthLevel);

      details.SynthLevel = 0U;
      Assert.AreEqual<uint>(0, details.SynthLevel);
    }

    [TestMethod]
    public void TestQuality()
    {
      State details = new State();
      Assert.AreEqual<uint>(0, details.Quality);

      details.Quality = 215U;
      Assert.AreEqual<uint>(215, details.Quality);

      details.Quality = 1823U;
      Assert.AreEqual<uint>(1823, details.Quality);

      details.Quality = 0U;
      Assert.AreEqual<uint>(0, details.Quality);
    }

    [TestMethod]
    public void TestMaxQuality()
    {
      State details = new State();
      Assert.AreEqual<uint>(0, details.MaxQuality);

      details.MaxQuality = 215U;
      Assert.AreEqual<uint>(215, details.MaxQuality);

      details.MaxQuality = 1823U;
      Assert.AreEqual<uint>(1823, details.MaxQuality);

      details.MaxQuality = 0U;
      Assert.AreEqual<uint>(0, details.MaxQuality);
    }

    [TestMethod]
    public void TestCondition()
    {
      State details = new State();
      Assert.AreEqual<Engine.Condition>(Engine.Condition.Normal, details.Condition);

      details.Condition = Engine.Condition.Poor;
      Assert.AreEqual<Engine.Condition>(Engine.Condition.Poor, details.Condition);

      details.Condition = Engine.Condition.Excellent;
      Assert.AreEqual<Engine.Condition>(Engine.Condition.Excellent, details.Condition);

      details.Condition = Engine.Condition.Normal;
      Assert.AreEqual<Engine.Condition>(Engine.Condition.Normal, details.Condition);
    }

    [TestMethod]
    public void TestManipulationTurns()
    {
      State details = new State();
      Assert.AreEqual<uint>(0, Manipulation.GetTurnsRemaining(details));

      for (int i = 3; i >= 0; --i)
      {
        Manipulation.SetTurnsRemaining(details, (uint)i);
        Assert.AreEqual<uint>((uint)i, Manipulation.GetTurnsRemaining(details));
      }
    }

    [TestMethod]
    public void TestGreatStridesTurns()
    {
      State details = new State();
      Assert.AreEqual<uint>(0, GreatStrides.GetTurnsRemaining(details));

      for (int i = 3; i >= 0; --i)
      {
        GreatStrides.SetTurnsRemaining(details, (uint)i);
        Assert.AreEqual<uint>((uint)i, GreatStrides.GetTurnsRemaining(details));
      }
    }

    [TestMethod]
    public void TestIngenuityTurns()
    {
      State details = new State();
      Assert.AreEqual<uint>(0, Ingenuity.GetTurnsRemaining(details));

      for (int i = 3; i >= 0; --i)
      {
        Ingenuity.SetTurnsRemaining(details, (uint)i);
        Assert.AreEqual<uint>((uint)i, Ingenuity.GetTurnsRemaining(details));
      }
    }

    [TestMethod]
    public void TestSteadyHandTurns()
    {
      State details = new State();
      Assert.AreEqual<uint>(0, SteadyHand.GetTurnsRemaining(details));

      for (int i = 5; i >= 0; --i)
      {
        SteadyHand.SetTurnsRemaining(details, (uint)i);
        Assert.AreEqual<uint>((uint)i, SteadyHand.GetTurnsRemaining(details));
      }
    }

    [TestMethod]
    public void TestMultipleStatAssignment2()
    {
      State details = new State();

      details.CrafterLevel = 13U;
      details.SynthLevel = 40U;
      details.Quality = 1215U;
      details.MaxQuality = 923U;
      details.Condition = Engine.Condition.Excellent;
      Manipulation.SetTurnsRemaining(details, 2);
      GreatStrides.SetTurnsRemaining(details, 3);
      Ingenuity.SetTurnsRemaining(details, 1);
      SteadyHand.SetTurnsRemaining(details, 4);

      Assert.AreEqual<uint>(13, details.CrafterLevel);
      Assert.AreEqual<uint>(40, details.SynthLevel);
      Assert.AreEqual<uint>(1215, details.Quality);
      Assert.AreEqual<uint>(923, details.MaxQuality);
      Assert.AreEqual<Engine.Condition>(Engine.Condition.Excellent, details.Condition);
      Assert.AreEqual<uint>(2, Manipulation.GetTurnsRemaining(details));
      Assert.AreEqual<uint>(3, GreatStrides.GetTurnsRemaining(details));
      Assert.AreEqual<uint>(1, Ingenuity.GetTurnsRemaining(details));
      Assert.AreEqual<uint>(4, SteadyHand.GetTurnsRemaining(details));
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
      Assert.AreEqual<Engine.SynthesisStatus>(Engine.SynthesisStatus.BUSTED, state.Status);
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
      Assert.AreEqual<Engine.SynthesisStatus>(Engine.SynthesisStatus.COMPLETED, state.Status);

      // Max Progress and zero durability is completed.
      state.Durability = 0;
      Assert.AreEqual<Engine.SynthesisStatus>(Engine.SynthesisStatus.COMPLETED, state.Status);
    }

    [TestMethod]
    public void InProgressSynthStatus()
    {
      Engine.State state = Utility.CreateDefaultState();


      state.MaxProgress = 20;
      state.Progress = state.MaxProgress - 1;
      state.MaxDurability = 20;
      state.Durability = 10;
      Assert.AreEqual<Engine.SynthesisStatus>(Engine.SynthesisStatus.IN_PROGRESS, state.Status);
    }
  }
}
