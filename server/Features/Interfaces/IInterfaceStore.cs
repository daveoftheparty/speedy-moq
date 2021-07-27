using Features.Model;

namespace Features.Interfaces
{
	public interface IInterfaceStore
	{
		InterfaceDefinition GetInterfaceDefinition(string interfaceName);

		/*
			TBD: a method for updating an interface definition when textDocument/didChange fires and changes an interface
		*/
	}
}