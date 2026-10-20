using System;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;

if (args.Length != 2) throw new ArgumentException("usage: Sanitizer <input> <output>");
var module = ModuleDefMD.Load(args[0]);
var asm = module.Assembly;
string newCompany = "مهندس رفیق";
string newCopyright = "Copyright © مهندس رفیق";
foreach (var ca in asm.CustomAttributes)
{
    string typeName = ca.TypeFullName;
    if (typeName.EndsWith("AssemblyCompanyAttribute") && ca.ConstructorArguments.Count == 1)
        ca.ConstructorArguments[0] = new CAArgument(ca.ConstructorArguments[0].Type, new UTF8String(newCompany));
    if (typeName.EndsWith("AssemblyCopyrightAttribute") && ca.ConstructorArguments.Count == 1)
        ca.ConstructorArguments[0] = new CAArgument(ca.ConstructorArguments[0].Type, new UTF8String(newCopyright));
    Console.WriteLine($"CA {typeName}");
}
var winMain = module.Types.First(t => t.FullName == "AutoEtabs.WinMain");
var download = winMain.Methods.First(m => m.Name == "Download");
Console.WriteLine($"Download token={download.MDToken} rva=0x{download.RVA:X} instr={download.Body?.Instructions.Count}");
download.Body = new CilBody();
download.Body.Instructions.Add(OpCodes.Ret.ToInstruction());
download.Body.KeepOldMaxStack = true;
var opts = new ModuleWriterOptions(module);
opts.MetadataOptions.Flags |= MetadataFlags.PreserveRids;
opts.Logger = DummyLogger.NoThrowInstance;
module.Write(args[1], opts);
Console.WriteLine($"WROTE {args[1]}");
