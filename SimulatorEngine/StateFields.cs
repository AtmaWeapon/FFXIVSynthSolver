using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Engine
{
  public enum KnownStateField : int
  {
    Craftsmanship,
    Control,
    CP,
    MaxCP,
    Durability,
    MaxDurability,
    Progress,
    MaxProgress,
    CrafterLevel,
    SynthLevel,
    Quality,
    MaxQuality,
    Condition
  }

  internal static class StateFields
  {
    private class FieldInfo
    {
      public uint offset;
      public uint count;
    }
    private static Dictionary<object, FieldInfo> fields;
    private static uint bitsUsed = 0;

    static StateFields()
    {
      fields = new Dictionary<object, FieldInfo>();
    }

    public static void RegisterField(object key, uint numBits)
    {
      FieldInfo info = new FieldInfo();
      info.count = numBits;
      info.offset = bitsUsed;
      fields[key] = info;

      bitsUsed += info.count;
    }

    public static void SetValue(ref StateStorage storage, object key, uint value)
    {
      FieldInfo info = fields[key];
      int lbyte = (int)info.offset / 8;
      int lbit = (int)info.offset % 8;

      int rbyte = (int)(info.offset + info.count) / 8;
      int rbit = (int)(info.offset + info.count) % 8;
      Debug.Assert(rbyte - lbyte <= 1);

      if (rbyte == lbyte)
      {
        ModifyByte(ref storage, rbyte, lbit, rbit, (byte)value);
      }
      else
      {
        // _ _ _ _ _ X X X  |  X X _ _ _ _ _ _
        //
        // When writing into the left byte, cut off the last |rbit|
        // bits.  In the above example, this would shift right by 2.
        ModifyByte(ref storage, lbyte, lbit, 7, (byte)(value >> rbit));

        // When writing into the right byte, keep only the last |rbit|
        // bits, but move them all the way over to the left.
        byte mask = (byte)((1 << rbit) - 1);
        byte modvalue = (byte)(((byte)value & mask) << (8 - rbit));
        ModifyByte(ref storage, rbyte, 0, rbit, modvalue);
      }
    }

    public static uint GetValue(ref StateStorage storage, object key)
    {
      FieldInfo info = fields[key];
      int lbyte = (int)info.offset / 8;
      int lbit = (int)info.offset % 8;

      int rbyte = (int)(info.offset + info.count) / 8;
      int rbit = (int)(info.offset + info.count) % 8;
      Debug.Assert(rbyte - lbyte <= 1);

      unsafe
      {
        int count = (int)info.count;
        fixed (byte* ptr = storage.storage)
        {
          if (lbyte == rbyte)
          {
            byte* byteptr = ptr + lbyte;
            byte mask = (byte)(((1 << count) - 1) << rbit);
            byte result = (byte)(*byteptr & mask);
            return (uint)(result >> rbit);
          }
          else
          {
            ushort* shortptr = (ushort*)(ptr + lbyte);
            rbit += 8;

            ushort mask = (ushort)(((1 << count) - 1) << rbit);
            ushort result = (ushort)(*shortptr & mask);
            return (uint)(result >> rbit);
          }
        }
      }
    }


    private static void ModifyByte(ref StateStorage storage, int byteoff, int lbit, int rbit, byte value)
    {
      unsafe
      {
        fixed (byte* ptr = storage.storage)
        {
          byte* modbyte = ptr + byteoff;

          int numbits = rbit - lbit + 1;
          byte mask1 = (byte)(((1 << numbits) - 1) << rbit);  // 0000000000011111000000
          byte mask2 = (byte)~mask1;                          // 1111111111100000111111

          // Clear the old value
          *modbyte &= mask2;

          byte assignmentMask = (byte)((byte)value << (int)lbit);
          // Or in the new value
          *modbyte |= assignmentMask;
        }

      }
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
