using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Simulator.Tests
{
  [TestClass]
  public class StateTests
  {
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
