![ScreenShot](https://raw.github.com/AtmaWeapon/FFXIVSynthSolver/master/screenshot.JPG)

FFXIVSynthSolver is a utility for FFXIV that uses a heuristic decision tree to
attempt to find the optimal sequence of actions for crafting items.

Disclaimers: 

1) This is very very very early code, not even alpha quality, so I expect there
   to be basic errors in the analysis process.
2) I am new to Github.  I also expect to make stupid mistakes while learning my
   way around here.
3) I'm using VS2013, and the solution file is VS2013.  Because of that, you will
   also probably want to use VS2013 if you wish to contribute.  The express
   edition is free, you can download it from Microsoft.

FAQ:

Q: Why did you use C#? 
A: I'm very familiar in C++ and other languages, which some may argue are more
   appropriate for a tool such as this, since large search spaces can take quite
   a while to solve.  I chose C# for a number of reasons.   
   i) It's easy to program in, so it lowers the barrier of entry for people wishing
      to contribute.
   ii) It provides easy support for unit testing.  A project like this lends itself
      especially well to unit testing, due to the algorithmic nature of the problem
      and the wide variety of different skills and actions that can be combined.
   iii) It's not *that* slow.  On my machine I can still solve a few million states
      in under 30 seconds, so it's really not gimping me that much.
   iv) I plan to add a nice Windowsy UI soon and many other features for which C++
      will by no means be impossible, but will definitely mean a bunch of painful
      ugly UI code which is unnecessarily complicated, but which is for the most
      part trivialized by C#.
      
Q: I want to contribute, what do I do?
A: Clone the repo, make some changes, and submit a patch.  I will not accept any
   patches that break any of the unit tests, so make sure all the unit tests pass.
   Adding additional unit tests is also very welcome.
   
Q: What kind of changes would be the most welcome?
A: * Correctness fixes (i.e. am I doing something blatantly wrong?)
   * Performance fixes (given the same input state, make it solve the synth faster.
   * Unit test improvements.
   * More abilities, currently I don't actually support every ability in the game.
   * Code cleanup / refactor (note that it's very easy to break the unit tests, so
     be careful).

Q: What do you have planned for future releases?
A: * Binary release, for people who don't want to / don't know how to compile the code.
   * GUI
   * Built-in recipe database so you don't have to hardcode the parameters of an item.
   * Class profiles, so you can configure your stats for individual crafts, so those don't
     have to be hardcoded either.
   * More user-friendly playback code path.
     
Q: OMG it's so slow, what do I do?
A: The easiest way to increase performance is to decrease the analysis depth.  You can do
   this by setting the MaxAnalysisDepth property on the Analyzer object during program
   initialization.  This comes at a slight accracy cost, as doing so will cause the analyzer
   to ignore branches after MaxAnalysisDepth until such time that your current # of steps
   reaches that point.  Example: Suppose you set MaxAnalysisDepth to 5 so that it completes
   quickly.  During the initial analysis it will not see past the 5th action in any sequence
   it examines.  If playback of the sequence takes more than 5 actions, then at the 5th action
   it will re-start the analysis process from the current state and continue from there.  This
   is less accurate than if it could have considered deeper branches from all the paths that you
   didn't choose, but significantly faster.
   
Q: What's a good value for MaxAnalysisDepth?
A: On my computer, anything under 10 completes almost instantly.  However it varies wildly depending
   on your stats (the decision tree of a user with 800 CP will be huge, for example, compared to that
   of a person with 200 CP) and the number of abilities that are supported by the analyzer.