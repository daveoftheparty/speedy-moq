namespace Demo.Lib
{
	public interface IHasIndexerNoSetter
	{
		string this[string key] { get; }
	}
}