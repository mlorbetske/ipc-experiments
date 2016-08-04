.assembly Ipc.Memory.Components
{
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilationRelaxationsAttribute::.ctor(int32) = ( 01 00 08 00 00 00 00 00 ) 
  .custom instance void [mscorlib]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() = ( 01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
                                                                                                             63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01 )       // ceptionThrows.
  .hash algorithm 0x00008004
  .ver 1:0:0:0
}

.module Ipc.Memory.Components.dll

.imagebase 0x00400000
.file alignment 0x00000200
.stackreserve 0x00100000
.subsystem 0x0003       // WINDOWS_CUI
.corflags 0x00000001    //  ILONLY

//Helper class for doing memory overlays
.class public Ipc.Memory.Components.SharedMemory
{
	//Returns a pointer as a pointer to a generic struct type
	.method public static !!T* StructRef<valuetype .ctor ([mscorlib]System.ValueType) T>(void* src) cil managed
	{
		.maxstack 1
		.custom instance void [mscorlib]System.Runtime.CompilerServices.MethodImplAttribute::.ctor(valuetype [mscorlib]System.Runtime.CompilerServices.MethodImplOptions) = (00 01 00 00)
		.custom instance void [System.Core]System.Runtime.CompilerServices.ExtensionAttribute::.ctor()
		ldarg.0
		ret
	}

	//Returns a pointer as a .NET reference type
	.method public static !!T ClassRef<([mscorlib]System.Object) T>(void* src) cil managed
	{
		.maxstack 1
		.custom instance void [mscorlib]System.Runtime.CompilerServices.MethodImplAttribute::.ctor(valuetype [mscorlib]System.Runtime.CompilerServices.MethodImplOptions) = (00 01 00 00)
		.custom instance void [System.Core]System.Runtime.CompilerServices.ExtensionAttribute::.ctor()
		ldarg.0
		ret
	}
}

