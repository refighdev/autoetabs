using System;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;

static Instruction At(MethodDef method, uint offset)
{
    var ins = method.Body.Instructions.FirstOrDefault(i => i.Offset == offset);
    if (ins == null) throw new InvalidOperationException($"{method.FullName}: no instruction at IL_{offset:X4}");
    return ins;
}
static void Set(MethodDef method, uint offset, OpCode op, object operand = null)
{
    var ins = At(method, offset);
    Console.WriteLine($"{method.Name} IL_{offset:X4}: {ins.OpCode} {ins.Operand} -> {op} {operand}");
    ins.OpCode = op;
    ins.Operand = operand;
}

if (args.Length != 2) throw new ArgumentException("usage: Patcher <input> <output>");
var module = ModuleDefMD.Load(args[0]);
var winMain = module.Types.FirstOrDefault(t => t.FullName == "AutoEtabs.WinMain")
    ?? throw new InvalidOperationException("AutoEtabs.WinMain not found");
var d3 = winMain.Methods.First(m => m.Name == "D3");
var da = winMain.Methods.First(m => m.Name == "da");
d3.Body.KeepOldMaxStack = true;
da.Body.KeepOldMaxStack = true;

// Full local license defaults in WinMain._points.
Set(d3, 0x000A, OpCodes.Ldc_I4_S, (sbyte)99);          // stories
Set(d3, 0x0013, OpCodes.Ldstr, "مهندس رفیق");            // user name
Set(d3, 0x001B, OpCodes.Ldc_I4_S, (sbyte)99);          // plan length
Set(d3, 0x0024, OpCodes.Ldc_I4_1);                     // foundation type -> size 99 in da tail
Set(d3, 0x002D, OpCodes.Ldc_I4_1);                     // Metreh enabled
Set(d3, 0x0036, OpCodes.Ldc_I4_1);                     // optimizer level -> max 99 in da tail
Set(d3, 0x003F, OpCodes.Ldc_I4_1);                     // Daftarcheh enabled
Set(d3, 0x0048, OpCodes.Ldstr, "9000000000");          // 10-digit user id accepted by EncrypteNum
Set(d3, 0x0050, OpCodes.Ldc_I4_S, (sbyte)78);          // checksum field paired with serial 12345678
Set(d3, 0x006E, OpCodes.Ldc_I4_S, (sbyte)99);          // expiry month counter (month field is 67)
Set(d3, 0x006F, OpCodes.Nop);                          // was ldfld month
Set(d3, 0x007D, OpCodes.Ldc_I4_S, (sbyte)99);          // foundation size
Set(d3, 0x0087, OpCodes.Ldc_I4_0);                     // license status: full
Set(d3, 0x0091, OpCodes.Ldc_I4, 12345678);             // serial / PC id (boxed as UInt32 by next instr)
Set(d3, 0x009B, OpCodes.Ldc_I4_S, (sbyte)99);          // optimizer max

// In da(): keep hasDongle=false, mark hasPC=true, then jump over ARM + PC-file
// authentication to the original post-license UI/limit finalization at IL_10A9.
Set(da, 0x0006, OpCodes.Ldc_I4_1);
Set(da, 0x000C, OpCodes.Br, At(da, 0x10A9));

var opts = new ModuleWriterOptions(module);
opts.MetadataOptions.Flags |= MetadataFlags.PreserveRids;
opts.Logger = DummyLogger.NoThrowInstance;
module.Write(args[1], opts);
Console.WriteLine($"WROTE {args[1]}");
