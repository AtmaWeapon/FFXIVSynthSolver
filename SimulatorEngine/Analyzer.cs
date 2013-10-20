using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Engine
{
  public class Analyzer
  {
    private const float kExcellentProbability = .01f;
    private const float kGoodProbability = .25f;
    private const float kNormalProbability = .74f;

    private int maxAnalysisDepth = 6;
    private int numSlowSolved = 0;
    private int numQuickSolved = 0;
    private int numLeafSolved = 0;
    private int numDepthLimit = 0;
    private ActionDatabase actions = new ActionDatabase();
    private Dictionary<State, SolvedScoreNode> solvedStates;

    private uint baseStep = 1;

    public class SolvedScoreNode
    {
      public double score;
      public State state;
      public Action bestAction;
    }

    private class RandomOutcome
    {
      public RandomOutcome(double probability, State state, Condition condition, bool success)
      {
        this.probability = probability;
        this.state = state;
        this.condition = condition;
        this.success = success;
      }

      public double probability;
      public State state;
      public Condition condition;
      public bool success;
    }

    public int MaxAnalysisDepth
    {
      get { return maxAnalysisDepth; }
      set { maxAnalysisDepth = value; }
    }

    public ActionDatabase Actions { get { return actions; } }
    public int NumSlowSolved { get { return numSlowSolved; } }
    public int NumQuickSolved { get { return numQuickSolved; } }
    public int NumLeafSolved { get { return numLeafSolved; } }
    public int NumStatesExamined
    {
      get
      {
        return NumSlowSolved + NumQuickSolved + NumLeafSolved;
      }
    }

    public Analyzer()
    {
      solvedStates = new Dictionary<State, SolvedScoreNode>();
    }

    public Action OptimalAction(State state)
    {
      SolvedScoreNode node = solvedStates[state];
      return node.bestAction;
    }

    public bool TryGetOptimalAction(State state, out Action action)
    {
      SolvedScoreNode node;
      action = null;
      if (!solvedStates.TryGetValue(state, out node))
        return false;
      action = node.bestAction;
      return true;
    }

    public void Run(State state)
    {
      // The base step allows us to resume a deep calculation from the middle of the tree, 
      // and ensure we can still travel an *additional* MaxAnalysisDepth nodes deep from
      // an arbitrary starting point.
      this.baseStep = state.Step;

      numSlowSolved = numLeafSolved = numQuickSolved = 0;

      SolvedScoreNode solvedScore = null;
      if (solvedStates.TryGetValue(state, out solvedScore))
      {
        ++numQuickSolved;
        return;
      }

      SimulateAllLegalActions(state);
    }

    private SolvedScoreNode SimulateAllLegalActions(State state)
    {
      SolvedScoreNode solved = new SolvedScoreNode();
      solved.bestAction = null;
      solved.score = 0.0;
      solved.state = state;
      foreach (Action action in actions)
      {
        if (!action.CanUse(state))
          continue;

        double score = SimulateActionUsed(state, action);
        if (score > solved.score)
        {
          solved.score = score;
          solved.bestAction = action;
        }
      }

      solvedStates[state] = solved;
      return solved;
    }

    private double SimulateActionUsed(State state, Action action)
    {
      double stateScore = 0.0;
      List<RandomOutcome> outcomes = GenerateRandomOutcomes(state, action);
      foreach (RandomOutcome outcome in outcomes)
      {
        SolvedScoreNode solvedScore;
        if (solvedStates.TryGetValue(outcome.state, out solvedScore))
        {
          stateScore += outcome.probability * solvedScore.score;
          ++numQuickSolved;
        }
        else if ((outcome.state.Step - baseStep + 1) <= MaxAnalysisDepth)
        {
          if (outcome.state.Status == SynthesisStatus.IN_PROGRESS)
          {
            solvedScore = SimulateAllLegalActions(outcome.state);
            stateScore += outcome.probability * solvedScore.score;
            ++numSlowSolved;
          }
          else
          {
            stateScore += outcome.probability * outcome.state.Score;
            ++numLeafSolved;
          }
        }
        else
        {
          stateScore += outcome.probability * outcome.state.Score;
          ++numDepthLimit;
        }
      }

      return stateScore;
    }
    private void PrintTransition(State s)
    {
      Action leadingAction = s.LeadingAction;
      Debug.WriteLine("CP: {0}/{1}, Dura: {2}/{3}, Progress: {4}/{5}, Quality: {6}/{7} ==> {8}",
                        s.CP, s.MaxCP, s.Durability, s.MaxDurability, s.Progress, s.MaxProgress,
                        s.Quality, s.MaxQuality, leadingAction.Attributes.Name);
    }

    private List<RandomOutcome> GenerateRandomOutcomes(State initialState, Action action)
    {
      double successProbability = Compute.SuccessRate(action.Attributes.SuccessRate, initialState);
      State successState = action.SuccessState(initialState);
      State failureState = action.FailureState(initialState);

      List<RandomOutcome> entries = new List<RandomOutcome>();
      switch (initialState.Condition)
      {
        case Condition.Excellent:
          // Excellent can only go to poor.

          // Success
          if (successState != null)
            entries.Add(new RandomOutcome(successProbability, successState, Condition.Poor, true));

          // Failure
          if (failureState != null)
            entries.Add(new RandomOutcome(1.0f - successProbability, failureState, Condition.Poor, false));
          break;
        case Condition.Good:
        case Condition.Poor:
          // Good and poor can both only go to normal.

          // Success
          if (successState != null)
            entries.Add(new RandomOutcome(successProbability, successState, Condition.Normal, true));

          // Failure
          if (failureState != null)
            entries.Add(new RandomOutcome(1.0f - successProbability, failureState, Condition.Normal, false));
          break;
        case Condition.Normal:
          // Normal can go to normal, good, or excellent.

          // Success
          if (successState != null)
          {
            entries.Add(new RandomOutcome(kNormalProbability * successProbability, successState, Condition.Normal, true));
            entries.Add(new RandomOutcome(kGoodProbability * successProbability, successState, Condition.Good, true));
            entries.Add(new RandomOutcome(kExcellentProbability * successProbability, successState, Condition.Excellent, true));
          }

          // Failure
          if (failureState != null)
          {
            entries.Add(new RandomOutcome(kNormalProbability * (1.0f - successProbability), failureState, Condition.Normal, false));
            entries.Add(new RandomOutcome(kGoodProbability * (1.0f - successProbability), failureState, Condition.Good, false));
            entries.Add(new RandomOutcome(kExcellentProbability * (1.0f - successProbability), failureState, Condition.Excellent, false));
          }
          break;
      }
      return entries;
    }
  }
}