//Helper class for transporting T* as a safe object
.class public auto ansi beforefieldinit Ipc.Memory.Components.RefStructWrapper`1<valuetype .ctor ([mscorlib]System.ValueType) T>
       extends [mscorlib]System.Object
       implements [mscorlib]System.IDisposable
{
	.field private initonly !T* _val
	.field private initonly class [System.Core]System.IO.MemoryMappedFiles.MemoryMappedFile _file
	.field private static int64 _size

	.method public static class Ipc.Memory.Components.RefStructWrapper`1<!T> Connect(class [mscorlib]System.String name)
	{
		.locals init (
			[0] class [System.Core]System.IO.MemoryMappedFiles.MemoryMappedFile,
			[1] uint8*	bPtr,
			[2] bool
		)

		sizeof			!T
		conv.i8
		stsfld			int64 class Ipc.Memory.Components.RefStructWrapper`1<!T>::_size
		
		.try
		{
			ldarg.0																		//name
			ldsfld		int64 class Ipc.Memory.Components.RefStructWrapper`1<!T>::_size	//size
			ldc.i4.0																	//0 -- ReadWrite
			//Create new memory mapped file
			call		class [System.Core]System.IO.MemoryMappedFiles.MemoryMappedFile [System.Core]System.IO.MemoryMappedFiles.MemoryMappedFile::CreateNew(string, int64, valuetype [System.Core]System.IO.MemoryMappedFiles.MemoryMappedFileAccess)
			//Store it in the output local
			stloc.0

			//Owner
			ldc.i4.1
			stloc.2
			leave.s AFTER_TRY
		}
		catch [mscorlib]System.Object
		{
			pop

			ldarg.0		//name
			ldc.i4.6	//6 -- ReadWrite
			//Open an existing memory mapped file
			call		class [System.Core]System.IO.MemoryMappedFiles.MemoryMappedFile [System.Core]System.IO.MemoryMappedFiles.MemoryMappedFile::OpenExisting(string, valuetype [System.Core]System.IO.MemoryMappedFiles.MemoryMappedFileRights)
			//Store it in the output local
			stloc.0

			//Not owner
			ldc.i4.0
			stloc.2
			leave.s AFTER_TRY
		}
		AFTER_TRY:

		//Get a view accessor
		ldloc.0
		callvirt		instance class [System.Core]System.IO.MemoryMappedFiles.MemoryMappedViewAccessor [System.Core]System.IO.MemoryMappedFiles.MemoryMappedFile::CreateViewAccessor()
		dup
		
		//Initialize the handle in the view accessor
		callvirt		instance class [System.Core]Microsoft.Win32.SafeHandles.SafeMemoryMappedViewHandle [System.Core]System.IO.MemoryMappedFiles.MemoryMappedViewAccessor::get_SafeMemoryMappedViewHandle()
		ldsfld			int64 class Ipc.Memory.Components.RefStructWrapper`1<!T>::_size
		callvirt		instance void [mscorlib]System.Runtime.InteropServices.SafeBuffer::Initialize(uint64)

		//We still have the view accessor on the stack (thanks to the DUP), get the safe handle from it
		callvirt		instance class [System.Core]Microsoft.Win32.SafeHandles.SafeMemoryMappedViewHandle [System.Core]System.IO.MemoryMappedFiles.MemoryMappedViewAccessor::get_SafeMemoryMappedViewHandle()
		ldloca.s		bPtr
		callvirt		instance void [mscorlib]System.Runtime.InteropServices.SafeBuffer::AcquirePointer(uint8*&)

		//If we're not the owner, skip zeroing the memory
		ldloc.3
		brfalse			NO_ZERO
		ldloc.1
		call			void class Ipc.Memory.Components.RefStructWrapper`1<!T>::Zero(uint8*)

		NO_ZERO:

		ldloc.1
		ldloc.0
		ldloc.2
		newobj			instance void class Ipc.Memory.Components.RefStructWrapper`1<!T>::.ctor(!T*, class [System.Core]System.IO.MemoryMappedFiles.MemoryMappedFile, bool)
		ret
	}

	.method static void Zero(uint8* ptr) cil managed
	{
		.locals init (
			[0] int32,
			[1] int32
		)

		ldc.i4.0
		stloc.0

		ldsfld			int64 class Ipc.Memory.Components.RefStructWrapper`1<!T>::_size
		conv.i4
		stloc.1

		START:
		ldloc.0
		ldloc.1

		bge				END
		ldarg.0
		ldc.i4.0
		stind.i1

		ldloc.0
		ldc.i4.1
		add
		stloc.0

		END:
		ret
	}

	.method assembly hidebysig specialname rtspecialname instance void .ctor(!T* ptr, class [System.Core]System.IO.MemoryMappedFiles.MemoryMappedFile file, bool isOwner) cil managed
	{
		.maxstack 3

		ldarg.0
		newobj		instance void [mscorlib]System.Object::.ctor()
		ldarg.0
		ldarg.1
		stfld		!T* class Ipc.Memory.Components.RefStructWrapper`1<!T>::_val
		ldarg.0
		ldarg.2
		stfld		class [System.Core]System.IO.MemoryMappedFiles.MemoryMappedFile class Ipc.Memory.Components.RefStructWrapper`1<!T>::_file
		ret
	}

	.method public hidebysig specialname instance !T get_Value() cil managed
	{
		.maxstack 1

		ldarg.0
		ldfld			!T* class Ipc.Memory.Components.RefStructWrapper`1<!T>::_val
		ldind.ref
		ret
	}

	.method public hidebysig specialname instance void set_Value(!T) cil managed
	{
		.maxstack 2

		ldarg.0
		ldfld			!T* class Ipc.Memory.Components.RefStructWrapper`1<!T>::_val
		ldarg.1
		stobj			!T
		ret
	}
	
	.property instance !T Value()
	{
		.get instance !T Ipc.Memory.Components.RefStructWrapper`1::get_Value()
		.set instance void Ipc.Memory.Components.RefStructWrapper`1::set_Value(!T)
	}

	.method public hidebysig newslot virtual final instance void  Dispose() cil managed
	{
		.maxstack  2
		ldarg.0
		ldc.i4.1
		callvirt   instance void class Ipc.Memory.Components.RefStructWrapper`1<!T>::Dispose(bool)
		ldarg.0
		call       void [mscorlib]System.GC::SuppressFinalize(object)
		ret
	}

	.method family hidebysig newslot virtual instance void Dispose(bool isDisposing) cil managed
	{
		ret
	}
}