using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simulator.Engine;
using System.Collections.Generic;

namespace Simulator.Tests
{
  [TestClass]
  public class TreeTests
  {
    [TestMethod]
    public void TestUrgentCompletion()
    {
      Analyzer analyzer = new Analyzer();
      analyzer.Actions.AddAction(new BasicSynthesis());
      analyzer.Actions.AddAction(new BasicTouch());
      analyzer.Actions.AddAction(new MastersMend());
      analyzer.Actions.AddAction(new SteadyHand());
      analyzer.Actions.AddAction(new Observe());

      State state = Utility.CreateDefaultState();

      state.CP = 0;
      state.Durability = 10;
      state.Progress = state.MaxProgress - 1;
      UserDecisionNode root = new UserDecisionNode();
      root.originalState = state;

      analyzer.Run(root);

      Assert.AreEqual(true, root.OptimalAction.originatingAction.Attributes.Type == ActionType.Progress);
    }

    [TestMethod]
    public void TestUrgentCompletion2()
    {
      Analyzer analyzer = new Analyzer();
      analyzer.Actions.AddAction(new BasicSynthesis());
      analyzer.Actions.AddAction(new BasicTouch());
      analyzer.Actions.AddAction(new MastersMend());
      analyzer.Actions.AddAction(new SteadyHand());
      analyzer.Actions.AddAction(new Observe());

      State state = Utility.CreateDefaultState();

      // Make sure we don't have enough CP to run Master's Mend.
      state.CP = Compute.CP(SynthAction<MastersMend>.Attributes.CP, state) - 1;
      state.Durability = 20;

      int progressGain = 30;
      state.Progress = 0;
      state.MaxProgress = progressGain * 2;
      UserDecisionNode root = new UserDecisionNode();
      root.originalState = state;

      analyzer.Run(root);

      foreach (PreRandomDecisionNode child in root.OptimalUserDecisions)
        Assert.AreEqual(true, child.originatingAction.Attributes.Type == ActionType.Progress);
    }

    public void NodesUnsolvedAfterMaxDepth()
    {
      Analyzer analyzer = new Analyzer();
      analyzer.Actions.AddAction(new BasicSynthesis());
      analyzer.Actions.AddAction(new BasicTouch());
      analyzer.Actions.AddAction(new MastersMend());
      analyzer.Actions.AddAction(new SteadyHand());
      analyzer.Actions.AddAction(new Observe());

      analyzer.MaxAnalysisDepth = 1;
      State state = Utility.CreateDefaultState();

      UserDecisionNode root = new UserDecisionNode();
      root.originalState = state;

      analyzer.Run(root);

      foreach (PreRandomDecisionNode child in root.choices)
        Assert.IsFalse(child.IsSolved);
    }

    [TestMethod]
    public void IdenticalActiveBuffsEqual()
    {
      MastersMend mm = new MastersMend();
      //Ingenuity ing = new Ingenuity();

      ActiveBuff mmbuff = new ActiveBuff();
      mmbuff.TurnsRemaining = mm.Attributes.BuffDuration;
      mmbuff.Buff = mm;

      ActiveBuff mmbuff2 = new ActiveBuff();
      mmbuff2.TurnsRemaining = mm.Attributes.BuffDuration;
      mmbuff2.Buff = mm;
      Assert.AreEqual(mmbuff, mmbuff2);
    }
  }
}
