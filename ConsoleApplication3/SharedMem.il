.class public auto ansi beforefieldinit Microsoft.SharedMemIpc.SharedMem
       extends [mscorlib]System.Object
{
	.field private initonly string _mutexRoot
	.field private initonly string _objRoot
	.field private initonly class [mscorlib]System.Collections.Concurrent.ConcurrentDictionary`2<string,class [mscorlib]System.IDisposable> _objectRegistry

	.method public hidebysig specialname rtspecialname 
		instance void .ctor(string objRoot, string mutexRoot) cil managed
	{
		// Code size       34 (0x22)
		.maxstack  8
		IL_0000:  ldarg.0
		IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
		IL_0008:  ldarg.0
		IL_0009:  ldarg.1
		IL_000a:  stfld      string Microsoft.SharedMemIpc.SharedMem::_objRoot
		IL_000f:  ldarg.0
		IL_0010:  ldarg.2
		IL_0011:  stfld      string Microsoft.SharedMemIpc.SharedMem::_mutexRoot
		IL_0016:  ldarg.0
		IL_0017:  newobj     instance void class [mscorlib]System.Collections.Concurrent.ConcurrentDictionary`2<string,class [mscorlib]System.IDisposable>::.ctor()
		IL_001c:  stfld      class [mscorlib]System.Collections.Concurrent.ConcurrentDictionary`2<string,class [mscorlib]System.IDisposable> Microsoft.SharedMemIpc.SharedMem::_objectRegistry
		IL_0021:  ret
	} // end of method SharedMem::.ctor

	.method public hidebysig instance !!T* CreateObj<valuetype .ctor ([System.Runtime]System.ValueType) T>(string name) cil managed
	{
		IL_0000:  ldarg.0
		IL_0001:  ldfld		string Microsoft.SharedMemIpc.SharedMem::_objRoot
		IL_0002:  ldarg.1
		IL_0003:  call		string [mscorlib]System.String::Format(string, string)
		IL_0004:  call		class Microsoft.SharedMemIpc.ObjectWrapper`1<!!T> class Microsoft.SharedMemIpc.ObjectWrapper<!!T>::Demand(string)
		IL_000A:  ret			
	} // end of method SharedMem::CreateObj

} // end of class ConsoleApplication1.SharedMem


.class public auto ansi beforefieldinit Microsoft.SharedMemIpc.ObjectWrapper`1<valuetype .ctor ([mscorlib]System.ValueType) T>
       extends [mscorlib]System.Object
       implements [mscorlib]System.IDisposable
{
	.method public hidebysig static class Microsoft.SharedMemIpc.ObjectWrapper`1<!T> Demand(string name) cil managed
	{
		.maxstack  1
		ldarg.0
		newobj     instance void class Microsoft.SharedMemIpc.ObjectWrapper`1<!T>::.ctor(class [System.Core]System.IO.MemoryMappedFiles.MemoryMappedFile, bool)
		ret
	}

	.method assembly hidebysig specialname rtspecialname 
		instance void .ctor(class [System.Core]System.IO.MemoryMappedFiles.MemoryMappedFile, bool isOwner) cil managed
	{
		ret
	}
}