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

      Assert.AreEqual<ActionType>(ActionType.Progress, root.OptimalAction.originatingAction.Attributes.Type);
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

      uint progressGain = 30;
      state.Progress = 0;
      state.MaxProgress = progressGain * 2;
      UserDecisionNode root = new UserDecisionNode();
      root.originalState = state;

      analyzer.Run(root);

      foreach (PreRandomDecisionNode child in root.OptimalUserDecisions)
        Assert.AreEqual<ActionType>(ActionType.Progress, child.originatingAction.Attributes.Type);
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
    public void EnsureMastersMendAtEndOfSingleStageSynth()
    {
      Analyzer analyzer = new Analyzer();
      analyzer.Actions.AddAction(new BasicSynthesis());
      analyzer.Actions.AddAction(new BasicTouch());
      analyzer.Actions.AddAction(new MastersMend());
      analyzer.Actions.AddAction(new SteadyHand());
      analyzer.Actions.AddAction(new Observe());
      analyzer.Actions.AddAction(new Manipulation());
      analyzer.Actions.AddAction(new TricksOfTheTrade());
      analyzer.Actions.AddAction(new StandardTouch());

      State initialState = new State();
      initialState.Condition = Condition.Normal;
      initialState.Control = 115;
      initialState.Craftsmanship = 123;
      initialState.CP = 251;
      initialState.MaxCP = 251;
      initialState.MaxDurability = 70;
      initialState.Durability = 10;
      initialState.Progress = 73;
      initialState.MaxProgress = 74;
      initialState.Quality = 0;
      initialState.MaxQuality = 1053;
      initialState.SynthLevel = 20;
      initialState.CrafterLevel = 18;

      analyzer.MaxAnalysisDepth = 1;

      UserDecisionNode root = new UserDecisionNode();
      root.originalState = initialState;
      analyzer.Run(root);

      Assert.AreEqual<Type>(typeof(MastersMend), root.OptimalAction.originatingAction.GetType());
   }

    [TestMethod]
    public void EnsureMastersMendAtEndOfMultiStageSynth()
    {
      // By using a 4 step max analysis depth, we can check that after 3 steps, if the
      // 4th step would end the synth, a Master's Mend can be used to regain durability.
      Analyzer analyzer = new Analyzer();
      analyzer.Actions.AddAction(new BasicSynthesis());
      analyzer.Actions.AddAction(new BasicTouch());
      analyzer.Actions.AddAction(new MastersMend());
      analyzer.Actions.AddAction(new SteadyHand());
      analyzer.Actions.AddAction(new Observe());
      analyzer.Actions.AddAction(new Manipulation());
      analyzer.Actions.AddAction(new TricksOfTheTrade());
      analyzer.Actions.AddAction(new StandardTouch());

      State initialState = new State();
      initialState.Condition = Condition.Normal;
      initialState.Control = 115;
      initialState.Craftsmanship = 123;
      initialState.CP = 251;
      initialState.MaxCP = 251;
      initialState.MaxDurability = 40;
      initialState.Durability = 40;
      initialState.MaxProgress = 74;
      initialState.Quality = 0;
      initialState.MaxQuality = 1053;
      initialState.SynthLevel = 20;
      initialState.CrafterLevel = 18;

      analyzer.MaxAnalysisDepth = 4;

      UserDecisionNode root = new UserDecisionNode();
      root.originalState = initialState;
      analyzer.Run(root);

      root = root.Choose(typeof(BasicSynthesis), true, Condition.Normal);
      root = root.Choose(typeof(BasicSynthesis), true, Condition.Normal);
      root = root.Choose(typeof(BasicSynthesis), true, Condition.Normal);
      Assert.AreEqual<Type>(typeof(MastersMend), root.OptimalAction.originatingAction.GetType());
    }
  }
}
