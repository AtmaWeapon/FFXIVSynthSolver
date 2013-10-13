using Simulator.Engine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Simulator
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private Analyzer analyzer;
    private Stopwatch stopwatch;
    private BackgroundWorker worker;

    public MainWindow()
    {
      analyzer = new Analyzer();
      stopwatch = new Stopwatch();
      worker = new BackgroundWorker();

      InitializeComponent();
    }

    private void Window_Initialized(object sender, EventArgs e)
    {
      analyzer.Actions.AddAction(new BasicSynthesis());
      analyzer.Actions.AddAction(new BasicTouch());
      analyzer.Actions.AddAction(new MastersMend());
      analyzer.Actions.AddAction(new SteadyHand());
      analyzer.Actions.AddAction(new Observe());
      analyzer.Actions.AddAction(new Simulator.Engine.Manipulation());
      analyzer.Actions.AddAction(new TricksOfTheTrade());
      analyzer.Actions.AddAction(new StandardTouch());

      analyzer.MaxAnalysisDepth = 4;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      txtStatusLog.Clear();
      State initialState = new State();
      initialState.Condition = Simulator.Engine.Condition.Normal;
      initialState.Control = 119;
      initialState.Craftsmanship = 131;
      initialState.CP = 254;
      initialState.MaxCP = 254;
      initialState.MaxDurability = 70;
      initialState.Durability = 70;
      initialState.MaxProgress = 74;
      initialState.Quality = 284;
      initialState.MaxQuality = 1053;
      initialState.SynthLevel = 20;
      initialState.CrafterLevel = 19;

      UserDecisionNode root = new UserDecisionNode();
      root.originalState = initialState;

      worker.DoWork += worker_StartAnalysis;
      worker.RunWorkerCompleted += worker_AnalysisComplete;
      worker.RunWorkerAsync(root);
    }

    void worker_StartAnalysis(object sender, DoWorkEventArgs e)
    {
 	    UserDecisionNode root = (UserDecisionNode)e.Argument;
      stopwatch.Reset();
      stopwatch.Start();
      analyzer.Run(root);
      e.Result = root;
    }
    
    void worker_AnalysisComplete(object sender, RunWorkerCompletedEventArgs e)
    {
      stopwatch.Stop();
      txtStatusLog.AppendText(String.Format("Solved {0} states in {1} seconds.", analyzer.NumStatesExamined, stopwatch.Elapsed.TotalSeconds));
      UserDecisionNode root = (UserDecisionNode)e.Result;

      while (root.originalState.Status == SynthesisStatus.IN_PROGRESS)
      {
        State state = root.originalState;
        if (!root.IsSolved)
        {
          worker.RunWorkerAsync(root);
          return;
        }

        PreRandomDecisionNode optimalAction = root.OptimalAction;

        txtStatusLog.AppendText(String.Format("Condition={9}, Progress {0}/{1}, Quality={2}/{3}, CP={4}/{5}, Dura={6}/{7}.  Best Action = {8}\n",
                                state.Progress, state.MaxProgress, state.Quality, state.MaxQuality,
                                state.CP, state.MaxCP, state.Durability, state.MaxDurability,
                                optimalAction.originatingAction.Attributes.Name, state.Condition));


        RandomOutcomeSelector selector = new RandomOutcomeSelector(state, optimalAction.originatingAction);
        selector.ShowDialog();

        string statusString = (selector.ResultSuccess) ? "Success" : "Failure";
        string conditionString = selector.ResultCondition.ConditionString();
        txtStatusLog.AppendText(String.Format("{0}!  New condition = {1}.\n", statusString, conditionString));
        root = optimalAction.FindMatchingOutcome(selector.ResultSuccess, selector.ResultCondition);
        if (root == null)
          return;
      }
    }
  }
}
