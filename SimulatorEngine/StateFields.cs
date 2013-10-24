using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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

      RegisterField(KnownStateField.Craftsmanship, 1000U);
      RegisterField(KnownStateField.Control, 1000U);
      RegisterField(KnownStateField.CP, 500U);
      RegisterField(KnownStateField.MaxCP, 500U);
      RegisterField(KnownStateField.Durability, 100U);
      RegisterField(KnownStateField.MaxDurability, 100U);
      RegisterField(KnownStateField.Progress, 150U);
      RegisterField(KnownStateField.MaxProgress, 150U);
      RegisterField(KnownStateField.SynthLevel, 50U);
      RegisterField(KnownStateField.CrafterLevel, 50U);
      RegisterField(KnownStateField.Quality, 2000U);
      RegisterField(KnownStateField.MaxQuality, 2000U);
      RegisterField(KnownStateField.Condition, 4U);

      Assembly thisAssembly = Assembly.GetExecutingAssembly();
      foreach (Type info in thisAssembly.GetTypes())
      {
        TemporaryEnhancementAttribute attr = SynthAction<TemporaryEnhancementAttribute>.Attributes(info);
        if (attr != null)
        {
          SynthActionAttribute baseattr = SynthAction<SynthActionAttribute>.Attributes(info);
          // A duration of 0 means its a permanent enhancement.  For those we need to track
          // the total number of turns that the effect has been active.  We assume no synth
          // can go longer than 31 steps, so we use 31 as the max value.
          if (attr.Duration == 0)
            RegisterField(baseattr.ActionId, 31);
          else
            RegisterField(baseattr.ActionId, attr.Duration);
        }
      }
    }

    public static void RegisterField(object key, uint maxSupportedValue)
    {
      uint numBits = Compute.NumBitsRequired(maxSupportedValue);
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

      int rbyte = (int)(info.offset + info.count - 1) / 8;
      int rbit = (int)(info.offset + info.count - 1) % 8;
      Debug.Assert(rbyte - lbyte <= 1);

      if (rbyte == lbyte)
      {
        ModifyByte(ref storage, lbyte, lbit, rbit, (byte)value);
      }
      else
      {
        ModifyShort(ref storage, lbyte, lbit, 8 + rbit, (ushort)value);
      }
    }

    public static uint GetValue(ref StateStorage storage, object key)
    {
      FieldInfo info = fields[key];
      int lbyte = (int)info.offset / 8;
      int lbit = (int)info.offset % 8;

      int rbyte = (int)(info.offset + info.count - 1) / 8;
      int rbit = (int)(info.offset + info.count - 1) % 8;
      Debug.Assert(rbyte - lbyte <= 1);

      unsafe
      {
        int count = (int)info.count;
        fixed (byte* ptr = storage.storage)
        {
          if (lbyte == rbyte)
          {
            byte* byteptr = ptr + lbyte;
            byte mask = (byte)(((1 << count) - 1) << (int)(7 - rbit));
            byte result = (byte)(*byteptr & mask);
            return (uint)(result >> (7 - rbit));
          }
          else
          {
            ushort* shortptr = (ushort*)(ptr + lbyte);
            rbit += 8;

            ushort mask = (ushort)(((1 << count) - 1) << (int)(15 - rbit));
            ushort result = (ushort)(*shortptr & mask);
            return (uint)(result >> (15 - rbit));
          }
        }
      }
    }

    private static void ModifyShort(ref StateStorage storage, int byteoff, int lbit, int rbit, ushort value)
    {
      unsafe
      {
        fixed (byte* ptr = storage.storage)
        {
          byte* modbyte = ptr + byteoff;
          ushort* modshort = (ushort*)modbyte;

          int numbits = rbit - lbit + 1;
          ushort mask1 = (ushort)(((1 << numbits) - 1) << (int)(15 - rbit));   // 0000000000011111000000
          ushort mask2 = (ushort)~mask1;                                       // 1111111111100000111111

          // Clear the old value
          *modshort &= mask2;

          ushort assignmentMask = (ushort)((ushort)value << (int)(15 - rbit));
          // Or in the new value
          *modshort |= assignmentMask;
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
          byte mask1 = (byte)(((1 << numbits) - 1) << (int)(7 - rbit));   // 0000000000011111000000
          byte mask2 = (byte)~mask1;                                      // 1111111111100000111111

          // Clear the old value
          *modbyte &= mask2;

          byte assignmentMask = (byte)((byte)value << (int)(7 - rbit));
          // Or in the new value
          *modbyte |= assignmentMask;
        }

      }
    }
  }
}
