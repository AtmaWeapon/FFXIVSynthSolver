using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Engine
{
  public static class StateFieldAccess
  {
    public static enum KnownStateField : int
    {
      Craftsmanship,
      Control,
      CP,
      MaxCP,
      Durability,
      Progress,
      MaxProgress,
      SynthLevel,
      Quality,
      MaxQuality,
      Condition,
      External
    }

    private class FieldInfo
    {
      public uint varIndex;
      public uint bitOffset;
      public uint fieldSize;
    }

    private static Dictionary<uint, uint> spaceMap;
    private static FieldInfo[] knownFields;
    private static List<FieldInfo> externalFields;
    private static uint nextExternalField = 0;

    static StateFieldAccess()
    {
      knownFields = new FieldInfo[11];
      externalFields = new List<FieldInfo>();
      spaceMap = new Dictionary<uint, uint>();
    }

    public static void RegisterField(KnownStateField type, uint numBits)
    {
      FieldInfo field = new FieldInfo();
      knownFields[(int)type] = field;
      field.bitOffset = nextBit;
      field.varIndex = (uint)(nextBit % Marshal.SizeOf(typeof(ulong)));
      field.fieldSize = numBits;

      nextBit = field.bitOffset + field.fieldSize;
    }

    public static uint RegisterExternalField(uint numBits)
    {
      FieldInfo info = new FieldInfo();
    }

    public static void SetValue(State state, KnownStateField type, State state, uint value)
    {

    }

    public static uint GetValue(State state, KnownStateField type)
    {
      return 0;
    }

    public static void SetValue(State state, uint offset, uint numBits, uint value)
    {

    }

    public static uint GetValue(State state, uint offset, uint numBits)
    {
      return 0;
    }

    public static int kCraftsmanshipOffset = 0;
    public static int kCraftsmanshipLength = 9;
    public static int kControlOffset = kCraftsmanshipOffset + kCraftsmanshipLength;
    public static int kControlLength = 9;
    public static int kCPOffset = kControlOffset + kControlLength;
    public static int kCPLength = 9;
    public static int kMaxCPOffset = kCPOffset + kCPLength;
    public static int kMaxCPLength = 9;
    public static int kDurabilityOffset = kMaxCPOffset + kMaxCPLength;
    public static int kDurabilityLength = 7;
    public static int kMaxDurabilityOffset = kDurabilityOffset + kDurabilityLength;
    public static int kMaxDurabilityLength = 7;
    public static int kProgressOffset = kMaxDurabilityOffset + kMaxDurabilityLength;
    public static int kProgressLength = 7;
    public static int kMaxProgressOffset = kProgressOffset + kProgressLength;
    public static int kMaxProgressLength = 7;

    /* 
       | steady hand (3 bits) | ingenuity (2 bits) | great strides (2 bits) | manipulation (2 bits) | condition (2 bits) | crafter level (6 bits) | synth level (6 bits) |
     */
    public static int kSynthLevelOffset = 0;
    public static int kSynthLevelLength = 6;
    public static int kQualityOffset = kSynthLevelOffset + kSynthLevelLength;
    public static int kQualityLength = 11;
    public static int kMaxQualityOffset = kQualityOffset + kQualityLength;
    public static int kMaxQualityLength = 11;
    public static int kCrafterLevelOffset = kMaxQualityOffset + kMaxQualityLength;
    public static int kCrafterLevelLength = 6;
    public static int kConditionOffset = kCrafterLevelOffset + kCrafterLevelLength;
    public static int kConditionLength = 2;
    public static int kManipulationOffset = kConditionOffset + kConditionLength;
    public static int kManipulationLength = 3;
    public static int kGreatStridesOffset = kManipulationOffset + kManipulationLength;
    public static int kGreatStridesLength = 3;
    public static int kIngenuityOffset = kGreatStridesOffset + kGreatStridesLength;
    public static int kIngenuityLength = 3;
    public static int kSteadyHandOffset = kIngenuityOffset + kIngenuityLength;
    public static int kSteadyHandLength = 3;
  }
}
