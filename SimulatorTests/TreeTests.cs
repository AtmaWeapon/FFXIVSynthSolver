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

      analyzer.Run(state);

      Ability bestAction = analyzer.OptimalAction(state);
      Assert.AreEqual<ActionType>(ActionType.Completion, bestAction.ActionType);
      CompletionAction completion = (CompletionAction)bestAction;
      Assert.AreNotEqual<uint>(0, (uint)(CompletionFlags.Progress & completion.CompletionFlags));
    }

    [TestMethod]
    public void TestUrgentCompletion2()
    {
      Analyzer analyzer = new Analyzer();
      BasicSynthesis bs = new BasicSynthesis();
      SteadyHand sh = new SteadyHand();
      analyzer.Actions.AddAction(bs);
      //analyzer.Actions.AddAction(new BasicTouch());
      //analyzer.Actions.AddAction(new MastersMend());
      analyzer.Actions.AddAction(sh);
      //analyzer.Actions.AddAction(new Observe());

      State state = Utility.CreateDefaultState();

      // Make sure we don't have enough CP to run Master's Mend.
      state.CP = Compute.CP(SynthAction<SynthActionAttribute, MastersMend>.Attributes.CP, state) - 1;
      state.Durability = 20;

      // Make sure exactly 2 Basic Synthesis' are required to finish the synth.
      state.Progress = 0;
      state.MaxProgress = 2 * Compute.Progress(state, bs.BaseEfficiency);

      analyzer.Run(state);

      // The best sequence is Steady Hand -> Basic Synthesis -> Basic Synthesis
      Simulator.Engine.Ability bestAction = analyzer.OptimalAction(state);
      Assert.AreEqual<Type>(typeof(SteadyHand), bestAction.GetType());

      state = sh.Activate(state, true);
      bestAction = analyzer.OptimalAction(state);
      Assert.AreEqual<Type>(typeof(BasicSynthesis), bestAction.GetType());

      state = bs.Activate(state, true);
      bestAction = analyzer.OptimalAction(state);
      Assert.AreEqual<Type>(typeof(BasicSynthesis), bestAction.GetType());

      state = bs.Activate(state, true);
      Assert.AreEqual(SynthesisStatus.COMPLETED, state.Status);
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

      analyzer.Run(initialState);

      Simulator.Engine.Ability bestAction = analyzer.OptimalAction(initialState);
      Assert.AreEqual<Type>(typeof(MastersMend), bestAction.GetType());
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

      analyzer.Run(initialState);

      BasicSynthesis basic = new BasicSynthesis();

      initialState = basic.Activate(initialState, true);
      initialState = basic.Activate(initialState, true);
      initialState = basic.Activate(initialState, true);

      Simulator.Engine.Ability bestAction = analyzer.OptimalAction(initialState);
      Assert.AreEqual<Type>(typeof(MastersMend), bestAction.GetType());
    }
  }
}
