using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Engine
{

  public class TreeNode
  {
    private double score;
    private bool isSolved;
    public State originalState;

    public TreeNode()
    {
      this.score = 0.0;
      this.isSolved = false;
      this.originalState = null;
    }

    public void Solve(double score)
    {
      Debug.Assert(!isSolved);
      this.score = score;
      isSolved = true;
    }

    public bool IsSolved { get { return isSolved; } }

    public double Score
    {
      get
      {
        if (!isSolved)
          throw new InvalidOperationException();
        return score;
      }
    }
  }

  public class UserDecisionNode : TreeNode
  {
    public List<PreRandomDecisionNode> choices = new List<PreRandomDecisionNode>();

    public PreRandomDecisionNode OptimalAction
    {
      get
      {
        PreRandomDecisionNode bestNode = null;
        foreach (PreRandomDecisionNode child in choices)
        {
          // All child interactions must be solved before this method can be called.
          if (!child.IsSolved)
            throw new InvalidOperationException();

          if (bestNode == null || child.Score > bestNode.Score)
            bestNode = child;
        }
        return bestNode;
      }
    }

    public IEnumerable<PreRandomDecisionNode> OptimalUserDecisions
    {
      get
      {
        PreRandomDecisionNode optimal = OptimalAction;
        yield return optimal;

        foreach (PostRandomDecisionNode outcome in optimal.outcomes)
        {
          // For each random outcome, get the optimal user decisions.
          UserDecisionNode childNode = outcome.interaction as UserDecisionNode;
          if (childNode != null)
          {
            foreach (PreRandomDecisionNode child in childNode.OptimalUserDecisions)
              yield return child;
          }
        }
      }
    }

    public UserDecisionNode Choose(Type interactionType, bool success, Condition newCondition)
    {
      foreach (PreRandomDecisionNode interaction in choices)
      {
        if (interaction.originatingAction.GetType() != interactionType)
          continue;

        foreach (PostRandomDecisionNode outcome in interaction.outcomes)
        {
          if (outcome.wasSuccess == success && outcome.newState.Condition == newCondition)
            return outcome.interaction;
        }
      }

      return null;
    }
  }

  public class SolvedScoreNode
  {
    public SolvedScoreNode(double score, UserDecisionNode originalNode)
    {
      Debug.Assert(originalNode == null || originalNode.IsSolved);

      this.score = score;
      this.originalNode = originalNode;
    }

    public UserDecisionNode originalNode;
    public double score;
  }

  public class PreRandomDecisionNode : TreeNode
  {
    public Action originatingAction;  // The user action that led to this random decision.

    internal List<PostRandomDecisionNode> outcomes = new List<PostRandomDecisionNode>();

    public UserDecisionNode FindMatchingOutcome(bool success, Condition newCondition)
    {
      PostRandomDecisionNode matchedOutcome = null;
      foreach (PostRandomDecisionNode outcome in outcomes)
      {
        if (outcome.interaction == null)
          continue;
        Condition outcomeCondition = outcome.interaction.originalState.Condition;
        if (outcome.wasSuccess == success && outcomeCondition == newCondition)
          matchedOutcome = outcome;
      }
      if (matchedOutcome == null)
        return null;
      return matchedOutcome.OutcomeInteractionNode;
    }
  }

  public class PostRandomDecisionNode : TreeNode
  {
    public bool wasSuccess;               // Was this node a result of a success or a failure?
    public float probability;             // The probability that this random outcome was chosen.
    public UserDecisionNode interaction;  // The interaction node following this decision's outcome.
    public State newState;

    public UserDecisionNode OutcomeInteractionNode { get { return interaction; } }

    public PostRandomDecisionNode(float probability, State originalState, Condition newCondition, bool wasSuccess)
    {
      this.probability = probability;
      this.wasSuccess = wasSuccess;
      this.newState = new State(originalState, null);
      this.newState.Condition = newCondition;
    }
  }
}
