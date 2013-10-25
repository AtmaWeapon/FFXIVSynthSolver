using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
    private StreamWriter logWriter;
    private uint indentLevel = 0;
    private uint lineNumber = 0;

    private uint baseStep = 1;

    public class SolvedScoreNode
    {
      public double score;
      public State state;
      public Ability bestAction;
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

    public string LogFile
    {
      set
      {
        if (logWriter != null)
          logWriter.Close();
        logWriter = File.CreateText(value);
      }
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

    public Ability OptimalAction(State state)
    {
      SolvedScoreNode node = solvedStates[state];
      return node.bestAction;
    }

    public bool TryGetOptimalAction(State state, out Ability action)
    {
      SolvedScoreNode node;
      action = null;
      if (!solvedStates.TryGetValue(state, out node))
        return false;
      action = node.bestAction;
      return true;
    }

    private void LogIndents()
    {
      if (logWriter == null)
        return;

      for (int i = 0; i < indentLevel; ++i)
        logWriter.Write("  ");
    }

    private void LogState(State s)
    {
      if (logWriter == null)
        return;

      ++lineNumber;
      LogIndents();
      logWriter.WriteLine("CP: {0}/{1}, Dura: {2}/{3}, Progress: {4}/{5}, Quality: {6}/{7}",
                          s.CP, s.MaxCP, s.Durability, s.MaxDurability, s.Progress, s.MaxProgress,
                          s.Quality, s.MaxQuality);
    }

    private void LogOutcome(Engine.Ability action, RandomOutcome outcome)
    {
      if (logWriter == null)
        return;

      ++lineNumber;
      LogIndents();
      logWriter.WriteLine("{0} -> {1}.  Probability = {2}", action.Name, outcome.success ? "Success!" : "Failure!", outcome.probability);
    }

    private void LogScore(double score)
    {
      if (logWriter == null)
        return;

      ++lineNumber;
      LogIndents();
      logWriter.WriteLine("Final Score: {0}", score);
    }

    private void LogQuickSolve(SolvedScoreNode node)
    {
      if (logWriter == null)
        return;

      ++lineNumber;
      LogIndents();
      if (node.bestAction == null)
        logWriter.WriteLine("Node slow solved!  Score={0}, bestAction=None!", node.score);
      else
        logWriter.WriteLine("Node slow solved!  Score={0}, bestAction={1}", node.score, node.bestAction.Name);
    }

    private void LogSlowSolve(SolvedScoreNode node)
    {
      if (logWriter == null)
        return;

      ++lineNumber;
      LogIndents();
      if (node.bestAction == null)
        logWriter.WriteLine("Node slow solved!  Score={0}, bestAction=None!", node.score);
      else
        logWriter.WriteLine("Node slow solved!  Score={0}, bestAction={1}", node.score, node.bestAction.Name);
    }

    private void LogLeafSolve(double score)
    {
      if (logWriter == null)
        return;

      ++lineNumber;
      LogIndents();
      logWriter.WriteLine("Node leaf solved!  Score={0}", score);
    }

    private void LogDepthLimit(double score)
    {
      if (logWriter == null)
        return;

      ++lineNumber;
      LogIndents();
      logWriter.WriteLine("Depth limit reached.  Computed score={0}", score);
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
      LogState(state);
      SolvedScoreNode solved = new SolvedScoreNode();
      solved.bestAction = null;
      solved.score = 0.0;
      solved.state = state;
      foreach (Ability action in actions)
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

    private double SimulateActionUsed(State state, Ability action)
    {
      ++indentLevel;
      double stateScore = 0.0;
      List<RandomOutcome> outcomes = GenerateRandomOutcomes(state, action);
      foreach (RandomOutcome outcome in outcomes)
      {
        LogOutcome(action, outcome);
        ++indentLevel;
        SolvedScoreNode solvedScore;
        if (solvedStates.TryGetValue(outcome.state, out solvedScore))
        {
          LogQuickSolve(solvedScore);
          stateScore += outcome.probability * solvedScore.score;
          ++numQuickSolved;
        }
        else if ((outcome.state.Step - baseStep + 1) <= MaxAnalysisDepth)
        {
          if (outcome.state.Status == SynthesisStatus.IN_PROGRESS)
          {
            solvedScore = SimulateAllLegalActions(outcome.state);
            LogSlowSolve(solvedScore);
            stateScore += outcome.probability * solvedScore.score;
            ++numSlowSolved;
          }
          else
          {
            stateScore += outcome.probability * outcome.state.Score;
            LogLeafSolve(outcome.state.Score);
            ++numLeafSolved;
          }
        }
        else
        {
          LogDepthLimit(outcome.state.Score);
          stateScore += outcome.probability * outcome.state.Score;
          ++numDepthLimit;
        }
        --indentLevel;
      }

      LogScore(stateScore);
      --indentLevel;
      return stateScore;
    }
    private void PrintTransition(State s)
    {
      Ability leadingAction = s.LeadingAction;
      Debug.WriteLine("CP: {0}/{1}, Dura: {2}/{3}, Progress: {4}/{5}, Quality: {6}/{7} ==> {8}",
                        s.CP, s.MaxCP, s.Durability, s.MaxDurability, s.Progress, s.MaxProgress,
                        s.Quality, s.MaxQuality, leadingAction.Name);
    }

    private List<RandomOutcome> GenerateRandomOutcomes(State initialState, Ability action)
    {
      double successProbability = Compute.SuccessRate(action.BaseSuccessRate, initialState);
      State successState = action.Activate(initialState, true);
      State failureState = action.Activate(initialState, false);

      List<RandomOutcome> entries = new List<RandomOutcome>();
      entries.Add(new RandomOutcome(successProbability, successState, Condition.Normal, true));
      if (failureState != null)
        entries.Add(new RandomOutcome(1.0 - successProbability, failureState, Condition.Normal, false));
      return entries;
    }
  }
}
