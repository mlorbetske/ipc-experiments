.assembly extern mscorlib
{
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 )                         // .z\V.4..
  .ver 4:0:0:0
}

.assembly extern System.Core
{
  .publickeytoken = (B0 3F 5F 7F 11 D5 0A 3A )                         // .?_....:
  .ver 4:1:0:0
}

.assembly extern Microsoft.SharedMemIpc.Helper
{
  .ver 1:0:0:0
}

.assembly Microsoft.SharedMemIpc.Helper
{
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilationRelaxationsAttribute::.ctor(int32) = ( 01 00 08 00 00 00 00 00 ) 
  .custom instance void [mscorlib]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() = ( 01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
                                                                                                             63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01 )       // ceptionThrows.
  .hash algorithm 0x00008004
  .ver 1:0:0:0
}

.module Microsoft.SharedMemIpc.Helper.dll

.imagebase 0x00400000
.file alignment 0x00000200
.stackreserve 0x00100000
.subsystem 0x0003       // WINDOWS_CUI
.corflags 0x00000001    //  ILONLY

.class public explicit ansi beforefieldinit Microsoft.SharedMemIpc.Helper.Memory
       extends [mscorlib]System.Object
{
	.method public static !!T* Link<valuetype .ctor ([System.Runtime]System.ValueType) T>(native int src) cil managed
	{
		.maxstack 1
		.custom instance void [mscorlib]System.Runtime.CompilerServices.MethodImplAttribute::.ctor(valuetype [mscorlib]System.Runtime.CompilerServices.MethodImplOptions) = (00 01 00 00)
		.custom instance void [System.Core]System.Runtime.CompilerServices.ExtensionAttribute::.ctor()
		ldarg.0
		ret
	}
}

.class public explicit ansi beforefieldinit Microsoft.SharedMemoryIpc.Helper.Ipc
		extends [mscorlib]System.Object
{
	.field private string _name
	
	.method public hidebysig specialname rtspecialname 
			instance void  .ctor(string name) cil managed
	{
		.maxstack  8
		ldarg.0
		call       instance void [mscorlib]System.Object::.ctor()
		ldarg.0
		ldarg.1
		stfld      string Microsoft.SharedMemoryIpc.Helper.Ipc::_name
		ret
	}

	.method public static Microsoft.SharedMemoryIpc.Helper.Ipc Connect([mscorlib]System.String name)
	{
		.maxstack  1
		ldarg.0
		newobj     instance void ConsoleApplication1.Ipc::.ctor(string)
		ret
	}
	
	.method public hidebysig instance !!T* Object<!T>([mscorlib]System.String name)
	{
	    .maxstack  3
		.locals init (
			[0] [mscorlib]System.String,
			[1] int32,
			[2] [System.Core]System.IO.MemoryMappedFiles.MemoryMappedFile,
            [3] class [System.Core]Microsoft.Win32.SafeHandles.SafeMemoryMappedViewHandle,
            [4] uint8*,
            [5] native int,
            [6] bool
		)
		ldstr      "{0}:obj:{1}"
		ldarg.0
		ldfld      string Microsoft.SharedMemoryIpc.Helper.Ipc::_name
		ldarg.1
		call       string [mscorlib]System.String::Format(string, object, object)
		stloc.0

		sizeof	!!T
		stloc.1

		.try
		{
			ldc.i4.1
			stloc.s		6	//Set owner mode

			ldloc.0
			ldloc.1
			ldc.i4.0
			call		class [System.Core]System.IO.MemoryMappedFiles.MemoryMappedFile [System.Core]System.IO.MemoryMappedFiles.MemoryMappedFile::CreateNew(string, int64, valuetype [System.Core]System.IO.MemoryMappedFiles.MemoryMappedFileAccess)
			leave.s
		}  // end .try
		catch [mscorlib]System.Object 
		{			
			ldc.i4.0
			stloc.s		6	//Set client mode

			pop
			ldloc.0
			ldc.i4.6
			call		class [System.Core]System.IO.MemoryMappedFiles.MemoryMappedFile [System.Core]System.IO.MemoryMappedFiles.MemoryMappedFile::OpenExisting(string, valuetype [System.Core]System.IO.MemoryMappedFiles.MemoryMappedFileRights)
			leave.s		AFTER_TRY
		} 
AFTER_TRY:	
		//Setup the view
		callvirt	instance class [System.Core]System.IO.MemoryMappedFiles.MemoryMappedViewAccessor [System.Core]System.IO.MemoryMappedFiles.MemoryMappedFile::CreateViewAccessor()
		dup
		//Get the view handle
		callvirt	instance class [System.Core]Microsoft.Win32.SafeHandles.SafeMemoryMappedViewHandle [System.Core]System.IO.MemoryMappedFiles.MemoryMappedViewAccessor::get_SafeMemoryMappedViewHandle()
		dup
		stloc.3
		ldloc.1
		conv.i8
		callvirt	instance void [mscorlib]System.Runtime.InteropServices.SafeBuffer::Initialize(uint64)
		//Get the pointer for the start of the region
		ldc.i4.0
		conv.u
		stloc.s		4
		ldloc.3
		ldloca.s	4
		callvirt	instance void [mscorlib]System.Runtime.InteropServices.SafeBuffer::AcquirePointer(uint8*&)
		//Get the pointer back
		ldloc.s		4
		call		instance void [mscorlib]System.IntPtr::.ctor(void*)
		stloc.s		5
		ldloc.s		6
		brfalse		SKIP_ZERO
		ldloc.s		5
		call		void class ConsoleApplication1.Ipc`1<!T>::Zero(native int)
SKIP_ZERO:	
		ldloc.s		5
		ldloc.s		6
		ldloc.2
		newobj		instance void class ConsoleApplication1.Ipc`1<!T>::.ctor(native int, bool, class [System.Core]System.IO.MemoryMappedFiles.MemoryMappedFile)
		ret
	}
}