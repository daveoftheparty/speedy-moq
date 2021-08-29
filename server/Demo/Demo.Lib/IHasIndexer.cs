namespace Demo.Lib
{
	public interface IHasIndexer
	{
		string this[string key] { get; set; }
	}
}