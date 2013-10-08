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
    private ActionDatabase actions = new ActionDatabase();
    private Dictionary<State, SolvedScoreNode> solvedStates;

    public int MaxAnalysisDepth
    {
      get { return maxAnalysisDepth; }
      set { maxAnalysisDepth = value; }
    }
    public int NumSolvedStates { get { return solvedStates.Count; } }
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

    class ScoreHashComparer : IEqualityComparer<State>
    {
      public bool Equals(State x, State y)
      {
        return x.Equals(y);
      }

      public int GetHashCode(State obj)
      {
        return obj.HashCode;
      }
    }

    public Analyzer()
    {
      solvedStates = new Dictionary<State, SolvedScoreNode>(new ScoreHashComparer());
    }

    public void Run(UserDecisionNode root)
    {
      foreach (Action action in actions)
      {
        if (!action.CanUse(root.originalState))
          continue;

        PreRandomDecisionNode outcome = new PreRandomDecisionNode();
        outcome.originatingAction = action;
        outcome.originalState = root.originalState;
        root.choices.Add(outcome);
      }

      ExpandInteractions(root);
    }

    private void ExpandInteractions(UserDecisionNode interaction)
    {
      //Debug.Assert(!solvedStates.ContainsKey(interaction.currentState));

      foreach (PreRandomDecisionNode choice in interaction.choices)
      {
        // This represents a user choice, so the probability hasn't changed yet.
        ExpandRandomOutcomes(choice);
      }

      // After expanding all the child user interactions, this node can be solved by
      // choosing the best child action.
      PreRandomDecisionNode optimal = interaction.OptimalAction;
      interaction.Solve(optimal.Score);
      SolvedScoreNode scoreNode = new SolvedScoreNode(optimal.Score, interaction);
      solvedStates.Add((State)interaction.originalState.Clone(), scoreNode);
      ++numSlowSolved;
    }

    private bool TryQuickSolve(PostRandomDecisionNode node)
    {
      // If this state was already solved, just set its score and its follow-up interaction
      // tree to the followup interaction tree of the previously solved state.
      SolvedScoreNode solvedScore = null;
      if (!solvedStates.TryGetValue(node.newState, out solvedScore))
        return false;

      node.Solve(solvedScore.score);
      node.interaction = solvedScore.originalNode;
      ++numQuickSolved;
      return true;
    }

    private void ExpandRandomOutcomes(PreRandomDecisionNode preDecisionNode)
    {
      State stateBeforeOutcome = preDecisionNode.originalState;
      Action action = preDecisionNode.originatingAction;

      preDecisionNode.outcomes = ExpandRandomOutcomes(action, stateBeforeOutcome);

      double finalScore = 0.0f;
      foreach (PostRandomDecisionNode outcome in preDecisionNode.outcomes)
      {
        Debug.Assert(outcome.newState != stateBeforeOutcome);

        if (outcome.newState.Status == SynthesisStatus.IN_PROGRESS)
        {
          // If this node has been solved before, just get the previously calcuated
          // score.
          if (!TryQuickSolve(outcome))
          {
            // This is a new state that we've never seen before.  Generate the list of
            // user interactions that are valid for the current state, and attach a
            // corresponding UserDecisionNode.
            outcome.interaction = new UserDecisionNode();
            outcome.interaction.originalState = outcome.newState;
            foreach (Action newAction in actions)
            {
              if (!newAction.CanUse(outcome.newState))
                continue;

              PreRandomDecisionNode nextTransition = new PreRandomDecisionNode();
              nextTransition.originatingAction = newAction;
              nextTransition.originalState = outcome.interaction.originalState;

              outcome.interaction.choices.Add(nextTransition);
            }

            // If there are no valid actions, then we shouldn't be here because in
            // that case it should be a busted or completed synth, which would have
            // been handled earlier.
            Debug.Assert(outcome.interaction.choices.Count > 0);

            if (outcome.newState.TransitionSequence.Count < MaxAnalysisDepth)
            {
              ExpandInteractions(outcome.interaction);

              // The child interaction should automatically be solved when its recursive
              // descent is complete.
              Debug.Assert(outcome.interaction.IsSolved);
              outcome.Solve(outcome.interaction.Score);
            }
          }
        }
        else
        {
          // Completed and busted synths don't need to go into the hash table, because
          // they are leafs that have no follow-up states. So leave them in the tree
          // as-is but mark them solved.  This saves expensive hash table lookups (to
          // see if they're already solved) as well as keeps the hash table smaller
          // so that important lookups will be faster.
          outcome.Solve(outcome.newState.Score);
          ++numLeafSolved;
        }

        // The outcome may not be solved if we were hit with the max depth limit.  In that case
        // Don't factor this term into the final score.  (This probably won't cause a huge effect
        // anyway, since this far down it will hardly effect the probability.
        if (outcome.IsSolved)
          finalScore += outcome.probability * outcome.Score;
        else
          finalScore += outcome.probability * outcome.newState.ScoreEstimate;
      }

      preDecisionNode.Solve(finalScore);
    }

    private void PlaybackSequence(State state)
    {
      foreach (State.Transition transition in state.TransitionSequence)
        PrintTransition(transition);
    }

    private void PrintTransition(State.Transition transition)
    {
      State s = transition.previousState;
      Debug.WriteLine("CP: {0}/{1}, Dura: {2}/{3}, Progress: {4}/{5}, Quality: {6}/{7} ==> {8}",
                        s.CP, s.MaxCP, s.Durability, s.MaxDurability, s.Progress, s.MaxProgress,
                        s.Quality, s.MaxQuality, transition.action.Attributes.Name);
    }

    private List<PostRandomDecisionNode> ExpandRandomOutcomes(Action action, State initialState)
    {
      float successProbability = Compute.SuccessRate(action.Attributes.SuccessRate, initialState);
      State successState = action.SuccessState(initialState);
      State failureState = action.FailureState(initialState);

      List<PostRandomDecisionNode> entries = new List<PostRandomDecisionNode>();
      switch (initialState.Condition)
      {
        case Condition.Excellent:
          // Excellent can only go to poor.

          // Success
          if (successState != null)
            entries.Add(new PostRandomDecisionNode(successProbability, successState, Condition.Poor, true));

          // Failure
          if (failureState != null)
            entries.Add(new PostRandomDecisionNode(1.0f - successProbability, failureState, Condition.Poor, false));
          break;
        case Condition.Good:
        case Condition.Poor:
          // Good and poor can both only go to normal.

          // Success
          if (successState != null)
            entries.Add(new PostRandomDecisionNode(successProbability, successState, Condition.Normal, true));

          // Failure
          if (failureState != null)
            entries.Add(new PostRandomDecisionNode(1.0f - successProbability, failureState, Condition.Normal, false));
          break;
        case Condition.Normal:
          // Normal can go to normal, good, or excellent.

          // Success
          if (successState != null)
          {
            entries.Add(new PostRandomDecisionNode(kNormalProbability * successProbability, successState, Condition.Normal, true));
            entries.Add(new PostRandomDecisionNode(kGoodProbability * successProbability, successState, Condition.Good, true));
            entries.Add(new PostRandomDecisionNode(kExcellentProbability * successProbability, successState, Condition.Excellent, true));
          }

          // Failure
          if (failureState != null)
          {
            entries.Add(new PostRandomDecisionNode(kNormalProbability * (1.0f - successProbability), failureState, Condition.Normal, false));
            entries.Add(new PostRandomDecisionNode(kGoodProbability * (1.0f - successProbability), failureState, Condition.Good, false));
            entries.Add(new PostRandomDecisionNode(kExcellentProbability * (1.0f - successProbability), failureState, Condition.Excellent, false));
          }
          break;
      }
      return entries;
    }
  }
}
